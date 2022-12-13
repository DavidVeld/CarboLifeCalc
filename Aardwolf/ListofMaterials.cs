using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Drawing;

namespace Aardwolf
{
    public class ListofMaterials : GH_Component
    {
        public ListofMaterials()
: base("ListMaterials", "List all Materials", "Lists the available Carbo Life Materials", "Aardwolf", "Aardwolf")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("List", "L", "Returns the project material names",GH_ParamAccess.list);//9
            //pManager.Register_StringParam("List2", "L2", "Returns the project material names");//9

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                CarboProject CP = new CarboProject();
                CarboDatabase DB = CP.CarboDatabase;

                List<string> listofCarboMaterials = new List<string>();

                foreach (CarboMaterial CM in DB.CarboMaterialList)
                {
                    listofCarboMaterials.Add(CM.Name);
                   // DA.SetData(1, CM.Name);
                }

                if(listofCarboMaterials.Count > 0)
                    DA.SetDataList(0, listofCarboMaterials);
            }
            catch(Exception ex)
            {
                DA.SetData(0, ex.Message);
            }

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{52928281-BA4C-4A64-8B00-50108C895D73}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Aardwolf.Properties.Resources.QueryList;
            }
        }

    }
}
