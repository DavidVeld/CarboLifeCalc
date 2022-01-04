using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static void ImportElements(UIApplication app, CarboRevitImportSettings settings, string updatePath)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);
            bool updateFile = false;

            if (File.Exists(updatePath))
                updateFile = true;
            //Create a new project
            CarboProject myProject = new CarboProject();

            myProject.Name = doc.ProjectInformation.Name.Trim();
            myProject.Number = doc.ProjectInformation.Number;

            ICollection<ElementId> selectionList = uidoc.Selection.GetElementIds();

            double area = 0;

            #region buildQuantitiestable

            if (selectionList.Count == 0)
            {
                //No elements are selected, all elements will be parsed
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
                    catch
                    {
                        IdsNotFound.Add(name);

                        //TaskDialog.Show("Error", ex.Message);
                    }

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

                                if (carboElement != null)
                                    myProject.AddElement(carboElement);
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

            //All element have been mapped, here the code will be split between an update or a new one.
            if (myProject.getAllElements.Count > 0)
            {
                CarboProject projectToOpen = new CarboProject();

                if (updateFile == false)
                {
                    //Create groups from all the individual elements
                    myProject.CreateGroups();
                    projectToOpen = myProject;
                }
                else //upadte an existing file:
                {
                    
                    CarboProject projectToUpdate = new CarboProject();

                    CarboProject buffer = new CarboProject();
                    projectToUpdate = buffer.DeSerializeXML(updatePath);

                    projectToUpdate.Audit();
                    projectToUpdate.UpdateProject(myProject);
                    projectToUpdate.CalculateProject();

                    projectToOpen = projectToUpdate;
                }

                double areaMSqr = Math.Round((area * (0.3048 * 0.3048)),2);
                projectToOpen.Area = areaMSqr;

                CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(projectToOpen);
                carboCalcProgram.IsRevit = true;

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                carboCalcProgram.ShowDialog();
                //Create a visual
                if(carboCalcProgram.createHeatmap == true)
                {
                    CreateHeatMap(app, carboCalcProgram.carboLifeProject);
                }
            }
            else
            {
                MessageBox.Show("No elements could be found to be calculated, please make sure you have a 3D view active and the building volume is clearly visible ", "Warning", MessageBoxButton.OK);
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

        private static void CreateHeatMap(UIApplication app, CarboProject carboLifeProject)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction t = new Transaction(doc, "Create Heatmap"))
            {
                t.Start();



                FilteredElementCollector elements = new FilteredElementCollector(doc);
                FillPatternElement solidFillPattern = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

                List<CarboElement> elementsFromGroups = carboLifeProject.getElementsFromGroups().ToList();

                foreach (CarboElement ce in elementsFromGroups)
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

                        Parameter carboPar = elementcheck.LookupParameter("EmbodiedCarbon");
                            if (carboPar != null)
                                carboPar.Set(ce.EC_Total);
                            else
                                CreateParameter(app, elementcheck);

                        }

                    }
                    catch
                    {

                    }
                }
                t.Commit();
            }
        }

        private static void CreateParameter(UIApplication app, Element element)
        {
            // Create Room Shared Parameter Routine: -->
            // 1: Check whether the Room shared parameter("External Room ID") has been defined.
            // 2: Share parameter file locates under sample directory of this .dll module.
            // 3: Add a group named "SDKSampleRoomScheduleGroup".
            // 4: Add a shared parameter named "External Room ID" to "Rooms" category, which is visible.
            //    The "External Room ID" parameter will be used to map to spreadsheet based room ID(which is unique)

            try
            {
                // create shared parameter file
                String modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String paramFile = modulePath + "\\CarbonSharedParams.txt";
                if (File.Exists(paramFile))
                {
                    File.Delete(paramFile);
                }
                FileStream fs = File.Create(paramFile);
                fs.Close();

                // cache application handle
                Autodesk.Revit.ApplicationServices.Application revitApp = app.Application;

                // prepare shared parameter file
                app.Application.SharedParametersFilename = paramFile;

                // open shared parameter file
                DefinitionFile parafile = revitApp.OpenSharedParameterFile();

                // create a group
                DefinitionGroup apiGroup = parafile.Groups.Create("CarbonSharedParamsGroup");

                // create a visible "External Room ID" of text type.
                ExternalDefinitionCreationOptions ExternalDefinitionCreationOptions = new ExternalDefinitionCreationOptions("EmbodiedCarbon", ParameterType.Number);
                Definition roomSharedParamDef = apiGroup.Definitions.Create(ExternalDefinitionCreationOptions);

                // get Rooms category

                //Category roomCat = app.ActiveUIDocument.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                Category category = element.Category;

                CategorySet categories = revitApp.Create.NewCategorySet();
                categories.Insert(category);

                // insert the new parameter
                InstanceBinding binding = revitApp.Create.NewInstanceBinding(categories);
                app.ActiveUIDocument.Document.ParameterBindings.Insert(roomSharedParamDef, binding);
                //return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create shared parameter: " + ex.Message);
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
