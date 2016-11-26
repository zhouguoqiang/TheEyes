using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Autodesk.RevitAddIns;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Data;
using Res = AddinInstall.Properties.Resources;
using System.Reflection;
using System.IO;


namespace AddinInstall
{
    class Utility
    {
    }

    public class ViewModel:INotifyPropertyChanged
    {
        private string path = string.Empty;
        public string Path
        {
            get 
            {
                return path;
            }
            set 
            {
                path = value;
                NotifyPropertyChanged("Path");
            }
        }

        private RevitProduct rvtProduct = null;
        public RevitProduct RvtProduct
        {
            get { return rvtProduct; }
            set { rvtProduct = value; NotifyPropertyChanged("RvtProduct"); }
        }
        private List<RevitProduct> rvtProducts = new List<RevitProduct>();
        public List<RevitProduct> RvtProducts 
        {
            get 
            {
                return rvtProducts;
            }
        }
        
        public BtnIsEnabledConvert Convertor
        {
            get { return new BtnIsEnabledConvert(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        { 
            if(PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public  ViewModel()
        {
            rvtProducts = RevitProductUtility.GetAllInstalledRevitProducts();
        }

        
        public ICommand OK_Command
        {
            get 
            {
                return new OK_Command(this);
            }
        }

        public ICommand Cancel_Command
        {
            get
            {
                return new Cancel_Command();
            }
        }
        public ICommand PathCommand
        {
            get 
            {
                return new PathCommand(this);
            }
        }
    }

    public class OK_Command : ICommand
    {
        ViewModel viewModel = null;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            string fullPath = viewModel.Path + "\\AddInManager.dll";
            MainWindow myWin = parameter as MainWindow;
            myWin.Close();
            Install(viewModel.RvtProduct, fullPath);        
        }
        private void Install(RevitProduct rvtProduct,string path)
        {
            byte[] dll = null;
            switch(rvtProduct.Version)
            {
                case RevitVersion.Revit2015:
                    dll = Res.AddInManager2015;
                    break;
                case RevitVersion.Revit2016:
                    dll = Res.AddInManager2016;
                    break;
                case RevitVersion.Revit2017:
                    dll = Res.AddInManager2017;
                    break;
            }
            using (FileStream fs = File.Create(path))
            {
                fs.Write(dll, 0, dll.Length);
                fs.Close();
            }
            string addinPath = rvtProduct.AllUsersAddInFolder+"\\Autodesk_AddInManager.addin";
            using(FileStream fs = File.Create(addinPath))
            {
                byte[] addin = Res.Autodesk_AddInManager;
                fs.Write(addin, 0, addin.Length);
                fs.Close();
            }
            string str = File.ReadAllText(addinPath, Encoding.UTF8);
            str = str.Replace("[TARGETDIR]AddInManager.dll", path);
            File.WriteAllText(addinPath, str, Encoding.UTF8);
        }
        
        public OK_Command(ViewModel vm)
        {
            this.viewModel = vm;
        }
    }
    public class Cancel_Command : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MainWindow myWin = parameter as MainWindow;
            //myWin.DialogResult = false;
            myWin.Close();
        }
    }

    public class PathCommand : ICommand
    {
        ViewModel viewModel = null;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult dr = dialog.ShowDialog();
            if(dr==DialogResult.OK)
            {
                viewModel.Path = dialog.SelectedPath;
            }
        }

        public PathCommand(ViewModel vm)
        {
            this.viewModel = vm;
        }

    }

    public class BtnIsEnabledConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value == null)
                    return false;
                else
                {
                    if(value is string)
                    {
                        if (value as string == string.Empty)
                            return false;
                    }
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
