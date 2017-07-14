using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace RevitApiTest
{
    public class Utils
    {
    }

    public static class Extensions
    {
        public static T PickElement<T>(this Document doc) where T:Element
        {
            T result = default(T);
            UIDocument uidoc = new UIDocument(doc);
            Reference re = uidoc.Selection.PickObject(ObjectType.Element, new TypeSelector<T>(), "请选择构件");
            result = doc.GetElement(re) as T;
            return result;
        }
    }

    public class TypeSelector<T> : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is T)
                return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

}
