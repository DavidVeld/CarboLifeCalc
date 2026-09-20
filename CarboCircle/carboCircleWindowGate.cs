using System;
using System.Windows;

namespace CarboCircle
{
    /// <summary>
    /// What happens when the ribbon button is pressed: build the window, or bring back the
    /// one that is already there.
    ///
    /// THE BUG THIS EXISTS TO STOP COMING BACK
    ///
    /// CarboCircle's Close button HIDES the window rather than closing it, so a mine and a
    /// project survive putting the tool away and bringing it back. The command that the
    /// ribbon button runs, though, refused to do anything at all while a window existed:
    ///
    ///     if (FormStatusChecker.isWindowOpen)
    ///     {
    ///         MessageBox.Show("Window is already open. Close it before opening a new one.");
    ///         return Result.Cancelled;
    ///     }
    ///
    /// which is precisely the state the Close button leaves behind. Close the window with
    /// its own button and the tool could not be opened again - the flag said a window was
    /// open, and the hidden window it was talking about was never shown. Close it with the
    /// title bar X instead and it worked, because a real close clears the flag. Two
    /// different ways to close, two different outcomes, which is why it looked intermittent.
    ///
    /// A modeless tool window has no business refusing to open. Pressing the button means
    /// "I want to see it", and the three states all have an obvious answer: build one, show
    /// the hidden one, or bring the visible one to the front. This class holds that
    /// decision, away from Revit types, so it can be tested by opening and closing a window
    /// for real rather than by reasoning about a bool.
    /// </summary>
    internal class carboCircleWindowGate
    {
        private Window current;

        /// <summary>The live window, or null when there is none. For shutdown.</summary>
        internal Window Window
        {
            get { return current; }
        }

        /// <summary>
        /// Hands back the window the user should now be looking at, building one with
        /// <paramref name="build"/> only when there is none to bring back.
        /// </summary>
        internal Window show(Func<Window> build)
        {
            if (build == null)
                throw new ArgumentNullException("build");

            if (current == null)
                current = adopt(build());

            try
            {
                current.Show();
            }
            catch (InvalidOperationException)
            {
                //"Cannot set Visibility or call Show after a window has closed." Normally
                //impossible - the Closed handler below drops the reference - but a window
                //whose constructor failed and closed itself was never adopted in the first
                //place, and that corpse used to be kept for the rest of the session with
                //every later press throwing on it. One rebuild, then let it fail for real.
                current = adopt(build());
                current.Show();
            }

            current.Activate();

            //Kept up to date for anything outside that still reads it. It is no longer
            //allowed to veto anything.
            FormStatusChecker.isWindowOpen = true;

            return current;
        }

        /// <summary>
        /// Forgets the window once it really closes. Hiding is not closing and must not
        /// reach here - that is the whole point of the Close button hiding.
        /// </summary>
        private Window adopt(Window window)
        {
            if (window == null)
                throw new InvalidOperationException("No CarboCircle window could be created.");

            window.Closed += delegate
            {
                //Only if it is still the current one. A rebuild after a failed Show would
                //otherwise have the old window's Closed clear the new one.
                if (ReferenceEquals(current, window))
                {
                    current = null;
                    FormStatusChecker.isWindowOpen = false;
                }
            };

            return window;
        }
    }
}
