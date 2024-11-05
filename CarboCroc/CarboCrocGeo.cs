using System;
using System.Drawing;
using CarboLifeAPI;
using Grasshopper.Kernel;
using System.Windows;
using CarboLifeAPI.Data;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace CarboCroc
{
    public class CarboCrocGeo : GH_Component
    {
        // Methods
        public CarboCrocGeo()
        : base("Carbo Life Geo Element", "Carbo Life Geo Element", "Build a Carbo Life Element from Brep", "CarboCroc", "Builder")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Id/GUID", "Id", "Identifier", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "Element Name", "Element Name", GH_ParamAccess.item);
            pManager.AddTextParameter("MaterialName", "Material", "Material Name", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geometry", "Geometry With Volume", GH_ParamAccess.item);
            pManager.AddTextParameter("Category", "Category", "Category", GH_ParamAccess.item);
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
            //double volume = 0;
            string category = "";
            string guid = "";

            try
            {
                IGH_GeometricGoo geometry = null;

                CarboElement result = new CarboElement();

                DA.GetData<int>(0, ref id);
                DA.GetData<string>(1, ref name);
                DA.GetData<string>(2, ref materialname);
                DA.GetData(3, ref geometry);
                DA.GetData<string>(4, ref category);

                IGH_GeometricGoo ghGeo = geometry as IGH_GeometricGoo;
                Brep brep = null;

                if (ghGeo != null)
                {
                    GH_Brep gh_brep = geometry as GH_Brep;
                    if (gh_brep != null)
                    {
                        brep = gh_brep.Value;
                    }
                }
                if(brep != null)
                { 
                    VolumeMassProperties mp = null;

                    //calc Geo
                    mp = VolumeMassProperties.Compute(brep, true, false, false, false);

                    result.Id = id;
                    result.Name = name;
                    result.MaterialName = materialname;
                    result.Category = category;
                    result.GUID = geometry.ReferenceID.ToString();

                    if (mp != null)
                    {
                        result.Volume = mp.Volume;
                    }
                    else
                    { 
                        result.Volume = 0;
                    }

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
                return new Guid("{9C57512A-5161-4D09-835E-811E7E52077C}");
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
