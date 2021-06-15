using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using CarboLifeUI;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboProject
    {
        public CarboDatabase CarboDatabase { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public double SocialCost { get; set; }
        //Calculated Values
        public double EE { get; set; }
        public double EC { get; set; }
        public double Value { get; set; }
        public double Area { get; set; }
        public string filePath { get; set; }
        
        //Global Calculations:
        public double A5Global { get; set; }
        public double A5Factor { get; set; }
        public double demoArea { get; set; }
        public double C1Factor { get; set; }
        public double C1Global { get; set; }
        //Absolute total including Global Values
        public double ECTotal { get; set; }
        /// <summary>
        /// This is the materialmap that can be used to map the materials.
        /// </summary>
        public List<CarboMapElement> carboMaterialMap  { get; set; }
        public List<CarboLevel> carboLevelList { get; set; }

        private ObservableCollection<CarboElement> elementList;

        private ObservableCollection<CarboGroup> groupList;
        public ObservableCollection<CarboElement> getAllElements
        {
            get { return elementList; }
        }
        public ObservableCollection<CarboGroup> getGroupList
        {
            get {return groupList;}
        }
        //System field
        public bool justSaved;
        public CarboGroup getTotalsGroup()
        {
            CarboGroup newGroup = new CarboGroup();
            newGroup.Category = "Total";
            newGroup.Material = null;
            newGroup.Description = "Totals";
            newGroup.Volume = 0;
            newGroup.Density = 0;
            newGroup.Mass = 0;
            newGroup.ECI = 0;
            newGroup.EC = 0;
            newGroup.Id = getNewId();
            double totals = 0;

            foreach (CarboGroup cgr in getGroupList)
            {
                totals += cgr.EC;
            }
            newGroup.EC = Math.Round(totals,2);
            newGroup.PerCent = 100;

            return newGroup;
        }
        internal void clearHeatmapAndValues()
        {
            foreach (CarboGroup grp in getGroupList)
            {
                try
                {
                    List<CarboElement> elements = grp.AllElements;
                    foreach (CarboElement cel in elements)
                    {
                        cel.r = 0;
                        cel.g = 0;
                        cel.b = 0;
                    }
                }
                catch
                {
                }
            }
        }
        public void SetGroups(ObservableCollection<CarboGroup> groupList)
        {
            this.groupList = groupList;
        }
        public CarboProject()
        {
            //UserPaths
            PathUtils.CheckFileLocationsNew();

            CarboDatabase = new CarboDatabase();
            CarboDatabase = CarboDatabase.DeSerializeXML("");

            groupList = new ObservableCollection<CarboGroup>();
            elementList = new ObservableCollection<CarboElement>();
            carboLevelList = new List<CarboLevel>();

            Name = "New Project";
            Number = "000000";
            Category = "A building";
            Description = "New Project";
            //C1 Global
            demoArea = 0;
            C1Global = 0;
            C1Factor = 3.40; // kg CO₂ per m2
            //A5 Global
            A5Global = 0;
            A5Factor = 1400; //kg CO₂ per vaue
            //Social
            SocialCost = 50;
            //Other
            justSaved = false;
            //Totals
            Value = 0;
            //New projects don't need to be saved
            justSaved = true;
        }
        public void CreateGroups()
        {
            //get default group settings;
            CarboSettings groupSettings = new CarboSettings().Load();
            //groupSettings = groupSettings.DeSerializeXML();

            this.groupList = CarboElementImporter.GroupElementsAdvanced(this.elementList, groupSettings.groupCategory, groupSettings.groupSubCategory, groupSettings.groupType, groupSettings.groupMaterial, groupSettings.groupSubStructure, groupSettings.groupDemolition, CarboDatabase, groupSettings.uniqueTypeNames);
            CalculateProject();
        }
        /// <summary>
        /// Update all the groups with the current materials in the database.
        /// </summary>
        public void UpdateAllMaterials()
        {
            List<string> updatedmaterials = new List<string>();
                foreach (CarboGroup gr in this.groupList)
                {
                try
                {
                    CarboMaterial cm = CarboDatabase.GetExcactMatch(gr.Material.Name);
                    if (cm != null)
                    {
                        if (cm.ECI != gr.ECI)
                        {
                            //The material has been changed, update required. 
                            gr.Material = cm;

                            if (gr.AllElements.Count > 0)
                            {
                                gr.RefreshValuesFromElements();
                            }

                            gr.CalculateTotals();

                            updatedmaterials.Add(gr.MaterialName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Updates the project with the elements inside another carbogroup
        /// </summary>
        /// <param name="newProject"></param>
        public void UpdateProject(CarboProject projectWithElements)
        {
            int totalNewElements;
            int totalOldElements;
            int updatedElements = 0;
            int newElementsInGroup = 0;
            int newElementsWithNoGroup = 0;
            int deletedElements = 0;
            int deletedGroups = 0;

            //Replace All Elements iterate though each element in the new list and the following things can happen:
            // 0. Elements that dont exist in the old file need to be deleted. This will be done first to schrink the search table
            // 1. The item was found (id match and material match) the volumes are then replaced
            // 2. The element wasnt found, and thus it needs to be added to a group, or a new group will be made for it.
            // 4. cleanup, groups without elements, that had elements before will need to be deleted.
            // 5. compound elements need to be reviewed for the future, as they can be trcky to deal with atm.

            totalNewElements = projectWithElements.elementList.Count;
            totalOldElements = this.elementList.Count;

            //Update the element list;
            this.elementList = new ObservableCollection<CarboElement>();
            foreach (CarboElement ce in projectWithElements.elementList)
            {
                CarboElement ceNew = new CarboElement();
                ceNew = ce.CopyMe();
                this.elementList.Add(ceNew);
            }

            //this.elementList = new List<CarboElement>(projectWithElements.elementList);

            //Clear the project so we can track the updates.
            foreach (CarboElement ce in elementList)
            {
                ce.isUpdated = false;
            }
            foreach (CarboGroup cg in getGroupList)
            {
                if (cg.AllElements.Count > 0)
                {
                    foreach (CarboElement ce in cg.AllElements)
                    {
                        ce.isUpdated = false;
                    }
                }
            }

            //1 Update similar elements and flag the ones that are done.
            for (int i = projectWithElements.elementList.Count - 1; i >= 0; i--)
            {
                CarboElement ceNew = projectWithElements.elementList[i] as CarboElement;
                if (ceNew != null)
                {
                    bool isfoundinGroup = false;
                    //bool isfoundinElementList = false;

                    //Check and update the groups: the groups
                    isfoundinGroup = updateinGroups(ceNew);

                    //if the element was found and updated, 
                    if (isfoundinGroup == true)
                    {
                        //remove from list, it has been done;
                        updatedElements++;
                        projectWithElements.elementList.RemoveAt(i);
                        //ceNew.isUpdated = true;
                    }
                }

            }

            //Elements that wern't flagged in the new list must be new, they need to be added to the right group.
            for (int i = projectWithElements.elementList.Count - 1; i >= 0; i--)
            {
                CarboElement ceNew = projectWithElements.elementList[i] as CarboElement;
                if (ceNew != null)
                {
                    //each element will be assesed individually.
                    //Look through the element list:
                    if (ceNew.isUpdated == false)
                    {
                        bool insertedinGroup = false;

                        //Check and update the groups: the groups
                        insertedinGroup = insertinGroups(ceNew);

                        if (insertedinGroup == true)
                        {
                            //The element was found in an existing group and was updated
                            newElementsInGroup++;
                            projectWithElements.elementList.RemoveAt(i);
                        }
                        else
                        {
                            //Elements that wern't flagged in the new list must need their own new group;
                            //A new group need to be made for this element;
                            newElementsWithNoGroup++;


                            this.NewGroup(ceNew);

                            //The materialname was given by the elements, the values now need to be matched with a own one.
                            //cg.MaterialName = closestGroupMaterial.Name;
                            //cg.setMaterial(closestGroupMaterial);


                            projectWithElements.elementList.RemoveAt(i);

                        }
                    }
                }
            }

            //Remove all the elements in groups that werent updated, and delete the groups if empty
            for (int i = this.groupList.Count - 1; i >= 0; i--)
            {
                CarboGroup group = this.groupList[i] as CarboGroup;
                
                if(group != null)
                {
                    bool deletegroup = false;

                    if (group.AllElements.Count > 0)
                    {
                        for (int j = group.AllElements.Count - 1; j >= 0; j--)
                        {
                            CarboElement ce = group.AllElements[j] as CarboElement;
                            if (ce != null)
                            {
                                if (ce.isUpdated == false)
                                {
                                    //We found a element that doesnt exist anymore
                                    group.AllElements.RemoveAt(j);
                                    deletedElements++;
                                }
                            }
                        }
                        //if the group is empty after all the elemetns are deleted, remove the group;
                        if(group.AllElements.Count == 0)
                        {
                            deletegroup = true;
                        }
                    }

                    if (deletegroup == true)
                    {
                        this.groupList.RemoveAt(i);
                        deletedGroups++;
                    }
                }
            }

            //getElementsFromGroups;

            string message =
                "Project updated: " + Environment.NewLine +
                "Old nr of elements: " + totalOldElements + Environment.NewLine +
                "New nr of elements: " + totalNewElements + Environment.NewLine +
                "Nr of elements updated: " + updatedElements + Environment.NewLine +
                "Nr of elements added to groups: " + newElementsInGroup + Environment.NewLine +
                "Nr of elements added to new groups: " + newElementsWithNoGroup + Environment.NewLine +
                "Elements deleted: " + deletedElements + Environment.NewLine +
                "Groups deleted: " + deletedGroups + Environment.NewLine;

            justSaved = false;

            MessageBox.Show(message, "Results", MessageBoxButton.OK);
        }

        private void NewGroup(CarboElement ceNew)
        {
            try
            {
                ceNew.isUpdated = true;
                CarboGroup newGroup = new CarboGroup(ceNew);

                CarboMaterial closestGroupMaterial = CarboDatabase.getClosestMatch(ceNew.MaterialName);
                //cg.MaterialName = closestGroupMaterial.Name;
                newGroup.setMaterial(closestGroupMaterial);
                closestGroupMaterial.CalculateTotals();

                this.AddGroup(newGroup);
            }
            catch
            { 
            }
        }

        private bool insertinGroups(CarboElement ceNew)
        {
            bool result = false;

            foreach (CarboGroup cg in getGroupList)
            {
                if (cg.AllElements.Count > 0)
                {
                    foreach (CarboElement ceOld in cg.AllElements)
                    {
                        //to be added, the category, subcategory & materialname need to be identical
                        if (
                            ceOld.Category == ceNew.Category &&
                            ceOld.SubCategory == ceNew.SubCategory &&
                             ceOld.MaterialName == ceNew.MaterialName
                             )
                        {
                            //A match was found, check the materials
                            ceNew.isUpdated = true;
                            cg.AllElements.Add(ceNew);
                            justSaved = false;

                            return true;
                            //break;
                        }
                    }
                }
            }
            //element is not found this will be put in a new group;
            justSaved = false;

            return result;

        }

        [Obsolete]
        private bool updateinElementList(CarboElement ceNew)
        {
            bool result = false;
            //check all existing elements for its 
            foreach (CarboElement ceOld in elementList)
            {
                bool isSimilar = isElementSimilar(ceOld, ceNew);
                if (ceOld.Id == ceNew.Id && ceOld.MaterialName == ceNew.MaterialName && ceOld.isUpdated == false)
                {
                    //A match was found, update the element

                        //These two match, we can update the volume
                        ceOld.Volume = ceNew.Volume;
                        ceOld.Volume_Total = 0;
                        //not sure about the below:
                        ceOld.Category = ceNew.Category;
                        ceOld.Category = ceNew.SubCategory;

                        //Set Flags
                        ceOld.isUpdated = true;
                    justSaved = false;

                    return true;
                }
            }
            //element is not found 
            return result;
        }

        private bool updateinGroups(CarboElement ceNew)
        {
            bool result = false;

            foreach (CarboGroup cg in getGroupList)
            {
                if (cg.AllElements.Count > 0)
                {
                    foreach (CarboElement ceOld in cg.AllElements)
                    {
                        bool isSimilar = isElementSimilar(ceOld, ceNew);

                        if (isSimilar == true)
                        {
                            //A match was found, check the materials

                            ceOld.Volume = ceNew.Volume;
                            ceOld.Volume_Total = 0;
                            //not sure about the below:
                            //ceOld.Category = ceNew.Category;
                            //ceOld.SubCategory = ceNew.SubCategory;

                            //Set Flags
                            ceOld.isUpdated = true;

                            //Dont go looking for more elements
                            return true;
                            //break;
                        }
                    }
                    //group found, don't go looking for more groups
                }
            }
            //element is not found 
            justSaved = false;
            return result;
        }

        private bool isElementSimilar(CarboElement ceOld, CarboElement ceNew)
        {
            bool result = false;

            if (ceOld.Id == ceNew.Id &&
                ceOld.MaterialName == ceNew.MaterialName &&
                ceOld.isUpdated == false)
            {
                return true;

            }
            return result;
        }


        /// <summary>
        /// After creating a mapping list, you can use this method to update all map all the groups in a project.
        /// </summary>
        public void mapAllMaterials()
        {
            if (carboMaterialMap != null)
            {
                if (carboMaterialMap.Count > 0)
                {
                    foreach (CarboGroup gr in this.groupList)
                    {
                        try
                        {
                            //MApping only works where elements are imported from Revit and the group contains elements
                            if (gr.AllElements != null && gr.AllElements.Count > 0)
                            {
                                //First see if a change is required;
                                //Find the map file of this group using a single element in the group;
                                CarboMapElement mapElement = GetMapItem(gr.AllElements[0].MaterialName, gr.Category);
                                if (mapElement != null)
                                {
                                    //Get the material from the mapping name;
                                    CarboMaterial cm = CarboDatabase.GetExcactMatch(mapElement.carboNAME);

                                    if (cm != null)
                                    {
                                        //see if the material need changing;
                                        if (cm.Name != gr.MaterialName)
                                        {
                                            //Only update if the mapping file suggest a change.
                                            gr.Material = cm;
                                            gr.RefreshValuesFromElements();
                                            gr.CalculateTotals();
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please setup your material map first.");
            }
        }

        private CarboMapElement GetMapItem(string materialName, string category)
        {
            CarboMapElement result = null;

            if (carboMaterialMap != null)
            {
                if (carboMaterialMap.Count > 0)
                {
                    foreach (CarboMapElement mapE in carboMaterialMap)
                    {
                        try
                        {
                            if(mapE.category == category && mapE.revitName == materialName)
                            {
                                return mapE;
                            }
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Resets all the isupadtedflags in all the elements and groups, usefull if you want to make precise selections
        /// </summary>
        public void ResetElementFlags()
        {
            foreach(CarboElement ce in elementList)
            {
                ce.isUpdated = false;
            }
            foreach(CarboGroup cg in groupList)
            {
                if(cg.AllElements.Count > 0)
                {
                    foreach (CarboElement ce in cg.AllElements)
                    {
                        ce.isUpdated = false;
                    }
                }
            }
        }

        public List<CarboDataPoint> getMaterialTotals()
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();
            try
            {

                foreach (CarboGroup CarboGroup in this.groupList)
                {

                    CarboDataPoint newelement = new CarboDataPoint();
                    newelement.Name = CarboGroup.MaterialName;
                    newelement.Value = CarboGroup.EC;

                    bool merged = false;

                    if (valueList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in valueList)
                        {
                            if (pp.Name == newelement.Name)
                            {
                                pp.Value += newelement.Value;
                                merged = true;
                                break;
                            }
                        }
                    }
                    if (merged == false)
                        valueList.Add(newelement);
                }
            }
            catch
            {
                return null;
            }
            
            //Values should return now;
            return valueList;
        }
        /// <summary>
        /// Returns a list of the project totals (9 items)
        /// A1-A3, A4, A5 (Material), A5(Global), B1-B7, C1-C4, C1(Global), D, Additional)
        /// These are net values(based on the corrected converted Total Volume
        /// </summary>
        /// <returns>Returns a list of the project totals (9 items) in kgCO₂</returns>
        public List<CarboDataPoint> getPhaseTotals()
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();
            try
            {
                CarboDataPoint cb_A1A3 = new CarboDataPoint("A1-A3", 0);
                CarboDataPoint cb_A4 = new CarboDataPoint("A4", 0);
                CarboDataPoint cb_A5 = new CarboDataPoint("A5(Material)",0);
                CarboDataPoint cb_A5Global = new CarboDataPoint("A5(Global)", this.A5Global * 1000);
                CarboDataPoint cb_B1B5 = new CarboDataPoint("B1-B7", 0);
                CarboDataPoint cb_C1C4 = new CarboDataPoint("C1-C4", 0);
                CarboDataPoint cb_C1Global = new CarboDataPoint("C1(Global)", this.C1Global * 1000);
                CarboDataPoint cb_D = new CarboDataPoint("D", 0);
                CarboDataPoint Added = new CarboDataPoint("Additional", 0);

                valueList.Add(cb_A1A3);
                valueList.Add(cb_A4);
                valueList.Add(cb_A5);
                valueList.Add(cb_A5Global);
                valueList.Add(cb_B1B5);
                valueList.Add(cb_C1C4);
                valueList.Add(cb_C1Global);
                valueList.Add(cb_D);
                valueList.Add(Added);

                foreach (CarboGroup CarboGroup in this.groupList)
                {
                    double EC_A1A3 = CarboGroup.getTotalA1A3;
                    double EC_A4 = CarboGroup.getTotalA4;
                    double EC_A5 = CarboGroup.getTotalA5;
                    double EC_B1B7 = CarboGroup.getTotalB1B7;
                    double EC_C1C4 = CarboGroup.getTotalC1C4;
                    double EC_D = CarboGroup.getTotalD;
                    double EC_Add = CarboGroup.getTotalMix;

                    if (valueList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in valueList)
                        {
                            string ppName = pp.Name;

                            if (ppName == "A1-A3")
                            {
                                pp.Value += EC_A1A3;
                            }
                            else if (ppName == "A4")
                            {
                                pp.Value += EC_A4;
                            }
                            else if (ppName == "A5(Material)")
                            {
                                pp.Value += EC_A5;
                            }
                            else if (ppName == "B1-B7")
                            {
                                pp.Value += EC_B1B7;
                            }
                            else if (ppName == "C1-C4")
                            {
                                pp.Value += EC_C1C4;
                            }
                            else if (ppName == "D")
                            {
                                pp.Value += EC_D;
                            }
                            else if (ppName == "Additional")
                            {
                                pp.Value += EC_Add;
                            }
                            else
                            {
                                pp.Value += 0;
                            }
                        }
                    }

                }
            }
            catch
            {
                return null;
            }

            //Values should return now;
            return valueList;
        }



        internal List<CarboMaterial> getUsedmaterials()
        {
            List<CarboMaterial> result = new List<CarboMaterial>();

            foreach (CarboGroup group in getGroupList)
            {
                //get material from group
                CarboMaterial material = group.Material;
                bool isunique = true;
                
                foreach (CarboMaterial mat in result)
                {
                    if (material.Name == mat.Name)
                    {
                        isunique = false;
                        break;
                    }
                }
                    if(isunique == true)
                        result.Add(material);
            }
            return result;
        }
        /// <summary>
        /// Returns the toal EC including the global values
        /// </summary>
        /// <returns></returns>
        public double getTotalEC() 
        {
            double totalMaterials = getTotalsGroup().EC;
            double totalA5 = A5Global;
            double totalC1 = C1Global;
            double totalTotal = totalMaterials + totalA5 + totalC1;
            
            return totalTotal;
        }
        /// <summary>
        /// Returns the total social carbon costs
        /// </summary>
        /// <returns></returns>
        public double getTotalSocialCost()
        {
            double result = getTotalEC() * SocialCost;

            return result;
        }
        public string getSummaryText(bool materials, bool globals, bool cars, bool trees)
        {
            string result = "";

            double totalMaterials = getTotalsGroup().EC;
            double globalA5 = A5Global;
            double globalC1 = C1Global;
            double totalTotal = totalMaterials + globalA5 + globalC1;

            if (materials == true)
                result += "Total material specific: " + Math.Round(totalMaterials, 2) + " tCO₂e " + Environment.NewLine;
            if (globals == true)
                result += "Total global project specific (A5): " + Math.Round(globalA5, 2) + " tCO₂e" + Environment.NewLine;
            
            if(globalA5 == 0)
                result += "(No project values to calculate A5 emissions )" + Environment.NewLine;
            //C1
            if (globalC1 == 0)
                result += "(No demolition estimation in project )" + Environment.NewLine;
            else
            {
                //result += "Total, global demolition value (C1): " + Math.Round(demoArea, 2) + " m² x " +  Math.Round(C1Factor, 2) + " kgCO₂e/m² / 1000 = " + Math.Round(globalC1, 2) + " tCO₂e"  + Environment.NewLine;
                result += "Total global demolition value (C1): " + Math.Round(globalC1, 2) + " tCO₂e" + Environment.NewLine;

            }
            result += Environment.NewLine;
            //Totals:
            result += "Total CO₂e = " + "Total materials" + " + A5 Global " + " + C1 Global " + Environment.NewLine;

            result += "Total CO₂e = " + Math.Round(totalMaterials, 2) + " + " + Math.Round(globalA5, 2) + " + " + Math.Round(globalC1, 2) + Environment.NewLine;


            result += "Total CO₂e = " + Math.Round(totalTotal, 2) + " tCO₂e (Metric tons of carbon dioxide equivalent)" + Environment.NewLine + Environment.NewLine;
            if (Area > 0)
            {
                result += "OR " + Math.Round(totalTotal / Area, 3) + " tCO₂e/m² (Metric tons of carbon dioxide equivalent per square meter)" + Environment.NewLine;
            }

            result += Environment.NewLine;

            if (materials == true)
                result += "This equals to: " + Math.Round(totalTotal / 1.40, 2) + " average car emission per year (1.40 tCO₂/car). (UK)" + Environment.NewLine;
            
            if (trees == true)
                result += "This requires " + Math.Round((totalTotal / 180) * 4440, 0) + " Trees (Spruce or Fir) to grow for at least 30 years" + Environment.NewLine;

            double socialcost = (totalTotal * SocialCost);

            result += "The Social Carbon Costs are: " + Math.Round(socialcost, 2) + " $/£/€ total" + Environment.NewLine + Environment.NewLine;


            return result;
        }

        public void CalculateProject()
        {
            //EE = 0;
            
            EC = 0;
            ECTotal = 0;
            //This Will calculate all totals and set all the individual element values;
            foreach (CarboGroup cg in groupList)
            {
                cg.CalculateTotals();
                //EE += cg.EE;
                EC += cg.EC;
            }

            foreach (CarboGroup cg in groupList)
            {
                cg.SetPercentageOf(EC);
            }

            //Set element totals
            setElementotals();

            //Set A5 based on value;
            //1.400tCO₂e/£100k
            A5Global = (A5Factor * (Value / 100000)) / 1000;
            
            C1Global = (demoArea * C1Factor) / 1000;

            ECTotal = EC + A5Global + C1Global;

            justSaved = false;

        }

        private void setElementotals()
        {
            //Generate a new ALL elements list from the calculated values in the Groups
            List<CarboElement> elementbuffer = getTemporaryElementListWithTotals();

            //Now Imprint them into the elements totals
            bool okSet = false;
            foreach (CarboGroup cg in groupList)
            {
                if (cg.AllElements != null)
                {
                    if (cg.AllElements.Count > 0)
                    {
                        for (int i = 0; i<=cg.AllElements.Count - 1; i++)
                        {
                            CarboElement ce = cg.AllElements[i];
                            ce = addBufferToElements(elementbuffer, ce, out okSet);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a full list of elements with their Revit element totals, usefull if elements are constructred using layer or parts. This is a copy 0f the elements containing their id's and total mass, volue EC and ECi values.
        /// </summary>
        /// <returns></returns>
        public List<CarboElement> getTemporaryElementListWithTotals()
        {
            //Cnstruct the buffer file
            List<CarboElement> elementbuffer = new List<CarboElement>();

            //First Count the totals
            bool ok = false;

            foreach (CarboGroup cg in groupList)
            {
                if (cg.AllElements != null)
                {
                    if (cg.AllElements.Count > 0)
                    {
                        foreach (CarboElement ce in cg.AllElements)
                        {
                            elementbuffer = addToBuffer(elementbuffer, ce, out ok);
                        }
                    }
                }
            }
            
            return elementbuffer;

        }
        /// <summary>
        /// Get a list of all the elemetns from the Groups, these contain all the calculated and cumulitive data 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CarboElement> getElementsFromGroups()
        {
            List<CarboElement> elementbuffer = new List<CarboElement>();
            List<CarboElement> SortedList = new List<CarboElement>();

            foreach (CarboGroup cg in groupList)
            {
                if (cg.AllElements != null)
                {
                    if (cg.AllElements.Count > 0)
                    {
                        foreach (CarboElement ce in cg.AllElements)
                        {
                            elementbuffer.Add(ce);
                        }
                    }
                }
            }

            if(elementbuffer.Count > 0)
                SortedList = elementbuffer.OrderBy(o => o.Id).ToList();

            return SortedList;

        }

        private CarboElement addBufferToElements(List<CarboElement> elementbuffer, CarboElement cElement, out bool ok)
        {
            ok = false;

            if (elementbuffer != null)
            {
                if (elementbuffer.Count > 0)
                {
                    //searc the buffer for the right totals
                    foreach (CarboElement buffer_CE in elementbuffer)
                    {
                        try
                        {
                            if (buffer_CE.Id == cElement.Id)
                            {
                                //Set the values;
                                cElement.EC_Total = buffer_CE.EC_Total;
                                cElement.ECI_Total = buffer_CE.ECI_Total;
                                cElement.Volume_Total = buffer_CE.Volume_Total;
                                ok = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ok = false;
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            justSaved = false;

            return cElement;
        }
        private List<CarboElement> addToBuffer(List<CarboElement> elementbuffer, CarboElement cElement, out bool ok)
        {
            bool isUnique = true;
            ok = false;

            if (elementbuffer != null)
            {
                    foreach(CarboElement buffer_CE in elementbuffer)
                    {
                        try
                        {
                            if (buffer_CE.Id == cElement.Id)
                            {
                                //Merge Elements
                                //double density = buffer_CE.EC_Total / (buffer_CE.Volume * buffer_CE.ECI_Total);
                                double volume_Total = buffer_CE.Volume + cElement.Volume;
                                double mass_Total = buffer_CE.Mass + cElement.Mass;
                                double mass_TotalCheck = (buffer_CE.EC / buffer_CE.ECI) + (cElement.EC / cElement.ECI);

                                //double Density_Total = mass_Total / volume_Total;

                                double EC_Total = buffer_CE.EC_Total + cElement.EC;
                                //Calculate combined ECI.
                                double ECI_Total = EC_Total / mass_Total;

                                //Total EC:
                                buffer_CE.EC_Total = EC_Total;
                                //Total Volume
                                buffer_CE.Volume_Total = volume_Total;
                                //Total ECI
                                buffer_CE.ECI_Total = ECI_Total;
                                //mass Total
                                buffer_CE.Mass = mass_Total;
                                //Density Total
                                // n/a

                                isUnique = false;
                                ok = true;
                                break;
                            }
                        }
                        catch(Exception ex)
                        {
                            ok = false;
                            MessageBox.Show(ex.Message);

                        }
                    }
                    //The element doesnt exist yet in the list; add to the list as a new element.
                    if (isUnique == true)
                    {
                        CarboElement newElement = new CarboElement();
                        newElement.Id = cElement.Id;
                        newElement.Volume = cElement.Volume;

                        newElement.ECI_Total = cElement.ECI;
                        newElement.EC_Total = cElement.EC;
                        newElement.Volume_Total = cElement.Volume;

                        newElement.Mass = cElement.Mass;
                        
                        elementbuffer.Add(newElement);
                        ok = true;
                    }
                
            }
            justSaved = false;

            return elementbuffer;
        }
        public void GenerateDummyList()
        {
            //Create a large list of dummy elements;
            elementList = new ObservableCollection<CarboElement>();

            Random rnd = new Random(1000);

            for (int i = 0; i<100; i++)
            {
                double volume = rnd.Next(10, 100);  // creates a random Volume
                int value1 = rnd.Next(1, 5);  // creates a random Volume
                int value2 = rnd.Next(1, 5);  // creates a random Volume

                string materialName = CarboElementImporter.getRandomMaterial(value1);
                string category = CarboElementImporter.getRandomCategory(value2);
                int id = rnd.Next(10000, 20000);

                CarboElement carboLifeElement = new CarboElement();

                carboLifeElement.Id = id;
                carboLifeElement.MaterialName = materialName;
                carboLifeElement.Volume = volume;
                carboLifeElement.Category = category;
                carboLifeElement.SubCategory = "";
                carboLifeElement.Density = 1000;
                //carboLifeElement.Material = new CarboMaterial(materialName);

                elementList.Add(carboLifeElement);
            }

            CalculateProject();
        }
        public void Audit()
        {
            //Check if all groups have unique Id;
            List<int> ids = new List<int>();
            bool renumberGroupIds = false;

            //find error
            foreach(CarboGroup cg in groupList)
            {
                int idToCheck = cg.Id;
                ids.Add(idToCheck);
                bool containsId = ids.Contains(idToCheck);

                if(containsId == true)
                {
                    renumberGroupIds = true;
                    break;
                }
            }
            //Fix
            if(renumberGroupIds == true)
            {
                int idnr = 10000;
                foreach (CarboGroup cg in groupList)
                {
                    cg.Id = idnr;
                    idnr++;
                }
            }
            justSaved = false;

        }
        public ObservableCollection<CarboGroup> GetGroupsWithoutElements()
        {
            ObservableCollection<CarboGroup> result = new ObservableCollection<CarboGroup>();

            foreach (CarboGroup cg in groupList)
            {
                if (cg.AllElements.Count == 0)
                    result.Add(cg);
            }

            return result;
        }
        public bool CreateNewGroup(string category = "")
        {
            bool result = false;

            CarboGroup newGroup = new CarboGroup();
            if(category != "")
                newGroup.Category = category;

            int id = getNewId();
            newGroup.Id = id;

            AddGroup(newGroup);

            justSaved = false;

            return result;
        }
        public void AddGroups(ObservableCollection<CarboGroup> groupList)
        {
            foreach(CarboGroup cg in groupList)
            {
                AddGroup(cg);
            }

            justSaved = false;
        }
        private int getNewId()
        {
            int id = 0;
            foreach(CarboGroup cg in groupList)
            {
                if(cg.Id > id)
                {
                    id = cg.Id;
                }
            }
            return id + 1;
        }
        public void UpdateGroup(CarboGroup carboGroup)
        {
            for (int i = 0; i < groupList.Count; i++)
            {
                CarboGroup cg = groupList[i];
                // Update selected group per se. 
                // 
                if (cg.Id == carboGroup.Id)
                {
                    cg.copyValues(carboGroup);

                    cg.CalculateTotals();
                    break;
                }
            }
        }
        public void DuplicateGroup(CarboGroup carboGroup)
        {
            CarboGroup newCarboGroup = carboGroup.Copy();
            newCarboGroup.Copy();
            newCarboGroup.Description +=  "- Copy";

            AddGroup(newCarboGroup);

            justSaved = false;

        }
        public void PurgeElements(CarboGroup carboGroup)
        {
            foreach (CarboGroup cg in groupList)
            {
                if (cg.Id == carboGroup.Id)
                {
                    cg.TrucateElements();
                }
            }
        }
        public bool AddGroup(CarboGroup newGroup)
        {
            bool result = false;

            if (!(groupList.Contains(newGroup)))
            {
                newGroup.Id = getNewId();
                groupList.Add(newGroup);
            }
            else
            {
                MessageBox.Show("new group already exists");
                return false;
            }
            justSaved = false;

            return result;

        }
        public void UpdateMaterial(CarboGroup TargetGroup, CarboMaterial NewMaterial)
        {
            if (NewMaterial != null)
            {
                foreach (CarboGroup cg in groupList)
                {
                    // Update selected group per se. 
                    // 
                    if (cg.Id == TargetGroup.Id)
                    {
                        cg.MaterialName = NewMaterial.Name;
                        cg.Material = NewMaterial;
                    }

                    //Update all idential materials
                    if (cg.Material.Id == NewMaterial.Id)
                    {
                        cg.MaterialName = NewMaterial.Name;
                        cg.Material = NewMaterial;
                    }
                }
            }

            justSaved = false;

        }
        public void DeleteGroup(CarboGroup groupToDelete)
        {
            groupList.Remove(groupToDelete);
            CalculateProject();
        }
        public void AddElement(CarboElement carboElement)
        {
            elementList.Add(carboElement);
            justSaved = false;
        }
        public CarboProject DeSerializeXML(string myPath)
        { 
            if (File.Exists(myPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboProject));
                    CarboProject bufferproject;

                    using (FileStream fs = new FileStream(myPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboProject;
                        bufferproject.justSaved = false;
                    }
                    bufferproject.filePath = myPath;
                     return bufferproject;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return null;
                }
            }
            return null;
        }
        public bool SerializeXML(string myPath)
        {
            try
            {
                foreach(CarboGroup grp in groupList)
                {
                    grp.Material.materiaA4Properties.calcResult = "";
                }
                
                XmlSerializer ser = new XmlSerializer(typeof(CarboProject));

                using (FileStream fs = new FileStream(myPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
                justSaved = true;
                return true;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
