using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
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

                    if (myProject.RevitImportSettings.mapReinforcement == true)
                        myProject = mapReinforcement(app, myProject);

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
                            
                            if(carboElementList != null 
                                && newCarboElement != null 
                                && newCarboElement.Id != -999 
                                && newCarboElement.Volume > 0)
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
            double m2Area = Math.Round((area * (0.3048 * 0.3048)), 2);

            if (myProject.Area == 1)
            {
                myProject.Area = m2Area; //to sqr m2
            }

            if (myProject.AreaNew == 1)
            {
                myProject.AreaNew = m2Area; //to sqr m2
            }
            //Apply settings to new projectfile
            myProject.RevitImportSettings = settings;




            return myProject;
        }

        /// <summary>
        /// Returns the project with the revit elements mapped accordingly.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static CarboProject mapReinforcement(UIApplication app, CarboProject myProject)
        {
            //UIDocument uidoc = app.ActiveUIDocument;
            //Document doc = uidoc.Document;
            myProject.CreateReinforcementGroup();
            /*
            //get the 
            CarboGroupSettings settings = myProject.RevitImportSettings;
            List<CarboNumProperty> rcMap = settings.rcQuantityMap;
            //string categoryType = settings.RCParameterType;
            //string categoryName = settings.RCParameterName;
            string rcMaterialName = settings.RCMaterialName;
            string carboMaterialCategory = settings.RCMaterialCategory;
            CarboMaterial reinforcementMaterial = myProject.CarboDatabase.getClosestMatch(rcMaterialName);


            List<CarboGroup> RCgroups = new List<CarboGroup>();
             
            //Iterate through each element, check the conditions, and reinforce if needed;
            foreach (CarboElement cEl in myProject.getElementsFromGroups())
            {
                //Add to grouplist or create new:
                //get the category material:
                CarboMaterial carboMaterial = myProject.CarboDatabase.getClosestMatch(cEl.CarboMaterialName, cEl.Grade);

                if(carboMaterial.Category == carboMaterialCategory)
                    RCgroups = addRCQuants(cEl, reinforcementMaterial, RCgroups, myProject);

            }
            
            foreach(CarboGroup carboGroup in RCgroups)
            {
                
                //map the groups
                carboGroup.Material = reinforcementMaterial;
                carboGroup.MaterialName = reinforcementMaterial.Name;

                bool found = false;

                foreach (CarboNumProperty key in myProject.RevitImportSettings.rcQuantityMap)
                {
                    if(key.PropertyName == carboGroup.Category)
                    {
                        carboGroup.Correction = "*(" + key.Value.ToString() + "/" + reinforcementMaterial.Density.ToString() + ")";
                        carboGroup.Description = "Reinforcement " + key.Value.ToString() +  " kg/m³";

                        foreach (CarboElement cEl in carboGroup.AllElements)
                        {
                            cEl.Correction = carboGroup.Correction;
                            cEl.rcDensity = key.Value;
                        }



                        found = true;
                        break;
                    }
                }

                if(found == false)
                {
                    if (carboGroup.Correction == "" && carboGroup.Volume > 0)
                    {
                        //default 100kg/m³
                        carboGroup.Correction = "*" + Math.Round((100 / reinforcementMaterial.Density), 3).ToString();
                        carboGroup.Description = "Reinforcement " + "100 kg/m³" + " (default)";
                    }
                    else
                    {
                        carboGroup.Description = "User to check";
                    }
                }

            }


            myProject.AddGroups(RCgroups);
            */
            myProject.CalculateProject();

            return myProject;

        }

        private static List<CarboGroup> addRCQuants(CarboElement cEl, CarboMaterial ReinforcementMat, List<CarboGroup> rCgroups, CarboProject myProject)
        {
            //unique for the rc quants.
            string elementCateg = cEl.Category;
            double volume = cEl.Volume;
            double rcDensity = cEl.rcDensity;

            bool contains = false;

            //Convert element to CarboElement
            CarboElement newcEl = cEl.CopyMe();

            newcEl.setMaterial(ReinforcementMat);
            newcEl.Name = newcEl.Name + " Reinforcement";
            newcEl.rcDensity = rcDensity;

            foreach (CarboGroup cbg in rCgroups)
            {
                if(cbg.Category == elementCateg)
                {
                    cbg.Volume += volume;
                    cbg.AllElements.Add(newcEl);
                    contains = true;
                }
            }
           if(contains == false)
            {
                CarboGroup newGroup = new CarboGroup();
                newGroup.Category = elementCateg;
                newGroup.Volume = volume;

                newGroup.AllElements.Add(newcEl);

                rCgroups.Add(newGroup);
            }

            return rCgroups;


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
