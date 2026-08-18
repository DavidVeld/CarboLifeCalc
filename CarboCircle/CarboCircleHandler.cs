using Autodesk.Revit.UI;
using System.Windows.Controls;
using System;
using Autodesk.Revit.DB;
using CarboLifeRevitCompat;
using System.Windows;
using System.Collections.Generic;
using CarboCircle.data;
using System.Linq;
using System.Runtime;
using System.IO;

namespace CarboCircle
{
    public class CarboCircleHandler : IExternalEventHandler

    {
        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        public int commandSwitch = 0; //0 = existing 1 = proposed 2 = colourmatch 

        /// <summary>
        /// How the pending import should pick its elements: one of the
        /// <see cref="carboCircleExtractionMethod"/> values.
        ///
        /// Part of the request, not of the settings. Both sides of the tool import through
        /// the same command, each wanting a different method, so this cannot live on the
        /// shared settings object - and it must never be read back from the settings file,
        /// which only remembers the two per-side preferences.
        /// </summary>
        private string requestedExtractionMethod = carboCircleExtractionMethod.AllVisibleInView;

        carboCircleSettings importSettings = null;
        carboCircleProject activeProject = null;
        carboCircleMatchElement matchedPair = null;

        //private carboCircleProject collectedProject;
        private List<carboCircleElement> collectedElements;
        //private List<carboCircleElement> collectedVolumes;

       // public UIApplication uiapp { get; }

        public CarboCircleHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            importSettings = new carboCircleSettings();
            activeProject = new carboCircleProject();
            matchedPair = new carboCircleMatchElement();

            //_revitEvent = ExternalEvent.Create(this);
        }

        public event EventHandler<List<carboCircleElement>> DataReady;

        public event EventHandler<string> ImageReady;
        public string imagePath;
        /// <summary>
        /// 0 = No Action
        /// 1 = ImportElementsfromActiveView.
        /// 2 = ColourView
        /// 3 = SelectPair
        /// 4 = GetactiveViewImage
        /// </summary>
        /// <param name="v"></param>
        public void SetSwitch(int v)
        {
            commandSwitch = v;
        }

        /// <summary>
        /// Sets how the next import should pick its elements. Call this alongside
        /// <see cref="SetSwitch"/> before raising the external event.
        /// </summary>
        internal void SetExtractionMethod(string method)
        {
            requestedExtractionMethod = string.IsNullOrEmpty(method)
                ? carboCircleExtractionMethod.AllVisibleInView
                : method;
        }
        public void Execute(UIApplication uiapp)
        {
            try
            {
                uidoc = uiapp.ActiveUIDocument;
                doc = uidoc.Document;


                if (doc != null)
                {
                    if (commandSwitch == 0)
                    {
                        //No Action
                    }
                    else if (commandSwitch == 1)
                    {
                        ImportElementsActiveView(uiapp);
                        //Push event in the dialogwindow to update the listbox:
                        DataReady?.Invoke(this, collectedElements);
                    }
                    else if (commandSwitch == 2)
                    {
                        VisualiseElementsInView(uiapp);
                    }
                    else if (commandSwitch == 3)
                    {
                        SelectPair(uiapp);
                    }
                    else if (commandSwitch == 4)
                    {
                        ExportImage(uiapp);
                        ImageReady?.Invoke(this, imagePath);

                    }
                    else
                    {
                        TaskDialog.Show("Error", "Revit did not receive a valid command");
                    }
                }


            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }


        }

        private void ExportImage(UIApplication uiapp)
        {
            //get temp Filepath
            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);
            string tempImgpath = MyAssemblyDir + "\\tempCircleImg.jpg";
            try
            {
                if (File.Exists(tempImgpath))
                { File.Delete(tempImgpath); }

                ImageExportOptions options = new ImageExportOptions();
                options.FilePath = tempImgpath;
                options.HLRandWFViewsFileType = ImageFileType.PNG;
                options.PixelSize = 1024;
                options.FitDirection = FitDirectionType.Horizontal;
                options.ExportRange = ExportRange.CurrentView;

                doc.ExportImage(options);
                imagePath = tempImgpath;
            }
            catch
            {
                imagePath = null;
            }
        }

        private void SelectPair(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            if (matchedPair != null)
            {
                ElementId element1 = matchedPair.mined_id.ToElementId();
                ElementId element2 = matchedPair.required_id.ToElementId();
                List<ElementId> elements = new List<ElementId>();
                elements.Add(element1);
                elements.Add(element2);

                uidoc.Selection.SetElementIds(elements);
                uidoc.RefreshActiveView();

            }

        }

        private void VisualiseElementsInView(UIApplication uiapp)
        {
            try
            {
                bool ok = carboCircleRevitCommands.visualiseElements(uiapp, activeProject);
            }
            catch (Exception ex)
            {

            }
        }

        private void ImportElementsActiveView(UIApplication uiapp)
        {
            carboCircleImportLog log = new carboCircleImportLog();
            log.ExtractionMethod = requestedExtractionMethod;

            if (importSettings == null)
            {
                log.Fail("No settings were handed to the import.");
                reportImport(log);
                return;
            }

            try
            {
                List<ElementId> ids = new List<ElementId>();


                List<carboCircleElement> collectedElementsBuffer = carboCircleRevitCommands.getElementsFromActiveView(uiapp, importSettings, requestedExtractionMethod, log);
                collectedElements = new List<carboCircleElement>();

                if (collectedElementsBuffer != null)
                {
                    if (collectedElementsBuffer.Count > 0)
                    {
                        collectedElements = new List<carboCircleElement>();
                        collectedElements.Clear();

                        foreach (carboCircleElement ccEl in collectedElementsBuffer)
                        {
                            collectedElements.Add(ccEl.Copy());
                            ids.Add(ccEl.id.ToElementId());
                        }
                    }
                    else
                    {
                        collectedElements = new List<carboCircleElement>();
                    }

                }
                else
                {
                    collectedElements = null;
                }

                if (collectedElements != null && ids.Count > 1)
                {
                    uidoc.Selection.SetElementIds(ids);
                    uidoc.RefreshActiveView();
                }
            }
            catch (Exception ex)
            {
                //An import that died half way used to leave the window untouched and
                //silent, which is indistinguishable from a model with nothing in it.
                log.Fail("Reading the model", ex);
                collectedElements = null;
            }

            log.ElementsCollected = collectedElements == null ? 0 : collectedElements.Count;
            reportImport(log);
        }

        /// <summary>
        /// Tells the user what the import did, but only when they would otherwise be left
        /// guessing.
        ///
        /// An import that worked says so by filling the lists and turning the step button
        /// green, so a dialog on top of that is just something to dismiss. What needs
        /// saying out loud is an import that came back with nothing, or one that hit a real
        /// failure - those are indistinguishable from a tool that quietly did nothing,
        /// which is the whole reason this reporting exists.
        ///
        /// Elements merely skipped do not count: a model always has some the import cannot
        /// use, and interrupting for those would put the dialog back on every run.
        /// </summary>
        private static void reportImport(carboCircleImportLog log)
        {
            if (log.ElementsCollected > 0 && !log.HasFailures())
                return;

            try
            {
                TaskDialog dialog = new TaskDialog("CarboCircle import");
                dialog.MainInstruction = log.Headline();
                dialog.MainContent = log.Details();
                dialog.Show();
            }
            catch
            {
                //Never let the report about a failure become a second failure.
            }
        }

        public List<carboCircleElement> getCollectedDataElements()
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            if (collectedElements != null)
            {
                if (collectedElements.Count > 0)
                {
                    foreach (carboCircleElement element in collectedElements)
                    {
                        result.Add(element.Copy());
                    }
                }
            }
            return result;
        }

        public List<carboCircleElement> getCollectedVolumeElements()
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            if (collectedElements != null)
            {
                if (collectedElements.Count > 0)
                {
                    foreach (carboCircleElement element in collectedElements)
                    {
                        result.Add(element.Copy());
                    }
                }
            }
            return result;
        }

        public string GetName()
        {
            return "CarboCircle : Reuse";
        }

        internal void SetSettings(carboCircleProject project)
        {
            activeProject = project;
            importSettings = project.settings;
        }

        internal void SetSettings(carboCircleMatchElement pair)
        {
            matchedPair = pair.Copy();
        }
    }
}