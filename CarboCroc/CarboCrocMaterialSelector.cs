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

namespace CarboCroc
{
    public class CarboCrocMaterialSelector : GH_Component
    {
        public CarboCrocMaterialSelector()
: base("Select Materials", "Select a CarboMaterial", "Selects the closest available Carbo Life Material", "CarboCroc", "Data")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Material Name", "Material Name", "Material Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Material Category", "Material Category", "Material Category", GH_ParamAccess.item,"");
            pManager.AddTextParameter("Material Grade", "Material Grade", "Material Grade", GH_ParamAccess.item, "");

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("CarboMaterial", "CM", "Returns a Carbo Material", GH_ParamAccess.item);//9
            pManager.Register_StringParam("CarboMaterial", "String", "Returns a Carbo Material as String");//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                string searchName = "";
                string searchCategory = "";
                string searchGrade = "";

                CarboProject CP = new CarboProject();
                CarboDatabase DB = CP.CarboDatabase;
                CarboMaterial CM = null;

                List<string> listofCarboMaterials = new List<string>();

                DA.GetData<string>(0, ref searchName);
                DA.GetData<string>(1, ref searchCategory);
                DA.GetData<string>(2, ref searchGrade);

                CM = DB.getClosestMatch(searchName,searchCategory,searchGrade);

                if (CM != null)
                {
                    DA.SetData(0, CM);
                    DA.SetData(1, CM.Name);
                }
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
                return new Guid("{FE52ABBA-C7C1-4486-94D6-47584C7AA906}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.QueryList;
            }
        }

    }
}
