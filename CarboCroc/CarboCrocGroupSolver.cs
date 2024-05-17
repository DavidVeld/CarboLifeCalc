using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace CarboCroc
{
    public class CarboCrocGroupSolver : GH_Component
    {
        // Methods
        public CarboCrocGroupSolver()
        : base("Project Group Solver", "Carbo Life Group Project Solver", "Generates a Carbo Life Project using Carbo Groups", "CarboCroc", "Solvers")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Groups", "CG", "Carbo Groups", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Switches", "CS", "Carbo Switches", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Total", "Total", "Total COe2"); //0
            pManager.Register_GenericParam("Carbo Project", "CP", "Returns the Carbo Project file");//9
            pManager.Register_StringParam("Message", "M", "Returns results in a string list", GH_ParamAccess.list);//10
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string errors = "";
            List<string> resultList = new List<string>();
            //string path = "";

            var provided_as_goo = new List<GH_ObjectWrapper>();
            List<CarboGroup> listOfGroups = new List<CarboGroup>();

            CarboProject runtimeProject = new CarboProject();

            List<bool> switches = new List<bool>(); ;

            //Get the save as.. path :
            //DA.GetData<string>(1, ref path);

            //Get the data
            Type carboGType = typeof(CarboGroup);
            //Type carboGType2 = listOfGroups[0].GetType();

            bool okSwitches = DA.GetDataList(1, switches);


            if (DA.GetDataList(0, provided_as_goo))
            {
                foreach (var goo in provided_as_goo)
                {
                    var obj = goo.ScriptVariable();

                    //dynamic cahngeob = Convert.ChangeType(obj, carboGType);

                    CarboLifeAPI.Data.CarboGroup cg = obj as CarboLifeAPI.Data.CarboGroup;
 
                    if (cg != null)
                    {
                        listOfGroups.Add(cg);
                    }
                }
            }

            if (listOfGroups.Count != 0)
            {
                runtimeProject = CarboCrocProcess.ProcessData(listOfGroups, switches);
            }

            List<CarboDataPoint> list = runtimeProject.getPhaseTotals();

            //double totals = runtimeProject.getTotalEC();
            double totals = 0;

            foreach (CarboDataPoint cdp in list)
            {
                totals += cdp.Value;
            }

            foreach (CarboDataPoint cdp in list)
                resultList.Add(cdp.Name + ";" + cdp.Value.ToString());

            DA.SetData(0, totals); //Totals
            DA.SetData(1, runtimeProject);
            DA.SetDataList(2, resultList);

        }
        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{735E875F-C89F-4D2B-B031-DF04520E1D1B}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.GroupSolver;
            }
        }
    }
}
