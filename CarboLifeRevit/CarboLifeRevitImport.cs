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
                //Create a visual
                if(carboCalcProgram.createHeatmap == true)
                {
                    CreateHeatMap(doc, carboCalcProgram.carboLifeProject);
                }
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

        private static void CreateHeatMap(Document doc, CarboProject carboLifeProject)
        {
            using (Transaction t = new Transaction(doc, "Set Element Override"))
            {
                t.Start();

                FilteredElementCollector elements = new FilteredElementCollector(doc);
                FillPatternElement solidFillPattern = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

                foreach (CarboElement ce in carboLifeProject.getAllElements)
                {
                    ElementId id = new ElementId(ce.Id);
                    ElementId patid = solidFillPattern.Id; //Solid

                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetHalftone(false);

                    ogs.SetProjectionLineColor(new Color(ce.r, ce.g, ce.b));
                    ogs.SetSurfaceTransparency(0);

                    ogs.SetProjectionLineWeight(1);

                    //Solid Fill;
                    Color elementColour = new Color(ce.r, ce.g, ce.b);

                    ogs.SetProjectionLineWeight(1);
                    ogs.SetSurfaceForegroundPatternId(patid);
                    ogs.SetSurfaceForegroundPatternColor(elementColour);
                    ogs.SetSurfaceForegroundPatternVisible(true);
                    
                    ogs.SetCutLineWeight(1);
                    ogs.SetCutForegroundPatternId(patid);
                    ogs.SetCutForegroundPatternVisible(true);
                    ogs.SetCutForegroundPatternColor(elementColour);

                    //Set the override
                    try
                    {
                        Element elementcheck = doc.GetElement(id);
                         if(elementcheck != null)
                        {
                            doc.ActiveView.SetElementOverrides(id, ogs);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }
                t.Commit();
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
