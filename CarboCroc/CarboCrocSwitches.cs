using System;
using System.Drawing;
using CarboLifeAPI;
using Grasshopper.Kernel;
using System.Windows;
using CarboLifeAPI.Data;
using System.Collections.Generic;

namespace CarboCroc
{
    public class CarboCrocSwitches : GH_Component
    {
        // Methods
        public CarboCrocSwitches()
        : base("Carbo Life Switches", "Carbo Life Switches", "Build a Carbo Life Switches", "CarboCroc", "Builder")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("A1-A3", "A1A3", "Fabrication", GH_ParamAccess.item);
            pManager.AddBooleanParameter("A4", "A4", "Transport", GH_ParamAccess.item);
            pManager.AddBooleanParameter("A5", "A5", "Construction", GH_ParamAccess.item);
            pManager.AddBooleanParameter("B1-B6", "B1B6", "InUse", GH_ParamAccess.item);
            pManager.AddBooleanParameter("C1-C4", "C1C4", "Deconstruction", GH_ParamAccess.item);
            pManager.AddBooleanParameter("D", "D", "Out Of Scope", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Seq", "Seq", "Sequestration", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Extra", "E", "Additional", GH_ParamAccess.item);


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Message", "M", "Internal Messages");
            pManager.Register_BooleanParam("Carbo Switches", "CS", "A List of Carbo Life Boolean Switches",GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string error = "";
            List<bool> result = new List<bool>();
            bool a13 = true;
            bool a4 = true;
            bool a5 = true;
            bool b = true;
            bool c = true;
            bool d = false;
            bool s = true;
            bool extra = false;

            try
            {

                DA.GetData<bool>(0, ref a13);
                DA.GetData<bool>(1, ref a4);
                DA.GetData<bool>(2, ref a5);
                DA.GetData<bool>(3, ref b);
                DA.GetData<bool>(4, ref c);
                DA.GetData<bool>(5, ref d);
                DA.GetData<bool>(6, ref s);
                DA.GetData<bool>(7, ref extra);

                result.Add(a13);
                result.Add(a4);
                result.Add(a5);
                result.Add(b);
                result.Add(c);
                result.Add(d);
                result.Add(s);
                result.Add(extra);


            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            DA.SetData(0, error);
            DA.SetDataList(1, result.ToArray());
        }
        
        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{FEC0EE1B-BE36-44F8-99A9-A2F8E3360928}");
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
