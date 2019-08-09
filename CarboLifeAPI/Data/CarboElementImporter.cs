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

        public static ObservableCollection<CarboGroup> GroupElementsAdvanced(ObservableCollection<CarboElement> carboElementList, 
            bool groupCategory, 
            bool groupSubCategory,
            bool groupType,
            bool groupMaterial, 
            bool groupSubStructure, 
            bool groupDemolition, 
            CarboDatabase materialData,
            string uniqueTypeNames = "")
        {
            ObservableCollection<CarboGroup> result = new ObservableCollection<CarboGroup>();

            foreach (CarboElement ce in carboElementList)
            {
                result = AddToCarboGroup(result, ce, groupCategory, groupSubCategory, groupType, groupMaterial, groupSubStructure, groupDemolition, uniqueTypeNames);
            }

            result = RefreshValues(result);
            result = mapGroupMaterials(result, materialData);

            return result;
        }

        private static ObservableCollection<CarboGroup> AddToCarboGroup(
            ObservableCollection<CarboGroup> carboGroupList, 
            CarboElement carboElement, 
            bool groupCategory, 
            bool groupSubCategory,
            bool groupType, 
            bool groupMaterial, 
            bool groupSubStructure, 
            bool groupDemolition, 
            string uniqueTypeNames)
        {
            int idbase = 1000;
            //define all constnts

            bool matchCategory = false;
            bool matchSubCategory = false;
            bool matchType = false;
            bool matchMaterial = false;
            bool matchSubStructure = false;
            bool matchDemolition = false;
            bool matchUniqueType = false;

            bool okCategory = false;
            bool okSubCategory = false;
            bool okType = false;
            bool okMaterial = false;
            bool okSubStructure = false;
            bool okDemolition = false;
            bool okUniqueType = false;

            //split 
            string[] uniquenamelist = uniqueTypeNames.Split(',');

            foreach (CarboGroup cg in carboGroupList)
            {
                //Find which conditions match
                //Category
                if (cg.Category == carboElement.Category)
                    matchCategory = true;
                //SubCategory
                if (cg.SubCategory == carboElement.SubCategory)
                    matchSubCategory = true;
                //TypeName
                if (cg.AllElements[0].Name == carboElement.Name)
                    matchType = true;
                //Material
                if (cg.MaterialName == carboElement.MaterialName)
                    matchMaterial = true;
                //Substructure
                if (cg.isSubstructure == carboElement.isSubstructure)
                    matchSubStructure = true;
                //Demoltion
                if (carboElement.isDemolished == true)
                    matchDemolition = true;
                //Uniquenames

                //TBC
                bool groupContainsUniqueTypename = false;

                foreach (string str in uniquenamelist)
                {
                    string str_trimmed = str.Trim();
                    string embededname = cg.AllElements[0].Name;
                    bool containsName = CaseInsensitiveContains(embededname, str_trimmed);
                    if (containsName == true)
                    {
                        //The elements in this group contain one of the words, see if they are identical
                        if(embededname == carboElement.Name)
                        {
                            //The item matched the qunique type group.
                            groupContainsUniqueTypename = true;
                        }
                        else
                        {
                            //This is not the group you are looking for
                            groupContainsUniqueTypename = false;
                        }
                    }
                    //Ignore switch:
                    //matchUniqueType = true;
                }
                //see if item contains unique groupname;

                //if all required conditions match requested then add, if not, create new.

                if (groupCategory == false)
                    okCategory = true;
                else
                {
                    if (matchCategory == true)
                        okCategory = true;
                    else
                        okCategory = false;
                }

                if (groupSubCategory == false)
                    okSubCategory = true;
                else
                {
                    if (matchSubCategory == true)
                        okSubCategory = true;
                    else
                        okSubCategory = false;
                }

                if (groupType == false)
                    okType = true;
                else
                {
                    if (matchType == true)
                        okType = true;
                    else
                        okType = false;
                }

                if (groupMaterial == false)
                    okMaterial = true;
                else
                {
                    if (matchMaterial == true)
                        okMaterial = true;
                    else
                        okMaterial = false;
                }

                if (groupSubStructure == false)
                    okSubStructure = true;
                else
                {
                    if (matchSubStructure == true)
                        okSubStructure = true;
                    else
                        okSubStructure = false;
                }

                if (groupDemolition == false)
                    okDemolition = true;
                else
                {
                    if (matchDemolition == true)
                        okDemolition = true;
                    else
                        okDemolition = false;
                }

                if(uniqueTypeNames == "")
                    okUniqueType = true;
                else
                {
                    if (matchUniqueType == true)
                        okUniqueType = true;
                    else
                        okUniqueType = false;
                }

                //If all passes add to group if not skip and create new group;
                if (
                    okCategory == true &&
                 okSubCategory == true &&
                 okType == true &&
                 okMaterial == true &&
                 okSubStructure == true &&
                 okDemolition == true &&
                 okUniqueType == true)
                {
                    cg.AllElements.Add(carboElement);
                    return carboGroupList;
                }
            }

            //NoCategoryWasFound: make new group
            int id = carboGroupList.Count + idbase;

            CarboGroup newGroup = new CarboGroup(carboElement);

            newGroup.Id = idbase + id;
            newGroup.MaterialName = carboElement.MaterialName;
            newGroup.Density = carboElement.Material.Density;
            newGroup.Description = "A new group";

            //newGroup.ECI = mappedMaterial.ECI;
            //newGroup.EEI = mappedMaterial.EEI;
            //newGroup.Volume = carboElement.Volume;

            carboGroupList.Add(newGroup);
            return carboGroupList;

        }

        private static bool CaseInsensitiveContains(string embededname, string str)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            return embededname.IndexOf(str, stringCompare) >= 0;
        }

        private static ObservableCollection<CarboGroup> mapGroupMaterials(ObservableCollection<CarboGroup> group, CarboDatabase materialData)
        {
            if (group.Count > 0)
            {
                foreach (CarboGroup cg in group)
                {
                    CarboMaterial closestGroupMaterial = materialData.getClosestMatch(cg.MaterialName);
                    //cg.MaterialName = closestGroupMaterial.Name;
                    cg.setMaterial(closestGroupMaterial);
                    cg.CalculateTotals();
                }
            }
            return group;
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

        private static ObservableCollection<CarboGroup> AddToCarboGroup(ObservableCollection<CarboGroup> carboGroupList, CarboElement carboElement, 
            bool hasPrimaryClass, bool hasSecondaryClass, bool hasSpecialRequests, bool hasSubstructure, bool hasDemolition)
        {
            //OBsolete
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
                        ce.SubCategory = carboElement.Category;
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
