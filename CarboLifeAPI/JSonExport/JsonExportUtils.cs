using CarboLifeAPI.Data;
using LCAx;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CarboLifeAPI
{
    public class JsonExportUtils
    {
        /// <summary>
        /// Exports a carbolifeproject as baked JSON
        /// </summary>
        /// <param name="path"></param>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool ExportToJson(string path, CarboProject carboLifeProject)
        {
            bool result = false;


            JsCarboProject jsProject = converToJsProject(carboLifeProject);

            try
            {
                var json = new JavaScriptSerializer().Serialize(jsProject);
                File.WriteAllText(path, json);

                result = true;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Gets a path file to save to
        /// </summary>
        /// <returns>path if valis, null if invalid</returns>
        public static string GetSaveAsLocation()
        {
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save LCA file";
            saveDialog.Filter = "json file |*.json";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            //Check if the file can be read and written to.
            if (File.Exists(path))
            {
                //FileInfo fileInfo = new FileInfo(path);
                bool isInUse = IsFileLocked(path);

                if (isInUse == true)
                    return null;
            }
            else
            {
                if (path != "")
                    //This is a new file
                    return path;
                else
                    return null;
            }


            //If this part is reached; return the valid path;
            return path;

        }
        private static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //The file is open
                return true;
            }

            //All is ok
            return false;
        }

        public static JsCarboProject converToJsProject(CarboProject carboProject) 
        {
            JsCarboProject jsProject = new JsCarboProject();

            jsProject.Name = carboProject.Name;
            jsProject.Number = carboProject.Number;
            jsProject.Category = carboProject.Category;
            jsProject.Description = carboProject.Description;
            jsProject.SocialCost = carboProject.SocialCost;
            jsProject.GIA = carboProject.Area;
            jsProject.A0Global = carboProject.A0Global;
            jsProject.A5Global = carboProject.A5Global;
            jsProject.b675Global = carboProject.b675Global;
            jsProject.C1Global = carboProject.C1Global;
            jsProject.valueUnit = carboProject.valueUnit;
            jsProject.designLife = carboProject.designLife;

            CarboGroup cg = carboProject.getTotalsGroup();

            if (cg.AllElements.Count > 0)
            {
                List<CarboElement> elements = cg.AllElements;

                foreach (CarboElement ce in elements)
                {
                    CarboMaterial material = carboProject.CarboDatabase.getClosestMatch(ce.CarboMaterialName);
                    if (material != null)
                    {
                        JsCarboElement JsCe = CopyCe(ce, carboProject);

                        //IndividualElements
                        JsCe.Density = material.Density;
                        double mass = ce.Mass;
                        if (mass == 0)
                            mass = ce.Volume_Total * JsCe.Density;

                        JsCe.EC_A1A3_Total = mass * material.ECI_A1A3;
                        JsCe.EC_A4_Total = mass * material.ECI_A4;
                        JsCe.EC_A5_Total = mass * material.ECI_A5;
                        JsCe.EC_B1B7_Total = mass * material.ECI_B1B5;
                        JsCe.EC_C1C4_Total = mass * material.ECI_C1C4;
                        JsCe.EC_D_Total = mass * material.ECI_D;
                        JsCe.EC_Mix_Total = mass * material.ECI_Mix;
                        JsCe.EC_Sequestration_Total = mass * material.ECI_Seq;

                        jsProject.elementList.Add(JsCe);
                    }
                }
            }

            // get all the groups without elements
            foreach(CarboGroup grp in carboProject.getGroupList)
            {
                if(grp.AllElements.Count == 0)
                {
                    //
                    JsCarboElement JsCe = new JsCarboElement();
                    JsCe = CopyCe(grp, carboProject);
                    jsProject.elementList.Add(JsCe);

                }
            }

            return jsProject;
        }

        public static Lcax convertToLCAx(JsCarboProject jsProject)
        {
            Lcax lcaxProject = new Lcax();

            lcaxProject.Name = jsProject.Name;
            lcaxProject.Description = jsProject.Number + Environment.NewLine + jsProject.Description;
            lcaxProject.LifeSpan = jsProject.designLife;

            lcaxProject.MetaData = new Dictionary<string, string>();
            lcaxProject.MetaData.Add("GIA", jsProject.GIA.ToString());
            lcaxProject.MetaData.Add("A0Global", jsProject.A0Global.ToString());
            lcaxProject.MetaData.Add("C1Global", jsProject.C1Global.ToString());
            lcaxProject.MetaData.Add("b675Global", jsProject.b675Global.ToString());
            lcaxProject.MetaData.Add("SocialCost", jsProject.SocialCost.ToString());
            lcaxProject.MetaData.Add("ECTotal", jsProject.ECTotal.ToString());

            //Build the assemblies
            //Run through each element and combine where Id's are identical
            lcaxProject.EmissionParts = new Dictionary<string, Assembly>();

            foreach (JsCarboElement jsCe in jsProject.elementList)
            {
                try
                {
                    bool exists = false;
                    foreach (var dict in lcaxProject.EmissionParts)
                    {
                        Assembly assTest = dict.Value as Assembly;
                        string assKey = dict.Key as string;
                        if (assTest != null)
                        {
                            if (assKey == jsCe.Id.ToString())
                            {
                                EpdPart newPart = getEpdPart(jsCe);
                                assTest.Parts.Add(jsCe.Id.ToString() + "." + assTest.Parts.Count, newPart);
                                assTest.Quantity += newPart.PartQuantity;
                                assTest.Unit = newPart.PartUnit;

                                exists = true;
                                break;
                            }
                        }
                    }

                    if (exists == false)
                    {
                        Assembly newAss = getAssembly(jsCe);
                        lcaxProject.EmissionParts.Add(jsCe.Id.ToString(), newAss);


                    }
                }
                catch (Exception ex)
                { 

                }

            }

            //Calculate Totals;
            foreach (var dict in lcaxProject.EmissionParts)
            {

            }

                return lcaxProject;
        }

        private static Assembly getAssembly(JsCarboElement jsCe)
        {
            Assembly assembly = new Assembly();
            try
            {
                string id = jsCe.Id.ToString();

                assembly.Id = id;
                assembly.Name = jsCe.Name;
                assembly.Comment = "";
                assembly.Category = jsCe.Category;
                assembly.Parts = new Dictionary<string, EpdPart>();

                EpdPart newPart = getEpdPart(jsCe);
                assembly.Parts.Add(id, newPart);

                assembly.Quantity += newPart.PartQuantity;
                assembly.Unit = newPart.PartUnit;
            }

            catch (Exception ex)
            {

            }
            return assembly;
        }

        private static EpdPart getEpdPart(JsCarboElement jsCe)
        {
            string id = jsCe.Id.ToString();

            EpdPart newPart = new EpdPart();
            newPart.Id = id;
            newPart.Name = jsCe.Name;
            newPart.PartQuantity = jsCe.Volume;
            newPart.PartUnit = Unit.M3;
            return newPart;
        }

        private static JsCarboElement CopyCe(CarboElement ce, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = ce.Name;
            JsCe.Id = ce.Id;
            JsCe.MaterialName = ce.MaterialName;
            JsCe.Category = ce.Category;
            JsCe.SubCategory = ce.SubCategory;
            JsCe.AdditionalData = ce.AdditionalData;
            JsCe.Grade = ce.Grade;
            JsCe.LevelName = ce.LevelName;

            JsCe.Volume = ce.Volume;
            JsCe.Volume_Total = ce.Volume_Total;
            JsCe.Mass = ce.Mass;
            JsCe.Level = ce.Level;
            JsCe.Density = ce.Density;

            JsCe.ECI = ce.ECI;
            JsCe.EC = ce.EC;

            JsCe.Volume_Cumulative = ce.Volume_Cumulative;
            JsCe.ECI_Cumulative = ce.ECI_Cumulative;
            JsCe.EC_Cumulative = ce.EC_Cumulative;

            JsCe.isDemolished = ce.isDemolished;
            JsCe.isExisting = ce.isExisting;
            JsCe.isSubstructure = ce.isSubstructure;
            JsCe.includeInCalc = ce.includeInCalc;

            return JsCe;

        }
        private static JsCarboElement CopyCe(CarboGroup grp, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = grp.Description;
            JsCe.Id = grp.Id;
            JsCe.MaterialName = grp.MaterialName;
            JsCe.Category = grp.Category;
            JsCe.SubCategory = grp.SubCategory;
            JsCe.AdditionalData = grp.additionalData;
            JsCe.Grade = "";
            JsCe.LevelName = "";

            JsCe.Volume = grp.Volume;
            JsCe.Volume_Total = grp.TotalVolume;

            JsCe.Level = 0;
            JsCe.Density = grp.Density;

            JsCe.ECI = grp.ECI;
            JsCe.EC = grp.EC;
            JsCe.EC_Cumulative = grp.ECI;
            JsCe.EC_Cumulative = grp.EC;

            JsCe.isDemolished = grp.isDemolished;
            JsCe.isExisting = grp.isExisting;
            JsCe.isSubstructure = grp.isSubstructure;
            JsCe.includeInCalc = true;

            //ToalValues
            JsCe.EC_A1A3_Total = grp.getTotalA1A3;
            JsCe.EC_A4_Total = grp.getTotalA4;
            JsCe.EC_A5_Total = grp.getTotalA5;
            JsCe.EC_B1B7_Total = grp.getTotalB1B7;
            JsCe.EC_C1C4_Total = grp.getTotalC1C4;
            JsCe.EC_D_Total = grp.getTotalD;
            JsCe.EC_Mix_Total = grp.getTotalMix;
            JsCe.EC_Sequestration_Total = grp.getTotalSeq;


            return JsCe;

        }

        public static bool ExportToLCAx(string path, CarboProject carboLifeProject)
        {
            bool result = false;

            JsCarboProject jsProject = converToJsProject(carboLifeProject);
            Lcax lcaProject = convertToLCAx(jsProject);

            try
            {
                var json = new JavaScriptSerializer().Serialize(lcaProject);
                File.WriteAllText(path, json);

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;

        }
    }
}
