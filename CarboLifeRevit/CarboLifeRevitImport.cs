using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

            double area = 0;

            #region buildQuantitiestable

            if (selectionList.Count == 0)
            {
                //No elements are selected: 
                View activeView = doc.ActiveView;

                FilteredElementCollector coll = new FilteredElementCollector(app.ActiveUIDocument.Document, activeView.Id);

                coll.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false),
                    new ElementIsElementTypeFilter(true)));

                //Now cast them as elements into a container
                IList<Element> collection = coll.ToElements();
                string name = "";

                List<string> IdsNotFound = new List<string>();

                    foreach (Element el in collection)
                    {
                    name = el.Id.ToString();

                    try
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
                        //See if is floor(then count area)
                            area += getFloorarea(el);
                        }


                    }
                    catch (Exception ex)
                    {
                        IdsNotFound.Add(name);

                        //TaskDialog.Show("Error", ex.Message);
                    }

                    if(IdsNotFound.Count > 0)
                    {
                        string message = "One or more elements weren't processed, most likely because they didn't contain any volume the element ids of these elements are: ";

                        foreach (string id in IdsNotFound)
                        {
                            message += "\n" + id;
                        }

                        MessageBox.Show(message, "Warning", MessageBoxButton.OK);
                    }
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

                            //See if is floor(then count area)
                            area += getFloorarea(el);
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

                double areaMSqr = Math.Round((area * (0.3048 * 0.3048)),2);
                myProject.Area = areaMSqr;

                CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(myProject);
                carboCalcProgram.IsRevit = true;

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                carboCalcProgram.ShowDialog();
            }

            //When assembly cant be find bind to current
            System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                System.Reflection.Assembly ayResult = null;
                string sShortAssemblyName = args.Name.Split(',')[0];
                System.Reflection.Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly ayAssembly in ayAssemblies)
                {
                    if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                    {
                        ayResult = ayAssembly;
                        break;
                    }
                }
                return ayResult;
            }
        }

        private static double getFloorarea(Element el)
        {
            double result = 0;

            BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.IntegerValue;

            if (enumCategory == BuiltInCategory.OST_Floors)
            {
                Floor floorElement = el as Floor;
                if (floorElement != null)
                {

                    Parameter floorPar = floorElement.LookupParameter("Area");
                    if (floorPar != null)
                    {
                        double floorArea = floorPar.AsDouble();
                        result += floorArea;
                    }
                }
            }

            return result;
        }
    }
}
