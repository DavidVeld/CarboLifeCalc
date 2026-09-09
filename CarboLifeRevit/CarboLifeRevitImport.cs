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
using CarboLifeRevitCompat;
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

            //How the GIA of the file being updated was arrived at. The collection below records
            //its own answer over the top of it, which is only the right one for a new project:
            //an update keeps the GIA that is already in the file, so it keeps its record too.
            string storedGIAMethod = "";

            CarboProject projectToUpdate = new CarboProject();

            if (File.Exists(updatePath))
            {
                if (updatePath.EndsWith(".clcx"))
                {
                    //if a file needs to be updated get the settings first.
                    updateFile = true;
                    try
                    {
                        CarboProject buffer = new CarboProject();
                        projectToUpdate = buffer.DeSerializeXML(updatePath);

                        //DeSerializeXML reports the problem itself and hands back null rather than
                        //throwing, so a damaged file never reached the catch below. updateFile
                        //stayed true and the update path then dereferenced the null, which came
                        //out as a second, misleading "an error occurred" dialog with the real
                        //cause already dismissed. Stop here instead: no import is better than an
                        //import that quietly discards the file the user asked to update.
                        if (projectToUpdate == null)
                        {
                            MessageBox.Show(
                                "The selected Carbo Life Project file could not be read, so nothing has been imported." +
                                Environment.NewLine + Environment.NewLine + updatePath + Environment.NewLine + Environment.NewLine +
                                "Open the file on its own to check it, or start a new project instead of updating this one.",
                                "File could not be read", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        settings = projectToUpdate.RevitImportSettings;

                        if (settings != null)
                            storedGIAMethod = settings.GIADeterminationMethod;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("The selected Carbo Life Project file is invalid \n\nError details: " + ex.Message, "Error", MessageBoxButton.OK);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("The selected file is not a Carbo Life Project file, the addin will now progress with a new project.", "Warning", MessageBoxButton.OK);
                }
            }

            //Create a new project
            CarboProject myProject = CollectVisibleorSelectedElements(app, settings, selectedTemplateFile);

            //All element have been mapped, here the code will be split between an update or a new one.
            if (myProject.getAllElements.Count > 0)
            {
                try
                {
                    CarboProject projectToOpen = new CarboProject();

                    if (updateFile == false)
                    {
                        //Create groups from all the individual elements
                        myProject.CreateGroups();

                        if (myProject.RevitImportSettings.mapReinforcement == true)
                            myProject = mapReinforcement(app, myProject);

                        //Builds the steel and timber connection allowances that are switched on.
                        myProject.CreateConnectionGroups();

                        //MapElements if required
                        if (myProject.RevitImportSettings.UseImportedMap == true)
                        {
                            CarboMapFile defaultMappingFile = CarboMapFile.LoadFromXml();
                            if (defaultMappingFile != null)
                            {
                                myProject.carboMaterialMap = defaultMappingFile.mappingTable;
                                myProject.mapAllMaterials();
                                myProject.CalculateProject();
                            }
                        }

                        //Say once, here, which groups were given a material the matcher was not
                        //sure about. Their carbon is already in the totals, and until now the
                        //only trace was a "[CHECK MATERIAL]" prefix in the description text that
                        //nothing read and nobody was told to look for.
                        string materialReview = myProject.getMaterialReviewSummary();
                        if (string.IsNullOrEmpty(materialReview) == false)
                        {
                            MessageBox.Show(materialReview, "Check these materials",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        projectToOpen = myProject;
                    }
                    else //upadte an existing file:
                    {
                        projectToUpdate.Audit();
                        projectToUpdate.UpdateProject(myProject);

                        //UpdateProject leaves the stored GIA alone, so the parameters are applied
                        //here too. A GIA typed into the model still wins over the one saved in the
                        //file, which is the whole point of keeping it in the model.
                        if (settings != null)
                        {
                            settings.GIADeterminationMethod =
                                ApplyGIAParameters(doc, projectToUpdate, settings)
                                    ? CarboGroupSettings.GIAFromUserInput
                                    : storedGIAMethod;
                        }

                        projectToUpdate.CalculateProject();
                        projectToOpen = projectToUpdate;
                    }

                    //Open the interface
                    CarboLifeMainWindow carboCalcProgram = new CarboLifeMainWindow(projectToOpen, true);
                    carboCalcProgram.IsRevit = true;

                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                    carboCalcProgram.ShowDialog();

                    //The GIA may have been corrected in the project settings while the window was
                    //open. Put it back into the model so the next run starts from it rather than
                    //measuring the floors again. Quiet on purpose, there is nothing to decide.
                    WriteGIAParameters(doc, projectToOpen);
                    SaveGIAMethodToSettings(projectToOpen.RevitImportSettings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while trying to import the elements: " + ex.Message, "Error", MessageBoxButton.OK);
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
            myProject.UncertFact = settings.UncertaintyFactor;

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
                    BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.LongValue();
                    List<CarboElement> carboElementList = new List<CarboElement>();

                    //Railings, ramps and curtain walls are not processed with the fast method, they require a geometry extraction.

                    if (enumCategory == BuiltInCategory.OST_Railings ||
                        enumCategory == BuiltInCategory.OST_StairsRailing ||
                        enumCategory == BuiltInCategory.OST_Ramps ||
                        enumCategory == BuiltInCategory.OST_CurtainWallMullions ||
                        enumCategory == BuiltInCategory.OST_CurtainWallPanels ||
                        enumCategory == BuiltInCategory.OST_StructConnectionPlates)
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

            //A GIA the user has typed into the project information beats the floors measured
            //above, which only ever sees what the active view happens to show. Recorded either
            //way so the number can be traced back to where it came from.
            bool giaFromParameters = ApplyGIAParameters(doc, myProject, settings);

            if (settings != null)
            {
                settings.GIADeterminationMethod = giaFromParameters
                    ? CarboGroupSettings.GIAFromUserInput
                    : CarboGroupSettings.GIAFromRevitEstimate;
            }

            //The GIA is settled now, so the A0 allowance can be seeded off it rather than off the
            //area of 1 the constructor had to use. Safe on the update path too: UpdateProject
            //copies elements and groups onto the file being updated and never touches its A0 or
            //its area, so the value set here is discarded there along with the rest of this
            //throwaway project - which is what should happen, A0 being the user's own figure once
            //a project exists.
            myProject.SeedA0FromArea();

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

            BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.LongValue();

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

        #region GIA project information parameters

        /// <summary>
        /// Revit keeps areas in square feet whatever the project units say, the calculator works
        /// in square metres.
        /// </summary>
        private const double squareMetresPerSquareFoot = 0.3048 * 0.3048;

        /// <summary>
        /// Within this two GIAs in square metres are the same number. A hundredth of a square
        /// metre is well under anything worth dividing a carbon figure by, and it keeps the write
        /// back from firing on rounding noise.
        /// </summary>
        private const double areaTolerance = 0.01;

        /// <summary>
        /// A CarboProject starts life with an area of 1, which the import replaces once it has a
        /// real one. At or below that there is no GIA: nothing is read from it and, more to the
        /// point, nothing is written back to the model over the top of a real number.
        /// </summary>
        private const double unsetArea = 1;

        /// <summary>
        /// Copies the GIA out of the Project Information parameters named in the settings, where
        /// those parameters exist and hold a value. Anything not filled in is left as it was, so
        /// the floor measurement or the value already in the file stands.
        /// </summary>
        /// <returns>True when at least one GIA came from a parameter.</returns>
        private static bool ApplyGIAParameters(Document doc, CarboProject project, CarboGroupSettings settings)
        {
            if (doc == null || project == null || settings == null)
                return false;

            bool applied = false;

            try
            {
                Element projectInfo = doc.ProjectInformation;

                if (projectInfo == null)
                    return false;

                double total = ReadAreaParameter(projectInfo, settings.GIAParameterName);
                double areaNew = ReadAreaParameter(projectInfo, settings.GIANewParameterName);

                if (total > unsetArea)
                {
                    project.Area = total;
                    applied = true;
                }

                if (areaNew > unsetArea)
                {
                    project.AreaNew = areaNew;
                    applied = true;
                }
            }
            catch (Exception)
            {
                //A model without the parameters is the normal case, not a problem to report.
                return applied;
            }

            return applied;
        }

        /// <summary>
        /// Writes the project's GIA back into the Project Information parameters named in its
        /// import settings, so the next run reads it instead of measuring the floors again.
        /// Silent throughout: a model without the parameters, or one that cannot be written to,
        /// is left alone.
        /// </summary>
        private static void WriteGIAParameters(Document doc, CarboProject project)
        {
            if (doc == null || project == null || doc.IsReadOnly == true)
                return;

            CarboGroupSettings settings = project.RevitImportSettings;

            if (settings == null)
                return;

            try
            {
                Element projectInfo = doc.ProjectInformation;

                if (projectInfo == null)
                    return;

                Parameter total = GetWritableAreaParameter(projectInfo, settings.GIAParameterName);
                Parameter areaNew = GetWritableAreaParameter(projectInfo, settings.GIANewParameterName);

                bool writeTotal = NeedsUpdate(total, project.Area);
                bool writeNew = NeedsUpdate(areaNew, project.AreaNew);

                if (writeTotal == false && writeNew == false)
                    return;

                using (Transaction t = new Transaction(doc, "Update CarboLife GIA"))
                {
                    t.Start();

                    if (writeTotal == true)
                        total.Set(project.Area / squareMetresPerSquareFoot);

                    if (writeNew == true)
                        areaNew.Set(project.AreaNew / squareMetresPerSquareFoot);

                    t.Commit();
                }
            }
            catch (Exception)
            {
                //Nothing here is worth interrupting the user for, the GIA is in the project file
                //either way.
            }
        }

        /// <summary>
        /// Keeps the record of how this import found its GIA in the application settings, where
        /// the import settings dialog reads it back. Only that one field: everything else in the
        /// file was written by the dialog itself when it closed, before any of this ran.
        /// </summary>
        private static void SaveGIAMethodToSettings(CarboGroupSettings settings)
        {
            if (settings == null || string.IsNullOrEmpty(settings.GIADeterminationMethod))
                return;

            try
            {
                CarboSettings stored = new CarboSettings().Load();

                if (stored == null || stored.defaultCarboGroupSettings == null)
                    return;

                if (stored.defaultCarboGroupSettings.GIADeterminationMethod == settings.GIADeterminationMethod)
                    return;

                stored.defaultCarboGroupSettings.GIADeterminationMethod = settings.GIADeterminationMethod;
                stored.Save();
            }
            catch (Exception)
            {
                //Nothing depends on this beyond a label in the settings dialog.
            }
        }

        /// <summary>
        /// Reads an area parameter off the project information in square metres.
        /// Returns 0 when the parameter is missing, empty, or not an area at all.
        /// </summary>
        private static double ReadAreaParameter(Element projectInfo, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return 0;

            Parameter parameter = projectInfo.LookupParameter(parameterName.Trim());

            if (parameter == null || parameter.HasValue == false || parameter.StorageType != StorageType.Double)
                return 0;

            return parameter.AsDouble() * squareMetresPerSquareFoot;
        }

        /// <summary>
        /// The named area parameter when it exists and can be written to, otherwise null.
        /// </summary>
        private static Parameter GetWritableAreaParameter(Element projectInfo, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return null;

            Parameter parameter = projectInfo.LookupParameter(parameterName.Trim());

            if (parameter == null || parameter.IsReadOnly == true || parameter.StorageType != StorageType.Double)
                return null;

            return parameter;
        }

        /// <summary>
        /// True when the parameter is there to be written and does not already hold this area.
        /// </summary>
        private static bool NeedsUpdate(Parameter parameter, double squareMetres)
        {
            if (parameter == null || squareMetres <= unsetArea + areaTolerance)
                return false;

            double current = parameter.HasValue ? parameter.AsDouble() * squareMetresPerSquareFoot : 0;

            return Math.Abs(current - squareMetres) > areaTolerance;
        }

        #endregion
    }
}
