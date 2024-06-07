using CarboLifeAPI.Data;
using LCAx;
using Microsoft.Office.Interop.Excel;
using Microsoft.Vbe.Interop;
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
                var JsonSerializer = new JavaScriptSerializer();
                JsonSerializer.MaxJsonLength = Int32.MaxValue;

                var json = JsonSerializer.Serialize(jsProject);
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
            jsProject.GIANew = carboProject.AreaNew;

            jsProject.A0Global = carboProject.A0Global;
            jsProject.A5Global = carboProject.A5Global;
            jsProject.b675Global = carboProject.b675Global;
            jsProject.C1Global = carboProject.C1Global;
            jsProject.valueUnit = carboProject.valueUnit;
            jsProject.designLife = carboProject.designLife;

            CarboGroup cg = carboProject.getTotalsGroup();

            //get all elements
            IList<CarboElement> elementList = carboProject.getElementsFromGroups().ToList();

                foreach (CarboElement ce in elementList)
                {
                    CarboMaterial material = carboProject.CarboDatabase.getClosestMatch(ce.CarboMaterialName, ce.Grade);
                    if (material != null)
                    {
                        JsCarboElement JsCe = ConvertoToJsCarboElement(ce, carboProject);

                        //
                        JsCe.Density = material.Density;
                        JsCe.Grade = material.Grade;

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
            

            // get all the groups without elements
            foreach(CarboGroup grp in carboProject.getGroupList)
            {
                if(grp.AllElements.Count == 0)
                {
                    //
                    JsCarboElement JsCe = new JsCarboElement();
                    JsCe = ConvertoToJsCarboElement(grp, carboProject);
                    jsProject.elementList.Add(JsCe);

                }
            }

            //Get the materials
            List<CarboMaterial> materialList = carboProject.getUsedmaterials();

            if (materialList.Count > 0)
            {
                foreach (CarboMaterial cm in materialList)
                {
                    JsCarboMaterial jsMaterial = converToJsMaterial(cm);

                    if (jsMaterial != null)
                        jsProject.materialList.Add(jsMaterial);
                }
            }


            return jsProject;
        }

        private static JsCarboMaterial converToJsMaterial(CarboMaterial cm)
        {
            JsCarboMaterial result = new JsCarboMaterial();

            try
            {
                result.Id = cm.Id;
                result.Name = cm.Name;
                result.Description = cm.Description;
                result.Category = cm.Category;
                result.Grade = cm.Grade;
                result.Density = cm.Density;
                result.EPDurl = cm.EPDurl;
                result.WasteFactor = cm.WasteFactor;
                result.isLocked = cm.isLocked;

                result.ECI_A1A3 = cm.ECI_A1A3;
                result.ECI_A4 = cm.ECI_A4;
                result.ECI_A5 = cm.ECI_A5;
                result.ECI_B1B5 = cm.ECI_B1B5;
                result.ECI_C1C4 = cm.ECI_C1C4;
                result.ECI_D = cm.ECI_D;
                result.ECI_Mix = cm.ECI_Mix;
                result.ECI_Seq = cm.ECI_Seq;
                result.ECI = cm.ECI;
                result.VolumeECI = cm.getVolumeECI;
            }
            catch
            {
                return null;
            }

            return result;
        }

        public static Lcax convertToLCAx(JsCarboProject jsProject)
        {
            Lcax lcaxProject = new Lcax();
            List<string> test = new List<string>();

            lcaxProject.Name = jsProject.Name;

            lcaxProject.Description =  jsProject.Description;
            lcaxProject.Id = jsProject.Number;

            lcaxProject.ReferenceStudyPeriod = jsProject.designLife;

            lcaxProject.ProjectInfo.BuildingInfo.GrossFloorArea.Unit = Unit.M2;
            lcaxProject.ProjectInfo.BuildingInfo.GrossFloorArea.Value = jsProject.GIA;
            lcaxProject.ProjectInfo.BuildingInfo.BuildingType = BuildingType.New;
            lcaxProject.ProjectInfo.BuildingInfo.BuildingTypology = BuildingTypology.Other;



            lcaxProject.MetaData = new Dictionary<string, string>();
            lcaxProject.MetaData.Add("GIA", jsProject.GIA.ToString());
            lcaxProject.MetaData.Add("A0Global", jsProject.A0Global.ToString());
            lcaxProject.MetaData.Add("C1Global", jsProject.C1Global.ToString());
            lcaxProject.MetaData.Add("b675Global", jsProject.b675Global.ToString());
            lcaxProject.MetaData.Add("SocialCost", jsProject.SocialCost.ToString());
            lcaxProject.MetaData.Add("ECTotal", jsProject.ECTotal.ToString());

            lcaxProject.Assemblies = new Dictionary<string, AssemblySource>();

            //Build the masterial Assembly;
            Assembly materials = getMaterialAssembly(jsProject);
            AssemblySource materialSource = new AssemblySource();
            materialSource.Assembly = materials;

            lcaxProject.Assemblies.Add("Material EPDs", materialSource);
            test.Add(materialSource.Assembly.Id);

            //Build the assemblies
            //Run through each element and combine where Id's are identical
            foreach (JsCarboElement jsCe in jsProject.elementList)
            {
                try
                {
                    bool exists = false;
                    foreach (var dict in lcaxProject.Assemblies)
                    {
                        AssemblySource elementAssemblySource = dict.Value as AssemblySource;

                        if (elementAssemblySource != null)
                        {
                            Assembly assTest = elementAssemblySource.Assembly;

                            string assKey = dict.Key as string;
                            //string id = jsCe.GUID;

                            if (assTest != null)
                            {
                                if (assKey == jsCe.GUID)
                                {
                                    //this assembly already exists ant the elment needs to be added to the assembly
                                    ProductSource newPart = getEpdPart(jsCe);

                                    assTest.Products.Add(jsCe.GUID.ToString() + "." + assTest.Products.Count, newPart);
                                    assTest.Quantity += newPart.Product.Quantity;
                                    assTest.Unit = newPart.Product.Unit;

                                    exists = true;
                                    break;
                                }
                            }
                        }
                    }

                    //This element requires a new Assembly
                    if (exists == false)
                    {
                        AssemblySource newAssSrc = getAssembly(jsCe);
                        //AssemblySource elementAssemblySource = new AssemblySource();

                        //elementAssemblySource.Assembly = newAss;
                        //elementAssemblySource.Assembly.Id = newAss.Id;
                        //elementAssemblySource.Assembly.Products = newAss.Products;

                        lcaxProject.Assemblies.Add(jsCe.GUID.ToString(), newAssSrc);
                        test.Add(newAssSrc.Assembly.Id);

                    }
                }
                catch (Exception ex)
                { 

                }

            }

            //Calculate Totals;
            //foreach (var dict in lcaxProject.EmissionParts)
            //{
            //}

                return lcaxProject;
        }

        private static Assembly getMaterialAssembly(JsCarboProject jsCarboProject)
        {
            Assembly materialAssembly = new Assembly();
            materialAssembly.Name = "Materials";
            materialAssembly.Comment = "This assembly contains all the used materials in the project";
            materialAssembly.Description = "This assembly contains all the used materials in the project";
            materialAssembly.Products = new Dictionary<string, ProductSource>();


            foreach (JsCarboMaterial jsMaterial in jsCarboProject.materialList)
            {
                ProductSource productSource = new ProductSource();
                Product newpart = new Product();

                try
                {
                    newpart.Id = jsMaterial.Id.ToString();
                    newpart.Name = jsMaterial.Name;

                    Epd newEpd = new Epd();
                    newEpd.Id = jsMaterial.Id.ToString();
                    newEpd.Name = jsMaterial.Name;
                    newEpd.Comment = jsMaterial.Description;

                    newEpd.Source = new Source();
                    newEpd.Source.Name = "URL";
                    newEpd.Source.Url = jsMaterial.EPDurl;
                    newEpd.DeclaredUnit = Unit.Kg;

                    ImpactDataSource impactDataSource = new ImpactDataSource();
                    impactDataSource.Epd = new Epd();


                    Dictionary<string,double?> data = new Dictionary<string,double?>();
                    data.Add("A1A3", jsMaterial.ECI_A1A3);
                    data.Add("A4", jsMaterial.ECI_A4);
                    data.Add("A5", jsMaterial.ECI_A5);
                    data.Add("B1", jsMaterial.ECI_B1B5);
                    data.Add("C1", jsMaterial.ECI_C1C4);
                    data.Add("D", jsMaterial.ECI_D);
                    data.Add("Sequestration", jsMaterial.ECI_Seq);
                    data.Add("Extra", jsMaterial.ECI_Mix);

                    impactDataSource.Epd.Impacts.Add("Gwp", data);

                    newEpd.Comment = jsMaterial.Description;

                    newEpd.MetaData.Add("Grade", jsMaterial.Grade);
                    newEpd.MetaData.Add("Sequestration", jsMaterial.ECI_Seq.ToString());
                    newEpd.MetaData.Add("Category", jsMaterial.Category);
                    newEpd.MetaData.Add("Default Waste Factor (%)", jsMaterial.WasteFactor.ToString());

                    //newpart.EpdSource = new EpdSource();
                    //newpart.EpdSource.Epd = newEpd;

                    productSource.Product = newpart;
                }
                catch
                {
                }
                materialAssembly.Products.Add(newpart.Id, productSource);
            }


            return materialAssembly;
        }


        private static AssemblySource getAssembly(JsCarboElement jsCe)
        {
            AssemblySource assemblyrsc = new AssemblySource();
            //Assembly assembly = new Assembly();
            try
            {
                string id = jsCe.Id.ToString();

                if (jsCe.GUID != "")
                    id = jsCe.GUID.ToString();


                assemblyrsc.Assembly.Id = id.ToString();
                assemblyrsc.Assembly.Name = jsCe.Name;
                assemblyrsc.Assembly.Description = "";
                assemblyrsc.Assembly.Comment = jsCe.Category;
                assemblyrsc.Assembly.Products = new Dictionary<string, ProductSource>();

                ProductSource newPart = getEpdPart(jsCe);
                assemblyrsc.Assembly.Products.Add(id, newPart);

                assemblyrsc.Assembly.Quantity += newPart.Product.Quantity;
                assemblyrsc.Assembly.Unit = newPart.Product.Unit;
            }

            catch (Exception ex)
            {

            }
            return assemblyrsc;
        }

        private static ProductSource getEpdPart(JsCarboElement jsCe)
        {
            string id = jsCe.GUID.ToString();

            if (jsCe.GUID != "")
                id = jsCe.GUID.ToString();

            ProductSource productSource = new ProductSource();
            Product newPart = new Product();

            try
            {
                newPart.Id = id;
                newPart.Name = jsCe.Name;

                newPart.Quantity = jsCe.Volume_Total;
                newPart.Unit = Unit.M3;
                //material
                newPart.ImpactData.Epd = new Epd();
                //newPart.EpdSource.Internalepd = new InternalEpd();
                newPart.ImpactData.Epd.Name = jsCe.CarboMaterialName;
                //newPart.ImpactData.Epd.Comment = jsCe.CarboMaterialName;

                newPart.MetaData = new Dictionary<string, string>();


                newPart.MetaData.Add("Grade", jsCe.Grade);
                newPart.MetaData.Add("Density", jsCe.Density.ToString());
                newPart.MetaData.Add("Category", jsCe.Category);
                newPart.MetaData.Add("IsSubstructure", jsCe.isSubstructure.ToString());
                newPart.MetaData.Add("MaterialName", jsCe.CarboMaterialName);
                newPart.MetaData.Add("RevitMaterialName", jsCe.MaterialName);
                newPart.MetaData.Add("Mass", jsCe.Mass.ToString());

                Dictionary<string,double?> data = new Dictionary<string, double?>();
                data.Add("A1A3",jsCe.EC_A1A3_Total);
                data.Add("A4", jsCe.EC_A4_Total);
                data.Add("A5", jsCe.EC_A5_Total);
                data.Add("B", jsCe.EC_B1B7_Total);
                data.Add("C1C4", jsCe.EC_C1C4_Total);
                data.Add("D", jsCe.EC_D_Total);
                data.Add("Seq", jsCe.EC_Sequestration_Total);
                data.Add("Mix", jsCe.EC_Mix_Total);

                newPart.Results.Add("GWp",data);

            }
            catch (Exception ex)
            {
                newPart.Name = ex.Message;
                newPart.Quantity = 0;
            }
            if(newPart != null)
                productSource.Product = newPart;

            return productSource;
        }

        private static CarboProject convertToCarboCalcProject(Lcax lcaxFile)
        {
            CarboProject result = new CarboProject();

            result.Name = lcaxFile.Name;
            result.Description = lcaxFile.Description;
            result.designLife = Convert.ToInt16(lcaxFile.ReferenceStudyPeriod);

            return result;

        }

        private static JsCarboElement ConvertoToJsCarboElement(CarboElement ce, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = ce.Name;
            JsCe.Id = ce.Id;
            JsCe.GUID = ce.GUID;
            JsCe.MaterialName = ce.MaterialName;
            JsCe.CarboMaterialName = ce.CarboMaterialName;
            JsCe.Category = ce.Category;
            JsCe.SubCategory = ce.SubCategory;
            JsCe.AdditionalData = ce.AdditionalData;
            JsCe.Grade = ce.Grade;
            JsCe.LevelName = ce.LevelName;

            JsCe.RCDensity = ce.rcDensity;
            JsCe.Correction = ce.Correction;
            JsCe.GUID = ce.GUID;

            JsCe.Volume = ce.Volume;
            JsCe.Volume_Total = ce.Volume_Total;
            JsCe.Mass = ce.Mass;
            JsCe.Level = ce.Level;
            JsCe.Density = ce.Density;
            JsCe.Area = ce.Area;

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
        private static JsCarboElement ConvertoToJsCarboElement(CarboGroup grp, CarboProject carboProject)
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

            JsCe.RCDensity = 0;
            JsCe.Correction = grp.Correction;
            JsCe.GUID = "";

            JsCe.Volume = grp.Volume;
            JsCe.Volume_Total = grp.TotalVolume;

            JsCe.Area = 0;

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
                var JsonSerializer = new JavaScriptSerializer();
                JsonSerializer.MaxJsonLength = Int32.MaxValue;

                var json = JsonSerializer.Serialize(lcaProject);
                File.WriteAllText(path, json);

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;

        }

        public static bool openLCAx(string path, out CarboProject carboLifeProject)
        {
            bool result = false;

            carboLifeProject = new CarboProject();

            try
            {
                var JsonSerializer = new JavaScriptSerializer();
                JsonSerializer.MaxJsonLength = Int32.MaxValue;
                Lcax lcaxFile = null;

                var lcaxFile2 = JsonSerializer.DeserializeObject(path);

                lcaxFile = lcaxFile2 as Lcax;

                carboLifeProject = convertToCarboCalcProject(lcaxFile);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                carboLifeProject = null;
                return false;
            }

            result = true;
            return result;

        }



        public static string GetLCAxFileLocation()
        {
            string result = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "LCAx File (*.json)|*.json|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    result = openFileDialog.FileName;
                }
            }
            catch
            {
                return "";
            }

            return result;
        }


    }
}
