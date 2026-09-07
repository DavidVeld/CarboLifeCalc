using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;

namespace CarboLifeRevit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CheckCarbonParams : IExternalCommand
    {
        /// <summary>
        /// Parameters that describe the project as a whole rather than any element in it.
        /// They are bound to Project Information only, as instance parameters, so they turn up
        /// once under Manage &gt; Project Information instead of on every wall in the model.
        /// </summary>
        private static readonly string[] projectInformationParameters =
        {
            CarboGroupSettings.DefaultGIAParameterName,
            CarboGroupSettings.DefaultGIANewParameterName
        };

        /// <summary>
        /// Parameters that have to be readable per element, so they cannot be type bound.
        /// </summary>
        private static readonly string[] instanceParameters =
        {
            "CLC_EmbodiedCarbon",
            "CLC_IsSubstructure",
            "CLC_MaterialGrade"
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            // 1. Get the path of the current DLL and find the .txt file in the same folder
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string carbonFilePath = Path.Combine(assemblyDir, "CarbonSharedParams.txt");

            if (!File.Exists(carbonFilePath))
            {
                TaskDialog.Show("Error", "Shared Parameter file not found at: " + carbonFilePath);
                return Result.Failed;
            }

            // 2. User Confirmation
            TaskDialog mainDialog = new TaskDialog("CarboLife");
            mainDialog.MainInstruction = "Would you like to check if the default CarboLife shared parameters are applied in this file?";
            mainDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;

            if (mainDialog.Show() != TaskDialogResult.Yes) return Result.Cancelled;

            // 3. Store original path to be a "good citizen"
            string originalPath = uiApp.Application.SharedParametersFilename;

            try
            {
                uiApp.Application.SharedParametersFilename = carbonFilePath;
                DefinitionFile defFile = uiApp.Application.OpenSharedParameterFile();

                if (defFile == null) return Result.Failed;

                // 4. Identify all model categories that allow parameters
                CategorySet catSet = uiApp.Application.Create.NewCategorySet();
                foreach (Category cat in doc.Settings.Categories)
                {
                    if (cat.CategoryType == CategoryType.Model && cat.AllowsBoundParameters)
                    {
                        catSet.Insert(cat);
                    }
                }

                // The project wide parameters go to Project Information on its own. Bound to the
                // set above they would sit on every element in the model, and a single GIA typed
                // into one wall is not what anyone is looking for.
                CategorySet projectInfoSet = uiApp.Application.Create.NewCategorySet();
                Category projectInfoCategory = GetCategory(doc, BuiltInCategory.OST_ProjectInformation);

                if (projectInfoCategory != null)
                    projectInfoSet.Insert(projectInfoCategory);

                using (Transaction t = new Transaction(doc, "Add CarboLife Parameters"))
                {
                    t.Start();

                    foreach (DefinitionGroup group in defFile.Groups)
                    {
                        foreach (Definition def in group.Definitions)
                        {
                            bool isProjectInformation = Array.IndexOf(projectInformationParameters, def.Name) >= 0;
                            bool isBound = IsParameterBound(doc, def.Name);

                            // A project wide parameter bound to anything other than Project
                            // Information is no use to the import, which only ever looks there.
                            // Left alone it would also be the one case the tool cannot fix, so
                            // it is rebound rather than skipped as already present.
                            bool needsRebinding = isProjectInformation && isBound
                                && IsBoundToProjectInformation(doc, def.Name, projectInfoCategory) == false;

                            if (isBound == true && needsRebinding == false)
                                continue;

                            ElementBinding binding;

                            if (isProjectInformation)
                            {
                                // Project Information holds a single element, so this has to
                                // be an instance binding. Nothing to bind to when the
                                // category is missing, which is the case in a family.
                                if (projectInfoSet.IsEmpty)
                                    continue;

                                binding = uiApp.Application.Create.NewInstanceBinding(projectInfoSet);
                            }
                            else if (Array.IndexOf(instanceParameters, def.Name) >= 0)
                            {
                                // Create Instance Binding
                                binding = uiApp.Application.Create.NewInstanceBinding(catSet);
                            }
                            else
                            {
                                // Create Type Binding for all other parameters
                                binding = uiApp.Application.Create.NewTypeBinding(catSet);
                            }

                            // Insert the binding into the document under the "Data" group
                            // Note: PG_DATA is for Revit < 2024. Use GroupTypeId.Data for 2024+
                            if (needsRebinding == true)
                                doc.ParameterBindings.ReInsert(def, binding, GroupTypeId.Data);
                            else
                                doc.ParameterBindings.Insert(def, binding, GroupTypeId.Data);
                        }
                    }
                    t.Commit();
                }

                TaskDialog.Show("Success", "CarboLife parameters are now loaded and bound to model categories.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            finally
            {
                // 6. Restore original user settings
                uiApp.Application.SharedParametersFilename = originalPath;
            }
        }

        /// <summary>
        /// Looks a built in category up without throwing when the document does not carry it.
        /// </summary>
        private static Category GetCategory(Document doc, BuiltInCategory builtInCategory)
        {
            try
            {
                return Category.GetCategory(doc, builtInCategory);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// True when the named parameter is already an instance parameter of Project Information.
        /// </summary>
        private static bool IsBoundToProjectInformation(Document doc, string name, Category projectInfoCategory)
        {
            if (projectInfoCategory == null)
                return false;

            DefinitionBindingMapIterator it = doc.ParameterBindings.ForwardIterator();
            it.Reset();

            while (it.MoveNext())
            {
                if (it.Key.Name != name)
                    continue;

                InstanceBinding binding = it.Current as InstanceBinding;

                return binding != null && binding.Categories.Contains(projectInfoCategory);
            }

            return false;
        }

        // Helper to check if a parameter name is already in the BindingMap
        private bool IsParameterBound(Document doc, string name)
        {
            BindingMap map = doc.ParameterBindings;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Key.Name == name) return true;
            }
            return false;
        }
    }
}