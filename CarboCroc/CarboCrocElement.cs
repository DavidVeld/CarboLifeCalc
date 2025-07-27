using System;
using System.Drawing;
using CarboLifeAPI;
using Grasshopper.Kernel;
using System.Windows;
using CarboLifeAPI.Data;

namespace CarboCroc
{
    public class CarboCrocElement : GH_Component
    {
        // Methods
        public CarboCrocElement()
        : base("CarboCrocElement", "CarboCrocElement", "Build a Carbo Life Element", "CarboCroc", "Builder")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Id", "Id", "Identifier", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Name", "Element Name", "Element Name", GH_ParamAccess.item, "");
            pManager.AddTextParameter("MaterialName", "Material", "Material Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Volume", "Volume", "Element Volume", GH_ParamAccess.item);
            pManager.AddTextParameter("Category", "Category", "Category", GH_ParamAccess.item, "");
            pManager.AddTextParameter("GUID", "GUID", "GUID (Rhino Element Id)", GH_ParamAccess.item, "");

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Carbo Element", "CE", "A List of Carbo Life Calculator elements, pass these in a project");
            pManager.Register_StringParam("Message", "M", "Internal Messages");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string error = "";
            int id = -999;
            string name = "";
            string materialname = "";
            double volume = 0;
            string category = "";
            string guid = "";

            try
            {
                CarboElement result = new CarboElement();

                DA.GetData<int>(0, ref id);
                DA.GetData<string>(1, ref name);
                DA.GetData<string>(2, ref materialname);
                DA.GetData<double>(3, ref volume);
                DA.GetData<string>(4, ref category);
                DA.GetData<string>(5, ref guid);

                if (volume != 0 && materialname != "")
                {
                    result.Id = id;
                    result.Name = name;
                    result.MaterialName = materialname;
                    result.Volume = volume;
                    result.Category = category;
                    result.GUID = guid;

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
                return new Guid("{9C57513A-5160-4D09-835C-816E7E52077C}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.NewElement;
            }
        }
    }
}
