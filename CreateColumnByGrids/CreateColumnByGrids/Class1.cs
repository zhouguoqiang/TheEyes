using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;

namespace CreateColumnByGrids
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class Class1:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FilteredElementCollector gridFilter = new FilteredElementCollector(doc);

            List<Grid> allGrids = gridFilter.OfClass(typeof(Grid)).Cast<Grid>().ToList();
            List<XYZ> Points = new List<XYZ>();

            foreach (Grid grid in allGrids)
            {
                Grid currentGrid = grid;
                foreach (Grid grd in allGrids)
                { 
                    IntersectionResultArray ira = null;
                    SetComparisonResult scr = currentGrid.Curve.Intersect(grd.Curve, out ira);
                    if (ira != null)
                    {
                        IntersectionResult ir = ira.get_Item(0);
                        if (!CheckPoint(Points,ir.XYZPoint))
                        {
                            Points.Add(ir.XYZPoint);
                        }
                    }
                }
            }
            MyDataContext myDataContext = new MyDataContext(doc);
            MyWin myWin = new MyWin(myDataContext);
            if (myWin.ShowDialog() ?? false)
            {
                FamilySymbol symbol = myDataContext.Symbol as FamilySymbol;
                Level topLevel = myDataContext.TopLevel as Level;
                Level btmLevel = myDataContext.BtmLevel as Level;
                double topOffset = myDataContext.TopOffset / 304.8;
                double btmOffset = myDataContext.BtmOffset / 304.8;
                Transaction trans = new Transaction(doc, "Create");
                trans.Start();
                foreach(XYZ p in Points)
                {
                    FamilyInstance column = doc.Create.NewFamilyInstance(p, symbol, btmLevel, StructuralType.NonStructural);
                    //设置底部偏移
                    column.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM).Set(btmOffset);
                    //设置顶部标高
                    column.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).Set(topLevel.Id);
                    //设置顶部偏移
                    column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).Set(topOffset);
                }
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private bool CheckPoint(List<XYZ> points, XYZ point)
        {
            bool flag = false;
            foreach (XYZ p in points)
            { 
                if(p.IsAlmostEqualTo(point))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
