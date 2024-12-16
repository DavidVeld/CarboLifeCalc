using Autodesk.Revit.UI;
using System.Windows.Controls;
using System;
using Autodesk.Revit.DB;
using System.Windows;
using System.Collections.Generic;
using CarboCircle.data;
using System.Linq;

namespace CarboCircle
{
    public class CarboCircleHandler : IExternalEventHandler

    {
        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        public int commandSwitch = 0; //0 = existing 1 = proposed 2 = colourmatch 

        //private carboCircleProject collectedProject;
        private List<carboCircleElement> collectedElements;
        //private List<carboCircleElement> collectedVolumes;

        public UIApplication uiapp { get; }

        public CarboCircleHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

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
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Revit did not receive a valid command");
                    }
                }

                //window event
                DataReady?.Invoke(this, collectedElements);

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }


        }

        private void ImportElementsActiveView(UIApplication uiapp)
        {

            carboCircleSettings appSettings = new carboCircleSettings();

            List<carboCircleElement> collectedElementsBuffer = carboCircleRevitCommands.getElementsFromActiveView(uiapp, appSettings);
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
            //success

            //Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

        }




        /// <summary>
        /// 0 = No Action
        /// 1 = ImportElementsfromActiveView.
        /// 2 = ColourMatches
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

    }
}