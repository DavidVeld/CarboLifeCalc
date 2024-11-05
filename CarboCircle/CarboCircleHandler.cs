using Autodesk.Revit.UI;
using System.Windows.Controls;
using System;
using Autodesk.Revit.DB;
using System.Windows;

namespace CarboCircle
{
    internal class CarboCircleHandler : IExternalEventHandler

    {
        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        public int commandSwitch = 0; //0 = colour 1 = import 

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

        private void ImportProposedElements(UIApplication uiapp)
        {
            MessageBox.Show("Import Proposed Elements");
        }

        private void ImportAvailableElements(UIApplication uiapp)
        {
            MessageBox.Show("Import Available Elements");
        }

        public string GetName()
        {
            throw new System.NotImplementedException();
        }
    }
}