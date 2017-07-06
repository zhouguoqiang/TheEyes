using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsApi;

namespace WindowLocationTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            RECT rect = new RECT();
            //HandleRef
            bool flag = NativeMethods.GetWindowRect(helper.Handle, out rect);
            if(flag)
            {
                string info = null;
                info += string.Format("Left:{0}", rect.Left) + "\n";
                info += string.Format("Right:{0}", rect.Right) + "\n";
                info += string.Format("Top:{0}", rect.Top) + "\n";
                info += string.Format("Bottom:{0}", rect.Bottom) + "\n";
                MessageBox.Show(info);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);

            NativeMethods.MoveWindow(helper.Handle, 0, 0, (int)Width/2,(int)Height/2, true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(btn)).Handle;
            MessageBox.Show(hwnd.ToInt32().ToString());
            RECT rect = new RECT();
            NativeMethods.GetWindowRect(hwnd , out rect);

            NativeMethods.MoveWindow(hwnd, rect.Left, rect.Top, 200, 30, true);

            //NativeMethods.MoveWindow(usu.Handle, 0, 0, 200, 25, true);

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //TestControl cotrol = new TestControl();

            //UserControlUtils usu = new UserControlUtils(cotrol);
            //WindowInteropHelper helper = new WindowInteropHelper(this);

            //NativeMethods.SetParent(usu.Handle, helper.Handle);
            //RECT rect = new RECT();
            //NativeMethods.GetWindowRect(helper.Handle, out rect);

            //NativeMethods.MoveWindow(usu.Handle, rect.Left, rect.Top, 200, 30, true);

            //NativeMethods.MoveWindow(usu.Handle, 0, 0, 200, 25, true);

            ChildrenWindow win = new ChildrenWindow();

            WindowInteropHelper helper = new WindowInteropHelper(win);
            WindowInteropHelper helper1 = new WindowInteropHelper(this);
            NativeMethods.SetParent(helper.Handle, helper1.Handle);
            win.Show();
        }
    }
}
