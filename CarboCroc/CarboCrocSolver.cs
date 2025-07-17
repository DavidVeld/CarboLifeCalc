using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            pManager.AddTextParameter("TemplatePath", "TP", "Template Path", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Uncertainty", "U", "Uncertainty factor (Between 0 and 1)", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Total", "Total", "Total COe2"); //0
            pManager.Register_GenericParam("Carbo Project", "CP", "Returns the Carbo Project file");//9
            pManager.Register_StringParam("Message", "M", "Returns results in a string list", GH_ParamAccess.list);//10
            pManager.Register_StringParam("Result Text", "Txt", "Returns results in a text Message", GH_ParamAccess.item);//10

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string errors = "";
            List<string> resultList = new List<string>();

            //string path = "";
            string templatePath = "";
            bool oktemplatePath = DA.GetData(2, ref templatePath);
            double uncertainty = 0;
            bool okUncertainty = DA.GetData(3, ref uncertainty);

            CarboProject runtimeProject = null;

            //Get the save as.. path :
            //DA.GetData<string>(1, ref path);
            //Get the switches
            var provided_as_goo = new List<GH_ObjectWrapper>();
            List<bool> switches = new List<bool>(); ;

            List<CarboElement> listOfElements = new List<CarboElement>();

            bool okSwitches = DA.GetDataList(1, switches);

            string messageText = "";
                //uncertainty.ToString() + Environment.NewLine
            //    + templatePath + Environment.NewLine;
                

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

            //Create The Project;

            if (listOfElements.Count != 0)
            {
                runtimeProject = CarboCrocProcess.ProcessData(listOfElements, switches, uncertainty, templatePath);
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

            messageText += runtimeProject.getGeneralText();

            DA.SetData(0, totals); //Totals
            DA.SetData(1, runtimeProject);
            DA.SetDataList(2, resultList);
            DA.SetData(3, messageText);


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
