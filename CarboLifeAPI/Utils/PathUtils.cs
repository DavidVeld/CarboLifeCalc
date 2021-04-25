using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows;

namespace CarboLifeAPI
{
    public static class PathUtils
    {
        /// <summary>
        /// The current location from where the application runs
        /// </summary>
        /// <returns>The application path </returns>
        public static string getAssemblyPath()
        {
            string _path = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(_path);
            return myPath;
        }
        /// <summary>
        /// Sets and prepares the usermaterials
        /// </summary>
        public static void CheckUserMaterials()
        {
            string pathDatabase = Utils.getAssemblyPath() + "\\db\\";
            string bufferPath = pathDatabase + "MaterialBuffer.cxml";
            string targetPath = pathDatabase + "UserMaterials.cxml";
            if (!(File.Exists(targetPath)))
            {
                if (File.Exists(bufferPath))
                {
                    File.Copy(bufferPath, targetPath);
                }
            }

            //All userfiles need to move to the local folder:
            string appdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CarboLifeCalc\\";
            string TemplateFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CarboLifeCalc\\UserMaterials.cxml";

            try
            {
                //Directory
                if (!Directory.Exists(appdatafolder))
                    Directory.CreateDirectory(appdatafolder);

                if (!(File.Exists(appdatafolder + "UserMaterials.cxml")))
                {
                    if (File.Exists(bufferPath))
                    {
                        File.Copy(bufferPath, appdatafolder + "UserMaterials.cxml");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        public static string getTemplateFolder()
        {
            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CarboLifeCalc\\UserMaterials.cxml";

                if (File.Exists(folder))
                    return folder;
                else
                    return "";
            }
            catch (Exception ex)
            {
                return "";
            }
            return "";
        }

        public static string getDataBasePath()
        {
            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CarboLifeCalc\\UserMaterials.cxml";

                if (File.Exists(folder))
                    return folder;
                else
                    return "";
            }
            catch (Exception ex)
            {
                return "";
            }
            return "";
        }


    }
}