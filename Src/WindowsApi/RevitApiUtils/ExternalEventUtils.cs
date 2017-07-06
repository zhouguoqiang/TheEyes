using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UIFrameworkServices;

namespace RevitApiUtils
{
    /// <summary>
    /// a class to quick raise a external event,
    /// and this class should be initialized at the start of the application
    /// </summary>
    public class ExternalEventUtils : IExternalEventHandler
    {
        private static ExternalEventUtils _default = null;
        public static ExternalEventUtils Default
        {
            get
            {
                if (_default == null)
                {
                    throw new Exception("not initialized");
                }
                return _default;
            }
        }

        private static object locker = new object();

        private string _cmdName = string.Empty;

        private ExternalEvent exEvent = null;

        private ExternalEventUtils()
        {
            exEvent = ExternalEvent.Create(this);
        }

        private Action<UIApplication> _excute = null;

        public void Execute(UIApplication app)
        {
            if (_excute != null)
            {
                lock (locker)
                {
                    _excute(app);
                }
            }
        }

        public string GetName()
        {
            if (!string.IsNullOrWhiteSpace(_cmdName))
            {
                return _cmdName;
            }
            return "cmdName";
        }

        public ExternalEventRequest RaiseEevent(Action<UIApplication> action, string cmdName = null)
        {
            lock (locker)
            {
                _excute = action;
                _cmdName = cmdName;
                return exEvent.Raise();
            }
        }

        public static void Init()
        {
            lock (locker)
            {
                _default = new ExternalEventUtils();
            }
        }
    }

    public class RvtExternalEventUtils
    {
        private Autodesk.Windows.RibbonButton ribbonButton = null;

        public Func<ExternalCommandData, Result> Excute { get; set; }
        private static object locker = new object();
        private static RvtExternalEventUtils _default = null;
        public static RvtExternalEventUtils Default
        {
            get
            {
                if (_default == null)
                {
                    lock (locker)
                    {
                        _default = new RvtExternalEventUtils();
                    }
                }
                return _default;
            }
        }

        private RvtExternalEventUtils()
        {

        }

        public void RaiseFuc(Func<ExternalCommandData, Result> fuc)
        {
            lock (locker)
            {
                Excute = fuc;
                Autodesk.Windows.ComponentManager.Ribbon.Dispatcher.Invoke(() =>
                {
                    ExternalCommandHelper.executeExternalCommand(ribbonButton.Id);
                });
            }
        }

        public void SetItem(PushButton btn)
        {
            MethodInfo method = btn.GetType().GetMethod("getRibbonButton", BindingFlags.NonPublic | BindingFlags.Instance);
            ribbonButton = method.Invoke(btn, null) as Autodesk.Windows.RibbonButton;
        }

    }
}
