using System;
using System.Collections.Generic;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Which of the five kinds of name a settings field holds. The import looks each one up in a
    /// different place, so a name that exists as one of these is not necessarily present as another.
    /// </summary>
    public enum CarboNameKind
    {
        /// <summary>Read off the element itself with Element.LookupParameter.</summary>
        InstanceParameter,

        /// <summary>Read off the element's ElementType with LookupParameter.</summary>
        TypeParameter,

        /// <summary>Read off Document.ProjectInformation. The GIA fields.</summary>
        ProjectInformation,

        /// <summary>A user workset. Matched by "contains", not by equality, see the substructure import.</summary>
        Workset,

        /// <summary>A phase name, matched exactly against the element's created phase.</summary>
        Phase
    }

    /// <summary>
    /// The parameter, workset and phase names of the model the import is about to run over.
    ///
    /// WHY THIS IS NOT A REVIT CLASS
    ///
    /// Filling it needs a Document and has to happen in Revit API context, so that part lives in
    /// CarboLifeRevit.CarboModelNameHarvest. Everything here is plain strings, which keeps the
    /// settings dialog free of Revit types and lets the pickers be exercised without Revit running.
    ///
    /// WHY IT EXISTS AT ALL
    ///
    /// Every name in the import settings is looked up and, when it is not there, silently skipped:
    /// a grade parameter that no element carries yields an empty grade, a phase name no phase
    /// matches makes nothing existing, a workset name that matches nothing makes nothing
    /// substructure. The import still finishes and still produces numbers, so a name that was
    /// right for the last project and wrong for this one costs an entire import before anyone
    /// notices. Knowing what the model holds is what lets the dialog say so beforehand.
    /// </summary>
    public static class CarboModelNames
    {
        //Sorted and case-insensitive: Revit lets a project and a shared parameter differ only in
        //case, and nobody choosing one from a list cares about the difference.
        private static SortedSet<string> instanceParameters = NewSet();
        private static SortedSet<string> typeParameters = NewSet();
        private static SortedSet<string> projectInformationParameters = NewSet();
        private static SortedSet<string> worksets = NewSet();
        private static SortedSet<string> phases = NewSet();

        /// <summary>
        /// True once a model has been read. False in every other case - a dialog opened outside
        /// Revit, or a harvest that found nothing at all - and then nothing is marked as missing,
        /// because an empty index is not evidence that a name is absent.
        /// </summary>
        public static bool HasModel { get; private set; }

        private static SortedSet<string> NewSet()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces everything known about the model. Called once per harvest, from Revit context.
        /// </summary>
        public static void SetNames(IEnumerable<string> instanceParameterNames,
                                    IEnumerable<string> typeParameterNames,
                                    IEnumerable<string> projectInformationNames,
                                    IEnumerable<string> worksetNames,
                                    IEnumerable<string> phaseNames)
        {
            instanceParameters = ToSet(instanceParameterNames);
            typeParameters = ToSet(typeParameterNames);
            projectInformationParameters = ToSet(projectInformationNames);
            worksets = ToSet(worksetNames);
            phases = ToSet(phaseNames);

            //Phases are the test rather than parameters: every Revit model has at least one phase,
            //so an empty phase list means the harvest never ran or fell over on its first step.
            HasModel = phases.Count > 0 || instanceParameters.Count > 0;
        }

        /// <summary>
        /// Forgets the model. Used when no document could be read, so a previous model's names
        /// are not held against this one.
        /// </summary>
        public static void Clear()
        {
            instanceParameters = NewSet();
            typeParameters = NewSet();
            projectInformationParameters = NewSet();
            worksets = NewSet();
            phases = NewSet();

            HasModel = false;
        }

        private static SortedSet<string> ToSet(IEnumerable<string> names)
        {
            SortedSet<string> result = NewSet();

            if (names == null)
                return result;

            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                result.Add(name.Trim());
            }

            return result;
        }

        /// <summary>
        /// The names of one kind, as a list a picker can own.
        ///
        /// A NEW LIST EVERY TIME, and that is not tidiness - it is required. WPF gives one
        /// collection instance one default CollectionView, and the pickers filter through that
        /// view as the user types, so two combo boxes handed the same list would filter each other.
        /// </summary>
        public static List<string> NamesOf(CarboNameKind kind)
        {
            return new List<string>(SetOf(kind));
        }

        /// <summary>
        /// True when the model is known to hold this name.
        ///
        /// WORKSETS ARE NEVER REPORTED AS MISSING. The other kinds are names: the import looks for
        /// exactly what is typed, so a name the model has not got can only ever find nothing. A
        /// workset setting is not a name but a fragment that any workset name may contain, which
        /// makes it a search term rather than a reference - and one that is usually an office
        /// default, deliberately kept broad enough to work across models with different workset
        /// naming. Marking it against this one model's worksets would flag a setting that is doing
        /// exactly what it was set up to do. The dropdown still offers the worksets this model has,
        /// which is the help that was wanted; it is the accusation that was not.
        /// </summary>
        /// <returns>
        /// True for an empty name and whenever no model has been read: nothing has been shown to
        /// be missing in either case, and a dialog that cries wolf is one nobody reads.
        /// </returns>
        public static bool Contains(CarboNameKind kind, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            if (HasModel == false)
                return true;

            if (kind == CarboNameKind.Workset)
                return true;

            return SetOf(kind).Contains(name.Trim());
        }

        /// <summary>
        /// How many names of this kind the model holds. Used to tell "this model has no worksets
        /// at all" from "this model has worksets, just not that one".
        /// </summary>
        public static int CountOf(CarboNameKind kind)
        {
            return SetOf(kind).Count;
        }

        private static SortedSet<string> SetOf(CarboNameKind kind)
        {
            switch (kind)
            {
                case CarboNameKind.TypeParameter:
                    return typeParameters;

                case CarboNameKind.ProjectInformation:
                    return projectInformationParameters;

                case CarboNameKind.Workset:
                    return worksets;

                case CarboNameKind.Phase:
                    return phases;

                default:
                    return instanceParameters;
            }
        }

        /// <summary>
        /// What one of these is called in a sentence, for the warning the dialog shows.
        /// </summary>
        public static string DescriptionOf(CarboNameKind kind)
        {
            switch (kind)
            {
                case CarboNameKind.TypeParameter:
                    return "type parameter";

                case CarboNameKind.ProjectInformation:
                    return "Project Information parameter";

                case CarboNameKind.Workset:
                    return "workset";

                case CarboNameKind.Phase:
                    return "phase";

                default:
                    return "instance parameter";
            }
        }
    }
}
