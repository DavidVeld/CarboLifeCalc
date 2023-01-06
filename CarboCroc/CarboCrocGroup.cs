using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Drawing;
using System.ComponentModel;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;

namespace CarboCroc
{
    public class CarboCrocGroup : GH_Component
    {
        public CarboCrocGroup()
: base("CarboGroup", "Create a CarboGroup", "Combines Multiple Materials into a CarboGroup", "CarboCroc", "Builder")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Element", "CE", "Carbo Elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Carbo Material", "CM", "Carbo Material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("CarboGroup", "CG", "Returns a Carbo Group");//9
            pManager.Register_StringParam("Message", "M", "Returns a summary");//9
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                string message = "";

                CarboGroup carboGroup = new CarboGroup();

                CarboMaterial carboMaterialToUse = null;

                var provided_as_goo = new List<GH_ObjectWrapper>();
                GH_ObjectWrapper carboMaterial_as_goo = null;

                List<CarboElement> listOfElements = new List<CarboElement>();

                //Get the Material
                bool hasmaterial = DA.GetData<GH_ObjectWrapper>(1, ref carboMaterial_as_goo);

                if (carboMaterial_as_goo != null)
                {
                    message += "A Carbo Material was provided" + Environment.NewLine;

                    if (carboMaterial_as_goo.Value is CarboMaterial)
                    {
                        CarboMaterial material = carboMaterial_as_goo.Value as CarboMaterial;
                        if (material != null)
                        {
                            carboMaterialToUse = material;
                        }
                    }
                }
                else
                {
                    //No valid input for a material was given. A empty group will be created.
                    message += "No material input was given, your results might not be accurate" + Environment.NewLine;
                }

                //material is set;
                if (carboMaterialToUse != null)
                {
                    carboGroup.Material = carboMaterialToUse;
                    carboGroup.MaterialName = carboMaterialToUse.Name;
                    message += "Carbo Material " + carboGroup.MaterialName  + " was applied to this group" +  Environment.NewLine;
                }

                //Get the Elements
                if (DA.GetDataList(0, provided_as_goo))
                {
                    foreach (var goo in provided_as_goo)
                    {
                        var obj = goo.Value;
                        CarboElement ce = obj as CarboElement;
                        if (ce != null)
                        {
                            listOfElements.Add(ce);
                        }
                    }
                }

                //Now Add the elements to the group
                foreach (CarboElement cel in listOfElements)
                {
                    carboGroup.AllElements.Add(cel);
                }

                //Give a nice description and name to the group
                if(listOfElements.Count > 0)
                {
                    carboGroup.Category = listOfElements[0].Category;
                    carboGroup.Description = "Group of: " + listOfElements[0].Name;
                    
                    //set default waste.
                    carboGroup.Waste = carboGroup.Material.WasteFactor;

                }

                message += listOfElements.Count + "Elements were added to this group" + Environment.NewLine;

                //All is good calculate the group and return;

                carboGroup.CalculateTotals();

                DA.SetData(0, carboGroup);
                DA.SetData(1, message);

            }
            catch (Exception ex)
            {
                DA.SetData(1, ex.Message);
            }

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{39989138-2344-4C0C-B6A9-022B7CB1BC95}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.CarboGroup;
            }
        }

    }
}
