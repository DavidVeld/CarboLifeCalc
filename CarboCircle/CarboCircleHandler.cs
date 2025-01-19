using Autodesk.Revit.UI;
using System.Windows.Controls;
using System;
using Autodesk.Revit.DB;
using System.Windows;
using System.Collections.Generic;
using CarboCircle.data;
using System.Linq;
using System.Runtime;

namespace CarboCircle
{
    public class CarboCircleHandler : IExternalEventHandler

    {
        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        public int commandSwitch = 0; //0 = existing 1 = proposed 2 = colourmatch 
        carboCircleSettings importSettings = null;
        carboCircleProject activeProject = null;
        carboCircleMatchElement matchedPair = null;

        //private carboCircleProject collectedProject;
        private List<carboCircleElement> collectedElements;
        //private List<carboCircleElement> collectedVolumes;

        public UIApplication uiapp { get; }

        public CarboCircleHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            importSettings = new carboCircleSettings();
            activeProject = new carboCircleProject();
            matchedPair = new carboCircleMatchElement();

            _revitEvent = ExternalEvent.Create(this);
        }

        public event EventHandler<List<carboCircleElement>> DataReady;


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

        private void SelectPair(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            if(matchedPair != null)
            {
                ElementId element1 = new ElementId(matchedPair.mined_id);
                ElementId element2 = new ElementId(matchedPair.required_id);
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
            catch   (Exception ex)
            {

            }
        }

        private void ImportElementsActiveView(UIApplication uiapp)
        {
            if (importSettings != null)
            {
                List<carboCircleElement> collectedElementsBuffer = carboCircleRevitCommands.getElementsFromActiveView(uiapp, importSettings);
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

            }
        }




        /// <summary>
        /// 0 = No Action
        /// 1 = ImportElementsfromActiveView.
        /// 2 = ColourView
        /// 3 = SelectPair
        /// </summary>
        /// <param name="v"></param>
        public void SetSwitch(int v)
        {
            commandSwitch = v;
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

        internal void SetSettings(carboCircleSettings settings)
        {
            importSettings = settings.Copy();
        }

        internal void SetSettings(carboCircleProject project)
        {
            activeProject = project;
        }

        internal void SetSettings(carboCircleMatchElement pair)
        {
            matchedPair = pair.Copy();

        }
    }
}