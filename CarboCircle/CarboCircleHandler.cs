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

        /// <summary>
        /// Raised when an import has finished. The result carries the elements AND which side
        /// asked for them, so a subscriber never has to remember which request it made.
        /// </summary>
        //Internal, like the result it carries: nothing outside this assembly subscribes, and
        //the payload type is internal too.
        internal event EventHandler<carboCircleImportResult> DataReady;

        /// <summary>
        /// Which side of the project the pending import is for: false = mine, true = proposed.
        ///
        /// Part of the request, like the extraction method and the report path. See
        /// carboCircleImportResult for why remembering this on the window was a bug.
        /// </summary>
        private bool requestedProjectSide;

        /// <summary>
        /// Where the pending report should be written.
        ///
        /// Part of the request, like the extraction method. It used to live on the window,
        /// which is exactly what broke the report - see CreateReport below.
        /// </summary>
        private string requestedReportPath = string.Empty;

        /// <summary>
        /// 0 = No Action
        /// 1 = ImportElementsfromActiveView.
        /// 2 = ColourView
        /// 3 = SelectPair
        /// 4 = CreateReport
        /// 5 = WriteReuseIds
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

        /// <summary>
        /// Sets the file the next report should be written to. Call this alongside
        /// <see cref="SetSwitch"/> before raising the external event.
        /// </summary>
        internal void SetReportPath(string path)
        {
            requestedReportPath = path == null ? string.Empty : path;
        }

        /// <summary>
        /// Sets which side of the project the next import is for. Call this alongside
        /// <see cref="SetSwitch"/> before raising the external event.
        /// </summary>
        internal void SetImportSide(bool forProject)
        {
            requestedProjectSide = forProject;
        }
        public void Execute(UIApplication uiapp)
        {
            try
            {
                uidoc = uiapp.ActiveUIDocument;
                doc = uidoc.Document;


                if (doc != null)
                {
                    //Every request is a trip into API context, which is the only place a
                    //Document can be read - so it is the only place the parameter list the
                    //settings dialog offers can be refreshed. Refreshing on each request
                    //keeps it current after the user switches model or adds a shared
                    //parameter, neither of which the add-in is told about.
                    carboCircleRevitCommands.collectParameterNames(doc);

                    if (commandSwitch == 0)
                    {
                        //No Action
                    }
                    else if (commandSwitch == 1)
                    {
                        ImportElementsActiveView(uiapp);
                        //Push event in the dialogwindow to update the listbox:
                        DataReady?.Invoke(this, new carboCircleImportResult(collectedElements, requestedProjectSide));
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
                        CreateReport(uiapp);
                    }
                    else if (commandSwitch == 5)
                    {
                        WriteReuseIds();
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

        /// <summary>
        /// Writes the report for the file this request named, and opens it.
        ///
        /// This used to be split across two objects: the handler exported an image and
        /// raised an ImageReady event, and the window wrote the report in response. The
        /// path lived on the window - and because the window hides rather than closes and
        /// never unsubscribes, every window ever opened answered that event, stale ones
        /// included, whose path was still empty. That is where "Empty path name is not
        /// legal" came from. Nothing about writing a report needs the window, so it happens
        /// here, against the path and the project that arrived with the request.
        /// </summary>
        private void CreateReport(UIApplication uiapp)
        {
            string imageFailure;
            string imageFile = exportActiveViewImage(out imageFailure);
            string imageAsString = "";

            if (imageFile != null)
                imageAsString = carboCircleReportUtils.getImageAsString(imageFile);

            string message;
            bool ok = carboCircleReportUtils.ExportReport(activeProject, imageAsString, requestedReportPath, out message);

            //A report without its picture is still a report, so a failed image is a note
            //rather than a reason to abandon the whole thing.
            if (ok && !string.IsNullOrEmpty(imageFailure))
                message += Environment.NewLine + Environment.NewLine + imageFailure;

            if (ok)
            {
                string openFailure;

                if (!carboCircleReportUtils.OpenReport(requestedReportPath, out openFailure))
                    message += Environment.NewLine + Environment.NewLine + openFailure;
            }

            discardTempImages();

            try
            {
                TaskDialog dialog = new TaskDialog("CarboCircle report");
                dialog.MainInstruction = ok ? "Report created." : "The report was not created.";
                dialog.MainContent = message;
                dialog.Show();
            }
            catch
            {
                //Never let the report about a failure become a second failure.
            }
        }

        /// <summary>
        /// Exports the active view and hands back the image file Revit actually wrote, or
        /// null with a reason.
        ///
        /// Two things worth knowing. Revit appends its own extension to match the chosen
        /// file type, so the old code - which asked for PNG while naming the file
        /// tempCircleImg.jpg - could leave the image somewhere the caller was not looking.
        /// The export therefore goes into a folder of its own, and whatever turns up in
        /// there is the answer. And that folder is under the user temp directory rather
        /// than beside the dll, which is read-only wherever the add-in is installed for
        /// all users.
        /// </summary>
        private string exportActiveViewImage(out string failure)
        {
            failure = "";

            try
            {
                string folder = tempImageFolder();

                discardTempImages();
                Directory.CreateDirectory(folder);

                ImageExportOptions options = new ImageExportOptions();
                options.FilePath = Path.Combine(folder, "view");
                options.HLRandWFViewsFileType = ImageFileType.PNG;
                options.PixelSize = 1024;
                options.FitDirection = FitDirectionType.Horizontal;
                options.ExportRange = ExportRange.CurrentView;

                doc.ExportImage(options);

                string[] produced = Directory.GetFiles(folder);

                if (produced.Length > 0)
                    return produced[0];

                failure = "Revit did not produce an image of the active view, so the report has no picture.";
            }
            catch (Exception ex)
            {
                failure = "The active view could not be exported as an image, so the report has no picture: " + ex.Message;
            }

            return null;
        }

        private static string tempImageFolder()
        {
            return Path.Combine(Path.GetTempPath(), "CarboCircleReport");
        }

        private static void discardTempImages()
        {
            try
            {
                string folder = tempImageFolder();

                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
            catch
            {
                //A leftover temp image is not worth reporting.
            }
        }

        /// <summary>
        /// Stamps the reuse id onto both ends of every matched pair.
        ///
        /// Here rather than on the window, for the same reason the report moved here: this
        /// is the only place a Document may be written to, and the project and the parameter
        /// name arrived with the request rather than being fetched from a window that may no
        /// longer be the one the user is looking at.
        /// </summary>
        private void WriteReuseIds()
        {
            string parameterName = importSettings == null
                ? carboCircleSettings.DefaultReuseIdParameter
                : importSettings.reuseIdParameterOrDefault();

            string report;
            bool ok = carboCircleRevitCommands.writeReuseIds(doc, activeProject, parameterName, out report);

            try
            {
                TaskDialog dialog = new TaskDialog("CarboCircle reuse IDs");
                dialog.MainInstruction = ok ? "Reuse IDs written." : "No reuse IDs were written.";
                dialog.MainContent = report;
                dialog.Show();
            }
            catch
            {
                //Never let the report about a failure become a second failure.
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


                List<carboCircleElement> collectedElementsBuffer = carboCircleRevitCommands.getElementsFromActiveView(uiapp, importSettings, requestedExtractionMethod, requestedProjectSide, log);
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