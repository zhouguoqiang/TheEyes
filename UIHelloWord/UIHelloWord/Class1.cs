using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace UIHelloWord
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MyApp:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string myTab = "MyTab";
            string myPanel = "MyPanel";
            application.CreateRibbonTab(myTab);
            RibbonPanel panel = application.CreateRibbonPanel(myTab, myPanel);

            string btnName = "MY_FIRST_BTN"; // 按钮的name必须要唯一，用户可以随意命名
            string btnText = "命令按钮"; // 按钮上面显示的文字，用户可以随意命名
            string btnAssemblyName = this.GetType().Assembly.Location; //命令所在的dll 的路径
            string btnClassName = "UIHelloWord.MyCommand";// 命令的命名空间 加类名
            PushButtonData btnData = new PushButtonData(btnName, btnText, btnAssemblyName, btnClassName);

            PushButton pbtn = (PushButton)panel.AddItem(btnData);

            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("我的第一个命令", "Hello Word");
            return Result.Succeeded;
        }
    }

}
