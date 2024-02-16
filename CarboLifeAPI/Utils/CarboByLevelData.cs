using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CarboLifeAPI
{
    public class CarboByLevelDataGroup
    {
        public List<CarboByLevelData> levelList { get; set; }

        public CarboByLevelDataGroup() 
        {
            levelList = new List<CarboByLevelData>();
        }

        public void AddItem(string levelName, double elevation, string groupName, double value)
        {
            bool existsItem = false;

            foreach (CarboByLevelData cld in levelList)
            {
                if (cld.LevelName == levelName)
                {
                    //the level row was found; asses if a new category is needed:
                    foreach (CarboDataPoint data in cld.DataPoints)
                    {
                        if (data.Name == groupName)
                        {
                            data.Value += value;
                            existsItem = true;
                            break;
                        }
                    }
                    //the level was found, but not the datapoint; add new datapoint to this level:
                    if (existsItem == false)
                    {
                        CarboDataPoint newDp = new CarboDataPoint(groupName, value);
                        cld.DataPoints.Add(newDp);
                        break;
                    }
                }
            }


                foreach (CarboByLevelData cld in levelList)
                {
                    int index = cld.DataPoints.FindIndex(f => f.Name == groupName);
                    if(!(index >= 0))
                    {
                        CarboDataPoint newDp = new CarboDataPoint(groupName, 0);
                        cld.DataPoints.Add(newDp);
                    }
                }
            
        }

        private void AddLevel(string levelName, double elevation, string groupName, double value)
        {
            AddLevel(levelName, elevation);
            foreach (CarboByLevelData level in levelList)
            {
                if (level.LevelName == levelName)
                {
                    level.AddCategory(groupName, value);
                }
            }
        }

        public void AddLevel(string name, double elevation)
        {
            CarboByLevelData lvel = new CarboByLevelData();
            lvel.LevelName = name;
            lvel.Elevation = elevation;
            levelList.Add(lvel);
        }

        public void SortedList()
        {
            levelList = levelList.OrderBy(o => o.Elevation).ToList();
        }
    }

    public class CarboByLevelData
    {
        public List<CarboDataPoint> DataPoints { get; set; }
        public string LevelName { get; set; }
        public double Elevation { get; set; }

        public CarboByLevelData()
        {
            this.LevelName = "";
            this.Elevation = 0;
            this.DataPoints = new List<CarboDataPoint>();
        }

        public CarboByLevelData(string Name, double Value)
        {
            this.LevelName = Name;
            this.Elevation = Value;
            this.DataPoints = new List<CarboDataPoint>();
        }

        internal void AddCategory(string groupName, double value)
        {
            CarboDataPoint dp = new CarboDataPoint();
            dp.Name = groupName;
            dp.Value = value;
            DataPoints.Add(dp);
        }
    }
}
