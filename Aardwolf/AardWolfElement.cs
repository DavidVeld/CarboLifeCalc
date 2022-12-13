using System;
using System.Drawing;
using CarboLifeAPI;
using Grasshopper.Kernel;
using System.Windows;
using CarboLifeAPI.Data;

namespace AardWolf
{
    public class AardWolfElement : GH_Component
    {
        // Methods
        public AardWolfElement()
        : base("Carbo Life Element", "Carbo Life Element", "Build a Carbo Life Element", "Aardwolf", "Aardwolf")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Id/GUID", "Id", "Identifier", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "Element Name", "Element Name", GH_ParamAccess.item);
            pManager.AddTextParameter("MaterialName", "Material", "Material Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Volume", "Volume", "Element Volume", GH_ParamAccess.item);
            pManager.AddTextParameter("Category", "Category", "Category", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Carbo Element", "Carbo Element", "A List of Carbo Life Calculator elements, pass these in a project");
            pManager.Register_StringParam("Message", "Message", "Internal Messages");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string error = "";
            int id = -999;
            string name = "";
            string materialname = "";
            double volume = 0;
            string category = "";

            try
            {
                CarboElement result = new CarboElement();

                DA.GetData<int>(0, ref id);
                DA.GetData<string>(1, ref name);
                DA.GetData<string>(2, ref materialname);
                DA.GetData<double>(3, ref volume);
                DA.GetData<string>(4, ref category);

                if (volume != 0 && materialname != "")
                {
                    result.Id = id;
                    result.Name = name;
                    result.MaterialName = materialname;
                    result.Volume = volume;
                    result.Category = category;

                    DA.SetData(0, result);
                    error = "Ok";
                }
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }        
            
            DA.SetData(1, error);

        }
        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{B23086F8-D611-49E4-8803-1C0DE0EC6E10}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Aardwolf.Properties.Resources.NewElement;
            }
        }
    }
}
