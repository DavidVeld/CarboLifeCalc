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
        public static CarboElement getNewCarboElement(Document doc, Element el, ElementId materialIds, CarboRevitImportSettings settings)
        {

            CarboElement newCarboElement = new CarboElement();
            try
            {
                int setId;
                string setName;
                string setMaterialName;
                string setCategory;
                string setSubCategory;
                double setVolume;
                double setLevel;
                bool setIsDemolished;
                bool setIsSubstructure;
                bool setIsExisting;
                int layernr;

                // Material material = doc.GetElement(materialIds) as Material;
                //Id:
                setId = el.Id.IntegerValue;
                
                //Name (Type)
                ElementId elId = el.GetTypeId();
                ElementType type = doc.GetElement(elId) as ElementType;
                setName = type.Name;

                //MaterialName
                setMaterialName = doc.GetElement(materialIds).Name.ToString();
                CarboMaterial carboMaterial = new CarboMaterial(setMaterialName);
                


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
                                carboMaterial.Density = density;
                            }
                        }
                    }
                }


                //Category
                setCategory = getValueFromList(el,type, settings.MainCategory, doc);

                //SubCategory
                setSubCategory = getValueFromList(el, type, settings.SubCategory, doc);
                
                //Volume
                               
                double volumeCubicFt = el.GetMaterialVolume(materialIds);
                setVolume = Utils.convertToCubicMtrs(volumeCubicFt);

                newCarboElement.isDemolished = false;
                
                Level lvl = doc.GetElement(el.LevelId) as Level;
                if (lvl != null)
                {
                    setLevel = Convert.ToDouble((lvl.Elevation) * 304.8);
                }
                else
                {
                    setLevel = 0;
                }

                if (setLevel <= settings.CutoffLevelValue)
                    setIsSubstructure = true;
                else
                    setIsSubstructure = false;

                //Get Phasing;
                Phase elCreatedPhase = doc.GetElement(el.CreatedPhaseId) as Phase;
                Phase elDemoPhase = doc.GetElement(el.DemolishedPhaseId) as Phase;
                

                if (elDemoPhase != null)
                {
                    setIsDemolished = true;
                }
                else
                {
                    setIsDemolished = false;
                }

                if (elCreatedPhase.Name == "Existing")
                {
                    setIsExisting = true;
                }
                else
                {
                    setIsExisting = false;
                }

                //Makepass;

                //Is existing and retained
                if (setIsExisting == true && setIsDemolished == false)
                {
                    if (settings.IncludeExisting == false)
                        return null;
                }

                //Is demolished
                if (setIsDemolished == true)
                {
                    if(settings.IncludeDemo == false)
                        return null;
                }

                //If it passed it is either proposed, or demolished and retained.

                newCarboElement.Id = setId;
                newCarboElement.Name = setName;
                newCarboElement.MaterialName = setMaterialName;
                newCarboElement.Category = setCategory;
                newCarboElement.SubCategory = setSubCategory;
                newCarboElement.Volume = Math.Round(setVolume, 4);
                newCarboElement.Material = carboMaterial;
                newCarboElement.Level = Math.Round(setLevel,3);
                newCarboElement.isDemolished = setIsDemolished;
                newCarboElement.isExisting = setIsExisting;
                newCarboElement.isSubstructure = setIsSubstructure;
                             
                if (newCarboElement.Volume != 0)
                {
                    return newCarboElement;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //TaskDialog.Show("Error", ex.Message);
                return null;
            }

        }
        private static string getValueFromList(Element el, ElementType type, string searchString, Document doc)
        {
            string result = "";

            if (searchString == "Type Comment")
            {
                Parameter commentpar = type.LookupParameter("Type Comments");
                if (commentpar != null)
                    result = commentpar.AsString();
            }
            else if (searchString == "Family Name")
            {
                result = type.FamilyName;
            }
            else if (searchString == "")
            {
                result = "";
            }
            else if (searchString == "CarboLifeCategory")
            {
                Parameter carbonpar = type.LookupParameter("CarboLifeCategory");
                if (carbonpar != null)
                    result = carbonpar.AsString();
            }
            else if ( searchString == "Level")
            {
                Element lvlEl = doc.GetElement(el.LevelId);
                if (lvlEl != null)
                {
                    Level lvl = doc.GetElement(el.LevelId) as Level;
                    result = lvl.Name;
                }
                else
                {
                    result = "";
                }
            }
            else
            {
                result = el.Category.Name;
            }

            return result;

        }

        public static bool isElementReal(Element el)
        {
            bool result = false;

            if (!(el is FamilySymbol || el is Family))
            {
                if (!(el.Category == null))
                {
                    if (el.get_Geometry(new Options()) != null)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }
}
