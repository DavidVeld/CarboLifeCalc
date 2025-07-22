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
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using System.IO;

namespace CarboCroc
{
    public class CarboCrocGroup : GH_Component
    {
        public CarboCrocGroup()
: base("CarboGroup", "Create a CarboGroup", "Combines Multiple Materials into a CarboGroup", "CarboCroc", "Builder")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Elements", "Carbo Elements", "Carbo Elements", GH_ParamAccess.list);//0
            pManager.AddTextParameter("Material Template", "Material Template", "Material Template (WIP)", GH_ParamAccess.item, ""); //1
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("CarboGroup", "CG", "Returns a Carbo Group");//0
            pManager.Register_StringParam("Message", "M", "Returns a summary");//1
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                string message = "";
                List<CarboGroup> carboGroupList = new List<CarboGroup>();

                var provided_as_goo = new List<GH_ObjectWrapper>();
                List<CarboElement> listOfElements = new List<CarboElement>();

                string path = "";
                if (!DA.GetData(1, ref path) || string.IsNullOrWhiteSpace(path))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide a valid cxml or csv file path.");
                    return;
                }

                if (!File.Exists(path))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File not found.");
                    return;
                }

                //Start a project in the new template
                CarboProject CP = new CarboProject(path);
                
                //Get the Elements
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

                //Now Add the elements to the group
                foreach (CarboElement cel in listOfElements)
                {
                    CP.AddElement(cel);
                }
                CP.CreateGroups();
                CP.CalculateProject();

                message += listOfElements.Count + "Elements were added to this group" + Environment.NewLine;

                carboGroupList = CP.getGroupList.ToList();

                //All is good calculate the group and return;


                DA.SetDataList(0, carboGroupList);
                DA.SetData(1, message);

            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                DA.SetData(1, ex.Message);
            }

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{39989138-2344-4C0C-B6A9-022B7CB1BC95}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.CarboGroup;
            }
        }

    }
}
