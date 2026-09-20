using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Turns a plain ComboBox into a type-to-search picker over the names the model holds: the
    /// user can type and watch the list narrow to what matches, pick one from the list, or type a
    /// name this model does not contain and keep it.
    ///
    /// WHY THE FREE TEXT MATTERS
    ///
    /// These settings are saved as the office default and carried from project to project. A name
    /// that is right for the next model and absent from this one is a legitimate thing to type, so
    /// the picker never rewrites or rejects it - it is the red border, not the control, that says
    /// the model has no such name.
    ///
    /// WHY THIS IS NOT JUST IsEditable + IsTextSearchEnabled
    ///
    /// WPF's own text search is prefix-only and it rewrites what the user typed. Typing "len"
    /// would jump to the first item starting with "len" and replace the text with it, so a
    /// parameter called "Cut Length" could not be found by typing "length", and a name that no
    /// item starts with would be fought over on every keystroke. Substring matching with the typed
    /// text left alone is what makes a list of a few hundred parameters usable, so
    /// IsTextSearchEnabled is off and the filtering is done here.
    ///
    /// THE TWO TRAPS IN DOING IT THIS WAY
    ///
    /// Applying a filter re-evaluates the item list, and if the current SelectedItem falls out of
    /// the filtered set the ComboBox clears the selection and writes the new selection's text -
    /// the empty string - over the editor. That is the well known "my typing disappears" bug.
    /// Everything that touches the filter therefore restores the text and the caret immediately
    /// afterwards, inside the same synchronous block.
    ///
    /// And the filter lives on the CollectionView, which WPF creates one of per collection
    /// INSTANCE. Two pickers handed the same List would filter each other, so every Attach takes
    /// its own list; see CarboModelNames.NamesOf.
    /// </summary>
    public static class CarboParameterPicker
    {
        /// <summary>
        /// Everything one picker needs to remember, parked on the combo box itself rather than in
        /// a static table: a table keyed by control would outlive every dialog that ever opened.
        /// </summary>
        private class PickerState
        {
            public CarboNameKind Kind;

            /// <summary>
            /// Re-entrancy guard. Restoring the text after a filter raises TextChanged again, and
            /// without this the handler would filter on its own restoration for ever.
            /// </summary>
            public bool Busy;

            /// <summary>The editor inside the control template, once it exists.</summary>
            public TextBox Editor;

            /// <summary>Raised after the user settles on a value, so the dialog can re-check it.</summary>
            public Action Changed;
        }

        /// <summary>
        /// Wires one combo box up as a picker over the given kind of name and shows the given value.
        /// </summary>
        /// <param name="box">The combo box. Its ItemsSource and Tag are taken over.</param>
        /// <param name="value">The value out of the settings, shown as typed. Empty stays empty.</param>
        /// <param name="kind">Which of the model's name lists to offer.</param>
        /// <param name="changed">
        /// Called when the user has finished changing the value: on losing focus and on picking
        /// from the list. Not on every keystroke - a border that flickers red between two letters
        /// of a name that is about to be correct is noise, not a warning.
        /// </param>
        public static void Attach(ComboBox box, string value, CarboNameKind kind, Action changed)
        {
            if (box == null)
                return;

            PickerState state = new PickerState();
            state.Kind = kind;
            state.Changed = changed;

            box.Tag = state;

            box.IsEditable = true;
            box.IsTextSearchEnabled = false;
            box.StaysOpenOnEdit = true;
            box.ItemsSource = CarboModelNames.NamesOf(kind);
            box.Text = value == null ? "" : value.Trim();

            //The editor only exists once the template has been applied. ApplyTemplate forces that
            //now, which matters because Attach is called from the window's Loaded handler - by
            //which time the combo box's own Loaded may already have been and gone, and subscribing
            //to it would silently wire up nothing.
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

                //Guarded against being wired twice: a second subscription would filter twice per
                //keystroke.
                if (editor.Tag is bool && (bool)editor.Tag)
                    return;

                editor.Tag = true;
                state.Editor = editor;

                editor.TextChanged += delegate
                {
                    if (state.Busy)
                        return;

                    state.Busy = true;

                    try
                    {
                        ApplyFilter(box, editor, true);
                    }
                    finally
                    {
                        state.Busy = false;
                    }
                };

                //Leaving the box has the ComboBox reconcile text against selection, which is the
                //other moment it can overwrite what was typed. Dropping the filter first means
                //every item is back in the list, so whatever was typed either matches an item or
                //is simply kept as free text.
                editor.LostFocus += delegate
                {
                    if (state.Busy)
                        return;

                    state.Busy = true;

                    try
                    {
                        ApplyFilter(box, editor, false);
                        box.IsDropDownOpen = false;
                    }
                    finally
                    {
                        state.Busy = false;
                    }

                    Raise(state);
                };
            };

            wire(box, null);

            //Picking from the list is a finished choice, the same as leaving the box - but it is
            //raised BEFORE the ComboBox has written the new selection into its editor, so reading
            //the box here hands back the previous value. Everything downstream of this reads the
            //text, so the call is put off until the control has finished with itself; the box
            //keeps the focus after a pick, so anything stale set here would simply stay stale.
            box.SelectionChanged += delegate
            {
                if (state.Busy)
                    return;

                box.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                           new Action(delegate { Raise(state); }));
            };

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
        /// Points an attached picker at another kind of name, keeping whatever is typed in it.
        /// Used when the Type/Instance box beside a field changes: the same name means a different
        /// thing then, and it is a different list that says whether the model holds it.
        /// </summary>
        public static void Repoint(ComboBox box, CarboNameKind kind)
        {
            PickerState state = StateOf(box);

            if (state == null)
                return;

            if (state.Kind == kind)
                return;

            state.Kind = kind;

            //Replacing the items clears the selection, and with it the text, so the text is put
            //back afterwards. Under the guard: this is not the user changing their answer.
            string typed = box.Text;

            state.Busy = true;

            try
            {
                box.ItemsSource = CarboModelNames.NamesOf(kind);
                box.Text = typed;
            }
            finally
            {
                state.Busy = false;
            }
        }

        /// <summary>Which kind of name an attached picker is currently offering.</summary>
        public static CarboNameKind KindOf(ComboBox box)
        {
            PickerState state = StateOf(box);

            return state == null ? CarboNameKind.InstanceParameter : state.Kind;
        }

        /// <summary>
        /// What the user settled on, in the form the settings file stores.
        /// </summary>
        public static string ValueOf(ComboBox box)
        {
            if (box == null || box.Text == null)
                return "";

            //Text, not SelectedItem. A name typed in full that this model happens not to hold is
            //a legitimate answer - these settings are carried between projects - and SelectedItem
            //would be null for it.
            return box.Text.Trim();
        }

        /// <summary>Puts a value into an attached picker without it counting as a user change.</summary>
        public static void SetValue(ComboBox box, string value)
        {
            PickerState state = StateOf(box);

            if (state == null)
            {
                if (box != null)
                    box.Text = value == null ? "" : value;

                return;
            }

            state.Busy = true;

            try
            {
                box.Text = value == null ? "" : value;
            }
            finally
            {
                state.Busy = false;
            }
        }

        private static PickerState StateOf(ComboBox box)
        {
            return box == null ? null : box.Tag as PickerState;
        }

        private static void Raise(PickerState state)
        {
            if (state.Changed != null)
                state.Changed();
        }

        /// <summary>
        /// Narrows the list to what contains the typed text, then puts the typed text and the
        /// caret back.
        /// </summary>
        /// <param name="narrow">
        /// False restores the full list. Used on the way out, so the control is never left holding
        /// a filtered view that the next Loaded would inherit.
        /// </param>
        private static void ApplyFilter(ComboBox box, TextBox editor, bool narrow)
        {
            string typed = editor.Text == null ? "" : editor.Text;
            int caret = editor.SelectionStart;

            if (narrow == false || typed.Length == 0)
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

            //Open the list only while the user is actually typing into it, and only when there is
            //something to show. An empty popup under the cursor is worse than none.
            if (narrow && editor.IsKeyboardFocusWithin)
                box.IsDropDownOpen = box.Items.Count > 0;
        }
    }
}
