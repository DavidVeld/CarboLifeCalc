using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    public static class CarboElementImporter
    {

        public static ObservableCollection<CarboGroup> CreateNewGroupLists(ObservableCollection<CarboElement> allElementsList, CarboDatabase materialData, string orderby = "Category")
        {
            ObservableCollection<CarboGroup> result = new ObservableCollection<CarboGroup>();

            //An element can allready exists (if Id existis in the group list. in the list; only volume and material will be updated;
            //
            foreach (CarboElement ce in allElementsList)
            {
                CarboMaterial elementMaterial = materialData.getClosestMatch(ce.material);
                
                result = AddToCarboGroup(result, ce, elementMaterial);
            }
            result = RefreshValues(result);

            return result;
        }

        private static ObservableCollection<CarboGroup> RefreshValues(ObservableCollection<CarboGroup> GroupList)
        {
            ObservableCollection<CarboGroup> result = GroupList;

            foreach (CarboGroup cg in result)
            {
                cg.RefreshValuesFromElements();
            }

            foreach (CarboGroup cg in result)
            {
                cg.RefreshValuesFromElements();
            }

            return result;
        }

        private static ObservableCollection<CarboGroup> AddToCarboGroup(ObservableCollection<CarboGroup> carboGroupList, CarboElement carboElement, CarboMaterial mappedMaterial)
        {
            int idbase = 1000;
            //Try Add
            foreach (CarboGroup cg in carboGroupList)
            {
                if(cg.Category == carboElement.Category)
                {
                    if (cg.MaterialName == mappedMaterial.Name)
                    {
                        cg.AllElements.Add(carboElement);
                        return carboGroupList;
                    }
                }
            }
            //NoCategoryWasFound: make new group
            int id = carboGroupList.Count;

            CarboGroup newGroup = new CarboGroup(carboElement);
            newGroup.Id = idbase;
            newGroup.Material = mappedMaterial;
            newGroup.MaterialName = mappedMaterial.Name;
            newGroup.Density = mappedMaterial.Density;
            //newGroup.ECI = mappedMaterial.ECI;
            //newGroup.EEI = mappedMaterial.EEI;
            //newGroup.Volume = carboElement.Volume;
            
            carboGroupList.Add(newGroup);
            return carboGroupList;

        }

        private static ObservableCollection<CarboGroup> UpdateElement(ObservableCollection<CarboGroup> carboGroupList, CarboElement carboElement)
        {

            foreach (CarboGroup cg in carboGroupList)
            {
                foreach (CarboElement ce in cg.AllElements)
                {
                    if (ce.Id == carboElement.Id)
                    {
                        if(ce.Category == carboElement.Category)
                        ce.Volume = carboElement.Volume;
                        ce.CarboCategory = carboElement.Category;
                    }
                    break;
                }
            }

            return carboGroupList;
        }

        private static bool ElementExists(ObservableCollection<CarboGroup> carboGroupList, CarboElement carboelement)
        {
            bool result = false;

            foreach(CarboGroup cg in carboGroupList)
            {
                foreach(CarboElement ce in cg.AllElements)
                {
                    if (ce.Id == carboelement.Id)
                        result = true;
                    break;
                }
            }

            return result;
        }

        public static string getRandomCategory(int value)
        {
            string category = "Other";


            switch (value)
            {
                case 1:
                    category = "Wall";
                    break;
                case 2:
                    category = "Floor";
                    break;
                case 3:
                    category = "Roof";
                    break;
                case 4:
                    category = "Structural Framing";
                    break;
                case 5:
                    category = "Structural Column";
                    break;
                default:
                    category = "Other";
                    break;
            }


            return category;
        }

        public static string getRandomMaterial(int value)

        {
            string material = "Other";

            switch (value)
            {
                case 1:
                    material = "Timber";
                    break;
                case 2:
                    material = "Steel";
                    break;
                case 3:
                    material = "Concrete";
                    break;
                case 4:
                    material = "Brick";
                    break;
                default:
                    material = "Other";
                    break;
            }


            return material;
        }
    }
}
