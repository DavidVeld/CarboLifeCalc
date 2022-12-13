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

namespace AardWolf
{
    public class AardWolfSolver : GH_Component
    {
        // Methods
        public AardWolfSolver()
        : base("Carbo Life Project", "Carbo Life Project Solver", "Carbo Life Project Solver", "Aardwolf", "Aardwolf")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Elements", "Carbo Elements", "Carbo Elements", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Total", "Total", "Total COe2"); //0

            pManager.Register_DoubleParam("A1-A3", "A1-A3", "A1-A3 Values"); //1
            pManager.Register_DoubleParam("A4", "A4", "A4 Values"); //2
            pManager.Register_DoubleParam("A5", "A5", "A5 Values"); //3
            pManager.Register_DoubleParam("B1-6", "B1-6", "B1-6 Values"); //4
            pManager.Register_DoubleParam("C", "C", "C Values");//5
            pManager.Register_DoubleParam("D", "D", "D Values");//6
            pManager.Register_DoubleParam("Other", "Other", "Other Values");//7
            pManager.Register_DoubleParam("Seq", "Seq", "Sequestration");//8

            pManager.Register_GenericParam("Project", "P", "Returns the project file");//9

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string errors = "";
            //string path = "";

            var provided_as_goo = new List<GH_ObjectWrapper>();
            List<CarboElement> listOfElements = new List<CarboElement>();

            CarboProject runtimeProject = new CarboProject();

            //Get the save as.. path :
            //DA.GetData<string>(1, ref path);

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
                runtimeProject = AardwolfProcess.ProcessData(listOfElements);
            }

            List<CarboDataPoint> list = runtimeProject.getPhaseTotals();

            double totals = runtimeProject.getTotalEC();

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


            DA.SetData(0, totals); //Totals

            DA.SetData(1, a13Total);
            DA.SetData(2, a4Total);
            DA.SetData(3, a5Total + a5Global);

            DA.SetData(4, B16Total);
            DA.SetData(5, CTotal + CGlobal);

            DA.SetData(6, DTotal);
            DA.SetData(7, Other);
            DA.SetData(8, Seq);

            DA.SetData(9, runtimeProject);


        }
        // Properties
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{2D1746FA-EE9D-4417-A978-E94A606639FC}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Aardwolf.Properties.Resources.SolveElement;
            }
        }
    }
}
