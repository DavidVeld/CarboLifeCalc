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
        public static void ImportElements(UIApplication app, CarboGroupSettings settings, string updatePath, string selectedTemplateFile)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);
            bool updateFile = false;

            CarboProject projectToUpdate = new CarboProject();


            if (File.Exists(updatePath))
            {
                //if a file needs to be updated get the settings first.
                updateFile = true;

                CarboProject buffer = new CarboProject();
                projectToUpdate = buffer.DeSerializeXML(updatePath);
                settings = projectToUpdate.RevitImportSettings;
            }
            //Create a new project
            CarboProject myProject = CollectVisibleorSelectedElements(app, settings, selectedTemplateFile);

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
                    projectToUpdate.Audit();
                    projectToUpdate.UpdateProject(myProject);
                    projectToUpdate.CalculateProject();
                    projectToOpen = projectToUpdate;
                }

                //Open the interface
                CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(projectToOpen);
                carboCalcProgram.IsRevit = true;

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                carboCalcProgram.ShowDialog();
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

        /// <summary>
        /// This function creates a CarboElement Out of each Revit Element
        /// </summary>
        /// <param name="app"></param>
        /// <param name="settings"></param>
        /// <param name="selectedTemplateFile"></param>
        /// <returns></returns>
        public static CarboProject CollectVisibleorSelectedElements(UIApplication app, CarboGroupSettings settings, string selectedTemplateFile)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

            double area = 0;

            //Create a new project based on the selected template
            CarboProject myProject = new CarboProject(selectedTemplateFile);

            myProject.Name = doc.ProjectInformation.Name.Trim();
            myProject.Number = doc.ProjectInformation.Number;

            ICollection<ElementId> selectionList = uidoc.Selection.GetElementIds();
            IList<Element> elementCollection = new List<Element>();


            if (selectionList.Count == 0) //Collect all elements in View.
            {
                //No elements are selected, all elements will be parsed
                View activeView = doc.ActiveView;

                FilteredElementCollector coll = new FilteredElementCollector(app.ActiveUIDocument.Document, activeView.Id);

                coll.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(false),
                    new ElementIsElementTypeFilter(true)));

                //Now cast them as elements into a container
                IList<Element> bufferCollection = coll.ToElements();

                foreach (Element el in bufferCollection)
                {
                    if (el != null && CarboRevitUtils.isElementReal(el) == true)
                    {
                        elementCollection.Add(el);
                    }
                }
             }
            else //If a selection was made:
            {
                try
                {
                    foreach (ElementId elid in selectionList)
                    {
                        Element el = doc.GetElement(elid);

                        if (el!= null && CarboRevitUtils.isElementReal(el) == true)
                        {
                            elementCollection.Add(el);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
            }

            //USe the above collection to create a new project or update an existing
            foreach (Element el in elementCollection)
            {
                try
                {
                    //This is an updated version where the getGeometry Method is used. 
                    //This is slower but needed for railings and ramps
                    BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.IntegerValue;
                    List<CarboElement> carboElementList = new List<CarboElement>();

                    //if (el.Category.Name == "Railings"
                    //|| el.Category.Name == "Ramps") //English

                    if (enumCategory == BuiltInCategory.OST_Railings ||
                        enumCategory == BuiltInCategory.OST_Ramps)

                    {
                        carboElementList = CarboRevitUtils.getGeometryElement(doc, el, settings);
                    }
                    else
                    {
                        //this is the fast method of extraction
                        ICollection<ElementId> MaterialIds = el.GetMaterialIds(false);
                        foreach (ElementId materialId in MaterialIds)
                        {
                            CarboElement newCarboElement = CarboRevitUtils.getNewCarboElement(doc, el, materialId, settings);
                            
                            if(carboElementList != null && newCarboElement != null)
                                carboElementList.Add(newCarboElement);
                        }
                    }

                    if (carboElementList != null || carboElementList.Count > 0)
                    {
                        foreach (CarboElement carboElement in carboElementList)
                        {
                            myProject.AddElement(carboElement);
                        }
                    }

                    //See if is floor(then count area)
                    area += getFloorarea(el);
                }
                catch (Exception ex)
                {
                }

            }

            /*
            if (IdsNotFound.Count > 0)
            {
                string message = "One or more elements weren't processed, most likely because they didn't contain any volume the element ids of these elements are: ";

                foreach (string id in IdsNotFound)
                {
                    message += "\n" + id;
                }

                MessageBox.Show(message, "Warning", MessageBoxButton.OK);
            }
            */

            // If Project contained area, make sure it is updated.
            if(myProject.Area == 0)
                myProject.Area = Math.Round((area * (0.3048 * 0.3048)), 2); //to sqr m2

            //Apply settings to new projectfile
            myProject.RevitImportSettings = settings;
            return myProject;
        }

        /// <summary>
        /// Retreives the area of a floor slab
        /// </summary>
        /// <param name="el">element</param>
        /// <returns>Area in sqr ft</returns>
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
                        result = floorArea;
                    }
                }
            }

            return result;
        }
    }
}
