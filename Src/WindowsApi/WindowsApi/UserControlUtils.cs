using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace WindowsApi
{
    public class UserControlUtils
    {
        private HwndSource _ps = null;
        public UserControlUtils(UserControl control)
        {
            object var = System.Windows.Interop.HwndSource.FromDependencyObject(control);// as HwndSource;
            var = HwndSource.FromVisual(control);
        }
        public IntPtr Handle
        {
            get
            {
                return _ps.Handle;
            }
        }
    }

    //Afx:ControlBar:40000000:8:10005:10
}
