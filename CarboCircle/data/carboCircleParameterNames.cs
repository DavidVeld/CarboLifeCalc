using System;
using System.Collections.Generic;

namespace CarboCircle.data
{
    /// <summary>
    /// The parameter names the settings dialog offers, and the translation between what that
    /// dialog shows and what the settings file stores.
    ///
    /// TYPE AND INSTANCE ARE KEPT APART, DELIBERATELY
    ///
    /// The section-name overrides are read with ElementType.LookupParameter, so only a type
    /// parameter can ever answer them. The reuse id is written to the element itself, so it
    /// has to be an instance parameter - two beams of one type are two different pieces of
    /// steel and must be able to carry different ids. Offering one list for both jobs would
    /// put half its entries in front of a user who cannot use them, and the failure would be
    /// silent either way round: a type parameter named as the reuse id would refuse the
    /// write, and an instance parameter named as the section name would simply never be found.
    ///
    /// WHY THIS IS NOT A REVIT CLASS
    ///
    /// Filling the lists needs a Document and must happen in Revit API context, so that part
    /// lives in carboCircleRevitCommands.collectParameterNames. Everything here is plain
    /// strings, which keeps the settings dialog free of Revit types and lets the picker be
    /// exercised without Revit running at all.
    ///
    /// THE EMPTY STRING AND "Type name"
    ///
    /// An empty section-name override has always meant "identify the element by its Revit
    /// type name". That was true but invisible: the dialog showed an empty box and a
    /// paragraph of prose explaining what empty meant. The box now shows
    /// <see cref="TypeNameToken"/> instead, which is a value the user can see, compare
    /// against the alternatives in the same dropdown, and deliberately change. The stored
    /// form is unchanged - still the empty string - so settings files written by older
    /// versions still load, and files written here still open in them.
    /// </summary>
    internal static class carboCircleParameterNames
    {
        /// <summary>
        /// What the dialog shows when no section-name override is set. Not a parameter name:
        /// it stands for the Revit type name itself, which is what the import falls back to.
        /// </summary>
        internal const string TypeNameToken = "Type name";

        //Revit's own built-in type parameter is called "Type Name" and returns exactly what
        //the fallback returns. Offering both would put two entries in the dropdown that do
        //the same thing, so the harvest drops it and the token above stands for both.
        private const string RevitTypeNameParameter = "Type Name";

        //Sorted and case-insensitive, because Revit lets a project and a shared parameter
        //differ only in case and the user does not care about the difference.
        private static List<string> typeNames = new List<string>();
        private static List<string> instanceNames = new List<string>();

        /// <summary>Replaces the known type parameter names, after a harvest.</summary>
        internal static void setTypeNames(IEnumerable<string> names)
        {
            typeNames = clean(names, dropTypeName: true);
        }

        /// <summary>Replaces the known instance parameter names, after a harvest.</summary>
        internal static void setInstanceNames(IEnumerable<string> names)
        {
            instanceNames = clean(names, dropTypeName: false);
        }

        private static List<string> clean(IEnumerable<string> names, bool dropTypeName)
        {
            SortedSet<string> unique = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            if (names != null)
            {
                foreach (string name in names)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    string trimmed = name.Trim();

                    if (dropTypeName &&
                        (string.Equals(trimmed, RevitTypeNameParameter, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(trimmed, TypeNameToken, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    unique.Add(trimmed);
                }
            }

            return new List<string>(unique);
        }

        /// <summary>
        /// True once a model has been read. The dialog says so when it is false, rather
        /// than showing empty dropdowns that look broken.
        /// </summary>
        internal static bool hasNames()
        {
            return typeNames.Count > 0 || instanceNames.Count > 0;
        }

        internal static int typeCount()
        {
            return typeNames.Count;
        }

        internal static int instanceCount()
        {
            return instanceNames.Count;
        }

        /// <summary>
        /// What the section-name picker offers: the type-name fallback first, then every
        /// type parameter found in the model, then the stored value if this model has no
        /// such parameter - which is normal, since the mine and the proposed design are
        /// often two different files.
        ///
        /// A NEW LIST EVERY TIME, and that is not wasteful tidiness - it is required. WPF
        /// gives one collection instance one default CollectionView, and the picker filters
        /// through that view as the user types. Two combo boxes handed the same list would
        /// share the view, so typing in one would filter the other.
        /// </summary>
        internal static List<string> forSectionPicker(string currentSetting)
        {
            List<string> result = new List<string>();
            result.Add(TypeNameToken);
            result.AddRange(typeNames);

            return withCurrent(result, toDisplay(currentSetting));
        }

        /// <summary>
        /// What an instance parameter picker offers. No type-name token: there is nothing to
        /// fall back to, the name either exists on the elements or the write reports that it
        /// does not.
        /// </summary>
        internal static List<string> forInstancePicker(string currentSetting)
        {
            List<string> result = new List<string>(instanceNames);

            return withCurrent(result, currentSetting == null ? "" : currentSetting.Trim());
        }

        private static List<string> withCurrent(List<string> list, string current)
        {
            if (string.IsNullOrEmpty(current))
                return list;

            foreach (string entry in list)
            {
                if (string.Equals(entry, current, StringComparison.OrdinalIgnoreCase))
                    return list;
            }

            //After the token, when there is one, so the fallback stays at the top.
            list.Insert(list.Count > 0 && list[0] == TypeNameToken ? 1 : 0, current);

            return list;
        }

        /// <summary>
        /// Stored section-name override to what the dialog shows. Empty becomes the token.
        /// </summary>
        internal static string toDisplay(string storedSetting)
        {
            return string.IsNullOrWhiteSpace(storedSetting) ? TypeNameToken : storedSetting.Trim();
        }

        /// <summary>
        /// What the dialog shows back to the stored section-name override. The token, and a
        /// box the user simply emptied, both mean "no override".
        /// </summary>
        internal static string toSetting(string displayed)
        {
            if (string.IsNullOrWhiteSpace(displayed))
                return string.Empty;

            string trimmed = displayed.Trim();

            if (string.Equals(trimmed, TypeNameToken, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            //Revit's own "Type Name" parameter returns the type name, so treating it as the
            //fallback rather than as an override costs nothing and keeps one meaning in the
            //settings file.
            if (string.Equals(trimmed, RevitTypeNameParameter, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return trimmed;
        }
    }
}
