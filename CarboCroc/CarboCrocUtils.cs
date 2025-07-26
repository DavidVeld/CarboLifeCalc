using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarboLifeAPI.Data;
using System.Data;
using CarboLifeAPI;

namespace CarboCroc
{
    internal class CarboCrocUtils
    {

        internal static string getExpectedTemplatePathFile()
        {
            try
            {
                GH_Document doc = Grasshopper.Instances.ActiveCanvas?.Document;
                string ghFilePath = doc.FilePath;


                string directory = Path.GetDirectoryName(ghFilePath);
                string fileName = Path.GetFileNameWithoutExtension(ghFilePath) + "templatePath";
                string txtPath = Path.Combine(directory, fileName + ".txt");
                
                return txtPath;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// This is the main function that retreives the template path of a project, either a direct teplate was set or a template path was set, or 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// 
        internal static string getSetTemplatePath(string pathOverride)
        {
            try
            {
                //Any overide would be rewuired.
                if (!string.IsNullOrWhiteSpace(pathOverride))
                {
                   if(isValidTemplate(pathOverride))
                        return pathOverride;
                }
                else if (pathOverride == "")
                {
                    string ghFilePath = getExpectedTemplatePathFile();

                    //read the content
                    if (File.Exists(ghFilePath) == true && DataExportUtils.IsFileLocked(ghFilePath) == false)
                    {
                        string path = File.ReadAllText(ghFilePath);
                        bool valid = isValidTemplate(path);
                        if (valid == true)
                            return path;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        internal static bool isValidTemplate(string templatePath)
        {
            if (File.Exists(templatePath) == false)
            {
                return false;
            }
            try
            {
                CarboProject project = new CarboProject(templatePath);
                if (project != null && project.CarboDatabase != null)
                {
                    if (project.CarboDatabase.CarboMaterialList.Count > 0)
                    {
                        //This means the path points to a valid template, with materials
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}