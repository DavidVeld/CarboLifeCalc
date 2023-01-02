using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System;
using System.Linq;
using CarboLifeAPI;

namespace CarboLifeRevit
{
    public class ColourViewerhandler : IExternalEventHandler
    {

        private static Document _doc;
        private static UIApplication _uiapp;
        private static Autodesk.Revit.ApplicationServices.Application _app;
        private static UIDocument _uidoc;

        private CarboGraphResult resultList;

        public CarboGraphResult resultModel { get { return resultList; } set { resultList = value; } }

        public void Execute(UIApplication uiapp)
        {
            _uiapp = uiapp;
            _uidoc = uiapp.ActiveUIDocument;
            _app = uiapp.Application;
            _doc = uiapp.ActiveUIDocument.Document;

            try
            {

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to execute the external event.\n" + ex.Message, "Execute Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        public string GetName()
        {
            return "Colour the Model";
        }

        private void ColourTheModel(CarboGraphResult results)
        {
            var view = _doc.ActiveView;

            TaskDialog.Show("Task Started", "Task Started");

            try
            {
                using (Transaction t = new Transaction(_doc, "Set Element Override"))
                {
                    t.Start();

                    //Here we colour the model


                    t.Commit();
                }
            }
            catch
            {
                throw;
            }

        }

    }
}
    
    
