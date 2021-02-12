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
        public double demoArea { get; set; }
        public double C1Factor { get; set; }
        public double A5Factor { get; set; }

        public string filePath { get; set; }
        
        //Global Calculations:
        public double A5Global { get; set; }
        public double C1Global { get; set; }
        //Absolute total including Global Values
        public double ECTotal { get; set; }


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
                catch(Exception ex)
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

            CarboDatabase = new CarboDatabase();
            CarboDatabase = CarboDatabase.DeSerializeXML("");

            groupList = new ObservableCollection<CarboGroup>();
            elementList = new ObservableCollection<CarboElement>();
            carboLevelList = new List<CarboLevel>();

            Name = "New Project";
            Number = "000000";
            Category = "A building";
            Description = "New Project";
            SocialCost = 0;
            //C1 Global
            demoArea = 0;
            C1Global = 0;
            C1Factor = 3.40; // kg CO2 per m2
            //A5 Global
            Value = 0;
            A5Global = 0;
            A5Factor = 1400; //kg CO2 per vaue
        }
        public void CreateGroups()
        {
            //get default group settings;
            CarboGroupSettings groupSettings = new CarboGroupSettings();
            groupSettings = groupSettings.DeSerializeXML();

            this.groupList = CarboElementImporter.GroupElementsAdvanced(this.elementList, groupSettings.groupCategory, groupSettings.groupSubCategory, groupSettings.groupType, groupSettings.groupMaterial, groupSettings.groupSubStructure, groupSettings.groupDemolition, CarboDatabase, groupSettings.uniqueTypeNames);
            CalculateProject();
        }

        public void UpdateAllMaterials()
        {
            List<string> updatedmaterials = new List<string>();
            foreach(CarboGroup gr in this.groupList)
            {
                CarboMaterial cm = CarboDatabase.GetExcactMatch(gr.Material.Name);
                if (cm != null)
                {
                    if (cm.ECI != gr.ECI)
                    {
                        //The material has been changed, update required. 
                        gr.Material = cm;
                        gr.RefreshValuesFromElements();
                        gr.CalculateTotals();
                        updatedmaterials.Add(gr.MaterialName);
                    }
                }
            }
        }

        public List<CarboDataPoint> getTotals(string value)
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();

            if (value != "")
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
            //Would be Per life
            else
            {
                CarboDataPoint cb_A1A3 = new CarboDataPoint("A1 A3",0);
                CarboDataPoint cb_A4 = new CarboDataPoint("A4", 0);
                CarboDataPoint cb_A5 = new CarboDataPoint("A5", this.A5Global);
                //CarboDataPoint cb_B1B5 = new CarboDataPoint("B1 B5", 0);
                CarboDataPoint cb_C1C4 = new CarboDataPoint("C1 C4", 0);
                CarboDataPoint cb_C1 = new CarboDataPoint("C1", this.C1Global);
                CarboDataPoint cb_D = new CarboDataPoint("D", 0);

                valueList.Add(cb_A1A3);
                valueList.Add(cb_A4);
                valueList.Add(cb_A5);
                valueList.Add(cb_C1);
                valueList.Add(cb_C1C4);
                valueList.Add(cb_D);
                               

                foreach (CarboGroup CarboGroup in this.groupList)
                {
                    double ECI_A1A3 = CarboGroup.Material.ECI_A1A3 * CarboGroup.Mass;
                    double ECI_A4 = CarboGroup.Material.ECI_A4 * CarboGroup.Mass;
                    double ECI_A5 = CarboGroup.Material.ECI_A5 * CarboGroup.Mass;
                    //double ECI_B1B5 = CarboGroup.Material.ECI_B1B5;
                    double ECI_C1C4 = CarboGroup.Material.ECI_C1C4 * CarboGroup.Mass;
                    double ECI_D = CarboGroup.Material.ECI_D * CarboGroup.Mass;

                    if (valueList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in valueList)
                        {
                            string ppName = pp.Name;

                            if (ppName == "A1 A3")
                            {
                                pp.Value += ECI_A1A3;
                            }
                            else if (ppName == "A4")
                            {
                                pp.Value += ECI_A4;
                            }
                            else if (ppName == "A5")
                            {
                                pp.Value += ECI_A5;
                            }
                            //else if (ppName == "B1 B5")
                            //{
                            //    pp.Value += ECI_B1B5;
                            //}
                            else if (ppName == "C1 C4")
                            {
                                pp.Value += ECI_C1C4;
                            }
                            else if (ppName == "D")
                            {
                                pp.Value += ECI_D;
                            }
                            else
                            {
                                pp.Value += 0;
                            }
                        }
                    }

                }
            }

     
            //ValidateData
            foreach(CarboDataPoint cp in valueList)
            {
                if (cp.Value < 0)
                {
                    cp.Value = cp.Value * -1;
                    cp.Name = cp.Name + "[NEGATIVE]";
                }

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

        public double getTotalEC() 
        {
            double totalMaterials = getTotalsGroup().EC;
            double totalA5 = A5Global;
            double totalTotal = totalMaterials + totalA5;
            
            return totalTotal;
        }

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
                result += "Total, material specific: " + Math.Round(totalMaterials, 2) + " MtCO2e " + Environment.NewLine;
            if (globals == true)
                result += "Total, global project specific (A5): " + Math.Round(globalA5, 2) + " MtCO2e" + Environment.NewLine;
            
            if(globalA5 == 0)
                result += "(No project values to calculate A5 emissions )" + Environment.NewLine;
            //C1
            if (globalC1 == 0)
                result += "(No demolition estimation in project )" + Environment.NewLine;
            else
            {
                //result += "Total, global demolition value (C1): " + Math.Round(demoArea, 2) + " m² x " +  Math.Round(C1Factor, 2) + " kgCO2e/m² / 1000 = " + Math.Round(globalC1, 2) + " MtCO2e"  + Environment.NewLine;
                result += "Total, global demolition value (C1): " + Math.Round(globalC1, 2) + " MtCO2e" + Environment.NewLine;

            }
            result += Environment.NewLine;
            //Totals:
            result += "Total CO2e = " + "Total materials" + " + A5 Global " + " + C1 Global " + Environment.NewLine;

            result += "Total CO2e = " + Math.Round(totalMaterials, 2) + " + " + Math.Round(globalA5, 2) + " + " + Math.Round(globalC1, 2) + Environment.NewLine;


            result += "Total CO2e = " + Math.Round(totalTotal, 2) + " MtCO2e (Metric tons of carbon dioxide equivalent)" + Environment.NewLine + Environment.NewLine;
            if (Area > 0)
            {
                result += "OR " + Math.Round(totalTotal / Area, 2) + " MtCO2e/m² (Metric tons of carbon dioxide equivalent per square meter)" + Environment.NewLine;
            }

            result += Environment.NewLine;

            if (materials == true)
                result += "This equals to: " + Math.Round(totalTotal / 68.5, 2) + " average car emission per year. (UK)" + Environment.NewLine + Environment.NewLine;
            
            //if (materials == true)
                //result += "This requires " + Math.Round(totalTotal / 0.0217724, 0) + " trees to exists a year" + Environment.NewLine;

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
            //1.400tCO2e/£100k
            A5Global = (A5Factor * (Value / 100000)) / 1000;
            
            C1Global = (demoArea * C1Factor) / 1000;

            ECTotal = EC + A5Global + C1Global;

        }

        private void setElementotals()
        {
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
            if (okSet == false) ;
                //MessageBox.Show("Elements processed");

        }

        /// <summary>
        /// Returns a full list of elements with their Revit element totals, usefull if elements are constructred using layer or parts. This is a copy f the elements containing their id's and total mass, volue EC and ECi values.
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
                carboLifeElement.Material = new CarboMaterial(materialName);

                elementList.Add(carboLifeElement);
            }

            //groupList = CarboElementImporter.GroupElementsAdvanced(getAllElements, CarboDatabase, "");

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

            return result;
        }

        public void AddGroups(ObservableCollection<CarboGroup> groupList)
        {
            foreach(CarboGroup cg in groupList)
            {
                AddGroup(cg);
            }
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
            foreach (CarboGroup cg in groupList)
            {
                // Update selected group per se. 
                // 
                if (cg.Id == carboGroup.Id)
                {
                    cg.Category = carboGroup.Category;
                    cg.Correction = carboGroup.Correction;
                    cg.Description = carboGroup.Description;
                    cg.Volume = carboGroup.Volume;
                    cg.SubCategory = carboGroup.SubCategory;
                    cg.CalculateTotals();
                }
            }
        }

        public void DuplicateGroup(CarboGroup carboGroup)
        {
            CarboGroup newCarboGroup = new CarboGroup();
            newCarboGroup.Id = getNewId();
            newCarboGroup.TrucateElements();
            newCarboGroup.Description = carboGroup.Description  + "- Copy";
            newCarboGroup.Category = carboGroup.Category;
            newCarboGroup.SubCategory = carboGroup.SubCategory;
            newCarboGroup.Volume = carboGroup.Volume;
            newCarboGroup.Density = carboGroup.Density;
            newCarboGroup.Material = carboGroup.Material;
            newCarboGroup.setMaterial(carboGroup.Material);
            AddGroup(newCarboGroup);
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
        }
        public void DeleteGroup(CarboGroup groupToDelete)
        {
            groupList.Remove(groupToDelete);
        }

        public void AddElement(CarboElement carboElement)
        {
            elementList.Add(carboElement);
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
            bool result = false;
            //this.filePath = "";
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboProject));

                using (FileStream fs = new FileStream(myPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
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
