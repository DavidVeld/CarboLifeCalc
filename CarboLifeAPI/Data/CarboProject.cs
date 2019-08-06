using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

            Name = "New Project";
            Number = "00000";
            Category = "";
            Description = "New Project";
        }
        public void CreateGroups()
        {
            this.groupList = CarboElementImporter.CreateNewGroupLists(this.elementList, CarboDatabase,"");
        }
        public void CalculateProject()
        {
            
            EE = 0;
            EC = 0;
            //This Will calculate all totals;
            foreach(CarboGroup cg in groupList)
            {
                cg.CalculateTotals();

                EE += cg.EE;
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
                carboLifeElement.material = new CarboMaterial(materialName);

                elementList.Add(carboLifeElement);
            }

            groupList = CarboElementImporter.CreateNewGroupLists(getAllElements, CarboDatabase, "");

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

        public bool SerializeXML(string myPath)
        {
            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboProject));

                using (FileStream fs = new FileStream(myPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
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
        public void DeleteGroup(CarboGroup groupToDelete)
        {
            groupList.Remove(groupToDelete);
        }

        public void AddElement(CarboElement carboElement)
        {
            elementList.Add(carboElement);
        }
    }
}
