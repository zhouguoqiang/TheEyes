using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitApiTest.Commands;

namespace RevitApiTest
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            IExternalCommand icmd = null;
            icmd = new JoinGeometryUtilsTest();
            //icmd = new SolidSolidCutUtilsTest();
            return icmd.Execute(commandData, ref message, elements);
        }
    }
}
