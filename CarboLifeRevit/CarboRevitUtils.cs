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
        public static CarboElement getNewCarboElement(Document doc, Element el, ElementId materialIds, int i)
        {

            CarboElement newCarboElement = new CarboElement();
            try
            {
               // Material material = doc.GetElement(materialIds) as Material;

                newCarboElement.Id = el.Id.IntegerValue;
                newCarboElement.MaterialName = doc.GetElement(materialIds).Name.ToString();

                if (el.Category.Name != null)
                {
                    newCarboElement.Category = el.Category.Name;
                }

                /*
                Phase elPhase = doc.GetElement(el.CreatedPhaseId) as Phase;
                if (elPhase != null)
                {
                    newCarboElement.PhaseCreated = elPhase.Name;
                }

                Phase elDemoPhase = doc.GetElement(el.DemolishedPhaseId) as Phase;
                if (elDemoPhase != null)
                {
                    newCarboElement.Demolished = true;
                }
                else
                {
                    newCarboElement.Demolished = false;
                }
                */

                CarboMaterial carboMaterial = new CarboMaterial();
                carboMaterial.Name = newCarboElement.MaterialName;

                //GetDensity
                Parameter paramMaterial = el.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (paramMaterial != null)
                {
                    Material material = doc.GetElement(paramMaterial.AsElementId()) as Material;
                    if (material != null)
                    {
                        PropertySetElement property = doc.GetElement(material.StructuralAssetId) as PropertySetElement;
                        if (material != null)
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

                newCarboElement.material = carboMaterial;

                double volumeCubicFt = el.GetMaterialVolume(materialIds);
                newCarboElement.Volume = Utils.convertToCubicMtrs(volumeCubicFt);

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
