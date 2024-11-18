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

        private List<carboCircleElement> collectedElements;
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
                        //End
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
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }

            DataReady?.Invoke(this, collectedElements);

        }

        private void ImportElementsActiveView(UIApplication uiapp)
        {

            carboCircleSettings appSettings = new carboCircleSettings();

            List<carboCircleElement> collectedElementsBuffer = getElementsFromActiveView(uiapp, appSettings);

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
                    collectedElements = null;
                }
            }
            else
            {
                collectedElements = null;
            }
            //success

            Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

        }

        private List<carboCircleElement> getElementsFromActiveView(UIApplication uiapp, carboCircleSettings appSettings)
        {
            List<carboCircleElement> resultCollection = new List<carboCircleElement>();

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            IEnumerable<Element> beamCollector = new FilteredElementCollector(doc,doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming)
                .WhereElementIsNotElementType().ToElements();

            IEnumerable<Element> columnCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).
                WhereElementIsNotElementType().ToElements();

            if(beamCollector.Count() > 0)
            {
                foreach (Element el in beamCollector)
                {
                    try
                    {
                        FamilyInstance inst = el as FamilyInstance;
                        if (inst != null)
                        {
                            FamilySymbol family = inst.Symbol;
                            List<ElementId> materials = inst.GetMaterialIds(false).ToList();
                            Material material = null;
                            if(materials.Count > 0)
                            {
                                material = doc.GetElement(materials[0]) as Material;
                            }


                            Parameter lengthParam = inst.LookupParameter("Cut Length");
                            double length = (lengthParam.AsDouble() * 304.8)/1000;

                            carboCircleElement ccEl = new carboCircleElement();
                            ccEl.id = inst.Id.IntegerValue;
                            ccEl.GUID = inst.UniqueId;
                            ccEl.name = el.Name;
                            ccEl.catergory = inst.Category.Name;
                            ccEl.length = length;

                            if (material != null)
                                ccEl.materialName = material.Name;

                            resultCollection.Add(ccEl);
                        }
                    }
                    catch
                    { }
                }
            }

            if (columnCollector.Count() > 0)
            {
                foreach (Element el in columnCollector)
                {
                    try
                    {
                        carboCircleElement ccEl = new carboCircleElement();
                        ccEl.name = el.Name;
                        resultCollection.Add(ccEl);
                    }
                    catch
                    { }
                }
            }

            return resultCollection;

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

        public List<carboCircleElement> getCollectedElements()
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