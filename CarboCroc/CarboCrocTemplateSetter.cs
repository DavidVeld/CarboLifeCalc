using CarboLifeAPI;
using CarboLifeAPI.Data;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCroc
{
    public class CarboCrocTemplateSetter : GH_Component
    {
        // Methods
        public CarboCrocTemplateSetter()
        : base("Set a CarboLifeCalc Template", "Set a CarboLifeCalc Template", "Set a CarboLifeCalc Template as the global template to be used for all nodes", "CarboCroc", "Data")
        {
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{AA9D1559-8090-47B6-BE26-0BB01B59D9C7}");
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Save As.. Path?", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Set", "Set", "A Switch", GH_ParamAccess.item, false);
            //pManager.AddBooleanParameter("Update", "Path", "Save As.. Path?", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Message", "Message", "Error Messages to mind");//9
            pManager.Register_StringParam("Path", "Template Path", "The path to the template");//9
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string TemplatePath = "";
            bool saveme = false;

            bool ok = false;
            string errorMessage = "";

            string ghFilePath = OnPingDocument()?.FilePath;

            DA.GetData<string>(0, ref TemplatePath);
            DA.GetData<bool>(1, ref saveme);

            try
            {

                if (string.IsNullOrEmpty(ghFilePath))
                {
                    errorMessage = "Please save the Grasshopper file first.";
                    DA.SetData(0, errorMessage);
                    return;
                }

                string TemplateSetFilePath = CarboCrocUtils.getExpectedTemplatePathFile();

                if (saveme == true)
                {
                    //once button is pressed this is the save action:

                    if (CarboCrocUtils.isValidTemplate(TemplatePath))
                        File.WriteAllText(TemplateSetFilePath, TemplatePath);
                }
                else
                {
                    //once button is released this is the response:
                    if (File.Exists(TemplateSetFilePath))
                    {
                        string pathSet = CarboCrocUtils.getSetTemplatePath("");
                        if(File.Exists(pathSet))
                        {
                            errorMessage = "Template Path set as: " + pathSet + Environment.NewLine;
                            errorMessage += "Template Path file saved to: " + TemplateSetFilePath;
                            TemplatePath = pathSet;
                        }
                        else
                        {
                            errorMessage = "Template file does not exist.";
                            TemplatePath = "Error 1";
                        }
                    }
                    else
                    {
                        errorMessage += "No Template Set file found ";
                        TemplatePath = "Error 2";

                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                TemplatePath = "Error 3";
            }

            DA.SetData(0, errorMessage); //Message
            DA.SetData(1, TemplatePath); //Set Path


        }



        protected override Bitmap Internal_Icon_24x24
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return CarboCroc.Properties.Resources.SaveProject;
            }
        }



    }
}
