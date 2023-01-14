using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Drawing;
using System.ComponentModel;

namespace CarboCroc
{
    public class ListofMaterials : GH_Component
    {
        public ListofMaterials()
: base("List Materials", "List all Materials", "Lists the available Carbo Life Materials", "CarboCroc", "Data")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("List", "L", "Returns the project material names", GH_ParamAccess.list);//9
            //pManager.Register_StringParam("List2", "L2", "Returns the project material names");//9

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                CarboProject CP = new CarboProject();
                CarboDatabase DB = CP.CarboDatabase;

                List<string> listofCarboMaterials = new List<string>();

                foreach (CarboMaterial CM in DB.CarboMaterialList)
                {
                    listofCarboMaterials.Add(CM.Name);
                    // DA.SetData(1, CM.Name);
                }

                if (listofCarboMaterials.Count > 0)
                    DA.SetDataList(0, listofCarboMaterials);

                //Add a new component if there is no output linked.

                if (this.Params.Output[0].Recipients.Count == 0)
                {
                    //trigger for debug
                    bool ok = true;

                    //var listIndex = new Grasshopper.Kernel.GH_ListUtil();


                    //build the valuelist;
                    var vallist = new Grasshopper.Kernel.Special.GH_ValueList();
                    vallist.CreateAttributes();
                    vallist.Name = "Materials";
                    vallist.NickName = "Material:";
                    vallist.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;

                    vallist.ListItems.Clear();

                    //Set Location
                    //int inputcount = this.Params.Input[0].SourceCount;
                    vallist.Attributes.Pivot = new PointF(
                        (float)this.Attributes.Bounds.X,
                        (float)this.Attributes.Bounds.Y);


                    //Populate
                    for (int i = 0; i < listofCarboMaterials.Count; i++)
                    {
                        vallist.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(listofCarboMaterials[i].ToString(), i.ToString()));
                    }

                    //place the component
                    this.OnPingDocument().AddObject(vallist, false);

                    //Attach to component

                    //this.Params.Output[0].AddSource(vallist);
                    //vallist.ExpireSolution(true);

                }
            }
            catch (Exception ex)
            {
                DA.SetData(0, ex.Message);
            }

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{992D35B8-12B0-4E8A-A9B8-62CA03DDAC59}");
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
