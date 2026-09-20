using CarboCircle.data;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarboCircle.UI
{
    /// <summary>
    /// Turns a plain ComboBox into a type-to-search parameter picker: the user can type a
    /// parameter name and watch the list narrow to what matches, pick one from the list, or
    /// type a name the model does not contain and keep it.
    ///
    /// WHY THIS IS NOT JUST IsEditable + IsTextSearchEnabled
    ///
    /// WPF's own text search is prefix-only and it rewrites what the user typed. Typing
    /// "len" would jump to the first item starting with "len" and replace the text with it,
    /// so a parameter called "Cut Length" could not be found by typing "length" and a name
    /// that no item starts with would be fought over on every keystroke. Substring matching
    /// with the typed text left alone is what makes a list of a few hundred parameters
    /// usable, so IsTextSearchEnabled is turned off and the filtering is done here.
    ///
    /// THE TWO TRAPS IN DOING IT THIS WAY
    ///
    /// Applying a filter re-evaluates the item list, and if the current SelectedItem falls
    /// out of the filtered set the ComboBox clears the selection and writes the new
    /// selection's text - the empty string - over the editor. That is the well known "my
    /// typing disappears" bug. Everything that touches the filter therefore restores the
    /// text and the caret immediately afterwards, inside the same synchronous block.
    ///
    /// And the filter lives on the CollectionView, which WPF creates one of per collection
    /// INSTANCE. Two pickers handed the same List would filter each other. Each attach()
    /// takes its own list; see carboCircleParameterNames.forPicker.
    /// </summary>
    internal static class carboCircleParameterPicker
    {
        /// <summary>
        /// Wires one combo box up as a parameter picker over the given names.
        ///
        /// Mechanics only. What the names mean - whether an empty box stands for the type
        /// name or for a default parameter - belongs to the setting, not to the control, so
        /// the caller brings the list and the text and reads the text back.
        /// </summary>
        /// <param name="box">The combo box. Its ItemsSource is replaced.</param>
        /// <param name="items">
        /// Its own list, not shared with another picker. See the class comment for why that
        /// matters rather than merely being tidy.
        /// </param>
        /// <param name="initialText">What to show before the user touches it.</param>
        internal static void attach(ComboBox box, List<string> items, string initialText)
        {
            if (box == null)
                return;

            if (items == null)
                items = new List<string>();

            box.IsEditable = true;
            box.IsTextSearchEnabled = false;
            box.StaysOpenOnEdit = true;
            box.ItemsSource = items;
            box.Text = initialText ?? "";

            //Re-entrancy guard. Restoring the text after a filter raises TextChanged again,
            //and without this the handler would filter on its own restoration for ever.
            bool busy = false;

            //The editor only exists once the template has been applied. ApplyTemplate forces
            //that now, which matters because attach() is called from the window's Loaded
            //handler - by which time the combo box's own Loaded may already have been and
            //gone, and subscribing to it would silently wire up nothing.
            box.ApplyTemplate();

            System.Windows.RoutedEventHandler wire = null;

            wire = delegate
            {
                TextBox editor = box.Template == null
                    ? null
                    : box.Template.FindName("PART_EditableTextBox", box) as TextBox;

                if (editor == null)
                {
                    //Not templated yet after all. Try again when it is, once.
                    box.Loaded += wire;
                    return;
                }

                box.Loaded -= wire;

                //Guarded against being wired twice. This window is shown and hidden rather
                //than created and destroyed, so Loaded can come round again, and a second
                //subscription would filter twice per keystroke.
                if (editor.Tag is bool && (bool)editor.Tag)
                    return;

                editor.Tag = true;

                editor.TextChanged += delegate
                {
                    if (busy)
                        return;

                    busy = true;

                    try
                    {
                        applyFilter(box, editor, true);
                    }
                    finally
                    {
                        busy = false;
                    }
                };

                //Leaving the box has the ComboBox reconcile text against selection, which
                //is the other moment it can overwrite what was typed. Dropping the filter
                //first means every item is back in the list, so whatever the user typed
                //either matches an item or is simply kept as free text.
                editor.LostFocus += delegate
                {
                    if (busy)
                        return;

                    busy = true;

                    try
                    {
                        applyFilter(box, editor, false);
                        box.IsDropDownOpen = false;
                    }
                    finally
                    {
                        busy = false;
                    }
                };
            };

            wire(box, null);

            //Escape closes the list without the window's Cancel button hearing about it.
            box.PreviewKeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape && box.IsDropDownOpen)
                {
                    box.IsDropDownOpen = false;
                    e.Handled = true;
                }
            };
        }

        /// <summary>
        /// What the user settled on, trimmed. The caller decides what it means.
        /// </summary>
        internal static string textOf(ComboBox box)
        {
            if (box == null)
                return string.Empty;

            //Text, not SelectedItem. A name typed in full that happens to match nothing in
            //this model is a legitimate answer - the proposed design is often a different
            //file from the one being mined, and the reuse id parameter may not exist yet -
            //and SelectedItem would be null for it.
            return (box.Text ?? "").Trim();
        }

        /// <summary>
        /// Narrows the list to what contains the typed text, then puts the typed text and
        /// the caret back.
        /// </summary>
        /// <param name="narrow">
        /// False restores the full list. Used on the way out, so the control is never left
        /// holding a filtered view that the next Loaded would inherit.
        /// </param>
        private static void applyFilter(ComboBox box, TextBox editor, bool narrow)
        {
            string typed = editor.Text ?? "";
            int caret = editor.SelectionStart;

            if (!narrow || typed.Length == 0)
            {
                box.Items.Filter = null;
            }
            else
            {
                string needle = typed;

                box.Items.Filter = delegate (object candidate)
                {
                    string name = candidate as string;
                    return name != null && name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }

            //Setting the filter can have cleared the selection and, with it, the text.
            if (editor.Text != typed)
                editor.Text = typed;

            editor.SelectionStart = caret > typed.Length ? typed.Length : caret;
            editor.SelectionLength = 0;

            //Open the list only while the user is actually typing into it, and only when
            //there is something to show. An empty popup under the cursor is worse than none.
            if (narrow && editor.IsKeyboardFocusWithin)
                box.IsDropDownOpen = box.Items.Count > 0;
        }
    }
}
