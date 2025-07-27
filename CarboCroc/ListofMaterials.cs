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
using System.IO;

namespace CarboCroc
{
    public class ListofMaterials : GH_Component
    {
        public ListofMaterials()
: base("List Materials", "List all Materials", "Lists the available Carbo Life Materials", "CarboCroc", "Data")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
          // pManager.AddTextParameter("Material Template", "Material Template", "Material Template (WIP)", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("List", "L", "Returns the project material names", GH_ParamAccess.list);//9
            //pManager.Register_StringParam("List2", "L2", "Returns the project material names");//9

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                string templatePath = "";

                templatePath = CarboCrocUtils.getSetTemplatePath("");

                CarboProject CP = new CarboProject(templatePath);
                CarboDatabase DB = CP.CarboDatabase;

                List<string> listofCarboMaterials = new List<string>();

                foreach (CarboMaterial CM in DB.CarboMaterialList)
                {
                    listofCarboMaterials.Add(CM.Name);
                    // DA.SetData(1, CM.Name);
                }

                listofCarboMaterials.Sort(StringComparer.InvariantCultureIgnoreCase);

                if (listofCarboMaterials.Count > 0)
                    DA.SetDataList(0, listofCarboMaterials);
            }
            catch (Exception ex)
            {
                DA.SetData(0, ex.Message);
            }

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{992D35B8-12B0-4E8A-A9B8-62CA03DDAC59}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.List;
            }
        }

    }
}
