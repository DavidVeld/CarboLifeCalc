namespace CarboCircle.data
{
    /// <summary>
    /// The ways an import can pick elements out of the active view.
    ///
    /// One source of truth for these four strings. They are the values shown in the two
    /// combo boxes, the values persisted as
    /// <see cref="carboCircleSettings.MineExtractionMethod"/> /
    /// <see cref="carboCircleSettings.RequiredExtractionMethod"/>, and the values the
    /// collector switches on - so a rename in one place cannot quietly stop matching in
    /// another and send the import down the wrong branch.
    ///
    /// Strings rather than an enum because they are also the interface labels and are
    /// written to the settings file, where a readable value survives editing by hand.
    /// </summary>
    internal static class carboCircleExtractionMethod
    {
        /// <summary>Everything the active view shows, whatever phase it belongs to.</summary>
        public const string AllVisibleInView = "All Visible in View";

        /// <summary>Elements created in the phase the active view is set to.</summary>
        public const string AllNewInView = "All New in View";

        /// <summary>Elements demolished in the phase the active view is set to.</summary>
        public const string AllDemolishedInView = "All Demolished in View";

        /// <summary>Whatever is selected in the model right now.</summary>
        public const string Selected = "Selected";

        /// <summary>
        /// The methods offered for mining an existing building, in interface order.
        /// </summary>
        public static string[] MineMethods()
        {
            return new string[] { AllVisibleInView, AllDemolishedInView, Selected };
        }

        /// <summary>
        /// The methods offered for reading the proposed structure, in interface order.
        /// </summary>
        public static string[] RequiredMethods()
        {
            return new string[] { AllVisibleInView, AllNewInView, Selected };
        }
    }
}
