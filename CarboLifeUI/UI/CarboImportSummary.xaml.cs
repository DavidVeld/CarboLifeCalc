using System;
using System.Windows;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// What the user is told once an import has finished and some groups were given a material
    /// the matcher was not confident about.
    ///
    /// Deliberately a count and one instruction rather than the listing this used to be. The old
    /// dialog named up to eight groups with their scores and match notes, which nobody read: by
    /// the time an import finishes the user wants the model, not a report. The per group detail
    /// is still in the Description column, where it can be read in context.
    ///
    /// Running the mapper is the point of the dialog. "Check these materials" with no way to act
    /// on it is not an instruction, it is a nag; pressing the mapper button opens it straight
    /// away, so the main window comes up with the materials already put right. That is why it is
    /// the first and the default button, and why the other one is labelled with what it actually
    /// means - the work comes back later - rather than "Ok", which reads like the way out.
    /// </summary>
    public partial class CarboImportSummary : Window
    {
        /// <summary>
        /// True when the user asked for the material mapper to be opened after the import.
        /// </summary>
        public bool RunMaterialMapper { get; private set; }

        private readonly string message;

        public CarboImportSummary()
        {
            message = "";
            InitializeComponent();
        }

        /// <param name="flaggedGroups">How many groups need their material checked.</param>
        /// <param name="matchedGroups">How many groups the matcher was asked about in total.</param>
        public CarboImportSummary(int flaggedGroups, int matchedGroups)
        {
            //"1 group was" rather than "1 groups were". A message this short has nowhere to hide
            //a grammar slip, and a small model flags a single group often enough.
            bool one = flaggedGroups == 1;

            string verb = one ? " was matched" : " were matched";
            string their = one ? "Its" : "Their";
            string them = one ? "it" : "them";

            message = flaggedGroups + " of " + matchedGroups + " material groups" + verb +
                      " with low confidence." + Environment.NewLine + Environment.NewLine +
                      their + " Element carbon was added to the project, but the material behind " + them +
                      " is a close guess." + Environment.NewLine + Environment.NewLine +
                      "Run the Material Mapper to confirm " + them + " the right material. " +
                      "What you map there is remembered for next time.";

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Description.Text = message;
        }

        private void Btn_Later_Click(object sender, RoutedEventArgs e)
        {
            RunMaterialMapper = false;
            this.Close();
        }

        private void Btn_OkAndMap_Click(object sender, RoutedEventArgs e)
        {
            RunMaterialMapper = true;
            this.Close();
        }
    }
}
