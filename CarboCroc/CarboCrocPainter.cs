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
using Rhino;
using System.Security.Cryptography;
using Grasshopper.Kernel.Special;
using Rhino.Commands;
using System.Windows;
using System.Windows.Controls;
using Rhino.DocObjects;

namespace CarboCroc
{
    public class CarboCrocPainter : GH_Component
    {
        public CarboCrocPainter()
: base("CarboCrocPainter", "Colour Geo If Available", "Colour Geo If Available", "CarboCroc", "Builder")
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Carbo Project", "Cp", "Carbo Project", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Settings", "S", "Colour Settings", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Message", "M", "CodeLog");
            //pManager.Register_GenericParam("Geo", "G", "a Colour");
            pManager.Register_StringParam("Geo", "G", "Colour List");
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper provided_as_goo = null;
            //List<CarboElement> listOfElements = new List<CarboElement>();
            string path = "";

            bool ok = false;
            string errorMessage = "";
            List<string> colours = new List<string>();

            RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;

            try
            {
                DA.GetData<GH_ObjectWrapper>(0, ref provided_as_goo);
                if (provided_as_goo != null)
                {
                    if (provided_as_goo.Value is CarboProject)
                    {
                        CarboProject project = provided_as_goo.Value as CarboProject;
                        if (project != null)
                        {
                            //write all GUIDS:
                           List<CarboElement> allelement  = project.getElementsFromGroups().ToList();
                            foreach (CarboElement element in allelement)
                                colours.Add(element.GUID);

 

                            CarboGraphResult resultlist = HeatMapCollector.GetPerElementData(project);

                           List<int> ids = new List<int>();
                            foreach(CarboValues cv in resultlist.entireProjectData)
                            {
                                ids.Add(cv.Id);
                            }



                            resultlist.FilterNonVisible(ids);

                            //Values we'd need for all options:
                            double xMaxCutoff = resultlist.getMaxValue();
                            double xMinCutoff = resultlist.getMinValue();

                            resultlist.FilterMinMax(xMinCutoff, xMaxCutoff);

                            errorMessage += "Validdata " + resultlist.validData.Count;

                            CarboColourPreset settingfs = new CarboColourPreset();

                            var result = HeatMapBarBuilder.GetBarGraph(resultlist, 1024, 1024, settingfs);

                            CarboGraphResult graphData = result.Item1 as CarboGraphResult;
                            List<UIElement> graph = result.Item2 as List<UIElement>;


                            //doc.Objects.Find
                            //string guid = allelement[0].GUID.ToString();
                            if (doc != null)
                            {
                                /*
                                Guid guid1 = new Guid(guid);

                                //Color slateBlue = Color.FromName("SlateBlue");
                                System.Drawing.Color obColour = Color.FromArgb(1, 0, 0, 0);

                                RhinoObject rhinoObject = null;
                                rhinoObject = doc.Objects.Find(guid1);

                                if (rhinoObject != null)
                                {
                                    rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                                    rhinoObject.Attributes.ObjectColor = obColour;
                                    rhinoObject.CommitChanges();
                                }

                                errorMessage += "Element Found " + guid1;
                                */
                                errorMessage += "Elements: " + graphData.validData.Count;

                                foreach (CarboValues cv in graphData.validData)
                                {

                                    Guid cvGuid = new Guid(cv.GUID);
                                    /*
                                    //Color slateBlue = Color.FromName("SlateBlue");
                                    System.Drawing.Color obColour = Color.FromArgb(1, cv.r, cv.r, cv.b);

                                    doc.Objects.Find(cvGuid).Attributes.ObjectColor = obColour;
                                    */
                                    //Color slateBlue = Color.FromName("SlateBlue");

                                    System.Drawing.Color obColour = Color.FromArgb(255, cv.r, cv.g, cv.b);

                                    RhinoObject rhinoObject = null;
                                    rhinoObject = doc.Objects.Find(cvGuid);

                                    if (rhinoObject != null)
                                    {
                                        rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                                        rhinoObject.Attributes.ObjectColor = obColour;
                                        rhinoObject.CommitChanges();
                                    }
                                    errorMessage += "Element :" + cvGuid + " ; " + rhinoObject.Attributes.ObjectColor.A + " ; " + cv.r + " ; " + cv.g + " ; " + cv.b + Environment.NewLine;
                                }
                            }
                            else
                            {
                                errorMessage += "Invalid Project Data  " + graphData.validData.Count;
                            }



                            ok = true;
                        }


                        else
                        {
                            errorMessage = "Project not accepted";
                        }
                    }
                }
                else
                {
                    errorMessage = "Could not find project in input";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            DA.SetData(0, errorMessage);
            DA.SetData(1, colours);
            

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{BB060C92-A96C-4842-A5DB-BD59941D4075}");
            }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.SolveElement;
            }
        }

    }
}
