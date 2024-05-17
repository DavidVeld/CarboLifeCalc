using System;
using System.Collections.Generic;
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
    public class CarboCrocSolver : GH_Component
    {
        // Methods
        public CarboCrocSolver()
        : base("Project Element Solver", "Carbo Life Project Solver by Elements", "Generates a Carbo Life Project using Elements", "CarboCroc", "Solvers")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Elements", "CE", "Carbo Elements", GH_ParamAccess.list);
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
            List<bool> switches = new List<bool>(); ;

            List<CarboElement> listOfElements = new List<CarboElement>();

            CarboProject runtimeProject = new CarboProject();

            //Get the save as.. path :
            //DA.GetData<string>(1, ref path);
            //Get the switches
            bool okSwitches = DA.GetDataList(1, switches);
                        

            //Get the data
            if (DA.GetDataList(0, provided_as_goo))
            {
                foreach (var goo in provided_as_goo)
                {
                    var obj = goo.Value;
                    CarboElement ce = obj as CarboElement;
                    if (ce != null)
                    {
                        listOfElements.Add(ce);
                    }
                }
            }

            if (listOfElements.Count != 0)
            {
                runtimeProject = CarboCrocProcess.ProcessData(listOfElements, switches);
            }

            List<CarboDataPoint> list = runtimeProject.getPhaseTotals();

            //double totals = runtimeProject.getTotalEC();
            double totals = 0;


            foreach (CarboDataPoint cdp in list)
            {
                totals += cdp.Value;
            }

            //double totals = runtimeProject.getTotalEC();

            foreach(CarboDataPoint cdp in list)
                resultList.Add(cdp.Name + ";" + cdp.Value.ToString());
            /*
            double a13Total = list[0].Value;
            double a4Total = list[1].Value;
            double a5Total = list[2].Value;
            double a5Global = list[3].Value;

            double B16Total = list[4].Value;
            double CTotal = list[5].Value;
            double CGlobal = list[6].Value;

            double DTotal = list[7].Value;
            double Seq = list[8].Value;
            double Other = list[9].Value;

            //Build a stringlist
            resultList.Add("A1-A3," + a13Total.ToString());
            resultList.Add("A4," + a4Total.ToString());
            resultList.Add("A5," + (a5Total + a5Global).ToString());
            resultList.Add("B," + B16Total.ToString());
            resultList.Add("C," + (CTotal + CGlobal).ToString());
            resultList.Add("D," + DTotal.ToString());
            resultList.Add("Other," + Other.ToString());
            resultList.Add("Seq," + Seq.ToString());
            */

            DA.SetData(0, totals); //Totals
            DA.SetData(1, runtimeProject);
            DA.SetDataList(2, resultList);


        }
        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{305B0190-EB88-44A6-88D1-FC799F4BF995}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.ElementSolver;
            }
        }
    }
}
