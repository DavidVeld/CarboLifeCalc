using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;

namespace CarboLifeRevit
{
    public class CarboLifeRevitImport
    {
        public static void ImportElements(UIApplication app, CarboRevitImportSettings settings)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

            CarboProject myProject = new CarboProject();

            myProject.Name = doc.ProjectInformation.Name.Trim();
            myProject.Number = doc.ProjectInformation.Number;

            ICollection<ElementId> selectionList = uidoc.Selection.GetElementIds();

            #region buildQuantitiestable

            if (selectionList.Count == 0)
            {
                //No elements are selected: 

                FilteredElementCollector coll = new FilteredElementCollector(app.ActiveUIDocument.Document);

                coll.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false),
                    new ElementIsElementTypeFilter(true)));

                //Now cast them as elements into a container
                IList<Element> collection = coll.ToElements();

                try
                {

                    foreach (Element el in collection)
                    {
                        if (CarboRevitUtils.isElementReal(el) == true)
                        {
                            ICollection<ElementId> MaterialIds = el.GetMaterialIds(false);
                            foreach (ElementId materialIds in MaterialIds)
                            {
                                CarboElement carboElement = CarboRevitUtils.getNewCarboElement(doc, el, materialIds, settings);

                                if (carboElement != null)
                                    myProject.AddElement(carboElement);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
            }
            else
            {
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
                                CarboElement carboElement = CarboRevitUtils.getNewCarboElement(doc, el, materialIds, settings);
                            }
                        }
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
                myProject.CreateGroups();
                CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(myProject);
                carboCalcProgram.ShowDialog();
            }
        }
    }
}
