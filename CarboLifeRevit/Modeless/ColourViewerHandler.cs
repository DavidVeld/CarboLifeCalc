using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System;
using System.Linq;
using CarboLifeAPI;
using CarboLifeAPI.Data;

namespace CarboLifeRevit
{
    public class ColourViewerHandler : IExternalEventHandler
    {

        private static Document doc;
        private static UIDocument uidoc;

        private CarboGraphResult resultList;
        private static bool colourMeSwitch;
        public CarboGraphResult resultModel { get { return resultList; } set { resultList = value; } }
        public ColourViewerHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;
        }

        public void Execute(UIApplication uiapp)
        {

            try
            {
                if (doc != null)
                {
                    var view = doc.ActiveView;

                    using (Transaction t = new Transaction(doc, "Colour The Model"))
                    {
                        t.Start();

                        FilteredElementCollector elements = new FilteredElementCollector(doc);
                        FillPatternElement solidFillPattern = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

                        //Here we colour the model
                        foreach (CarboValues cv in resultList.elementData)
                        {
                            Element el = doc.GetElement(new ElementId(cv.Id));
                            if(el != null)
                            {
                                OverrideGraphicSettings ogs = new OverrideGraphicSettings();

                                //if switch is false reset overrides.
                                if (colourMeSwitch == true)
                                {
                                    ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                                    ogs.SetSurfaceForegroundPatternColor(new Color(cv.r, cv.g, cv.b));

                                    ogs.SetCutForegroundPatternId(solidFillPattern.Id);
                                    ogs.SetCutForegroundPatternColor(new Color(cv.r, cv.g, cv.b));
                                }
                                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                            }
                        }
                        doc.Regenerate();

                        t.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
        {
            return "Colour the Model";
        }

        public void ColourTheModel(CarboGraphResult colourResults, bool colourMe)
        {
            if (doc != null && colourResults != null)
            {
                try
                {
                    resultList = colourResults;
                    colourMeSwitch = colourMe;
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    colourMeSwitch = false;

                }
            }
        }
    }
}
    
    
