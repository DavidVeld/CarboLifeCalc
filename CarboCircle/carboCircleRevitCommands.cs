using Autodesk.Revit.DB;
using CarboLifeRevitCompat;
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
//using System.Net.Configuration;
using System.Runtime;
using System.Windows.Media;

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
                        if (ccEl.materialName == ccE.materialName && ccEl.materialClass == ccE.materialClass)
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


        /// <summary>
        /// Collects elements from the active view, using the given extraction method.
        /// </summary>
        /// <param name="appSettings">
        /// The live settings the caller is working with. Used as handed in - deliberately
        /// not reloaded from disk, because a reload would discard whatever the user has
        /// just changed and not yet saved.
        /// </param>
        /// <param name="extractionMethod">
        /// Which elements to pick: one of the <see cref="carboCircleExtractionMethod"/>
        /// values. Travels with the call rather than sitting on the settings, because it
        /// describes this one operation - the mine and project sides ask for different
        /// methods through this same code path, so it is not a preference either of them
        /// can own.
        /// </param>
        /// <param name="log">
        /// Collects everything that went wrong or got dropped. Nothing on this path
        /// discards a reason silently any more: an import that returns nothing has to be
        /// able to say why, or it is indistinguishable from a model with nothing in it.
        /// </param>
        internal static List<carboCircleElement> getElementsFromActiveView(UIApplication uiapp, carboCircleSettings appSettings, string extractionMethod, carboCircleImportLog log)
        {
            if (log == null)
                log = new carboCircleImportLog();

            //Only reach for the file when there is nothing at all to work with.
            if (appSettings == null)
                appSettings = new carboCircleSettings().Load();

            List<carboCircleElement> resultCollection = new List<carboCircleElement>();

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            log.ExtractionMethod = extractionMethod;
            log.ViewName = doc.ActiveView != null ? doc.ActiveView.Name : "";

            IEnumerable<Element> wallCollector = null;
            List<Element> filteredWallCollector = null;

            IEnumerable<Element> floorCollector = null;
            List<Element> filteredFloorCollector = null;

            IEnumerable<Element> beamCollector = null;
            List<Element> filteredBeamCollector = null;

            IEnumerable<Element> columnCollector = null;
            List<Element> filteredColumnCollector = null;

            //All three switches are honoured now. Beams and columns used to be collected
            //whatever the setting said, so turning them off to mine only walls did nothing.
            if (appSettings.ConsiderColumnBeams == true)
            {
                beamCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType().ToElements();
                columnCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).WhereElementIsNotElementType().ToElements();
            }

            if (appSettings.ConsiderWalls == true)
            {
                wallCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
            }

            if (appSettings.ConsiderSlabs == true)
            {
                floorCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements();
            }

            if (appSettings.ConsiderColumnBeams == false && appSettings.ConsiderWalls == false
                && appSettings.ConsiderSlabs == false)
                log.Fail("Beams and columns, walls and floors are all switched off in the " +
                         "settings, so there was nothing to look for.");

            //Filter down based on settings:

            if (extractionMethod == carboCircleExtractionMethod.Selected)
            {
                List<ElementId> selectedElements = uidoc.Selection.GetElementIds().ToList();
                if (selectedElements.Count == 0)
                    log.Fail("Nothing is selected in the model, so there was nothing to import.");

                if (beamCollector != null)
                    filteredBeamCollector = getOnlySelected(beamCollector, selectedElements, log);

                if (columnCollector != null)
                    filteredColumnCollector = getOnlySelected(columnCollector, selectedElements, log);

                if (wallCollector != null)
                    filteredWallCollector = getOnlySelected(wallCollector, selectedElements, log);

                if (floorCollector != null)
                    filteredFloorCollector = getOnlySelected(floorCollector, selectedElements, log);

            }
            else if (extractionMethod == carboCircleExtractionMethod.AllNewInView)
            {
                //Get current Phase:
                string phasename = readPhaseName(uidoc.ActiveGraphicalView, log);

                if (phasename != "")
                {
                    if (beamCollector != null)
                        filteredBeamCollector = getOnPhase(beamCollector, phasename, log);

                    if (columnCollector != null)
                        filteredColumnCollector = getOnPhase(columnCollector, phasename, log);

                    if (wallCollector != null)
                        filteredWallCollector = getOnPhase(wallCollector, phasename, log);

                    if (floorCollector != null)
                        filteredFloorCollector = getOnPhase(floorCollector, phasename, log);
                }
            }
            else if (extractionMethod == carboCircleExtractionMethod.AllDemolishedInView)
            {
                //Get current Phase:
                string phasename = readPhaseName(uidoc.ActiveGraphicalView, log);

                if (phasename != "")
                {
                    if (beamCollector != null)
                        filteredBeamCollector = getOnDemolishedPhase(beamCollector, phasename, log);

                    if (columnCollector != null)
                        filteredColumnCollector = getOnDemolishedPhase(columnCollector, phasename, log);

                    if (wallCollector != null)
                        filteredWallCollector = getOnDemolishedPhase(wallCollector, phasename, log);

                    if (floorCollector != null)
                        filteredFloorCollector = getOnDemolishedPhase(floorCollector, phasename, log);
                }
            }
            else
            {
                //carboCircleExtractionMethod.AllVisibleInView, and the fallback for
                //any value this build does not recognise.
                //
                //Each collector is left null for a category the settings exclude, and
                //ToList() on a null reference ended the import right here - which, with
                //walls and floors off by default, was every time this branch ran.
                filteredBeamCollector = toList(beamCollector);
                filteredColumnCollector = toList(columnCollector);

                filteredWallCollector = toList(wallCollector);
                filteredFloorCollector = toList(floorCollector);

            }

            //Two different counts, and the difference between them matters. An empty view
            //is a problem worth reporting; a full view whose elements were all filtered out
            //is not, and the filter tally already explains that one.
            int foundInView = countInView(beamCollector) + countInView(columnCollector)
                + countInView(wallCollector) + countInView(floorCollector);

            log.ElementsInView = foundInView;
            log.ElementsExamined = countOf(filteredBeamCollector) + countOf(filteredColumnCollector)
                + countOf(filteredWallCollector) + countOf(filteredFloorCollector);

            if (foundInView == 0)
                log.Fail("This view contains no beams, columns, walls or floors at all. Check that " +
                         "those categories are visible here, and that the view phase filter shows " +
                         "the elements you are after - a filter set to Show Complete hides " +
                         "demolished geometry entirely.");

            //Convert to proper Elements

            List<carboCircleElement> beamCollection = getcarboCircleElements(filteredBeamCollector, doc, appSettings, log);
            List<carboCircleElement> columnCollection = getcarboCircleElements(filteredColumnCollector, doc, appSettings, log);

            List<carboCircleElement> wallCollection = getcarboCircleElements(filteredWallCollector, doc, appSettings, log);
            List<carboCircleElement> floorCollection = getcarboCircleElements(filteredFloorCollector, doc, appSettings, log);


            //Walls and floors now go in alongside beams and columns. They were converted
            //here and then dropped, which is why the concrete and masonry side of the tool
            //never had anything to work with.
            addAll(resultCollection, beamCollection);
            addAll(resultCollection, columnCollection);
            addAll(resultCollection, wallCollection);
            addAll(resultCollection, floorCollection);

            //Map the elements to a database element
            List<carboCircleElement> mappedResultCollection = MapElementsTodataBase(resultCollection, appSettings, log);

            log.ElementsCollected = mappedResultCollection == null ? 0 : mappedResultCollection.Count;

            return mappedResultCollection;

        }

        /// <summary>
        /// Size of a filtered list, which is left null when its branch never ran.
        /// </summary>
        private static int countOf(List<Element> collection)
        {
            return collection == null ? 0 : collection.Count;
        }

        /// <summary>
        /// Size of a raw collector, which is left null for a category the settings exclude.
        /// </summary>
        private static int countInView(IEnumerable<Element> collection)
        {
            return collection == null ? 0 : collection.Count();
        }

        /// <summary>
        /// A collector as a list, treating a category the settings excluded as empty rather
        /// than as a null to trip over.
        /// </summary>
        private static List<Element> toList(IEnumerable<Element> collection)
        {
            return collection == null ? new List<Element>() : collection.ToList();
        }

        /// <summary>
        /// Copies one converted category into the result.
        /// </summary>
        private static void addAll(List<carboCircleElement> target, List<carboCircleElement> source)
        {
            if (source == null)
                return;

            foreach (carboCircleElement ccEl in source)
                target.Add(ccEl.Copy());
        }

        /// <summary>
        /// The name of the phase the view is set to, or an empty string when it cannot be
        /// read.
        ///
        /// A view with no phase used to leave every filtered list null, so the import
        /// returned nothing and looked exactly like an empty model. Now it says so.
        /// </summary>
        private static string readPhaseName(View activeView, carboCircleImportLog log)
        {
            if (activeView == null)
            {
                log.Fail("There is no active graphical view to read a phase from.");
                return "";
            }

            Parameter phaseParam = activeView.LookupParameter("Phase");

            if (phaseParam == null)
            {
                log.Fail("The view " + activeView.Name + " has no Phase parameter, so nothing could be " +
                         "filtered by phase. Legends, schedules and drafting views have no phase - run " +
                         "the import from a plan, section or 3D view.");
                return "";
            }

            string phasename = phaseParam.AsValueString();

            if (string.IsNullOrEmpty(phasename))
            {
                log.Fail("The phase of view " + activeView.Name + " could not be read.");
                return "";
            }

            return phasename;
        }

        /// <summary>
        /// Get a list of elements created on the phase shown;
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="phasename"></param>
        /// <returns></returns>
        private static List<Element> getOnPhase(IEnumerable<Element> collection, string phasename, carboCircleImportLog log)
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

                    if (phaseCreatedParam == null)
                    {
                        log.Skip("no Phase Created parameter", el.Id.LongValue());
                        continue;
                    }

                    string phaseCreatedName = phaseCreatedParam.AsValueString();

                    if (phaseCreatedName == phasename)
                        result.Add(el);
                    else
                        log.Filter("created in another phase (" + phaseCreatedName + "), not " + phasename);
                }
                catch (Exception ex)
                {
                    log.Skip("could not read Phase Created: " + ex.Message, el.Id.LongValue());
                }
            }

            return result;
        }

        private static List<Element> getOnDemolishedPhase(IEnumerable<Element> collection, string phasename, carboCircleImportLog log)
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

                    if (phaseDemolishedParam == null)
                    {
                        log.Skip("no Phase Demolished parameter", el.Id.LongValue());
                        continue;
                    }

                    string phaseDemolishedName = phaseDemolishedParam.AsValueString();

                    if (phaseDemolishedName == phasename)
                        result.Add(el);
                    else
                        log.Filter("not demolished in phase " + phasename);
                }
                catch (Exception ex)
                {
                    log.Skip("could not read Phase Demolished: " + ex.Message, el.Id.LongValue());
                }
            }

            return result;
        }

        private static List<Element> getOnlySelected(IEnumerable<Element> collection, List<ElementId> selectedElementIds, carboCircleImportLog log)
        {
            //get a list of selected elemetnts
            List<Element> result = new List<Element>();
            List<int> ids = new List<int>();

            if (collection == null || selectedElementIds == null)
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
                        result.Add(el);
                    else
                        log.Filter("not selected");
                }
                catch (Exception ex)
                {
                    log.Skip("could not be read: " + ex.Message);
                }
            }

            return result;
        }

        private static List<carboCircleElement> MapElementsTodataBase(List<carboCircleElement> beamColumnCollection, carboCircleSettings appSettings, carboCircleImportLog log)
        {
            List<carboCircleElement> SteelSectionDataBase = new List<carboCircleElement>();
            SteelSectionDataBase = getSteelDataBase(appSettings, log);

            //An empty section table takes every collected element down with it, because
            //GetClosestBeamMatch returns nothing when it has nothing to match against. That
            //is a total loss of the import and has to be said out loud.
            if (SteelSectionDataBase == null || SteelSectionDataBase.Count == 0)
                log.Fail("No steel sections could be loaded from " + appSettings.getSectionDatabasePath() +
                         ", so none of the collected elements could be mapped and all of them were dropped.");

            beamColumnCollection = GetClosestBeamMatch(beamColumnCollection, SteelSectionDataBase, appSettings, log);

            return beamColumnCollection;
        }

        private static List<carboCircleElement> GetClosestBeamMatch(List<carboCircleElement> steelBeams, List<carboCircleElement> steelSectionDataBase, carboCircleSettings appSettings, carboCircleImportLog log)
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            //getSteelDataBase returns null on an unreadable file. Reading .Count off that
            //threw, which aborted the whole import from inside a catch that said nothing.
            //An empty result is the same outcome, minus the mystery - MapElementsTodataBase
            //has already reported why.
            if (steelBeams.Count > 0 && steelSectionDataBase != null && steelSectionDataBase.Count > 0 && appSettings != null)
            {

                foreach (carboCircleElement ccE in steelBeams)
                {
                    int indexFound = 0;
                    int lowestLevDist = 9999;
                    int i = 0;
                    carboCircleElement matchingBeam = ccE.Copy();

                    //Only a section can be matched to a section. A steel wall or floor is
                    //a volume of steel, not a member, and mapping it onto the nearest
                    //catalogue beam would invent a section it does not have.
                    if (matchingBeam.isVolumeElement == true || matchingBeam.materialClass != "Steel")
                    {
                        result.Add(matchingBeam);
                        continue;
                    }

                    try
                    {
                        //find the closest steel beam section

                        foreach (carboCircleElement dataBaseBeam in steelSectionDataBase)
                        {
                            int levDist = Utils.CalcLevenshteinDistance(matchingBeam.name, dataBaseBeam.standardName);
                            if (levDist < lowestLevDist)
                            {
                                lowestLevDist = levDist;
                                indexFound = i;
                            }
                            i++;
                        }

                        carboCircleElement closestMatchingBeam = steelSectionDataBase[indexFound];
                        matchingBeam.standardName = closestMatchingBeam.name;
                        matchingBeam.standardDepth = closestMatchingBeam.standardDepth;
                        matchingBeam.standardWidth = closestMatchingBeam.standardWidth;

                        matchingBeam.standardCategory = closestMatchingBeam.standardCategory;
                        matchingBeam.Iy = closestMatchingBeam.Iy;
                        matchingBeam.Wy = closestMatchingBeam.Wy;
                        matchingBeam.Iz = closestMatchingBeam.Iz;
                        matchingBeam.Wz = closestMatchingBeam.Wz;

                        result.Add(matchingBeam);
                    }
                    catch (Exception ex)
                    {
                        log.Skip("could not be matched to a steel section: " + ex.Message, ccE.id);
                    }
                }
            }

            return result;
        }

        private static List<carboCircleElement> getSteelDataBase(carboCircleSettings appSettings, carboCircleImportLog log)
        {
            List<carboCircleElement> result = new List<carboCircleElement>();
            string dbpath = appSettings.getSectionDatabasePath();

            //Each of these used to end the same way: an empty or null table, and an import
            //that returned nothing without naming the file it could not read.
            if (dbpath == null)
            {
                log.Fail("No steel section database path could be resolved.");
                return result;
            }

            if (!File.Exists(dbpath))
            {
                log.Fail("The steel section database was not found at " + dbpath + ".");
                return result;
            }

            if (IsFileLocked(dbpath) == true)
            {
                log.Fail("The steel section database at " + dbpath + " is open in another " +
                         "program, or cannot be opened for writing. Close it and try again.");
                return result;
            }

            DataTable data = CarboLifeAPI.Utils.LoadCSV(dbpath);

            if (data == null)
            {
                log.Fail("The steel section database at " + dbpath + " could not be read as a csv file.");
                return result;
            }

            int badRows = 0;

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
                    ccE.standardWidth = Utils.ConvertMeToDouble(dr[9].ToString()); //check

                    ccE.standardCategory = dr[3].ToString();
                    ccE.Wy = Utils.ConvertMeToDouble(dr[25].ToString());
                    ccE.Iy = Utils.ConvertMeToDouble(dr[21].ToString());
                    ccE.Wz = Utils.ConvertMeToDouble(dr[26].ToString());
                    ccE.Iz = Utils.ConvertMeToDouble(dr[22].ToString());

                    ccE.materialName = dr[5].ToString();

                    result.Add(ccE);
                }
                catch (Exception)
                {
                    //Tallied and reported once below rather than per row: a malformed
                    //database would otherwise produce one message per line.
                    badRows++;
                }
            }

            if (badRows > 0)
                log.Note("Note: " + badRows + " rows of the steel section database at " + dbpath +
                         " could not be read and were ignored.");

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
            catch (Exception)
            {
                //A read-only file, or one the user has no write access to, raises
                //UnauthorizedAccessException rather than IOException. That escaped this
                //method entirely and took the import with it, past every message the
                //caller was about to write. Unreadable for our purposes is unreadable.
                return true;
            }

            //All is ok
            return false;
        }

        /// <summary>
        /// converts a Revit Element to a CarboCircleElement
        /// </summary>
        /// <param name="Collection"></param>
        /// <param name="doc"></param>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        private static List<carboCircleElement> getcarboCircleElements(IEnumerable<Element> Collection, Document doc, carboCircleSettings appSettings, carboCircleImportLog log)
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
                        if (isElementReal(el) == false || el.Id.LongValue() < 0)
                        {
                            log.Skip("no geometry, or an excluded category", el.Id.LongValue());
                            continue;
                        }

                        //Two kinds of element are understood: family instances, which are
                        //beams and columns and carry a section, and host objects, which are
                        //walls and floors and carry only volume. Host objects used to fall
                        //out here, which is why nothing ever reached the volume side of the
                        //tool from them.
                        if (!(el is FamilyInstance) && !(el is HostObject))
                        {
                            log.Skip("not a beam, column, wall or floor", el.Id.LongValue());
                            continue;
                        }

                        //GetMaterialIds and GetMaterialVolume are Element members, not
                        //FamilyInstance ones, so a wall answers them as readily as a beam.
                        //A compound wall or floor returns one id per structural layer.
                        List<ElementId> materials = el.GetMaterialIds(false).ToList();

                        if (materials.Count == 0)
                        {
                            log.Skip("no materials on the element geometry", el.Id.LongValue());
                            continue;
                        }

                        foreach (ElementId materialid in materials)
                        {
                            carboCircleElement newElement = getElementFromMaterialId(materialid, doc, el, appSettings, log);
                            if (newElement != null)
                                resultCollection.Add(newElement);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Skip("could not be read: " + ex.Message, el.Id.LongValue());
                    }
                }
            }
            return resultCollection;
        }

        /// <summary>
        /// Reads one material of one element into a carboCircleElement.
        ///
        /// Two shapes of element arrive here and they are read differently:
        ///
        /// A family instance - a beam or a column - carries a section, so it is read as a
        /// length of a profile that the matcher can try to substitute.
        ///
        /// A host object - a wall or a floor - has no section and no meaningful length, so
        /// it is read as a volume of material and goes to the concrete and masonry side of
        /// the tool instead. Compound walls and floors return one element per structural
        /// layer, which is what GetMaterialVolume is measuring.
        /// </summary>
        private static carboCircleElement getElementFromMaterialId(ElementId materialid, Document doc, Element el, carboCircleSettings appSettings, carboCircleImportLog log)
        {
            carboCircleElement resultElement = new carboCircleElement();

            try
            {
                //Everything that differs between a beam and a wall hangs off this.
                FamilyInstance inst = el as FamilyInstance;
                bool isSectionElement = inst != null;

                //Name (Type)
                ElementType type = doc.GetElement(el.GetTypeId()) as ElementType;

                Autodesk.Revit.DB.Material material = doc.GetElement(materialid) as Autodesk.Revit.DB.Material;

                if (material == null)
                {
                    //Reading the volume needs the material, and a null one used to throw
                    //here and take the whole element with it.
                    log.Skip("a material on the element could not be read", el.Id.LongValue());
                    return null;
                }

                long Revitid = el.Id.LongValue();
                string materialName = material.Name;
                string materialGrade = material.MaterialClass;
                string materialClass = materialClassOf(el, material, log);
                string elementName = el.Name;

                //override default if there is a parameter set:
                if (appSettings.MineParameterName != "" && type != null)
                {
                    Parameter mineParam = type.LookupParameter(appSettings.MineParameterName);
                    if (mineParam != null)
                    {
                        string paramNamed = mineParam.AsString();
                        if (!string.IsNullOrEmpty(paramNamed))
                            elementName = paramNamed;
                    }
                }

                string elementCategoty = el.Category != null ? el.Category.Name : "";

                //A wall or a floor can only ever come back as a volume of material: there is
                //no section to substitute. A beam or column is a volume too unless it is
                //steel or timber, which the matcher can swap for an equivalent member.
                bool isVolumne = true;

                if (isSectionElement && (materialClass == "Steel" || materialClass == "Wood"))
                    isVolumne = false;

                double volume = el.GetMaterialVolume(material.Id);
                if (volume != 0)
                    volume = convertToCubicMtrs(volume); //to m3

                //Length, and the section properties below it, only mean anything for a
                //member. A wall has a length, but it is not a length of section and feeding
                //it to the matcher would be worse than leaving it at zero.
                double length = 0;

                if (isSectionElement)
                {
                    Parameter lengthParam = inst.LookupParameter("Cut Length");
                    if (lengthParam != null)
                    {
                        length = (lengthParam.AsDouble() * 304.8) / 1000; //to m1
                    }
                    else
                    {
                        BuiltInParameter paraIndex = BuiltInParameter.INSTANCE_LENGTH_PARAM;
                        Parameter coLength = inst.get_Parameter(paraIndex);
                        if (coLength != null)
                        {
                            length = (coLength.AsDouble() * 304.8) / 1000; //to m1
                        }
                    }
                }

                //if the element is a wooden beam, give width and depth as typename:
                string typeName = "";
                double typeDepth = 0;
                double typeWidth = 0;

                double typeIy = 0;
                double typeIz = 0;
                double typeWy = 0;
                double typeWz = 0;

                //Try to find a width and depth
                //if this is a steel beam this will be overwritten later anyways
                if (isSectionElement && type != null)
                {

                    Parameter bparam = type.LookupParameter("b");
                    Parameter hparam = type.LookupParameter("d");

                    if (bparam != null && hparam != null)
                    {
                        double width = (bparam.AsDouble() * 304.8); //to mm1
                        double depth = (hparam.AsDouble() * 304.8); //to mm1
                        typeDepth = depth;
                        typeWidth = width;

                        typeIy = (width * (Math.Pow(depth, 3))) / 12;
                        typeIz = (depth * (Math.Pow(width, 3))) / 12;
                        typeWy = (width * (Math.Pow(depth, 2))) / 6;
                        typeWz = (depth * (Math.Pow(width, 2))) / 6;

                        typeName = width.ToString() + "x" + depth.ToString();

                    }
                }


                //set properties:

                resultElement.id = Revitid;
                resultElement.GUID = el.UniqueId;
                resultElement.humanId = Revitid.ToString("X");
                resultElement.name = elementName;
                resultElement.category = elementCategoty;


                //material Props
                resultElement.materialName = materialName;
                resultElement.materialClass = materialClass;
                resultElement.grade = materialGrade;

                resultElement.length = length;
                resultElement.volume = volume;

                resultElement.standardName = typeName;
                resultElement.standardDepth = typeDepth;
                resultElement.standardWidth = typeWidth;
                resultElement.Iy = typeIy;
                resultElement.Iz = typeIz;
                resultElement.Wy = typeWy;
                resultElement.Wz = typeWz;


                resultElement.isVolumeElement = isVolumne;

            }
            catch (Exception ex)
            {
                //Returning null here drops the element. It used to do so without a word,
                //which is how a single unassigned material could quietly remove a whole
                //frame from the results.
                log.Skip("could not be converted: " + ex.Message, el.Id.LongValue());
                return null;
            }
            return resultElement;

        }

        /// <summary>
        /// The material class the rest of the tool switches on: "Steel", "Wood",
        /// "Concrete", "Masonry".
        ///
        /// A family instance answers this itself through StructuralMaterialType, which is
        /// the most reliable source for a structural member and stays the first choice.
        /// Walls and floors have no such property, so they are classified from the Revit
        /// material class - which now also serves as the fallback for a family instance
        /// whose structural material type was never set.
        /// </summary>
        private static string materialClassOf(Element el, Autodesk.Revit.DB.Material material, carboCircleImportLog log)
        {
            FamilyInstance inst = el as FamilyInstance;

            if (inst != null)
            {
                //Fully qualified: the file pulls in SpecTypeId wholesale, which shadows
                //the bare name with something inaccessible.
                Autodesk.Revit.DB.Structure.StructuralMaterialType structuralType = inst.StructuralMaterialType;

                //Undefined and Generic say nothing useful; anything else is the best answer
                //available for a member.
                if (structuralType != Autodesk.Revit.DB.Structure.StructuralMaterialType.Undefined
                    && structuralType != Autodesk.Revit.DB.Structure.StructuralMaterialType.Generic)
                {
                    return normaliseMaterialClass(structuralType.ToString());
                }

                //Only when the instance says nothing, ask the family how it behaves. This
                //was guarded by a test for an empty string, which the enum never returns,
                //so it never ran.
                FamilySymbol familySymbol = inst.Symbol;

                if (familySymbol != null && familySymbol.Family != null)
                {
                    Parameter familyBehaviour = familySymbol.Family.LookupParameter("Material for Model Behavior");

                    if (familyBehaviour != null)
                    {
                        string behaviour = familyBehaviour.AsValueString();

                        if (!string.IsNullOrEmpty(behaviour))
                            return normaliseMaterialClass(behaviour);
                    }
                }
            }

            if (material != null && !string.IsNullOrEmpty(material.MaterialClass))
                return normaliseMaterialClass(material.MaterialClass);

            log.Skip("no material class could be determined", el.Id.LongValue());

            return "";
        }

        /// <summary>
        /// Settles on one spelling per material class.
        ///
        /// Revit names the same material differently depending on where it is read from,
        /// and everything downstream compares these strings literally: "PrecastConcrete"
        /// never matched the "Concrete" the volume side looks for, and a wall built from a
        /// stock steel material reports its class as "Metal".
        ///
        /// Anything unrecognised is passed through untouched rather than folded into an
        /// "Other" bucket, so a class this list has not met yet still reaches the user.
        /// </summary>
        private static string normaliseMaterialClass(string rawClass)
        {
            if (string.IsNullOrEmpty(rawClass))
                return "";

            string trimmed = rawClass.Trim();

            if (trimmed.Equals("Metal", StringComparison.OrdinalIgnoreCase))
                return "Steel";

            if (trimmed.Equals("Timber", StringComparison.OrdinalIgnoreCase))
                return "Wood";

            if (trimmed.Equals("PrecastConcrete", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Precast Concrete", StringComparison.OrdinalIgnoreCase))
                return "Concrete";

            if (trimmed.Equals("Brick", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("Blockwork", StringComparison.OrdinalIgnoreCase))
                return "Masonry";

            return trimmed;
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

        internal static bool visualiseElements(UIApplication uiapp, carboCircleProject project)
        {
            bool result = false;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            carboCircleSettings settings = project.settings;
            ///structure reused
            ///structure not reused
            ///structure from reused
            ///structure new
            List<ElementId> reusedMinedElementIds = new List<ElementId>();//mined
            List<ElementId> reusedRequiredElementIds = new List<ElementId>();//
            List<ElementId> NOTreusedMinedElementIds = new List<ElementId>();
            List<ElementId> NOTreusedRequireddElementIds = new List<ElementId>();
            List<ElementId> reusedAbleVolumeElementIds = new List<ElementId>();//

            //Colour all elemen
            List<carboCircleMatchElement> matchedData = project.getCarboMatchesListSimplified();
            ///List<carboCircleElement> leftOverElements = project.getLeftOverData();

            foreach (carboCircleMatchElement element in matchedData)
            {
                ElementId matchedMinedElement = element.mined_id.ToElementId();
                ElementId matchedRequiredElement = element.required_id.ToElementId();

                reusedRequiredElementIds.Add(matchedRequiredElement);
                reusedMinedElementIds.Add(matchedMinedElement);
            }

            //collect leftovers
            //get all elements in view;
            IEnumerable<Element> allCollector = null;
            allCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().ToElements();
            List<Element> newElementsInView = new List<Element>();

            //The visualisation does not report yet - only the import does, through
            //CarboCircleHandler.reportImport. This log is written and dropped; wire it up
            //to the caller when the colouring gets the same treatment.
            carboCircleImportLog colourLog = new carboCircleImportLog();

            //Get elements on current Phase:
            string phasename = readPhaseName(uidoc.ActiveGraphicalView, colourLog);

            if (phasename != "" && allCollector != null)
                newElementsInView = getOnPhase(allCollector, phasename, colourLog);

            foreach (Element element in allCollector)
            {
                if (element != null && isElementReal(element))
                {
                    if (newElementsInView.Contains(element))
                    {
                        NOTreusedRequireddElementIds.Add(element.Id);
                    }
                    else
                    {
                        NOTreusedMinedElementIds.Add(element.Id);
                    }

                }
            }

            //VolumeOpportunities
            //These can be combined and then the Idlist needs to be used
            //Messy but true.

            List<carboCircleElement> volumeOpportunities = project.getCarboVolumeOpportunities();
            foreach (carboCircleElement cce in volumeOpportunities)
            {
                try
                {
                    if (cce.isVolumeElement == true)
                    {
                        if (cce.id != 0)
                        {
                            //single object Volume Elements
                            ElementId eid = null;

                            eid = cce.id.ToElementId();

                            Element el = doc.GetElement(eid);
                            if (el != null)
                                reusedAbleVolumeElementIds.Add(eid);
                        }
                        else
                        {
                            if (cce.idList.Count > 0)
                            {
                                //Combined object Volumes
                                foreach (int id in cce.idList)
                                {
                                    ElementId eid = new ElementId(id);

                                    Element el = doc.GetElement(eid);
                                    if (el != null)
                                        reusedAbleVolumeElementIds.Add(eid);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { }

            }


            //colour the elements:
            using (Transaction t = new Transaction(doc, "Colour The Model"))
            {
                t.Start();

                FilteredElementCollector elements = new FilteredElementCollector(doc);
                FillPatternElement solidFillPattern = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

                //colour all elements in view first:
                colourElements(NOTreusedRequireddElementIds, solidFillPattern, settings.colour_NotFromReused, doc);
                colourElements(NOTreusedMinedElementIds, solidFillPattern, settings.colour_NotReused, doc);

                //now colour the ones that are reused
                colourElements(reusedMinedElementIds, solidFillPattern, settings.colour_ReusedMinedData, doc);
                colourElements(reusedRequiredElementIds, solidFillPattern, settings.colour_FromReusedData, doc);
                colourElements(reusedAbleVolumeElementIds, solidFillPattern, settings.colour_ReusedMinedVolumes, doc);

                carboCircleLegendBuilder.drawLegend(settings, doc);

                t.Commit();
            }

            return true;
        }

        private static void colourElements(List<ElementId> reusedFromdElementIds, FillPatternElement solidFillPattern, CarboColour colour_ReusedMinedData, Document doc)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();

            foreach (ElementId id in reusedFromdElementIds)
            {
                Element el = doc.GetElement(id);
                if (el != null)
                {
                    //if switch is false reset overrides.
                    ogs = getOverrideObject(solidFillPattern.Id, colour_ReusedMinedData);
                    doc.ActiveView.SetElementOverrides(el.Id, ogs);
                }
            }
        }

        private static OverrideGraphicSettings getOverrideObject(ElementId id, CarboColour colour)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();

            ogs.SetSurfaceForegroundPatternId(id);
            ogs.SetSurfaceForegroundPatternColor(new Autodesk.Revit.DB.Color(colour.r, colour.g, colour.b));

            ogs.SetCutForegroundPatternId(id);
            ogs.SetCutForegroundPatternColor(new Autodesk.Revit.DB.Color(colour.r, colour.g, colour.b));

            return ogs;
        }
    }
}