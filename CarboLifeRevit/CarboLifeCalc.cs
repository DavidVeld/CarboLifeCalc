using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeRevit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    class CarboLifeCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

            CarboProject myProject = new CarboProject();

            ICollection<ElementId> selectionList = uidoc.Selection.GetElementIds();

            #region buildQuantitiestable

            if (selectionList.Count == 0)
            {
                FilteredElementCollector coll = new FilteredElementCollector(app.ActiveUIDocument.Document);

                coll.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false),
                    new ElementIsElementTypeFilter(true)));

                //Now cast them as elements into a container
                IList<Element> collection = coll.ToElements();

                int i = 1000;

                try
                {
                    foreach (Element el in collection)
                    {
                        if (CarboRevitUtils.isElementReal(el) == true)
                        {
                            ICollection<ElementId> MaterialIds = el.GetMaterialIds(false);
                            foreach (ElementId materialIds in MaterialIds)
                            {
                                CarboElement carboElement = CarboRevitUtils.getNewCarboElement(doc, el, materialIds, i);

                                if (carboElement != null)
                                    myProject.AddElement(carboElement);
                            }
                        }
                        i++;

                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
            }
            else
            {
                int i = 1000;
                try
                {
                    foreach (ElementId elid in selectionList)
                    {
                        Element el = doc.GetElement(elid);
                        ICollection<ElementId> MaterialIds = el.GetMaterialIds(false);

                        if (CarboRevitUtils.isElementReal(el) == true)
                        {
                            foreach (ElementId materialIds in MaterialIds)
                            {
                                myProject.AddElement(CarboRevitUtils.getNewCarboElement(doc, el, materialIds, i));
                            }
                        }
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }

            }

            #endregion

            if (myProject.getAllElements.Count > 0)
            {

                CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(myProject);
                carboCalcProgram.ShowDialog();
            }

            return Result.Succeeded;
        }
    }
}
