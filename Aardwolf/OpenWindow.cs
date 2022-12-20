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
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Aardwolf
{
    public class OpenWindow : GH_Component
    {
        // Methods
        public OpenWindow()
        : base("Opens a Carbo Life Project", "Open Project", "Opends the Carbo Life Project", "Aardwolf", "Aardwolf")
        {
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5567D21C-AF71-40D2-9A76-45206B97C97B}");
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Project", "CP", "Carbo Life Project", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_BooleanParam("Saved", "Sucess", "Returns true if file was saved");//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper provided_as_goo = null;

            bool ok = false;
            string errorMessage = "";

            try
            {
                DA.GetData<GH_ObjectWrapper>(0, ref provided_as_goo);
                if (provided_as_goo != null)
                {
                    if (provided_as_goo.Value is CarboProject)
                    {
                        CarboProject project = provided_as_goo.Value as CarboProject;
                        if (project != null)
                        {
                            string title = "Carbo Life Project Viewer";

                            // check if the window is already open
                            bool isOpen = false;
                            foreach (Window win in System.Windows.Application.Current.Windows)
                            {
                                if (win.Title == title)
                                {
                                    isOpen = true;
                                    win.Tag = project;
                                    break;
                                }
                            }
                            if(isOpen == false)
                            {
                                OverViewViewer viewer = new OverViewViewer(project);
                                viewer.Show();
                            }
                            errorMessage = "File saved";
                            ok = true;
                        }
                        else
                        {
                            errorMessage = "Could not convert project from Goo";
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

            DA.SetData(1, ok); //Totals


        }
        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Aardwolf.Properties.Resources.SaveElement;
            }
        }



    }
}
