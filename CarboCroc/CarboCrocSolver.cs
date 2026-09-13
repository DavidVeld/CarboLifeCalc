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
            pManager.AddGenericParameter("Carbo Elements", "CE", "Carbo Elements", GH_ParamAccess.list); //0
            pManager.AddBooleanParameter("Switches", "CS", "Carbo Switches", GH_ParamAccess.list); //1
            pManager.AddNumberParameter("Uncertainty", "U", "Uncertainty factor (between 0 and 1). Leave unset to use the factor from the Carbo Life import settings, which is the one the main application applies.", GH_ParamAccess.item, CarboCrocProcess.UncertaintyNotSupplied);//2
            pManager.AddNumberParameter("GIA", "GIA", "The GIA of the project in m2. Needed for A5 and for every per m2 result.", GH_ParamAccess.item, 0);//3
            pManager.AddGenericParameter("Allowances", "A", "Reinforcement and connection allowances from the Carbo Allowances component. Leave unset to use the Carbo Life import settings.", GH_ParamAccess.item);//4

            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Total", "Total", "Total COe2"); //0
            pManager.Register_GenericParam("Carbo Project", "CP", "Returns the Carbo Project file");//9
            pManager.Register_StringParam("Message", "M", "Returns results in a string list", GH_ParamAccess.list);//10
            pManager.Register_StringParam("Result Text", "TXT", "Returns results in a text Message", GH_ParamAccess.item);//10

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string errors = "";
            List<string> resultList = new List<string>();

            //string path = "";
            string templatePath = CarboCrocUtils.getSetTemplatePath("");

            double uncertainty = CarboCrocProcess.UncertaintyNotSupplied;
            bool okUncertainty = DA.GetData(2, ref uncertainty);

            double gia = 0;
            bool giaOk = DA.GetData(3, ref gia);

            //Without a GIA there is no A5 and nothing to divide the totals by, and both used to
            //come out silently - A5 as zero, the per m2 figures as NaN.
            if (gia <= 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "No GIA was given, so A5 is not calculated and the per m2 results are meaningless. " +
                    "Wire the project GIA, in m2, into the GIA input.");

            if (uncertainty > 1)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "The uncertainty factor is a fraction between 0 and 1, so 10% is 0.1. " +
                    uncertainty.ToString() + " reads as " + (uncertainty * 100).ToString() + "%.");

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

            //Optional, and the project falls back to the Carbo Life import settings without it.
            CarboCrocAllowanceSet allowances = null;
            GH_ObjectWrapper allowanceGoo = null;
            if (DA.GetData(4, ref allowanceGoo) && allowanceGoo != null)
            {
                allowances = allowanceGoo.Value as CarboCrocAllowanceSet;
                if (allowances == null)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "The Allowances input is not a Carbo Allowances output and was ignored.");
            }

            //Create The Project;

            if (listOfElements.Count != 0)
            {
                runtimeProject = CarboCrocProcess.ProcessData(listOfElements, switches, uncertainty, templatePath, gia, allowances);
            }
            else
            {
                //getPhaseTotals below used to be called on a null project, so a definition that
                //had not been given any elements yet threw a null reference instead of saying so.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "No Carbo Elements were supplied, so there is no project to calculate.");
                DA.SetData(3, "No Carbo Elements were supplied.");
                return;
            }

            //Nothing rewrites a material behind the definition any more, so a name the matcher
            //had to guess at is worth saying out loud. Exact names, which is what the material
            //list and the selector component hand out, never appear here.
            string materialReview = runtimeProject.getMaterialReviewSummary();
            if (string.IsNullOrEmpty(materialReview) == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, materialReview);
                messageText += materialReview + Environment.NewLine;
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
