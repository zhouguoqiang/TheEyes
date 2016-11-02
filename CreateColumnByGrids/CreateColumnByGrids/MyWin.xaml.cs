using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using System.ComponentModel;

namespace CreateColumnByGrids
{
    /// <summary>
    /// MyWin.xaml 的交互逻辑
    /// </summary>
    public partial class MyWin : Window
    { 
        public MyWin()
        {
            InitializeComponent();           
        }
        public MyWin(MyDataContext dataContext)
        {
            InitializeComponent();             
            this.DataContext = dataContext;
        }
    }

    public class MyDataContext : INotifyPropertyChanged 
    {
        private List<ComboBoxData> _AllLevels = new List<ComboBoxData>();
        public List<ComboBoxData> AllLevels { get { return _AllLevels; } private set { _AllLevels = value; } }

        private List<ComboBoxData> _AllSymbol = new List<ComboBoxData>();
        public List<ComboBoxData> AllSymbol { get { return _AllSymbol; } private set { _AllSymbol = value; } }

        private Element symbol = null;
        public Element Symbol
        {
            get 
            {
                if (symbol == null)
                    return _AllSymbol.First().Element;
                return symbol;
            }
            set 
            {
                symbol = value;
                NotifyPropertyChanged("Symbol");
            }
        }

        private Element topLevel = null;
        public Element TopLevel
        {
            get
            {
                if (topLevel == null)
                    return _AllLevels.First().Element;
                return topLevel;
            }
            set
            {
                topLevel = value;
                NotifyPropertyChanged("TopLevel");
                (OK_Command as OK_Command).NotifyPropertyChanged("OK_Command"); 
            }
        }

        private Element btmLevel = null;
        public Element BtmLevel
        {
            get
            {
                if (btmLevel == null)
                    return _AllLevels.First().Element;
                return btmLevel;
            }
            set
            {
                btmLevel = value; 
                NotifyPropertyChanged("BtmLevel");
                (OK_Command as OK_Command).NotifyPropertyChanged("OK_Command"); 
            }
        }

        private double topOffset = 0.0;
        public double TopOffset
        {
            get { return topOffset; }
            set
            {
                topOffset = value;
                NotifyPropertyChanged("TopOffset");
                (OK_Command as OK_Command).NotifyPropertyChanged("OK_Command");
            }
        }

        private double btmOffset = 0.0;
        public double BtmOffset { get { return btmOffset; } 
            set 
            { 
                btmOffset = value; 
                NotifyPropertyChanged("BtmOffset");
                (OK_Command as OK_Command).NotifyPropertyChanged("OK_Command"); 
            }
        }
        public ICommand OK_Command { get; set; }
        public ICommand Cancel_Command { get; set; }

        public MyDataContext(Document doc)
        {
            FilteredElementCollector lvlFilter = new FilteredElementCollector(doc);
            List<Level> lvls = lvlFilter.OfClass(typeof(Level)).Cast<Level>().ToList();
            foreach(Element elm in lvls)
            {
                _AllLevels.Add(new ComboBoxData(elm));
            }

            FilteredElementCollector symbolFilter = new FilteredElementCollector(doc);
            List<FamilySymbol> symbols = symbolFilter.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Columns).Cast<FamilySymbol>().ToList();
            foreach (Element elm in symbols)
            {
                _AllSymbol.Add(new ComboBoxData(elm));
            }

            OK_Command = new OK_Command(this);
            Cancel_Command = new Cancel_Command();

        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string Name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }
    }
    public class OK_Command : ICommand
    {
        MyDataContext _context;
        public OK_Command(MyDataContext context)
        {
            _context = context;
        }
        public bool CanExecute(object parameter)
        {
            Level topLevel = _context.TopLevel as Level;
            Level btmLevel = _context.BtmLevel as Level;
            if (topLevel == null || btmLevel == null)
                return false;
            if (topLevel.Elevation + _context.TopOffset - (btmLevel.Elevation + _context.BtmOffset) > 0.001)
                return true;
            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void NotifyPropertyChanged(string Name)
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public void Execute(object parameter)
        {
            MyWin myWin = parameter as MyWin;
            if (myWin == null)
                return;

            if (myWin.symbol.SelectedItem == null)
                return;
            if (myWin.topLvl.SelectedItem == null)
                return;
            double TopOffset = 0.0;
            if (!double.TryParse(myWin.topOffset.Text, out TopOffset))
            {
                return;
            }
            if (myWin.btmLvl.SelectedItem == null)
                return;
            double BtmOffset = 0.0;
            if (!double.TryParse(myWin.btmOffset.Text, out BtmOffset))
            {
                return;
            }
            Level TopLevel = myWin.topLvl.SelectedValue as Level;
            Level BtmLevel = myWin.btmLvl.SelectedValue as Level;
            if (TopLevel != null && BtmLevel != null)
            {
                if (BtmLevel.Elevation + BtmOffset > TopLevel.Elevation + TopOffset)
                    return;
            }
            else
            {
                return;
            }

            myWin.DialogResult = true;
            myWin.Close();
        }
    }
    public class Cancel_Command : ICommand
    {
        public bool CanExecute(object parameter)
        {
            MyWin myWin = parameter as MyWin;

            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MyWin myWin = parameter as MyWin;
            myWin.DialogResult = false;
            myWin.Close();
        }
    }

    public class ComboBoxData
    {
        public Element Element { get; set; }
        public string Name { get; set; }

        public ComboBoxData(Element element)
        {
            this.Element = element;
            this.Name = element.Name;
        }

    }
}
