using CarboLifeAPI.Data;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCroc
{
    public class ImgProject : GH_Component
    {
        // Methods
        public ImgProject()
        : base("Img a Carbo Life Project", "Img Project", "Img the Carbo Life Project to a specified path", "CarboCroc", "Solvers")
        {
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{8A9D1555-8009-47B6-BB26-7BB00B54D9C7}");
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "CP", "Carbo Life Project", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Save As.. Path?", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Message", "Message", "Error Messages to mind");//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string errorMessage = "";

            GH_ObjectWrapper provided_as_goo = null;
            try
            {
                DA.GetData<GH_ObjectWrapper>(0, ref provided_as_goo);
                if (provided_as_goo != null)
                {
                    if (provided_as_goo.Value is CarboProject)
                    {
                        CarboProject project = provided_as_goo.Value as CarboProject;
                    }
                }
                else
                {
                    errorMessage = "Could not find project in input";
                }
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }
            DA.SetData(0, errorMessage); //Totals
        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.SaveProject;
            }
        }

    }
}
