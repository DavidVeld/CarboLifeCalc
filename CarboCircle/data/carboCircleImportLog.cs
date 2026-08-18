using System;
using System.Collections.Generic;
using System.Text;

namespace CarboCircle.data
{
    /// <summary>
    /// What one import actually did, and why it dropped whatever it dropped.
    ///
    /// The import path used to be lined with empty catch blocks, so every failure looked
    /// identical from the outside - a missing section database, a view with no phase, an
    /// element with no material, and a model with genuinely nothing in it all produced the
    /// same thing: a window that did not change. This collects the reasons instead of
    /// discarding them, so the import can say what happened.
    ///
    /// Skips are tallied by reason rather than listed per element. A model can drop
    /// thousands of elements for the same handful of reasons, and a wall of identical
    /// lines buries the one that matters. A few example ids are kept per reason so a
    /// reason can still be traced back to real geometry.
    ///
    /// Deliberately free of Revit types, so the csv import can report the same way.
    /// </summary>
    internal class carboCircleImportLog
    {
        /// <summary>Example ids kept per reason, purely to give the user somewhere to look.</summary>
        private const int examplesPerReason = 5;

        private readonly List<string> failures;
        private readonly List<string> notes;

        //Reason -> count and a few ids. reasonOrder keeps first-seen order, which follows
        //the order of the import and reads better than sorting alphabetically.
        private readonly List<string> reasonOrder;
        private readonly Dictionary<string, int> reasonCounts;
        private readonly Dictionary<string, List<string>> reasonExamples;

        //Elements the extraction method deliberately left out, tallied the same way but
        //kept apart: excluding an element the user did not ask for is the filter working,
        //not a fault, so it must not make the import look broken.
        private readonly List<string> filterOrder;
        private readonly Dictionary<string, int> filterCounts;

        /// <summary>The extraction method this import ran with.</summary>
        public string ExtractionMethod { get; set; }

        /// <summary>The view this import read.</summary>
        public string ViewName { get; set; }

        /// <summary>Revit elements the view offered, before the extraction method ran.</summary>
        public int ElementsInView { get; set; }

        /// <summary>Revit elements left after the extraction method, handed to the converter.</summary>
        public int ElementsExamined { get; set; }

        /// <summary>CarboCircle elements the import produced.</summary>
        public int ElementsCollected { get; set; }

        public carboCircleImportLog()
        {
            failures = new List<string>();
            notes = new List<string>();
            reasonOrder = new List<string>();
            reasonCounts = new Dictionary<string, int>();
            reasonExamples = new Dictionary<string, List<string>>();
            filterOrder = new List<string>();
            filterCounts = new Dictionary<string, int>();

            ExtractionMethod = string.Empty;
            ViewName = string.Empty;
            ElementsInView = 0;
            ElementsExamined = 0;
            ElementsCollected = 0;
        }

        //--------------------------------------------------------------------------------
        // Recording
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Something that stopped the import, or quietly cost it a whole category of
        /// elements. Always reported.
        /// </summary>
        public void Fail(string what)
        {
            if (!string.IsNullOrEmpty(what))
                failures.Add(what);
        }

        /// <summary>
        /// As <see cref="Fail(string)"/>, with the exception message appended. Pass the
        /// action that was being attempted rather than the words of the exception:
        /// "Reading the steel section database" tells the user far more than
        /// "Access to the path is denied" on its own.
        /// </summary>
        public void Fail(string what, Exception ex)
        {
            if (ex == null)
            {
                Fail(what);
                return;
            }

            Fail(what + ": " + ex.Message);
        }

        /// <summary>Context worth reporting even when the import went fine.</summary>
        public void Note(string message)
        {
            if (!string.IsNullOrEmpty(message))
                notes.Add(message);
        }

        /// <summary>One element the import could not use.</summary>
        public void Skip(string reason)
        {
            record(reason, null);
        }

        /// <summary>One element the import could not use, with its Revit id as an example.</summary>
        public void Skip(string reason, long elementId)
        {
            record(reason, elementId.ToString());
        }

        /// <summary>
        /// An element the extraction method deliberately left out.
        ///
        /// Not a problem, but worth reporting: "412 examined, 412 not demolished in this
        /// phase" is the one line that explains an empty import, and it is the difference
        /// between a wrong setting and an empty model.
        /// </summary>
        public void Filter(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return;

            if (!filterCounts.ContainsKey(reason))
            {
                filterOrder.Add(reason);
                filterCounts.Add(reason, 0);
            }

            filterCounts[reason] = filterCounts[reason] + 1;
        }

        private void record(string reason, string elementId)
        {
            if (string.IsNullOrEmpty(reason))
                return;

            if (!reasonCounts.ContainsKey(reason))
            {
                reasonOrder.Add(reason);
                reasonCounts.Add(reason, 0);
                reasonExamples.Add(reason, new List<string>());
            }

            reasonCounts[reason] = reasonCounts[reason] + 1;

            List<string> examples = reasonExamples[reason];

            if (elementId != null && examples.Count < examplesPerReason)
                examples.Add(elementId);
        }

        //--------------------------------------------------------------------------------
        // Reporting
        //--------------------------------------------------------------------------------

        /// <summary>Total elements dropped, across every reason.</summary>
        public int SkippedCount()
        {
            int total = 0;

            foreach (string reason in reasonOrder)
                total += reasonCounts[reason];

            return total;
        }

        /// <summary>
        /// True when the import failed outright or could not use something it was given.
        /// Elements excluded by the extraction method do not count: that is the filter
        /// doing its job.
        /// </summary>
        public bool HasProblems()
        {
            return failures.Count > 0 || reasonOrder.Count > 0;
        }

        /// <summary>
        /// True only when something actually went wrong - as opposed to
        /// <see cref="HasProblems"/>, which also counts elements the import could not use.
        /// Skipping a handful of elements is normal and is not worth interrupting anyone
        /// over; a failure is.
        /// </summary>
        public bool HasFailures()
        {
            return failures.Count > 0;
        }

        /// <summary>One line: what came back.</summary>
        public string Headline()
        {
            if (failures.Count > 0 && ElementsCollected == 0)
                return "The import could not be completed.";

            if (ElementsCollected == 0)
                return "No elements were imported.";

            return "Imported " + ElementsCollected + " elements.";
        }

        /// <summary>
        /// The full account: what was read, what stopped it, and what it dropped.
        /// </summary>
        public string Details()
        {
            StringBuilder text = new StringBuilder();

            if (!string.IsNullOrEmpty(ViewName))
                text.AppendLine("View: " + ViewName);

            if (!string.IsNullOrEmpty(ExtractionMethod))
                text.AppendLine("Method: " + ExtractionMethod);

            //Three counts, because the gap between any two of them is where an empty
            //import gets explained: nothing in the view, everything filtered out, or
            //everything dropped on the way through.
            text.AppendLine("Found " + ElementsInView + " in the view, " + ElementsExamined +
                            " after filtering, imported " + ElementsCollected + ".");

            if (failures.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("Problems:");

                foreach (string failure in failures)
                    text.AppendLine("  - " + failure);
            }

            if (filterOrder.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("Left out by the extraction method:");

                foreach (string reason in filterOrder)
                    text.AppendLine("  - " + filterCounts[reason] + " x " + reason);
            }

            if (reasonOrder.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("Could not be used (" + SkippedCount() + " elements):");

                foreach (string reason in reasonOrder)
                    text.AppendLine("  - " + describeReason(reason));
            }

            if (notes.Count > 0)
            {
                text.AppendLine();

                foreach (string note in notes)
                    text.AppendLine(note);
            }

            return text.ToString();
        }

        private string describeReason(string reason)
        {
            string line = reasonCounts[reason] + " x " + reason;
            List<string> examples = reasonExamples[reason];

            if (examples.Count == 0)
                return line;

            return line + " (e.g. " + string.Join(", ", examples.ToArray()) + ")";
        }
    }
}
