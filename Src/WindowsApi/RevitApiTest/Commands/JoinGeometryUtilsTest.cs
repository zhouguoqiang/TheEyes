using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitApiTest.Commands
{
    public class JoinGeometryUtilsTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Element elem1 = doc.PickElement<Element>();
            Element elem2 = doc.PickElement<Element>();
            Transaction trans = new Transaction(doc, "trans");
            trans.Start();
            if (JoinGeometryUtils.AreElementsJoined(doc, elem1, elem2))
            {
                JoinGeometryUtils.UnjoinGeometry(doc, elem1, elem2);
                //JoinGeometryUtils.SwitchJoinOrder(doc, elem1, elem2);

            }
            else
            {
                JoinGeometryUtils.JoinGeometry(doc, elem1, elem2);
                if(!JoinGeometryUtils.IsCuttingElementInJoin(doc,elem1,elem2))
                {
                    JoinGeometryUtils.SwitchJoinOrder(doc, elem1, elem2);
                }
            }
            trans.Commit();
            return Result.Succeeded;
        }
    }

    public class SolidSolidCutUtilsTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Element elem1 = doc.PickElement<Element>();
            Element elem2 = doc.PickElement<Element>();

            Transaction trans = new Transaction(doc, "trans");
            trans.Start();
            SolidSolidCutUtils.AddCutBetweenSolids(doc, elem1, elem2);
            trans.Commit();
            return Result.Succeeded;
        }
    }
}
