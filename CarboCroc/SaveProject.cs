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
    public class SaveProject : GH_Component
    {
        // Methods
        public SaveProject()
        : base("Save a Carbo Life Project", "Save Project", "Saves the Carbo Life Project to a specified path", "CarboCroc", "Solvers")
        {
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{8A9D1559-8099-47B6-BB26-7BB01B59D9C7}");
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "CP", "Carbo Life Project", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Save As.. Path?", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save", "Save", "A Switch", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("Update", "Path", "Save As.. Path?", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Message", "Message", "Error Messages to mind");//9
            //pManager.Register_BooleanParam("Saved", "Sucess", "Returns true if file was saved");//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper provided_as_goo = null;
            List<CarboElement> listOfElements = new List<CarboElement>();
            string path = "";
            bool saveme = false;

            bool ok = false;
            string errorMessage = "";

            DA.GetData<string>(1, ref path);
            DA.GetData<bool>(2, ref saveme);

            try
            {
                DA.GetData<GH_ObjectWrapper>(0, ref provided_as_goo);
                if (provided_as_goo != null)
                {
                    if (provided_as_goo.Value is CarboProject)
                    {
                        CarboProject project = provided_as_goo.Value as CarboProject;
                        if (project != null && saveme == true)
                        {
                            project.SerializeXML(path);
                            errorMessage = "Project saved to " + path;
                            ok = true;
                        }
                        else
                        {
                            errorMessage = "Project not Saved";
                        }
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
            //DA.SetData(1, ok); //Totals


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
