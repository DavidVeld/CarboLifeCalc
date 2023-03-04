using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeRevit
{
    public static class CarboRevitUtils
    {
        /// <summary>
        /// This is the main function to form a carbo element based on a Revit Input.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="el"></param>
        /// <param name="materialIds"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static CarboElement getNewCarboElement(Document doc, Element el, ElementId materialIds, CarboGroupSettings settings)
        {

            CarboElement newCarboElement = new CarboElement();
            try
            {
                int setId;
                string setName;
                string setImportedMaterialName;
                string setCategory;
                string setSubCategory;
                double setVolume;
                double setLevel;
                bool setIsDemolished;
                bool setIsSubstructure;
                bool setIsExisting;
                //int layernr;

                // Material material = doc.GetElement(materialIds) as Material;
                //Id:
                setId = el.Id.IntegerValue;
                
                //Name (Type)
                ElementId elId = el.GetTypeId();
                ElementType type = doc.GetElement(elId) as ElementType;
                setName = type.Name;

                //MaterialName
                setImportedMaterialName = doc.GetElement(materialIds).Name.ToString();

                //CarboMaterial carboMaterial = new CarboMaterial(setMaterialName);
               
                //GetDensity
                Parameter paramMaterial = el.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (paramMaterial != null)
                {
                    Material material = doc.GetElement(paramMaterial.AsElementId()) as Material;
                    if (material != null)
                    {
                        PropertySetElement property = doc.GetElement(material.StructuralAssetId) as PropertySetElement;
                        if (property != null)
                        {
                            Parameter paramDensity = property.get_Parameter(BuiltInParameter.PHY_MATERIAL_PARAM_STRUCTURAL_DENSITY);
                            if (paramDensity != null)
                            {
                                double density = paramDensity.AsDouble();
                                newCarboElement.Density = density;
                            }
                        }
                    }
                }


                //Get the right Category name
                setCategory = getCategoryValue(el,type, settings.CategoryName, doc, settings.CategoryParamName);

                //SubCategory (Not used at the moment)
                setSubCategory = "";
                
                //Volume
                double volumeCubicFt = el.GetMaterialVolume(materialIds);
                setVolume = Utils.convertToCubicMtrs(volumeCubicFt);

                //Get the level (in meter)
                Level lvl = doc.GetElement(el.LevelId) as Level;
                if (lvl != null)
                {
                    setLevel = Convert.ToDouble((lvl.Elevation) * 304.8);
                }
                else
                {
                    setLevel = 0;
                }

                //Get Phasing;
                Phase elCreatedPhase = doc.GetElement(el.CreatedPhaseId) as Phase;
                Phase elDemoPhase = doc.GetElement(el.DemolishedPhaseId) as Phase;

                newCarboElement.isDemolished = false;

                if (elDemoPhase != null)
                {
                    setIsDemolished = true;
                }
                else
                {
                    setIsDemolished = false;
                }

                if (elCreatedPhase.Name == settings.ExistingPhaseName)
                {
                    setIsExisting = true;
                }
                else
                {
                    setIsExisting = false;
                }

                //Makepass;

                //Is demolished
                if (setIsDemolished == true)
                {
                    if(settings.IncludeDemo == false)
                        return null; //don't make a element
                }

                //Is existing and retained
                if (setIsExisting == true)
                {
                    if (settings.IncludeExisting == false)
                        return null; //don't make a element
                }


                //Is Substructure
                setIsSubstructure = false;

                Parameter substructParam = el.LookupParameter(settings.SubStructureParamName);
                if(substructParam != null)
                {
                    if (substructParam.StorageType == StorageType.Integer)
                    {
                        if (substructParam.AsInteger() == 1)
                            setIsSubstructure = true;
                    }
                }

                //If it passed it matches all criteria:
                newCarboElement.Id = setId;
                newCarboElement.Name = setName;
                newCarboElement.MaterialName = setImportedMaterialName;
                newCarboElement.Category = setCategory;
                newCarboElement.SubCategory = setSubCategory;
                newCarboElement.Volume = Math.Round(setVolume, 4);
                //newCarboElement.Material = carboMaterial; //Material removed
                newCarboElement.Level = Math.Round(setLevel,3);
                newCarboElement.isDemolished = setIsDemolished;
                newCarboElement.isExisting = setIsExisting;
                newCarboElement.isSubstructure = setIsSubstructure;
                newCarboElement.includeInCalc = true;

                if (newCarboElement.Volume != 0)
                {
                    return newCarboElement;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }


        /*            
            categorylist.Add("(Revit) Category");
            categorylist.Add("Type Parameter");
            categorylist.Add("Instance Parameter");
        */
        /// <summary>
        /// Returns the category value based on inputs
        /// </summary>
        /// <param name="el">the element</param>
        /// <param name="type">the element type</param>
        /// <param name="searchString">the parameter type</param>
        /// <param name="doc">document</param>
        /// <param name="paramName">the name of the category parameter</param>

        /// <returns> category value based on inputs</returns>
        private static string getCategoryValue(Element el, ElementType type, string searchString, Document doc, string paramName = "")
        {
            string result = "";
            try
            {
                if (searchString == "(Revit) Category")
                {
                    result = el.Category.Name;
                }
                else if (searchString == "Type Parameter")
                {
                    Parameter carbonpar = type.LookupParameter(paramName);
                    if (carbonpar != null)
                    {
                        if (carbonpar.StorageType == StorageType.String)
                            result = carbonpar.AsString();
                        else
                            result = "Wrong Category Type";
                    }
                    else
                    {
                        result = "No Category Found";
                    }
                }
                else if (searchString == "Instance Parameter")
                {
                    Parameter carbonpar = el.LookupParameter(paramName);

                    if (carbonpar != null)
                    {
                        if (carbonpar.StorageType == StorageType.String)
                            result = carbonpar.AsString();
                        else
                            result = "Wrong Category Type";
                    }
                    else
                    {
                        result = "No Category Found";
                    }
                }
                else
                {
                    result = el.Category.Name;
                }
            }
            catch(Exception ex)
            {
                result = "Export Error";

            }
            return result;
        }
       
        /// <summary>
        /// Validates the elements and it's class;
        /// </summary>
        /// <param name="el"></param>
        /// <returns>True if the element can be extracted</returns>
        public static bool isElementReal(Element el)
        {
            bool result = false;

            if (!(el is FamilySymbol || el is Family))
            {
                if (!(el.Category == null))
                {
                    if (el.get_Geometry(new Options()) != null)
                    {
                        //Check if not of any forbidden categories such as runs:
                        string Typename = el.Category.Name;
                        if(Typename != "Run")
                            result = true;
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

            List<string> nonValidClasses = new List<string>();
            //List of categories to exclude
            nonValidClasses.Add("Runs");
            nonValidClasses.Add("Ramps");

            string catName = el.Category.Name;

            if (nonValidClasses.Contains(catName))
                result = false;

            return result;
        }
    }
}
