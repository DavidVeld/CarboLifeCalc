using Autodesk.Revit.UI;
using System.Windows.Controls;
using System;
using Autodesk.Revit.DB;
using System.Windows;
using System.Collections.Generic;

namespace CarboCircle
{
    public class CarboCircleHandler : IExternalEventHandler

    {
        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        public int commandSwitch = 0; //0 = existing 1 = proposed 2 = colourmatch 

        private List<carboCircleElement> collectedElements;
        public CarboCircleHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            _revitEvent = ExternalEvent.Create(this);
        }

        public UIApplication uiapp { get; }

        public void Execute(UIApplication uiapp)
        {
            try
            {
                if (doc != null)
                {
                    if (commandSwitch == 0)
                    {
                        ImportAvailableElements(uiapp);
                    }
                    else if (commandSwitch == 1)
                    {
                        ImportProposedElements(uiapp);
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
        private void ImportAvailableElements(UIApplication uiapp)
        {
            MessageBox.Show("Import Available Elements");
        }

        private void ImportProposedElements(UIApplication uiapp)
        {
            MessageBox.Show("Import Proposed Elements");
        }

        private void ColourElements(UIApplication uiapp)
        {
            MessageBox.Show("Colour Matching Elements");
        }


        public void GrabData(int v)
        {
            commandSwitch = v;
        }

        public List<carboCircleElement> getCollectedElements()
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            if (collectedElements.Count > 0)
            {
                foreach (carboCircleElement element in collectedElements)
                {
                    result.Add(element.Copy());
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