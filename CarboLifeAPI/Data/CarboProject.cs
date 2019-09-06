using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        //Calculated Values
        public double EE { get; set; }
        public double EC { get; set; }
        public double Value { get; set; }
        public double Area { get; set; }
        public string filePath { get; set; }

        public List<CarboLevel> carboLevelList { get; set; }

        private ObservableCollection<CarboElement> elementList;
        private ObservableCollection<CarboGroup> groupList;

        public ObservableCollection<CarboElement> getAllElements
        {
            get { return elementList; }
        }
        public ObservableCollection<CarboGroup> getGroupList
        {
            get { return groupList; }
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
        }
        public void CreateGroups()
        {
            //get default group settings;
            CarboGroupSettings groupSettings = new CarboGroupSettings();
            groupSettings = groupSettings.DeSerializeXML();

            this.groupList = CarboElementImporter.GroupElementsAdvanced(this.elementList, groupSettings.groupCategory, groupSettings.groupSubCategory, groupSettings.groupType, groupSettings.groupMaterial, groupSettings.groupSubStructure, groupSettings.groupDemolition, CarboDatabase, groupSettings.uniqueTypeNames);
            CalculateProject();
        }

        public List<CarboDataPoint> getTotals(string value)
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();

            if(value == "Material")
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

            }

            //Values should return now;



            return valueList;
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

        public void CreateNewGroup()
        {
            CarboGroup newGroup = new CarboGroup();
            int id = getNewId();
            newGroup.Id = id;

            AddGroup(newGroup);
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
                    cg.Description = carboGroup.Description;
                    cg.Volume = carboGroup.Volume;
                    cg.SubCategory = carboGroup.SubCategory;
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
        public void AddGroup(CarboGroup newGroup)
        {
            if (!(groupList.Contains(newGroup)))
            {
                newGroup.Id = getNewId();
                groupList.Add(newGroup);
            }
            else
            {
                MessageBox.Show("new group already exists");
            }
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

            return result;
        }




    }
}
