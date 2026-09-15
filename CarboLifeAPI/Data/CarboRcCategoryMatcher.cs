using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CarboLifeAPI.Data.Superseded;

namespace CarboLifeAPI.Data
{
    /// <summary>How a category was found in the reinforcement map.</summary>
    public enum CarboRcMatchKind
    {
        /// <summary>Nothing matched, the caller falls back to its default rate.</summary>
        None = 0,

        /// <summary>The row name is the category, character for character.</summary>
        Exact = 1,

        /// <summary>The names agree once case, spacing, punctuation and a plural are set aside.</summary>
        Normalised = 2,

        /// <summary>The names are only similar, at or above the similarity threshold.</summary>
        Similar = 3
    }

    /// <summary>A row of the reinforcement map, and how confidently it was arrived at.</summary>
    public class CarboRcCategoryMatch
    {
        public CarboNumProperty Property { get; set; }
        public CarboRcMatchKind Kind { get; set; }

        /// <summary>Similarity as a percentage, 100 for an exact or normalised match.</summary>
        public int Score { get; set; }

        public CarboRcCategoryMatch()
        {
            Property = null;
            Kind = CarboRcMatchKind.None;
            Score = 0;
        }

        /// <summary>True when a row was found at all.</summary>
        public bool Found { get { return Property != null; } }

        /// <summary>True when the row was not named exactly as the category is.</summary>
        public bool IsApproximate
        {
            get { return Property != null && Kind != CarboRcMatchKind.Exact; }
        }
    }

    /// <summary>
    /// Finds a category in the reinforcement rate map (ReinforcementCats.csv, held as
    /// CarboGroupSettings.rcQuantityMap).
    ///
    /// The key is a group's Category, which at import is either the Revit category name or the
    /// value of a type or instance parameter somebody fills in by hand - see
    /// CarboRevitUtils.getCategoryValue. A plain == against the map meant "Structural Framing"
    /// matched and "structural framing", "Structural  Framing", "PT Slab" against a row spelt
    /// "PT slab", or "Pile Cap" against a row spelt "Pilecap" all silently fell through to the
    /// 100 kg/m3 default, with nothing on screen to say a rate had been missed. Nobody typing a
    /// category into Revit is going to reproduce the csv character for character.
    ///
    /// So the lookup is tried in descending confidence, and says which tier answered:
    ///
    ///   1. Exact       - the old behaviour, so a map that works today keeps working unchanged.
    ///   2. Normalised  - case folded, "and" for "&amp;", everything but letters and digits
    ///                    dropped, and a trailing plural set aside. "PT Slab", "pt slab" and
    ///                    "PT-slab" are the same row; so are "Wall" and "Walls", "Truss" and
    ///                    "Trusses", "Substructure &amp; Foundations" and "Substructure and
    ///                    Foundations".
    ///   3. Similar     - Levenshtein over the normalised forms, best row wins provided it
    ///                    reaches MinimumSimilarity. This is the tier that forgives a typo or a
    ///                    dropped letter, and the one that can be wrong, which is why the caller
    ///                    is told the match was approximate and puts it in the description.
    ///
    /// Ties inside a tier go to the earliest row, which is the first-match-wins the map has
    /// always had. Duplicate rows in the csv therefore still behave as they did.
    /// </summary>
    public static class CarboRcCategoryMatcher
    {
        /// <summary>
        /// How alike two names have to be before tier 3 accepts them, as a percentage of the
        /// longer name. 90 keeps the near misses of the standard map apart: "Light Column"
        /// against "Heavy Column" scores 58, "General Slab" against "General Wall" 67.
        /// </summary>
        public const int MinimumSimilarity = 90;

        /// <summary>
        /// Looks a category up in the map.
        /// </summary>
        /// <param name="map">The reinforcement rates, normally CarboGroupSettings.rcQuantityMap</param>
        /// <param name="category">The group category to find a rate for</param>
        /// <returns>The row and how it was found. Never null; check Found.</returns>
        public static CarboRcCategoryMatch Match(IList<CarboNumProperty> map, string category)
        {
            CarboRcCategoryMatch result = new CarboRcCategoryMatch();

            if (map == null || map.Count == 0 || string.IsNullOrWhiteSpace(category))
                return result;

            //--- Tier 1, exact ---------------------------------------------------------------
            for (int i = 0; i < map.Count; i++)
            {
                CarboNumProperty property = map[i];

                if (property != null && property.PropertyName == category)
                {
                    result.Property = property;
                    result.Kind = CarboRcMatchKind.Exact;
                    result.Score = 100;
                    return result;
                }
            }

            string wanted = Normalise(category);

            //A category of nothing but punctuation normalises away to nothing, and an empty
            //string is similar to every short row. "N/A" is exactly this case.
            if (wanted.Length == 0)
                return result;

            //--- Tier 2, normalised ----------------------------------------------------------
            for (int i = 0; i < map.Count; i++)
            {
                CarboNumProperty property = map[i];

                if (property == null)
                    continue;

                string candidate = Normalise(property.PropertyName);

                if (candidate.Length == 0)
                    continue;

                if (candidate == wanted || SamePluralStem(candidate, wanted))
                {
                    result.Property = property;
                    result.Kind = CarboRcMatchKind.Normalised;
                    result.Score = 100;
                    return result;
                }
            }

            //--- Tier 3, similar -------------------------------------------------------------
            int bestScore = 0;

            for (int i = 0; i < map.Count; i++)
            {
                CarboNumProperty property = map[i];

                if (property == null)
                    continue;

                string candidate = Normalise(property.PropertyName);

                if (candidate.Length == 0)
                    continue;

                int score = Similarity(candidate, wanted);

                //Strictly greater, so the earliest of equally good rows is the one kept.
                if (score > bestScore)
                {
                    bestScore = score;
                    result.Property = property;
                }
            }

            if (bestScore >= MinimumSimilarity)
            {
                result.Kind = CarboRcMatchKind.Similar;
                result.Score = bestScore;
            }
            else
            {
                result.Property = null;
            }

            return result;
        }

        /// <summary>
        /// Similarity of two normalised names as a percentage of the longer one, 100 being equal.
        /// </summary>
        public static int Similarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return 0;

            int longest = Math.Max(a.Length, b.Length);

            //Names this far apart in length cannot reach the threshold whatever their letters
            //are, and this skips the O(n*m) call for most of the map.
            if ((100 * Math.Abs(a.Length - b.Length)) / longest > 100 - MinimumSimilarity)
                return 0;

            int distance = Utils.CalcLevenshteinDistance(a, b);

            //CalcLevenshteinDistance answers 999 for an empty operand, guarded against above.
            if (distance >= longest)
                return 0;

            return (100 * (longest - distance)) / longest;
        }

        /// <summary>
        /// Reduces a category name to the part of it a person would call the name: lower case,
        /// "and" for "&amp;", and nothing else but letters and digits. Everything the separator,
        /// the spacing and the capitals could differ by is gone, so "Structural Framing",
        /// "structural_framing" and "STRUCTURAL FRAMING" all come out the same.
        /// </summary>
        public static string Normalise(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            string lowered = name.Trim().ToLower(CultureInfo.InvariantCulture);

            StringBuilder builder = new StringBuilder(lowered.Length);

            for (int i = 0; i < lowered.Length; i++)
            {
                char c = lowered[i];

                if (c == '&')
                    builder.Append("and");
                else if (char.IsLetterOrDigit(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Whether two normalised names are the same word in the singular and the plural. The map
        /// holds the Revit category names, which are plural ("Walls", "Structural Columns"), while
        /// a hand typed category is usually singular, and that one letter is far too small a
        /// difference for the similarity tier to catch on a short name: "wall" against "walls"
        /// only scores 80.
        /// </summary>
        private static bool SamePluralStem(string a, string b)
        {
            return IsPluralOf(a, b) || IsPluralOf(b, a);
        }

        /// <summary>Whether <paramref name="plural"/> is <paramref name="singular"/> with s or es on the end.</summary>
        private static bool IsPluralOf(string plural, string singular)
        {
            if (plural.Length == singular.Length + 1)
                return plural[plural.Length - 1] == 's' && plural.StartsWith(singular, StringComparison.Ordinal);

            if (plural.Length == singular.Length + 2)
                return plural.EndsWith("es", StringComparison.Ordinal) && plural.StartsWith(singular, StringComparison.Ordinal);

            return false;
        }
    }
}
