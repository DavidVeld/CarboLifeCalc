using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CarboLifeRevit
{
    public static class CarboRevitUtils
    {
        /// <summary>
        /// This is the main function to form a carbo element based on a Revit Input.
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="el">Element</param>
        /// <param name="materialIds"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static CarboElement getNewCarboElement(Document doc, Element el, ElementId materialId ,CarboGroupSettings settings)
        {

            CarboElement result = new CarboElement();
            CarboElement newCarboElement = new CarboElement();

            if (materialId == null)
                return null;

                try
                {
                    newCarboElement = getNewElementProps(el, doc, settings);

                Material ImportedMaterial = null;
                string importedMaterialName = "";
                string importedMaterialCategoryName = "";

                double setVolume = 0;
                double setArea = 0;

                ImportedMaterial = doc.GetElement(materialId) as Material;

                //MaterialCategoryName

                //MaterialCategoryName
                if (ImportedMaterial != null )
                {
                    importedMaterialName = ImportedMaterial.Name.ToString();

                    if (ImportedMaterial.MaterialClass != "")
                    {
                        importedMaterialCategoryName = ImportedMaterial.MaterialClass;
                    }
                    else if(ImportedMaterial.MaterialCategory != "")
                    { 
                        importedMaterialCategoryName = ImportedMaterial.MaterialCategory;
                    }
                }
                //Volume
                double volumeCubicFt = el.GetMaterialVolume(materialId);
                setVolume = Utils.convertToCubicMtrs(volumeCubicFt);

                double area = el.GetMaterialArea(materialId,false);
                setArea = Utils.convertToSqreMtrs(area);

                //If it passed it matches all criteria:
                newCarboElement.MaterialName = importedMaterialName;
                newCarboElement.MaterialCategoryName = importedMaterialCategoryName;
                newCarboElement.Volume = Math.Round(setVolume, 4);
                newCarboElement.Area = Math.Round(setArea, 4);

            }
            catch
                {
                    return null;
                }

                if (newCarboElement != null && newCarboElement.Volume > 0)
                {
                    result = newCarboElement;
                }
            
                return result;
        }

        /// <summary>
        /// This is the main function to form a carbo element based on a Revit Input.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="el"></param>
        /// <param name="materialIds"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [Obsolete("This is a very slow method for some reason as it contructs a list for each item, so don't even think about using this")]
        internal static List<CarboElement> getNewCarboElement(Document doc, Element el, CarboGroupSettings settings)
        {
            
            List<CarboElement> result = new List<CarboElement>();

            ICollection<ElementId> MaterialIds = el.GetMaterialIds(false);
            if (MaterialIds == null || MaterialIds.Count <= 0)
                return null;

            foreach (ElementId materialIds in MaterialIds)
            {
                CarboElement newCarboElement = new CarboElement();

                try
                {
                    newCarboElement = getNewElementProps(el, doc, settings);

                    string setImportedMaterialName;
                    double setVolume;

                    //MaterialName
                    setImportedMaterialName = doc.GetElement(materialIds).Name.ToString();

                    //Volume
                    double volumeCubicFt = el.GetMaterialVolume(materialIds);
                    setVolume = Utils.convertToCubicMtrs(volumeCubicFt);

                    //If it passed it matches all criteria:
                    newCarboElement.MaterialName = setImportedMaterialName;
                    newCarboElement.Volume = Math.Round(setVolume, 4);
                }
                catch
                {
                    newCarboElement = null;
                }

                if (newCarboElement != null && newCarboElement.Volume > 0)
                {
                    result.Add(newCarboElement);
                }
            }

            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
                return null;

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
            string result = null;
            string testResult = null;

            try
            {

                Parameter carbonpar = null;

                if(searchString == "(Revit) Category")
                {
                    result = el.Category.Name.ToString();
                }
                else if (searchString == "Type Parameter")
                {
                    carbonpar = type.LookupParameter(paramName);

                    if (carbonpar != null)
                    {
                        if (carbonpar.StorageType == StorageType.String)
                            testResult = carbonpar.AsString();
                    }
                }
                else
                {
                    carbonpar = el.LookupParameter(paramName);

                    if (carbonpar != null)
                    {
                        if (carbonpar.StorageType == StorageType.String)
                            testResult = carbonpar.AsString();
                    }
                }
                //empty parameters should be seen as null
                if (testResult == "")
                    testResult = null;

                //if no category could be found make sure the revit category is used:
                if(testResult != null)
                    result = testResult;
                else
                    result = el.Category.Name.ToString();

            }
            catch (Exception ex)
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
                        bool isValidCategory = ValidCategory(el);

                        if(isValidCategory == true)
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
        private static bool ValidCategory(Element el)
        {
            bool result = true;

            BuiltInCategory enumCategory = (BuiltInCategory)el.Category.Id.IntegerValue;
            
            if(enumCategory == BuiltInCategory.OST_StairsRuns)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// A method to extract ALL geometry from an element when it is valid but data is not ready availble such as Railings & Ramp Families
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="el">Element TO scoop</param>
        /// <param name="settings">CarboGroupSettings</param>
        /// <returns> List<CarboElement> with geometry data</returns>
        internal static List<CarboElement> getGeometryElement(Document doc, Element el, CarboGroupSettings settings)
        {
            List<CarboElement> result = new List<CarboElement>();

            try
            {

                Options opt = new Options();
                opt.DetailLevel = ViewDetailLevel.Fine;

                List<Solid> lisofGeos = new List<Solid>();
                
                //First Try High Level:
                lisofGeos = el.get_Geometry(opt).OfType<Solid>().Where(s => s.Volume > 0).ToList();


                double volumeCheck = 0;
                if (lisofGeos.Count > 0)
                    volumeCheck = lisofGeos[0].Volume;

                //if no volume or no solids where found, try a deeper snoop.
               if (volumeCheck == 0)
                    lisofGeos = el.get_Geometry(opt).OfType<GeometryInstance>().SelectMany(g => g.GetInstanceGeometry().OfType<Solid>().Where(s => s.Volume > 0)).ToList();
                

                if (lisofGeos == null || lisofGeos.Count <= 0)
                    return null; //this is not a valid item


                foreach (Solid solidShape in lisofGeos)
                {
                    string materialName = "";
                    string materialCategory = "";

                    double volume = 0;

                    if (solidShape.Volume > 0)
                    {
                        volume = solidShape.Volume;

                        FaceArray array = solidShape.Faces;
                        PlanarFace face = array.get_Item(0) as PlanarFace;
                        Material faceMat = null;

                        ///Check the material of this element;
                        if (face != null)
                        {
                            ElementId eid = face.MaterialElementId;
                            
                            faceMat = doc.GetElement(eid) as Material;
                            if (faceMat != null)
                            {
                                materialName = faceMat.Name;

                                if (faceMat.MaterialClass != "")
                                    materialCategory = faceMat.MaterialClass;
                                else if(faceMat.MaterialCategory != "")
                                    materialCategory = faceMat.MaterialCategory;
                            }
                        }

                        if (materialName != "" && volume > 0 && faceMat != null)
                            result = CarboRevitUtils.addToCarboElement(result, materialName, materialCategory, volume, el, doc, settings);

                    }
                }
                foreach (CarboElement cel in result)
                {
                    cel.Volume = Utils.convertToCubicMtrs(cel.Volume);
                }
                return result;
            }
            catch (Exception ex)
            {

            }

             return null;

        }

        private static List<CarboElement> addToCarboElement(List<CarboElement> result, string materialName,string materialCategoryName, double volume, Element el, Document doc, CarboGroupSettings settings)
        {
            if(result.Count > 0)
            {
                foreach(CarboElement carboElement in result)
                {
                    if (carboElement.MaterialName == materialName)
                    {
                        carboElement.Volume += volume;
                        return result;
                    }
                }
            }

            //first item or not found; new element added to list;
            CarboElement newElement = getNewElementProps(el, doc, settings);

            if (newElement != null)
            {
                newElement.MaterialName = materialName;
                newElement.Volume = volume;

                result.Add(newElement);
            }

            return result;

        }

        /// <summary>
        /// Extracts all data from a revit element apart from volume and material.
        /// </summary>
        /// <param name="el">Element</param>
        /// <param name="doc">Document</param>
        /// <param name="settings">CarboGroupSettings</param>
        /// <returns>CarboElement with no volume or material</returns>
        private static CarboElement getNewElementProps(Element el, Document doc, CarboGroupSettings settings)
        {
            CarboElement newCarboElement = new CarboElement();
            try
            {
                int setId;
                string setGUID = "";
                string setName;
                string setCategory;
                string setSubCategory;
                string additionalParameter;
                double setLevel;
                string levelname;
                bool setIsDemolished;
                bool setIsSubstructure;
                bool setIsExisting;
                //int layernr;
                //new ones to add:
                string grade;
                double rcDensity;
                string correction;
                
                double area;

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

                //Is demolished
                if (setIsDemolished == true)
                {
                    if (settings.IncludeDemo == false)
                        return null; //don't make a element
                }

                //Is existing and retained
                if (setIsExisting == true)
                {
                    if (settings.IncludeExisting == false)
                        return null; //don't make a element
                }

                //Id:
                setId = el.Id.IntegerValue;
                setGUID = el.UniqueId;

                //Name (Type)
                ElementId elId = el.GetTypeId();
                ElementType type = doc.GetElement(elId) as ElementType;
                setName = type.Name;

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
                setCategory = getCategoryValue(el, type, settings.CategoryName, doc, settings.CategoryParamName);

                //SubCategory (Not used at the moment)
                setSubCategory = "";

                //Get Additional Parameter:
                additionalParameter = getParametervalue(el, settings.AdditionalParameterElementType,settings.AdditionalParameter);

                grade = getParametervalue(el, settings.GradeParameterType, settings.GradeParameterName);
                rcDensity = Utils.ConvertMeToDouble(getParametervalue(el, settings.RCParameterType, settings.RCParameterName));
                correction = getParametervalue(el, settings.CorrectionParameterType, settings.CorrectionParameterName);

                //Get the level (in meter)

                Level lvl = doc.GetElement(el.LevelId) as Level;
                if (lvl != null)
                {
                    levelname = lvl.Name;
                    setLevel = Convert.ToDouble((lvl.Elevation) * 304.8);
                }
                else
                {
                    Parameter levelParam = el.LookupParameter("Reference Level");
                    Parameter levelValueParam = el.LookupParameter("Reference Level Elevation");


                    if (levelParam != null && levelValueParam != null)
                    {
                        levelname = levelParam.AsValueString();
                        setLevel = Convert.ToDouble((levelValueParam.AsDouble()) * 304.8);
                    }
                    else
                    { 
                    setLevel = 0;
                    levelname = "";
                    }
                }



                //Makepass;

                //Is Substructure
                setIsSubstructure = false;

                if (settings.SubStructureParamType.ToLower().Contains("workset"))
                {
                    Parameter worksetvalue = el.LookupParameter("Workset");

                    if(worksetvalue != null)
                    {
                        string worksetName = worksetvalue.AsValueString().ToLower();

                        if (worksetName.Contains(settings.SubStructureParamName.ToLower()))
                            {
                            //substructure workset
                            setIsSubstructure = true;
                        }
                    }
                }
                else
                {
                    //Check a boolean parameter
                    Parameter substructParam = el.LookupParameter(settings.SubStructureParamName);
                    if (substructParam != null)
                    {
                        if (substructParam.StorageType == StorageType.Integer)
                        {
                            if (substructParam.AsInteger() == 1)
                                setIsSubstructure = true;
                        }
                    }
                }


                //If it passed it matches all criteria:
                newCarboElement.Id = setId;
                newCarboElement.GUID = setGUID;
                newCarboElement.Name = setName;
                newCarboElement.Category = setCategory;
                newCarboElement.SubCategory = setSubCategory;
                newCarboElement.LevelName = levelname;
                newCarboElement.Level = Math.Round(setLevel, 3);

                newCarboElement.isDemolished = setIsDemolished;
                newCarboElement.isExisting = setIsExisting;
                newCarboElement.isSubstructure = setIsSubstructure;
                newCarboElement.includeInCalc = true;

                newCarboElement.AdditionalData = additionalParameter;
                newCarboElement.Grade = grade;
                newCarboElement.rcDensity = rcDensity;
                newCarboElement.Correction = correction;

                return newCarboElement;
            }
            catch
            {
                return null;
            }
        }

        private static string getParametervalue(Element el, string gradeParameterType, string gradeParameterName)
        {
            string result = "";

            try
            {
                if (gradeParameterName != "" && gradeParameterType != "")
                {
                    Parameter parameterToSnoop = null;

                    if (gradeParameterType.ToLower().Contains("type"))
                    {
                        //use a type parameter
                        ElementId elId = el.GetTypeId();
                        ElementType type = el.Document.GetElement(elId) as ElementType;

                        parameterToSnoop = type.LookupParameter(gradeParameterName);
                    }
                    else
                    {
                        //Is Instance Parameter
                        parameterToSnoop = el.LookupParameter(gradeParameterName);
                    }

                    if (parameterToSnoop != null)
                    {
                        if (parameterToSnoop.StorageType == StorageType.String)
                            result = parameterToSnoop.AsString();
                        else
                            result = parameterToSnoop.AsValueString();
                        
                        if(result != null)
                            return result;
                    }
                    else
                    {
                        result = "";
                    }

                }
                else
                {
                    //not required / no value;
                    return "";
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return "";
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            if (Application.Current != null)
            {
                return string.IsNullOrEmpty(name)
                   ? Application.Current.Windows.OfType<T>().Any()
                   : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
            }
            else
                return false;
        }
        

    }
}
