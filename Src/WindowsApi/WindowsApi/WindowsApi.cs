using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsApi
{

    /// <summary>
    /// to install the key and mouse hook
    /// and then the PickObjects method can be completed by space key and right mouse button
    /// </summary>
    public class HookUtils
    {
        /// <summary>
        /// click button
        /// </summary>
        private static readonly uint BM_CLICK = 0x00F5;
        /// <summary>
        /// mouse right button up
        /// </summary>
        private static readonly int WM_RBUTTONUP = 0x205;

        private static readonly uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private static readonly uint KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// ESC key's ascii vaule
        /// </summary>
        private static readonly byte ESC = 0x1B;

        private static IntPtr rvtIntPtr = IntPtr.Zero;
        private static IntPtr keyHookValue = IntPtr.Zero;
        private static IntPtr mouseHookValue = IntPtr.Zero;

        private static HookUtils _default = null;
        private static readonly object locker = new object();
        public static HookUtils Default
        {
            get
            {
                if (_default == null)
                {
                    lock (locker)
                    {
                        _default = new HookUtils();
                    }
                }
                return _default;
            }
        }
        private HookUtils()
        {
            rvtIntPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
            _keyHookProc = new HookProc(KeyHookProc);
            _mouseHookProc = new HookProc(MouseHookProc);
        }

        private HookProc _keyHookProc = null;
        private HookProc _mouseHookProc = null;

        private int KeyHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int w = wParam.ToInt32();
            long l = lParam.ToInt64();

            #region when space key down,to click the finish button(to complete the PickObjects method)
            if (w == 32 && l == 3735553)
            {
                ControlIntPrtUtils utils = new ControlIntPrtUtils("Button", "完成");
                List<IntPtr> prts = utils.GetHandle(rvtIntPtr, IntPtr.Zero);
                if (prts != null && prts.Count > 0)
                {
                    if (prts.First() != IntPtr.Zero)
                    {
                        bool f = NativeMethods.PostMessage(prts.First(), BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            #endregion

            #region if the Esc key down, to close the current dialog window(if the window is not null)
            if (w == 27 && l == 65537)
            {
                //WindowUtils.RaiseEscape();
            }
            #endregion

            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            long w = wParam.ToInt64();
            long l = lParam.ToInt64();

            #region when space key down,to click the finish button(to complete the PickObjects method)
            if (w == WM_RBUTTONUP)
            {
                ControlIntPrtUtils utils = new ControlIntPrtUtils("Button", "完成");
                List<IntPtr> prts = utils.GetHandle(rvtIntPtr, IntPtr.Zero);
                if (prts != null && prts.Count > 0)
                {
                    if (prts.First() != IntPtr.Zero)
                    {
                        bool f = NativeMethods.PostMessage(prts.First(), BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                        return -1;
                    }
                }
            }
            #endregion
            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        /// <summary>
        /// install hook
        /// </summary>
        public void Install()
        {
            if (keyHookValue == IntPtr.Zero)
                keyHookValue = NativeMethods.SetWindowsHookEx(HookType.WH_KEYBOARD, _keyHookProc, IntPtr.Zero, NativeMethods.GetCurrentThreadId());
            if (mouseHookValue == IntPtr.Zero)
                mouseHookValue = NativeMethods.SetWindowsHookEx(HookType.WH_MOUSE, _mouseHookProc, IntPtr.Zero, NativeMethods.GetCurrentThreadId());
        }

        /// <summary>
        /// simulate Esc down and up
        /// to exit the PickObjets or similar methods,after the modeless window closed
        /// </summary>
        public void PressESC()
        {
            NativeMethods.SetForegroundWindow(rvtIntPtr);
            NativeMethods.keybd_event(ESC, 0, KEYEVENTF_EXTENDEDKEY, 0);
            NativeMethods.keybd_event(ESC, 0, KEYEVENTF_KEYUP, 0);
        }
    }



    #region NativeMethod

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }

    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }

    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
    public enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    public class NativeMethods
    {
        //设置钩子
        /// <summary>
        ///     Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the
        ///     system for certain types of events. These events are associated either with a specific thread or with all threads
        ///     in the same desktop as the calling thread.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms644990%28v=vs.85%29.aspx for more information</para>
        /// </summary>
        /// <param name="hookType">
        ///     C++ ( idHook [in]. Type: int )<br />The type of hook procedure to be installed. This parameter can be one of the
        ///     following values.
        ///     <list type="table">
        ///     <listheader>
        ///         <term>Possible Hook Types</term>
        ///     </listheader>
        ///     <item>
        ///         <term>WH_CALLWNDPROC (4)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages before the system sends them to the
        ///         destination window procedure. For more information, see the CallWndProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CALLWNDPROCRET (12)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages after they have been processed by the
        ///         destination window procedure. For more information, see the CallWndRetProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CBT (5)</term>
        ///         <description>
        ///         Installs a hook procedure that receives notifications useful to a CBT application. For more
        ///         information, see the CBTProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_DEBUG (9)</term>
        ///         <description>
        ///         Installs a hook procedure useful for debugging other hook procedures. For more information,
        ///         see the DebugProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_FOREGROUNDIDLE (11)</term>
        ///         <description>
        ///         Installs a hook procedure that will be called when the application's foreground thread is
        ///         about to become idle. This hook is useful for performing low priority tasks during idle time. For more
        ///         information, see the ForegroundIdleProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_GETMESSAGE (3)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages posted to a message queue. For more
        ///         information, see the GetMsgProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK (1)</term>
        ///         <description>
        ///         Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook
        ///         procedure. For more information, see the JournalPlaybackProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD (0)</term>
        ///         <description>
        ///         Installs a hook procedure that records input messages posted to the system message queue. This
        ///         hook is useful for recording macros. For more information, see the JournalRecordProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD (2)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors keystroke messages. For more information, see the
        ///         KeyboardProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL (13)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors low-level keyboard input events. For more information,
        ///         see the LowLevelKeyboardProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE (7)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors mouse messages. For more information, see the
        ///         MouseProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL (14)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors low-level mouse input events. For more information,
        ///         see the LowLevelMouseProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MSGFILTER (-1)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages generated as a result of an input event in a
        ///         dialog box, message box, menu, or scroll bar. For more information, see the MessageProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SHELL (10)</term>
        ///         <description>
        ///         Installs a hook procedure that receives notifications useful to shell applications. For more
        ///         information, see the ShellProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SYSMSGFILTER (6)</term><description></description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <param name="lpfn">
        ///     C++ ( lpfn [in]. Type: HOOKPROC )<br />A pointer to the hook procedure. If the dwThreadId parameter
        ///     is zero or specifies the identifier of a thread created by a different process, the lpfn parameter must point to a
        ///     hook procedure in a DLL. Otherwise, lpfn can point to a hook procedure in the code associated with the current
        ///     process.
        /// </param>
        /// <param name="hMod">
        ///     C++ ( hMod [in]. Type: HINSTANCE )<br />A handle to the DLL containing the hook procedure pointed to
        ///     by the lpfn parameter. The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread
        ///     created by the current process and if the hook procedure is within the code associated with the current process.
        /// </param>
        /// <param name="dwThreadId">
        ///     C++ ( dwThreadId [in]. Type: DWORD )<br />The identifier of the thread with which the hook
        ///     procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with
        ///     all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks
        ///     section.
        /// </param>
        /// <returns>
        ///     C++ ( Type: HHOOK )<br />If the function succeeds, the return value is the handle to the hook procedure. If
        ///     the function fails, the return value is NULL.
        ///     <para>To get extended error information, call GetLastError.</para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///     SetWindowsHookEx can be used to inject a DLL into another process. A 32-bit DLL cannot be injected into a
        ///     64-bit process, and a 64-bit DLL cannot be injected into a 32-bit process. If an application requires the use
        ///     of hooks in other processes, it is required that a 32-bit application call SetWindowsHookEx to inject a 32-bit
        ///     DLL into 32-bit processes, and a 64-bit application call SetWindowsHookEx to inject a 64-bit DLL into 64-bit
        ///     processes. The 32-bit and 64-bit DLLs must have different names.
        ///     </para>
        ///     <para>
        ///     Because hooks run in the context of an application, they must match the "bitness" of the application. If a
        ///     32-bit application installs a global hook on 64-bit Windows, the 32-bit hook is injected into each 32-bit
        ///     process (the usual security boundaries apply). In a 64-bit process, the threads are still marked as "hooked."
        ///     However, because a 32-bit application must run the hook code, the system executes the hook in the hooking app's
        ///     context; specifically, on the thread that called SetWindowsHookEx. This means that the hooking application must
        ///     continue to pump messages or it might block the normal functioning of the 64-bit processes.
        ///     </para>
        ///     <para>
        ///     If a 64-bit application installs a global hook on 64-bit Windows, the 64-bit hook is injected into each
        ///     64-bit process, while all 32-bit processes use a callback to the hooking application.
        ///     </para>
        ///     <para>
        ///     To hook all applications on the desktop of a 64-bit Windows installation, install a 32-bit global hook and a
        ///     64-bit global hook, each from appropriate processes, and be sure to keep pumping messages in the hooking
        ///     application to avoid blocking normal functioning. If you already have a 32-bit global hooking application and
        ///     it doesn't need to run in each application's context, you may not need to create a 64-bit version.
        ///     </para>
        ///     <para>
        ///     An error may occur if the hMod parameter is NULL and the dwThreadId parameter is zero or specifies the
        ///     identifier of a thread created by another process.
        ///     </para>
        ///     <para>
        ///     Calling the CallNextHookEx function to chain to the next hook procedure is optional, but it is highly
        ///     recommended; otherwise, other applications that have installed hooks will not receive hook notifications and
        ///     may behave incorrectly as a result. You should call CallNextHookEx unless you absolutely need to prevent the
        ///     notification from being seen by other applications.
        ///     </para>
        ///     <para>
        ///     Before terminating, an application must call the UnhookWindowsHookEx function to free system resources
        ///     associated with the hook.
        ///     </para>
        ///     <para>
        ///     The scope of a hook depends on the hook type. Some hooks can be set only with global scope; others can also
        ///     be set for only a specific thread, as shown in the following table.
        ///     </para>
        ///     <list type="table">
        ///     <listheader>
        ///         <term>Possible Hook Types</term>
        ///     </listheader>
        ///     <item>
        ///         <term>WH_CALLWNDPROC (4)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CALLWNDPROCRET (12)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CBT (5)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_DEBUG (9)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_FOREGROUNDIDLE (11)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_GETMESSAGE (3)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK (1)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD (0)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD (2)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL (13)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE (7)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL (14)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MSGFILTER (-1)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SHELL (10)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SYSMSGFILTER (6)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     </list>
        ///     <para>
        ///     For a specified hook type, thread hooks are called first, then global hooks. Be aware that the WH_MOUSE,
        ///     WH_KEYBOARD, WH_JOURNAL*, WH_SHELL, and low-level hooks can be called on the thread that installed the hook
        ///     rather than the thread processing the hook. For these hooks, it is possible that both the 32-bit and 64-bit
        ///     hooks will be called if a 32-bit hook is ahead of a 64-bit hook in the hook chain.
        ///     </para>
        ///     <para>
        ///     The global hooks are a shared resource, and installing one affects all applications in the same desktop as
        ///     the calling thread. All global hook functions must be in libraries. Global hooks should be restricted to
        ///     special-purpose applications or to use as a development aid during application debugging. Libraries that no
        ///     longer need a hook should remove its hook procedure.
        ///     </para>
        ///     <para>
        ///     Windows Store app development If dwThreadId is zero, then window hook DLLs are not loaded in-process for the
        ///     Windows Store app processes and the Windows Runtime broker process unless they are installed by either UIAccess
        ///     processes (accessibility tools). The notification is delivered on the installer's thread for these hooks:
        ///     </para>
        ///     <list type="bullet">
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK</term>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL </term>
        ///     </item>
        ///     </list>
        ///     <para>
        ///     This behavior is similar to what happens when there is an architecture mismatch between the hook DLL and the
        ///     target application process, for example, when the hook DLL is 32-bit and the application process 64-bit.
        ///     </para>
        ///     <para>
        ///     For an example, see Installing and
        ///     <see
        ///         cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644960%28v=vs.85%29.aspx#installing_releasing">
        ///         Releasing
        ///         Hook Procedures.
        ///     </see>
        ///     [
        ///     https://msdn.microsoft.com/en-us/library/windows/desktop/ms644960%28v=vs.85%29.aspx#installing_releasing ]
        ///     </para>
        /// </remarks> 
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        //卸载钩子
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        //调用下一个钩�? 
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int cch);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

        /// <summary>
        ///     Brings the thread that created the specified window into the foreground and activates the window. Keyboard input is
        ///     directed to the window, and various visual cues are changed for the user. The system assigns a slightly higher
        ///     priority to the thread that created the foreground window than it does to other threads.
        ///     <para>See for https://msdn.microsoft.com/en-us/library/windows/desktop/ms633539%28v=vs.85%29.aspx more information.</para>
        /// </summary>
        /// <param name="hWnd">
        ///     C++ ( hWnd [in]. Type: HWND )<br />A handle to the window that should be activated and brought to the foreground.
        /// </param>
        /// <returns>
        ///     <c>true</c> or nonzero if the window was brought to the foreground, <c>false</c> or zero If the window was not
        ///     brought to the foreground.
        /// </returns>
        /// <remarks>
        ///     The system restricts which processes can set the foreground window. A process can set the foreground window only if
        ///     one of the following conditions is true:
        ///     <list type="bullet">
        ///     <listheader>
        ///         <term>Conditions</term><description></description>
        ///     </listheader>
        ///     <item>The process is the foreground process.</item>
        ///     <item>The process was started by the foreground process.</item>
        ///     <item>The process received the last input event.</item>
        ///     <item>There is no foreground process.</item>
        ///     <item>The process is being debugged.</item>
        ///     <item>The foreground process is not a Modern Application or the Start Screen.</item>
        ///     <item>The foreground is not locked (see LockSetForegroundWindow).</item>
        ///     <item>The foreground lock time-out has expired (see SPI_GETFOREGROUNDLOCKTIMEOUT in SystemParametersInfo).</item>
        ///     <item>No menus are active.</item>
        ///     </list>
        ///     <para>
        ///     An application cannot force a window to the foreground while the user is working with another window.
        ///     Instead, Windows flashes the taskbar button of the window to notify the user.
        ///     </para>
        ///     <para>
        ///     A process that can set the foreground window can enable another process to set the foreground window by
        ///     calling the AllowSetForegroundWindow function. The process specified by dwProcessId loses the ability to set
        ///     the foreground window the next time the user generates input, unless the input is directed at that process, or
        ///     the next time a process calls AllowSetForegroundWindow, unless that process is specified.
        ///     </para>
        ///     <para>
        ///     The foreground process can disable calls to SetForegroundWindow by calling the LockSetForegroundWindow
        ///     function.
        ///     </para>
        /// </remarks>
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, string lpString, int cch);

        /// <summary>
        /// 获取类名�?
        /// </summary>
        /// <param name="hwnd">需要获取类名的句柄</param>
        /// <param name="lpClassName">类名(执行完成以后查看)</param>
        /// <param name="nMaxCount">缓冲�?</param>
        /// private static bool isIEServerWindow(IntPtr hWnd)
        ///{
        ///    int nRet;
        ///        // Pre-allocate 256 characters, since this is the maximum class name length.
        ///        StringBuilder ClassName = new StringBuilder(256);
        ///        //Get the window class name
        ///        nRet = GetClassName(hWnd, ClassName, ClassName.Capacity);
        ///    if(nRet != 0)
        ///    {
        ///        return (string.Compare(ClassName.ToString(), "Internet Explorer_Server",true, CultureInfo.InvariantCulture) == 0);
        ///    }
        ///    else
        ///    {
        ///        return false;
        ///    }
        ///}
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        ///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
        ///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
        ///     to the upper-left corner of the parent window's client area.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
        /// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
        /// <param name="bRepaint">
        ///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
        ///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
        ///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///     parent window uncovered as a result of moving a child window.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
        ///     <br />To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window.
        /// </param>
        /// <param name="lpwndpl">
        /// A pointer to the WINDOWPLACEMENT structure that receives the show state and position information.
        /// <para>
        /// Before calling GetWindowPlacement, set the length member to sizeof(WINDOWPLACEMENT). GetWindowPlacement fails if lpwndpl-> length is not set correctly.
        /// </para>
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// <para>
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </para>
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }

    /// <summary>
    /// to find the windows control use the class name and window title
    /// </summary>
    public class ControlIntPrtUtils
    {
        private string className = string.Empty;
        private string windowTitle = string.Empty;
        public ControlIntPrtUtils(string className, string windowTitle)
        {
            this.className = className;
            this.windowTitle = windowTitle;
        }
        public List<IntPtr> GetHandle(IntPtr parentHandle, IntPtr childAfter)
        {
            List<IntPtr> prts = new List<IntPtr>();
            IntPtr hwnd = NativeMethods.FindWindowEx(parentHandle, IntPtr.Zero, className, windowTitle);
            if (hwnd != IntPtr.Zero)
            {
                prts.Add(hwnd);
                return prts;
            }
            GCHandle gch = GCHandle.Alloc(prts);
            NativeMethods.EnumChildWindows(parentHandle, new EnumWindowProc(EnumCallBack), GCHandle.ToIntPtr(gch));
            return prts;
        }
        private bool EnumCallBack(IntPtr hWnd, IntPtr parameter)
        {
            IntPtr hwnd = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, className, windowTitle);
            if (hwnd != IntPtr.Zero)
            {
                GCHandle gch = GCHandle.FromIntPtr(parameter);
                List<IntPtr> prts = gch.Target as List<IntPtr>;
                prts.Add(hwnd);
                return false;
            }
            return true;
        }
    }

    #endregion

    //HandleRef
    //System.Windows.Interop.HwndSource.FromDependencyObject

}
