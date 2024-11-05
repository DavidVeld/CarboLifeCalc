using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System;
using System.Linq;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.CodeDom;
using System.Collections.Generic;
using Autodesk.Revit.DB.Structure;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace CarboLifeRevit
{
    public class ColourViewerHandler : IExternalEventHandler
    {

        private static Document doc;
        private static UIDocument uidoc;
        public ExternalEvent _revitEvent;

        private CarboGraphResult resultList;
        private static bool colourMeSwitch;
        private static bool colourOutOfBoundsSwitch;
        private static bool clearValueSwitch;

        private static bool createLegend;

        private CarboProject targetProject;
        private string parameterName;
        private string viewname;



        public int commandSwitch = 0; //0 = colour 1 = import 

        public bool drawLegendView { get; private set; }

        public CarboGraphResult resultModel { get { return resultList; } set { resultList = value; } }
        public ColourViewerHandler(UIApplication uiapp)
        {
            UIApplication app = uiapp;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;
            parameterName = "";

            _revitEvent = ExternalEvent.Create(this);
        }

        public void Execute(UIApplication uiapp)
        {

            try
            {
                if (doc != null)
                {
                    if (commandSwitch == 0)
                    {
                        ColourTheModelTransaction(doc);
                    }
                    else if(commandSwitch == 1)
                    {
                        ImportvaluesToElements(uiapp);
                    }
                    else if (commandSwitch == 3)
                    {
                        drawResultViewCmd(uiapp);
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

        private void drawResultViewCmd(UIApplication uiapp)
        {
            if (doc != null)
            {
                using (Transaction t = new Transaction(doc, "Create Results View"))
                {
                    t.Start();
                    
                    bool ok = CarboLegendCreator.CreateResultsView(targetProject, doc, viewname);
                    
                    t.Commit();
                }
            }
        }

        private void ColourTheModelTransaction(Document doc)
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

                    //Reset all coulours
                    foreach (CarboValues cv in resultList.entireProjectData)
                    {
                        Element el = doc.GetElement(new ElementId(cv.Id));

                        if (el != null)
                        {
                            doc.ActiveView.SetElementOverrides(el.Id, ogs);
                        }
                    }

                    //Colour them if required.
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

                        if (colourOutOfBoundsSwitch == true)
                        {
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

                        if (drawLegendView == true)
                        {
                            bool ok = CarboLegendCreator.CreateALegendForData(resultList, doc);
                        }

                    }


                    //coulour out of bounds max


                    doc.Regenerate();

                    t.Commit();
                }
            }
        }



        private void ImportvaluesToElements(UIApplication uiapp)
        {
            if (doc != null)
            {

                using (Transaction t = new Transaction(doc, "Import values to Model"))
                {
                    t.Start();

                    CarboLifeImportData.ParseDataToModel(uiapp, targetProject, parameterName, clearValueSwitch);

                    doc.Regenerate();

                    t.Commit();
                }
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
            return "CarboLifeCalc : Modify the Model";
        }

        public void ColourTheModel(CarboGraphResult colourResults, bool colourMe, bool colourOutOfBounds,bool createLegend)
        {
            if (doc != null && colourResults != null)
            {
                try
                {
                    resultList = colourResults;
                    colourMeSwitch = colourMe;
                    colourOutOfBoundsSwitch = colourOutOfBounds;
                    commandSwitch = 0;
                    drawLegendView = createLegend;
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    colourMeSwitch = false;
                    commandSwitch = 999;
                }
            }
        }

        public void Importvalues(CarboProject project, string paramname, bool clearValue)
        {
            if (doc != null && project != null)
            {
                try
                {
                    targetProject = project;
                    commandSwitch = 1;
                    parameterName = paramname;
                    clearValueSwitch = clearValue;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    colourMeSwitch = false;
                    commandSwitch = 999;
                    parameterName = "";
                    clearValueSwitch = false;
                }
            }
        }

        public void drawResultView(CarboProject carboProject, string viewName)
        {
            if (doc != null && carboProject != null)
            {
                try
                {
                    targetProject = carboProject;
                    commandSwitch = 3;
                    viewname = viewName;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    colourMeSwitch = false;
                    commandSwitch = 999;
                    parameterName = "";
                    clearValueSwitch = false;
                }
            }
        }
    }
}
    
    
