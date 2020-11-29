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
        //Project based value
        public double A5 { get; set; }

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
            Number = "00000";
            Category = "";
            Description = "New Project";
            SocialCost = 0;
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
                CarboDataPoint cb_A5 = new CarboDataPoint("A5", this.A5);
                //CarboDataPoint cb_B1B5 = new CarboDataPoint("B1 B5", 0);
                CarboDataPoint cb_C1C4 = new CarboDataPoint("C1 C4", 0);
                CarboDataPoint cb_D = new CarboDataPoint("D", 0);

                valueList.Add(cb_A1A3);
                valueList.Add(cb_A4);
                valueList.Add(cb_A5);
                //valueList.Add(cb_B1B5);
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
        /// <summary>
        /// Creates a heatmap in the elements 
        /// </summary>
        /// <param name="v">1 = material, 2=Group, 3=Elements</param>
        public void CreateMaterialHeat()
        {
                string list = "List: " + Environment.NewLine;
                List<CarboMaterial> materialList = getUsedmaterials();
                //By Material
                List<CarboMaterial> SortedMatList = materialList.OrderBy(o => o.ECI).ToList();

                double low = SortedMatList[0].ECI;
                double high = SortedMatList[SortedMatList.Count - 1].ECI;

                foreach (CarboGroup grp in getGroupList)
                {
                    System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, grp.Material.ECI);

                    List<CarboElement> elements = grp.AllElements;
                    foreach (CarboElement cel in elements)
                    {
                        cel.r = groupColour.R;
                        cel.g = groupColour.G;
                        cel.b = groupColour.B;
                    }
                }

             //   MessageBox.Show(list);
        }

        public void CreateMaterialHeatNorm()
        {

            string list = "List: " + Environment.NewLine;
            List<CarboMaterial> materialList = getUsedmaterials();
            //By Material
            List<CarboMaterial> SortedMatList = materialList.OrderBy(o => o.ECI).ToList();

            int low = 0; //l0wst value
            int high = SortedMatList.Count - 1; //highest index

            foreach (CarboGroup grp in getGroupList)
            {
                CarboMaterial material = grp.Material;
                int index = SortedMatList.IndexOf(material);

                System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, index);

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    cel.r = groupColour.R;
                    cel.g = groupColour.G;
                    cel.b = groupColour.B;
                }
            }

            //   MessageBox.Show(list);
        }

        public void CreateGroupHeat()
        {
            string list = "List: " + Environment.NewLine;
            List<CarboGroup> groupList = getGroupList.ToList();
            //By Material
            List<CarboGroup> SortedgroupList = groupList.OrderBy(o => o.EC).ToList();

            /*
            foreach (CarboGroup group in SortedgroupList)
            {
                list += group.Category + " : " + group.EC + Environment.NewLine;
            }
*/
            double low = SortedgroupList[0].EC;
            double high = SortedgroupList[SortedgroupList.Count - 1].EC;

            foreach (CarboGroup grp in getGroupList)
            {
                System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, grp.EC);

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    cel.r = groupColour.R;
                    cel.g = groupColour.G;
                    cel.b = groupColour.B;
                }
            }

            //MessageBox.Show(list);
        }

        public void CreateGroupHeatNorm()
        {
            string list = "List: " + Environment.NewLine;
            List<CarboGroup> groupList = getGroupList.ToList();
            //By Material
            List<CarboGroup> SortedgroupList = groupList.OrderBy(o => o.EC).ToList();

            double low = 0;
            double high = SortedgroupList.Count -1;

            foreach (CarboGroup grp in getGroupList)
            {
                int index = SortedgroupList.IndexOf(grp);

                System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, index);

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    cel.r = groupColour.R;
                    cel.g = groupColour.G;
                    cel.b = groupColour.B;
                }
            }

            //MessageBox.Show(list);
        }


        public void CreateElementHeat()
        {
            //string list = "List: " + Environment.NewLine;
            List<CarboGroup> groupList = getGroupList.ToList();
            List<CarboElement> allElements = new List<CarboElement>();

            //Write all EC into the elements:
            foreach (CarboGroup group in groupList)
            {
                foreach (CarboElement carEl in group.AllElements)
                {
                    carEl.EC = (carEl.Volume * group.Density * group.ECI);
                    allElements.Add(carEl);
                }
            }

            List<CarboElement> sortedElements = allElements.OrderBy(o => o.EC).ToList();

            double low = sortedElements[0].EC;
            double high = sortedElements[sortedElements.Count - 1].EC;

            foreach (CarboGroup grp in getGroupList)
            {
                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, cel.EC);

                    cel.r = groupColour.R;
                    cel.g = groupColour.G;
                    cel.b = groupColour.B;
                }
            }
            
            //MessageBox.Show(list);

        }
        public void CreateElementHeatNorm()
        {
            //string list = "List: " + Environment.NewLine;
            List<CarboGroup> groupList = getGroupList.ToList();
            List<CarboElement> allElements = new List<CarboElement>();

            //Write all EC into the elements:
            foreach (CarboGroup group in groupList)
            {
                foreach (CarboElement carEl in group.AllElements)
                {
                    carEl.EC = (carEl.Volume * group.Density * group.ECI);
                    allElements.Add(carEl);
                }
            }

            List<CarboElement> sortedElements = allElements.OrderBy(o => o.EC).ToList();

            double low = 0;
            double high = sortedElements.Count -1;

            foreach (CarboGroup grp in getGroupList)
            {
                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    int index = sortedElements.IndexOf(cel);

                    System.Drawing.Color groupColour = Utils.GetBlendedColor(high, low, index);

                    cel.r = groupColour.R;
                    cel.g = groupColour.G;
                    cel.b = groupColour.B;
                }
            }

            //MessageBox.Show(list);

        }

        private List<CarboMaterial> getUsedmaterials()
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
            double totalA5 = A5;
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
            double totalA5 = A5;
            double totalTotal = totalMaterials + totalA5;

            if (materials == true)
                result += "Total, material specific: " + Math.Round(totalMaterials, 2) + " MtCO2e " + Environment.NewLine;
            if (globals == true)
                result += "Total, global project specific (A5): " + Math.Round(totalA5, 2) + " MtCO2e" + Environment.NewLine;
            
            if(totalA5 == 0)
                result += "(No project value to base A5 emissions )" + Environment.NewLine;

            result += "Total: " + Math.Round(totalTotal, 2) + " MtCO2e (Metric tons of carbon dioxide equivalent)" + Environment.NewLine + Environment.NewLine;
            if (Area > 0)
            {
                result += "Total: " + Math.Round(totalTotal / Area, 2) + " MtCO2e/m² (Metric tons of carbon dioxide equivalent)" + Environment.NewLine;
            }

            result += Environment.NewLine;

            if (materials == true)
                result += "This equals to: " + Math.Round(totalTotal / 68.5, 2) + " average car emission per year. (UK)" + Environment.NewLine + Environment.NewLine;
            if (materials == true)
                result += "This requires " + Math.Round(totalTotal / 0.0217724, 0) + " trees to exists a year" + Environment.NewLine;

            double socialcost = (totalTotal * SocialCost);

            result += Environment.NewLine;
            result += "The Social Carbon Costs are: " + Math.Round(socialcost, 2) + " $/£/€ total" + Environment.NewLine + Environment.NewLine;


            return result;
        }

        public void CalculateProject()
        {
            //EE = 0;
            
            EC = 0;
            //This Will calculate all totals;
            foreach(CarboGroup cg in groupList)
            {
                cg.CalculateTotals();

                //EE += cg.EE;
                EC += cg.EC;
                
            }
            foreach (CarboGroup cg in groupList)
            {
                cg.SetPercentageOf(EC);
            }

            //Set A5 based on value;
            //1.400tCO2e/£100k
            A5 = 1.400 * (Value / 100000);
                       
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
            foreach(CarboGroup cg in groupList)
            {
                // Update selected group per se. 
                // 
                if(cg.Id == TargetGroup.Id)
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
