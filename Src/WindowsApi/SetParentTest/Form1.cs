using SetParentTest.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using WindowsApi;

namespace SetParentTest
{
    public partial class Form1 : Form
    {
        TestControl tc = null;
        Window1 testWin = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tc = new TestControl();
            
            RECT rect = new RECT();
            NativeMethods.GetWindowRect(this.Handle, out rect);
            NativeMethods.SetParent(tc.Handle, this.Handle);
            NativeMethods.MoveWindow(tc.Handle, 0, 0, tc.Width, tc.Height, true);
            //tc.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tc.Dispose();
            //NativeMethods.SetParent(tc.Handle, IntPtr.Zero);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            testWin = new Window1();

            WindowInteropHelper helper = new WindowInteropHelper(testWin);
            //helper.Owner = this.Handle;
            RECT rect = new RECT();
            NativeMethods.GetWindowRect(this.Handle, out rect);
          
            NativeMethods.SetParent(helper.Handle, this.Handle);

            NativeMethods.MoveWindow(helper.Handle,0, 0, (int)testWin.ActualWidth, (int)testWin.ActualHeight, true);
            testWin.ResizeMode = System.Windows.ResizeMode.NoResize;
            testWin.Show();


        }

        private void button4_Click(object sender, EventArgs e)
        {
            testWin.Close();
        }
    }
}
