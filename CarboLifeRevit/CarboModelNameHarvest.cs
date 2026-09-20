using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeRevitCompat;
using CarboLifeUI.UI;

namespace CarboLifeRevit
{
    /// <summary>
    /// Reads out of the model everything the import settings dialog can be asked to name: the
    /// instance and type parameters the elements carry, the Project Information parameters, the
    /// worksets and the phases. Handed to <see cref="CarboModelNames"/>, where the dialog reads
    /// them back without needing Revit types.
    ///
    /// WHY BEFORE THE DIALOG RATHER THAN DURING THE IMPORT
    ///
    /// Every one of these names is looked up and, when it is absent, silently skipped: the import
    /// finishes and produces numbers either way. A substructure parameter that this model does not
    /// carry does not fail, it just makes nothing substructure. The only moment that can be said
    /// out loud is before the import is started, which means before the dialog is shown.
    /// </summary>
    public static class CarboModelNameHarvest
    {
        /// <summary>
        /// Fills <see cref="CarboModelNames"/> from the document the import will run over.
        /// Never throws and never interrupts: not being able to list what the model holds is a
        /// reason to say nothing about missing names, not a reason to stop the user importing.
        /// </summary>
        public static void Collect(UIApplication app)
        {
            if (app == null || app.ActiveUIDocument == null || app.ActiveUIDocument.Document == null)
            {
                CarboModelNames.Clear();
                return;
            }

            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            SortedSet<string> instanceParameters = NewSet();
            SortedSet<string> typeParameters = NewSet();
            SortedSet<string> projectInformation = NewSet();
            SortedSet<string> worksets = NewSet();
            SortedSet<string> phases = NewSet();

            try
            {
                CollectElementParameters(uidoc, doc, instanceParameters, typeParameters);
            }
            catch (Exception)
            {
                //Whatever was gathered before the fault is still worth offering.
            }

            try
            {
                AddParameterNames(doc.ProjectInformation, projectInformation);
            }
            catch (Exception)
            {
            }

            try
            {
                CollectWorksets(doc, worksets);
            }
            catch (Exception)
            {
            }

            try
            {
                CollectPhases(doc, phases);
            }
            catch (Exception)
            {
            }

            CarboModelNames.SetNames(instanceParameters, typeParameters, projectInformation,
                                     worksets, phases);
        }

        private static SortedSet<string> NewSet()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Walks the same elements the import will walk - the current selection when there is one,
        /// the active view otherwise - and notes what parameters they carry.
        ///
        /// ONE ELEMENT PER TYPE IS ENOUGH, and that is what makes this quick enough to run on the
        /// way to a dialog. Which parameters an element has is decided by its category bindings
        /// and its family, both of which every instance of a type shares, so the second wall of a
        /// type has nothing to add that the first did not. On a full building model that turns
        /// hundreds of thousands of parameter reads into a few thousand.
        /// </summary>
        private static void CollectElementParameters(UIDocument uidoc, Document doc,
                                                     SortedSet<string> instanceParameters,
                                                     SortedSet<string> typeParameters)
        {
            //Element ids of the types already looked at, and of the categories already seen for
            //elements that have no type at all.
            HashSet<long> seenTypes = new HashSet<long>();
            HashSet<long> seenTypelessCategories = new HashSet<long>();

            foreach (Element el in ElementsToRead(uidoc, doc))
            {
                if (el == null)
                    continue;

                try
                {
                    ElementId typeId = el.GetTypeId();

                    //InvalidElementId is negative, every real id is positive. Compared as a
                    //number rather than against InvalidElementId itself, which is a different
                    //type between the two Revit versions this builds against.
                    bool hasType = typeId != null && IdValue(typeId) > 0;

                    if (hasType == true)
                    {
                        if (seenTypes.Add(IdValue(typeId)) == false)
                            continue;
                    }
                    else
                    {
                        //System elements without a type still carry instance parameters, and they
                        //are bound per category, so one of each category answers for all of them.
                        long categoryId = el.Category == null ? -1 : IdValue(el.Category.Id);

                        if (seenTypelessCategories.Add(categoryId) == false)
                            continue;
                    }

                    AddParameterNames(el, instanceParameters);

                    if (hasType == true)
                        AddParameterNames(doc.GetElement(typeId), typeParameters);
                }
                catch (Exception)
                {
                    //One awkward element is not worth losing the rest of the list over.
                }
            }
        }

        /// <summary>
        /// The elements the import would read: what is selected, or everything in the active view.
        /// Mirrors CarboLifeRevitImport.CollectVisibleorSelectedElements, so the names offered are
        /// the names that would really be looked up.
        /// </summary>
        private static IEnumerable<Element> ElementsToRead(UIDocument uidoc, Document doc)
        {
            ICollection<ElementId> selection = uidoc.Selection.GetElementIds();

            if (selection != null && selection.Count > 0)
            {
                List<Element> selected = new List<Element>();

                foreach (ElementId id in selection)
                {
                    Element el = doc.GetElement(id);

                    if (el != null)
                        selected.Add(el);
                }

                return selected;
            }

            View activeView = doc.ActiveView;

            if (activeView == null)
                return new List<Element>();

            return new FilteredElementCollector(doc, activeView.Id)
                .WhereElementIsNotElementType()
                .ToElements();
        }

        private static void AddParameterNames(Element el, SortedSet<string> names)
        {
            if (el == null)
                return;

            foreach (Parameter parameter in el.Parameters)
            {
                if (parameter == null || parameter.Definition == null)
                    continue;

                string name = parameter.Definition.Name;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                names.Add(name.Trim());
            }
        }

        /// <summary>
        /// The user worksets, which is what the substructure setting is matched against. A model
        /// that is not workshared has none, and the dialog says as much rather than offering an
        /// empty list.
        /// </summary>
        private static void CollectWorksets(Document doc, SortedSet<string> worksets)
        {
            if (doc.IsWorkshared == false)
                return;

            FilteredWorksetCollector collector =
                new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset);

            foreach (Workset workset in collector)
            {
                if (workset == null || string.IsNullOrWhiteSpace(workset.Name))
                    continue;

                worksets.Add(workset.Name.Trim());
            }
        }

        private static void CollectPhases(Document doc, SortedSet<string> phases)
        {
            PhaseArray phaseArray = doc.Phases;

            if (phaseArray == null)
                return;

            foreach (Phase phase in phaseArray)
            {
                if (phase == null || string.IsNullOrWhiteSpace(phase.Name))
                    continue;

                phases.Add(phase.Name.Trim());
            }
        }

        /// <summary>
        /// ElementId.Value in Revit 2024 and later, ElementId.IntegerValue before it. The solution
        /// builds against both, so neither is named here directly; see CarboLifeRevitCompat.
        /// </summary>
        private static long IdValue(ElementId id)
        {
            return id == null ? -1 : id.LongValue();
        }
    }
}
