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

        public ExternalEvent _revitEvent;


        public CarboGraphResult resultModel { get { return resultList; } set { resultList = value; } }
        public ColourViewerHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            _revitEvent = ExternalEvent.Create(this);
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
                        
                        //applies for all overrides
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();

                        //Rest all coulours
                        foreach (CarboValues cv in resultList.entireProjectData)
                        {
                            Element el = doc.GetElement(new ElementId(cv.Id));

                            if (el != null)
                            {
                                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                            }
                        }


                        if (colourMeSwitch == true)
                        {
                            foreach (CarboValues cv in resultList.validData)
                            {
                                Element el = doc.GetElement(new ElementId(cv.Id));
                                if (el != null)
                                {
                                    //if switch is false reset overrides.
                                    ogs = getOverrideObject(cv, solidFillPattern.Id);

                                }

                                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                            }

                            foreach (CarboValues cv in resultList.outOfBoundsMaxData)
                            {
                                Element el = doc.GetElement(new ElementId(cv.Id));
                                if (el != null)
                                {
                                    //if switch is false reset overrides.
                                    ogs = getOverrideObject(cv, solidFillPattern.Id);

                                }

                                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                            }

                            foreach (CarboValues cv in resultList.outOfBoundsMinData)
                            {
                                Element el = doc.GetElement(new ElementId(cv.Id));
                                if (el != null)
                                {
                                    //if switch is false reset overrides.
                                    ogs = getOverrideObject(cv, solidFillPattern.Id);

                                }

                                doc.ActiveView.SetElementOverrides(el.Id, ogs);
                            }

                        }
                        

                        //coulour out of bounds max


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

        private OverrideGraphicSettings getOverrideObject(CarboValues cv, ElementId id)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();

            ogs.SetSurfaceForegroundPatternId(id);
            ogs.SetSurfaceForegroundPatternColor(new Color(cv.r, cv.g, cv.b));

            ogs.SetCutForegroundPatternId(id);
            ogs.SetCutForegroundPatternColor(new Color(cv.r, cv.g, cv.b));

            return ogs;
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
    
    
