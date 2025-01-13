using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboCircle.data;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using static Autodesk.Revit.DB.SpecTypeId;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Configuration.Assemblies;

namespace CarboCircle
{
    internal class carboCircleRevitCommands
    {
        private static List<carboCircleElement> CombineVolumesData(List<carboCircleElement> minedVolumes, carboCircleSettings appSettings)
        {
            List<carboCircleElement> CombinedList = new List<carboCircleElement>();

            foreach (carboCircleElement ccE in minedVolumes)
            {
                bool found = false;
                if (CombinedList.Count > 0)
                {
                    foreach (carboCircleElement ccEl in CombinedList)
                    {
                        if(ccEl.materialName == ccE.materialName && ccEl.materialClass == ccE.materialClass)
                        {
                            ccEl.volume += ccE.volume;
                            ccEl.netVolume += ccE.netVolume;
                        }
                    }
                }

                //Add a new Value
                if (found == false)
                {
                    carboCircleElement newCombinedValue = new carboCircleElement();
                    newCombinedValue = ccE.Copy();
                }
            }

            return CombinedList;
        }

        
        internal static List<carboCircleElement> getElementsFromActiveView(UIApplication uiapp, carboCircleSettings appSettings)
        {
            List<carboCircleElement> resultCollection = new List<carboCircleElement>();

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;


            IEnumerable<Element> wallCollector = null;
            List<Element> filteredWallCollector = null;

            IEnumerable<Element> floorCollector = null;
            List<Element> filteredFloorCollector = null;

            IEnumerable<Element> beamCollector = null;
            List<Element> filteredBeamCollector = null;

            IEnumerable<Element> columnCollector = null;
            List<Element> filteredColumnCollector = null;

            beamCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType().ToElements();
            columnCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType().ToElements();

            if (appSettings.ConsiderWalls == true)
            {
                wallCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
            }

            if (appSettings.ConsiderSlabs == true)
            {
                floorCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements();
            }

            //Filter down based on settings:

            if (appSettings.extractionMethod == "Selected")
            {
                List<ElementId> selectedElements = uidoc.Selection.GetElementIds().ToList();
                if (beamCollector != null)
                    filteredBeamCollector = getOnlySelected(beamCollector, selectedElements);

                if (columnCollector != null)
                    filteredColumnCollector = getOnlySelected(columnCollector, selectedElements);

                if (wallCollector != null)
                    filteredWallCollector = getOnlySelected(wallCollector, selectedElements);

                if (floorCollector != null)
                    filteredFloorCollector = getOnlySelected(floorCollector, selectedElements);

            }
            else if(appSettings.extractionMethod == "All New in View")
            {
                //Get current Phase:
                View activeView = uidoc.ActiveGraphicalView;
                Parameter phaseParam = activeView.LookupParameter("Phase");
                if (phaseParam != null)
                {
                    string phasename = phaseParam.AsValueString();
                    if(beamCollector != null)
                        filteredBeamCollector = getOnPhase(beamCollector, phasename);

                    if(columnCollector != null)
                        filteredColumnCollector = getOnPhase(columnCollector, phasename);

                    if (wallCollector != null)
                        filteredWallCollector = getOnPhase(wallCollector, phasename);

                    if (floorCollector != null)
                        filteredFloorCollector = getOnPhase(floorCollector, phasename);
                }
            }
            else if (appSettings.extractionMethod == "All Demolished in View")
            {
                //Get current Phase:
                View activeView = uidoc.ActiveGraphicalView;
                Parameter phaseParam = activeView.LookupParameter("Phase");
                if (phaseParam != null)
                {
                    string phasename = phaseParam.AsValueString();
                    if (beamCollector != null)
                        filteredBeamCollector = getOnDemolishedPhase(beamCollector, phasename);

                    if (columnCollector != null)
                        filteredColumnCollector = getOnDemolishedPhase(columnCollector, phasename);

                    if (wallCollector != null)
                        filteredWallCollector = getOnDemolishedPhase(wallCollector, phasename);

                    if (floorCollector != null)
                        filteredFloorCollector = getOnDemolishedPhase(floorCollector, phasename);
                }
            }
            else
            {
                //All visible in view (Default)
                filteredBeamCollector = beamCollector.ToList();
                filteredColumnCollector = columnCollector.ToList();

                filteredWallCollector = wallCollector.ToList();
                filteredFloorCollector = floorCollector.ToList();

            }

            //Convert to proper Elements

            List<carboCircleElement> beamCollection = getcarboCircleElements(filteredBeamCollector, doc, appSettings);
            List<carboCircleElement> columnCollection = getcarboCircleElements(filteredColumnCollector, doc, appSettings);

            List<carboCircleElement> wallCollection = getcarboCircleElements(filteredWallCollector, doc, appSettings);
            List<carboCircleElement> floorCollection = getcarboCircleElements(filteredFloorCollector, doc, appSettings);


            if (beamCollection.Count() > 0)
            {
                foreach (carboCircleElement ccEl in beamCollection)
                    resultCollection.Add(ccEl.Copy());
            }

            if (columnCollection.Count() > 0)
            {
                foreach (carboCircleElement ccEl in columnCollection)
                    resultCollection.Add(ccEl.Copy());
            }

            //Map the elements to a database element
            List<carboCircleElement> mappedResultCollection = MapElementsTodataBase(resultCollection, appSettings);

            return mappedResultCollection;

        }

        private static List<Element> getOnPhase(IEnumerable<Element> collection, string phasename)
        {
            //get a list of selected elemetnts
            List<Element> result = new List<Element>();

            if (collection == null || phasename == "")
                return result;

            //add only elements that are selected in the pool:
            foreach (Element el in collection)
            {
                try
                {
                    Parameter phaseCreatedParam = el.LookupParameter("Phase Created");

                    if (phaseCreatedParam != null)
                    {
                        string phaseCreatedName = phaseCreatedParam.AsValueString();

                        if (phaseCreatedName == phasename)
                        {
                            result.Add(el);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }

            return result;
        }

        private static List<Element> getOnDemolishedPhase(IEnumerable<Element> collection, string phasename)
        {
            //get a list of selected elemetnts
            List<Element> result = new List<Element>();

            if (collection == null || phasename == "")
                return result;

            //add only elements that are selected in the pool:
            foreach (Element el in collection)
            {
                try
                {
                    Parameter phaseDemolishedParam = el.LookupParameter("Phase Demolished");

                    if (phaseDemolishedParam != null)
                    {
                        string phaseDemolishedName = phaseDemolishedParam.AsValueString();
                        if (phaseDemolishedName != null)
                        {
                            if (phaseDemolishedName == phasename)
                            {
                                result.Add(el);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }

            return result;
        }

        private static List<Element> getOnlySelected(IEnumerable<Element> collection, List<ElementId> selectedElementIds)
        {
            //get a list of selected elemetnts
            List<Element> result = new List<Element>();
            List<int> ids = new List<int>();

            if(collection == null || selectedElementIds == null)    
                return result;

            foreach (ElementId id in selectedElementIds)
            {
                ids.Add(id.IntegerValue);
            }

            //add only elements that are selected in the pool:
            foreach (Element el in collection)
            {
                try
                {
                    int id = el.Id.IntegerValue;

                    if (ids.Contains(id))
                    {
                        result.Add(el);
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
        
            return result;
        }

        private static List<carboCircleElement> MapElementsTodataBase(List<carboCircleElement> beamColumnCollection, carboCircleSettings appSettings)
        {
            List<carboCircleElement> SteelSectionDataBase = new List<carboCircleElement>();
            SteelSectionDataBase = getSteelDataBase(appSettings);

            beamColumnCollection = GetClosestBeamMatch(beamColumnCollection, SteelSectionDataBase, appSettings);

            return beamColumnCollection;
        }

        private static List<carboCircleElement> GetClosestBeamMatch(List<carboCircleElement> steelBeams, List<carboCircleElement> steelSectionDataBase, carboCircleSettings appSettings)
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            if (steelBeams.Count > 0 && steelSectionDataBase.Count > 0 && appSettings != null)
            {

                foreach (carboCircleElement ccE in steelBeams)
                {
                    int indexFound = 0;
                    int lowestLevDist = 9999;
                    int i = 0;
                    carboCircleElement matchingBeam = ccE.Copy();

                    if (matchingBeam.materialClass != "Steel")
                    {
                        result.Add(matchingBeam);
                        continue;
                    }

                    try
                    {
                        //This needs alot of work:

                        foreach (carboCircleElement dataBaseBeam in steelSectionDataBase)
                        {
                            int levDist = Utils.CalcLevenshteinDistance(matchingBeam.name, dataBaseBeam.standardName);
                            if(levDist < lowestLevDist)
                            {
                                lowestLevDist = levDist;
                                indexFound = i;
                            }
                            i++;
                        }

                        carboCircleElement closestMatchingBeam = steelSectionDataBase[indexFound];
                        matchingBeam.standardName = closestMatchingBeam.name;
                        matchingBeam.standardDepth = closestMatchingBeam.standardDepth;
                        matchingBeam.standardCategory = closestMatchingBeam.standardCategory;
                        matchingBeam.Iy = closestMatchingBeam.Iy;
                        matchingBeam.Wy = closestMatchingBeam.Wy;

                        result.Add(matchingBeam);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                //get closest index nr;


            }

            return result;
        }

        private static List<carboCircleElement> getSteelDataBase(carboCircleSettings appSettings)
        {
            List<carboCircleElement> result = new List<carboCircleElement>();
            string dbpath = "";

            if (File.Exists(appSettings.dataBasePath))
            {
                dbpath = appSettings.dataBasePath;
            }
            else
            {
                string assemblyath = Utils.getAssemblyPath();
                dbpath = assemblyath + "\\db\\" + "CarboCircleMasterSections.csv";
            }

            if (File.Exists(dbpath))
                if (IsFileLocked(dbpath) == false)
                {
                    DataTable data = CarboLifeAPI.Utils.LoadCSV(dbpath);

                    if (data == null)
                        return null;

                    foreach (DataRow dr in data.Rows)
                    {
                        try
                        {
                            carboCircleElement ccE = new carboCircleElement();


                            ccE.id = 0;
                            ccE.name = dr[1].ToString();
                            ccE.category = dr[3].ToString(); //check

                            ccE.standardName = dr[1].ToString();
                            ccE.standardDepth = Utils.ConvertMeToDouble(dr[8].ToString()); //check
                            ccE.standardCategory = dr[2].ToString();
                            ccE.Wy = Utils.ConvertMeToDouble(dr[25].ToString());
                            ccE.Iy = Utils.ConvertMeToDouble(dr[21].ToString());

                            ccE.materialName = dr[5].ToString();

                            result.Add(ccE);
                        }
                        catch (Exception ex)
                        { 
                        }
                    }
                }


            return result;
        }

        private static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //The file is open
                return true;
            }

            //All is ok
            return false;
        }

        private static List<carboCircleElement> getcarboCircleElements(IEnumerable<Element> Collection, Document doc, carboCircleSettings appSettings)
        {
            List<carboCircleElement> resultCollection = new List<carboCircleElement>();

            if (Collection == null || doc == null || appSettings == null)
            {
                return resultCollection;
            }

            if (Collection.Count() > 0)
            {
                foreach (Element el in Collection)
                {
                    try
                    {
                        if (isElementReal(el) == false || el.Id.IntegerValue < 0)
                        {
                            continue;
                        }




                        //a valid element is further checked:
                        FamilyInstance inst = el as FamilyInstance;
                        if (inst != null)
                        {

                            List<ElementId> materials = inst.GetMaterialIds(false).ToList();

                            foreach(ElementId materialid  in materials)
                            {
                                carboCircleElement newElement = getElementFromMaterialId(materialid, doc, el,appSettings);
                                if(newElement != null)
                                    resultCollection.Add(newElement);
                            }


 
                        }
                    }
                    catch
                    { }
                }
            }
            return resultCollection;
        }

        private static carboCircleElement getElementFromMaterialId(ElementId materialid, Document doc, Element el, carboCircleSettings appSettings)
        {
            carboCircleElement resultElement = new carboCircleElement();

            try
            {
                FamilyInstance inst = el as FamilyInstance;
                if (inst == null)
                {
                    return null;
                }

                int Revitid = inst.Id.IntegerValue;
                string materialClass = "";
                string materialName = "";
                string materialGrade = "";
                string elementName = inst.Name.ToString();
                string elementCategoty = inst.Category.Name.ToString();


                bool isVolumne = true;
                //First Look in the material:

                materialClass = inst.StructuralMaterialType.ToString();

                Autodesk.Revit.DB.Material material = doc.GetElement(materialid) as Autodesk.Revit.DB.Material;
                if (material != null) 
                    {
                        materialName = material.Name;
                        materialGrade = material.MaterialClass.ToString();
                    }
                

                //Only if material fails check the family class:
                if (materialClass == "")
                {
                    FamilySymbol familySymbol = inst.Symbol;
                    Family family = familySymbol.Family;
                    Parameter familyBehaviour = family.LookupParameter("Material for Model Behavior");
                    if (familyBehaviour != null)
                    {
                        materialClass = familyBehaviour.AsValueString();
                    }
                }

                if (materialClass == "Steel" || materialClass == "Wood")
                {
                    //In this case the element is a steel beam or column
                    isVolumne = false;
                }

                double volume = inst.GetMaterialVolume(material.Id);
                if (volume != 0)
                    volume = convertToCubicMtrs(volume); //to m3

                double length = 0;
                Parameter lengthParam = inst.LookupParameter("Cut Length");
                if (lengthParam != null)
                {
                    length = (lengthParam.AsDouble() * 304.8) / 1000; //to m1
                }
                else
                {
                    BuiltInParameter paraIndex = BuiltInParameter.INSTANCE_LENGTH_PARAM;
                    Parameter coLength = inst.get_Parameter(paraIndex);
                    if(coLength != null)
                    {
                        length = (coLength.AsDouble() * 304.8) / 1000; //to m1
                    }
                }

                //set properties:

                resultElement.id = Revitid;
                resultElement.GUID = inst.UniqueId;
                resultElement.humanId = Revitid.ToString("X");
                resultElement.name = elementName;
                resultElement.category = elementCategoty;


                //material Props
                resultElement.materialName = materialName;
                resultElement.materialClass = materialClass;
                resultElement.grade = materialGrade;

                resultElement.length = length;
                resultElement.volume = volume;


                resultElement.isVolumeElement = isVolumne;

            }
            catch (Exception ex)
            {
                return null;
            }
            return resultElement;

        }

        /// <summary>
        /// Validates the elements and it's class;
        /// </summary>
        /// <param name="el"></param>
        /// <returns>True if the element can be extracted</returns>
        internal static bool isElementReal(Element el)
        {
            bool result = false;
            if (!(el is FamilySymbol || el is Family))
            {
                if (!(el.Category == null))
                {
                    if (el.get_Geometry(new Options()) != null)
                    {
                        if (el.Id.IntegerValue > 0)
                        {
                            //Check if not of any forbidden categories such as runs:
                            bool isValidCategory = ValidCategory(el);
                            if (isValidCategory == true)
                                result = true;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Some classes do not return a valid embodied carbon value, these need to be reviewed separately
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        internal static bool ValidCategory(Element el)
        {
            bool result = true;

            BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.IntegerValue;

            if (enumCategory == BuiltInCategory.OST_StairsRuns)
            {
                result = false;
            }

            return result;
        }

        public static double convertToCubicMtrs(double volumeCubicFt)
        {
            double result = 0;
            double factor = Math.Pow((0.3048), 3);
            result = volumeCubicFt * factor;
            return result;

        }
    }
}