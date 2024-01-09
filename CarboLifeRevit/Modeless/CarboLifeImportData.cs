using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;

namespace CarboLifeRevit
{
    public static class CarboLifeImportData
    {
        public static void ParseDataToModel(UIApplication app, CarboProject carboLifeProject, string parametername, bool clearValue)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;


            //This part will import the data from Carbo Life Calculator back into the BIM model
            //First we need to make sure we have the shared parameters loaded, if not we will create a shared parameter file and use this for the model.
            try
            {

                List<CarboElement> elementsFromGroups = carboLifeProject.getElementsFromGroups().ToList();

                int nonvalidelements = 0;
                bool block = false;

                if (elementsFromGroups.Count > 0)
                {
                    foreach (CarboElement ce in elementsFromGroups)
                    {
                        ElementId id = new ElementId(ce.Id);
                        Element testElement = doc.GetElement(id);

                        if (testElement != null)
                        {
                            Parameter carboPar = testElement.LookupParameter(parametername);
                            if (carboPar == null)
                            {
                                block = true;
                                nonvalidelements++;
                            }
                        }
                    }
                }
                //only give this message if you want to ADD values
                if (nonvalidelements > 0 && clearValue == false)
                {
                    //Justy a warning that the parameters will not be written to ALL the elements. 
                    var result = MessageBox.Show("The Parameter: '" + parametername + "' could not be found in " + nonvalidelements + " objects, do you want to continue? ", "Error", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        block = false;
                    }
                    else
                    {
                        MessageBox.Show("Add the Number parameter: '" + parametername + "' to your elements and try again ", "Error", MessageBoxButton.OK);
                    }
                }
                else if (clearValue == true)
                {
                    //clearing values always goes through.
                    block = false;
                }
                else
                {
                    //if you get here something was wrong, so stop please
                    block = false;
                }


                if (block == false)
                {
                    int ok = 0;
                    int notOk = 0;

                    foreach (CarboElement ce in elementsFromGroups)
                    {
                        ElementId id = new ElementId(ce.Id);
                        Element targetElement = doc.GetElement(id);

                        if (targetElement != null)
                        {

                            Parameter carboPar = targetElement.LookupParameter(parametername);
                            if (carboPar != null)
                            {
                                double valueNumber = 0;

                                if (clearValue == false)
                                {
                                    valueNumber = Math.Round(ce.EC_Cumulative, 3);
                                    carboPar.Set(valueNumber);
                                }
                                else
                                    carboPar.Set(valueNumber);

                                ok++;
                            }
                            else
                            {
                                notOk++;
                            }
                        }
                    }

                    MessageBox.Show(ok + " Elements succesfully updated" + Environment.NewLine + notOk + " Elements skipped", "Success", MessageBoxButton.OK);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }

        }

        [Obsolete]
        /*
        public static void xxx(UIApplication app, string parametername)
        {
            // Create Shared Parameter Routine: -->
            // 1: Check whether the shared parameter("parametername") has been defined.
            // 2: Share parameter file locates under sample directory of this .dll module.
            // 3: Add a group named "SDKSampleRoomScheduleGroup".
            // 4: Add a shared parameter named "External Room ID" to "Rooms" category, which is visible.
            //    The "External Room ID" parameter will be used to map to spreadsheet based room ID(which is unique)

            try
            {
                // create shared parameter file
                String modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String paramFile = modulePath + "\\CarbonSharedParams.txt";
                bool newFile = false;

                if (File.Exists(paramFile))
                {
                    newFile = true;

                    //File.Delete(paramFile);
                }
                else
                {
                    newFile = true;

                }

                FileStream fs = File.Create(paramFile);
                fs.Close();

                // cache application handle
                Autodesk.Revit.ApplicationServices.Application revitApp = app.Application;

                // prepare shared parameter file
                app.Application.SharedParametersFilename = paramFile;

                // open shared parameter file
                DefinitionFile parafile = revitApp.OpenSharedParameterFile();

                //This is a small alteration
                Definition roomSharedParamDef = null;
              
                // create a group
                DefinitionGroup apiGroup = parafile.Groups.Create("CarbonSharedParamsGroup");
                // create a visible "EmbodiedCarbon" of numder type.
                ExternalDefinitionCreationOptions ExternalDefinitionCreationOptions = new ExternalDefinitionCreationOptions("EmbodiedCarbon", ParameterTypeId.Number);
                roomSharedParamDef = apiGroup.Definitions.Create(ExternalDefinitionCreationOptions);
                

                //Assign the shared parameter to all the relevant categories
                //Get All Revit Categories
                Categories categories = app.ActiveUIDocument.Document.Settings.Categories;

                List<string> myCategories = new List<string>();
                myCategories.Clear();

                CategorySet categorySet = revitApp.Create.NewCategorySet();

                foreach (Category c in categories)
                {
                    if (c.AllowsBoundParameters)
                    {
                        if (c.CategoryType == CategoryType.Model)
                        {
                            categorySet.Insert(c);
                            myCategories.Add(c.Name);
                        }
                    }
                }

                // insert the new parameter
                InstanceBinding binding = revitApp.Create.NewInstanceBinding(categorySet);
                app.ActiveUIDocument.Document.ParameterBindings.Insert(roomSharedParamDef, binding);
                //return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create shared parameter: " + ex.Message);
            }
        }
        */
        internal static bool CreateParameterFromFile(UIApplication app, string parametername)
        {
            // Create Shared Parameter Routine: -->
            // 1: Check whether the shared parameter("parametername") has been defined.
            // 2: Share parameter file locates under sample directory of this .dll module.
            // 3: Add a group named "SDKSampleRoomScheduleGroup".
            // 4: Add a shared parameter named "External Room ID" to "Rooms" category, which is visible.
            //    The "External Room ID" parameter will be used to map to spreadsheet based room ID(which is unique)

            bool result = true;

            try
            {
                // create shared parameter file
                String modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String paramFile = modulePath + "\\CarbonSharedParams.txt";

                if (!File.Exists(paramFile))
                {
                    MessageBox.Show("Could not find shared parameter file : " + paramFile, "Error");
                    return false;
                }

                // cache application handle
                Autodesk.Revit.ApplicationServices.Application revitApp = app.Application;

                // prepare shared parameter file
                app.Application.SharedParametersFilename = paramFile;

                // open shared parameter file
                DefinitionFile parafile = revitApp.OpenSharedParameterFile();

                if (parafile == null)
                {
                    MessageBox.Show("Error opening shared parameter file", "Error");
                    return false;
                }

                //This is a small alteration
                Definition carboSharedParamDef = null;


                // collect the group
                DefinitionGroup retreivedDefGroup = parafile.Groups.get_Item("CarbonSharedParamsGroup");
                if (retreivedDefGroup == null)
                {
                    MessageBox.Show("Could not find CarbonSharedParamsGroup", "Error");
                    return false;
                }

                //Retreive the parameterdef
                Definition carboDefinition = retreivedDefGroup.Definitions.get_Item(parametername);
                if (carboDefinition == null)
                {
                    MessageBox.Show("Could not find parameter " + parametername + " in the shared parameter file", "Error");
                    return false;
                }

                //Assign the shared parameter to all the relevant categories
                //Get All Revit Categories
                Categories categories = app.ActiveUIDocument.Document.Settings.Categories;

                List<string> myCategories = new List<string>();
                myCategories.Clear();

                CategorySet categorySet = revitApp.Create.NewCategorySet();

                foreach (Category c in categories)
                {
                    if (c.AllowsBoundParameters)
                    {
                        if (c.CategoryType == CategoryType.Model)
                        {
                            categorySet.Insert(c);
                            myCategories.Add(c.Name);
                        }
                    }
                }

                // Bind the new parameter
                InstanceBinding binding = revitApp.Create.NewInstanceBinding(categorySet);
                app.ActiveUIDocument.Document.ParameterBindings.Insert(carboDefinition, binding);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create shared parameter: " + ex.Message);
            }

            return true;

        }

    }
}
