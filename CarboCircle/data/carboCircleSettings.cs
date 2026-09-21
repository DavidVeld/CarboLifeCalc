using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.IO;
using System.Xml.Serialization;

namespace CarboCircle.data
{
    /// <summary>
    /// The single store for every CarboCircle setting.
    ///
    /// Deliberately self-contained: CarboCircle does not read or write the main
    /// calculator's CarboSettings. The only thing it takes from the main application is
    /// material *values*, through a CarboLifeAPI material file named by
    /// <see cref="materialDataBasePath"/> - and even that path is a CarboCircle setting
    /// with a local fallback.
    ///
    /// Persisted as XML to circledb\CarboCircleSettings.xml. Unknown elements in an older
    /// file are ignored by XmlSerializer, and any property missing from the file keeps the
    /// constructor default, so older settings files load without migration.
    /// </summary>
    public class carboCircleSettings
    {
        //--------------------------------------------------------------------------------
        // Import - parameter mapping
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Type parameter holding the section size of a mined element. Empty = use the
        /// Revit type name.
        /// </summary>
        public string MineParameterName { get; set; }

        /// <summary>
        /// Type parameter holding the section size of a required element. Empty = use the
        /// Revit type name.
        /// </summary>
        public string RequiredParameterName { get; set; }

        /// <summary>
        /// Which of the two section-name overrides applies to the side being imported, or
        /// the empty string when that side has none and the Revit type name should be used.
        ///
        /// One method rather than the caller picking, because the choice is a property of
        /// these settings and getting it wrong is invisible: reading the mine's parameter
        /// on the project side does not fail, it silently identifies every proposed member
        /// by its type name instead.
        /// </summary>
        internal string sectionNameParameterFor(bool forProject)
        {
            string name = forProject ? RequiredParameterName : MineParameterName;

            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        }

        /// <summary>
        /// INSTANCE parameter that "Write reuse IDs into the model" writes the pairing id
        /// into, on the proposed member and on the existing member it comes out of.
        ///
        /// An instance parameter, not a type one, and that is the whole point: two beams of
        /// the same type get different ids because they are different pieces of steel. A
        /// type parameter would give every beam of that type the same answer.
        ///
        /// Empty means the default - see <see cref="reuseIdParameterOrDefault"/>. It is not
        /// a switch: there is no way to turn the write off here, because the write only ever
        /// happens when the user presses the button.
        /// </summary>
        public string reuseIdParameter { get; set; }

        /// <summary>
        /// What the default actually is, in one place.
        ///
        /// Spelt exactly as CarbonSharedParams.txt spells it, and that matters rather than
        /// being pedantry: this is the one parameter CarboCircle will add to a model itself,
        /// and it does so by binding that shared definition. A different spelling here would
        /// have the write look for "CLC_ReuseID", fail to find it, add "CLC_ReuseId", and
        /// still fail to find it - for ever.
        /// </summary>
        public const string DefaultReuseIdParameter = "CLC_ReuseId";

        /// <summary>
        /// The parameter to write into, never blank.
        ///
        /// A settings file written before this existed has no element for it, and
        /// XmlSerializer leaves absent elements at whatever the constructor set - so an old
        /// file comes back with the default rather than with null.
        ///
        /// A name that differs from the default only in case comes back as the default. The
        /// default used to be spelt "CLC_ReuseID" and was written into every settings file
        /// saved while it was, so without this those files would keep asking for a parameter
        /// that differs by one letter's case from the one CarboCircle now adds - and Revit's
        /// LookupParameter is case sensitive, so the write would never find it.
        /// </summary>
        internal string reuseIdParameterOrDefault()
        {
            if (string.IsNullOrWhiteSpace(reuseIdParameter))
                return DefaultReuseIdParameter;

            string trimmed = reuseIdParameter.Trim();

            return string.Equals(trimmed, DefaultReuseIdParameter, StringComparison.OrdinalIgnoreCase)
                ? DefaultReuseIdParameter
                : trimmed;
        }

        /// <summary>
        /// True when the name is the CarboLife reuse id parameter, which is the one name the
        /// write is allowed to add to a model - it has a shared parameter definition, with a
        /// fixed GUID, in CarbonSharedParams.txt. Any other name is the user's own and is
        /// used exactly as it is.
        /// </summary>
        internal static bool isDefaultReuseIdParameter(string name)
        {
            return name != null &&
                   string.Equals(name.Trim(), DefaultReuseIdParameter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Type parameter holding the steel grade.
        ///
        /// Persisted, but nothing reads it: grade comes from the material class on the
        /// element's material. The settings dialog says so rather than offering it as a
        /// setting, and no longer writes to it - so a value an older version left here
        /// survives untouched.
        /// </summary>
        public string gradeParameter { get; set; }

        /// <summary>
        /// Type parameter holding the width of a timber section.
        ///
        /// Persisted, but nothing reads it: the import takes width from the type parameter
        /// "b", the name Revit's own timber families use. As with
        /// <see cref="gradeParameter"/>, the dialog states that rather than offering a
        /// setting, and leaves whatever is stored here alone.
        /// </summary>
        public string timberWidthParameter { get; set; }

        /// <summary>
        /// Type parameter holding the depth of a timber section. Persisted and unread; the
        /// import takes depth from the type parameter "d". See
        /// <see cref="timberWidthParameter"/>.
        /// </summary>
        public string timberDepthParameter { get; set; }

        //--------------------------------------------------------------------------------
        // Import - extraction
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Remembered choice for the mine side. One of the
        /// <see cref="carboCircleExtractionMethod"/> values offered by
        /// <see cref="carboCircleExtractionMethod.MineMethods"/>.
        /// </summary>
        public string MineExtractionMethod { get; set; }

        /// <summary>
        /// Remembered choice for the project side. One of the
        /// <see cref="carboCircleExtractionMethod"/> values offered by
        /// <see cref="carboCircleExtractionMethod.RequiredMethods"/>.
        /// </summary>
        public string RequiredExtractionMethod { get; set; }

        //--------------------------------------------------------------------------------
        // Import - what to collect, and what is lost in deconstruction
        //--------------------------------------------------------------------------------

        public bool ConsiderWalls { get; set; }
        public bool ConsiderSlabs { get; set; }
        public bool ConsiderColumnBeams { get; set; }

        /// <summary>
        /// Length lost cutting a steel member free, per end, in mm.
        /// </summary>
        public double cutoffbeamLength { get; set; }

        /// <summary>
        /// Length lost cutting a timber member free, per end, in mm.
        /// </summary>
        public double timberCutoffLength { get; set; }

        /// <summary>
        /// Below these allowances the ends cannot be released by ordinary means, so the
        /// figure only holds if a specialist deconstruction method is actually procured.
        ///
        /// Advisory, not a limit. A lower allowance is a legitimate thing to model - it is
        /// what makes the case for the specialist method in the first place - so nothing here
        /// clamps or rejects the value. The two windows that edit these lengths say so on
        /// screen instead.
        ///
        /// Const rather than a setting: the numbers describe what site plant can do, not what
        /// this project has chosen, and both windows have to agree on them or a value that
        /// warned in one place would pass silently in the other.
        /// </summary>
        public const double SteelCutoffAdvisoryMin = 500;

        /// <summary>Timber counterpart of <see cref="SteelCutoffAdvisoryMin"/>, in mm.</summary>
        public const double TimberCutoffAdvisoryMin = 250;

        /// <summary>What both windows show when an allowance falls below its advisory minimum.</summary>
        public const string CutoffAdvisoryMessage = "Reduced value requires specialist demolition";

        /// <summary>
        /// Concrete volume lost in deconstruction, in %.
        /// </summary>
        public int VolumeLoss { get; set; }

        /// <summary>
        /// Masonry volume lost in deconstruction, in %.
        /// </summary>
        public int MasonryLoss { get; set; }

        //--------------------------------------------------------------------------------
        // Databases - both CarboCircle-owned, both fall back to the shipped copy
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Steel section mapping table (csv). Empty or missing = circledb\CarboCircleMasterSections.csv.
        /// </summary>
        public string dataBasePath { get; set; }

        /// <summary>
        /// Material file (cxml) supplying the carbon values for reused materials. Empty or
        /// missing = circledb\carboCircleMaterials.cxml.
        ///
        /// This is the one place CarboCircle leans on the main calculator: the file is in
        /// CarboLifeAPI's material format and is handed to CarboProject as its template.
        /// Point it at one of the main application's databases to use those values instead.
        /// </summary>
        public string materialDataBasePath { get; set; }

        //--------------------------------------------------------------------------------
        // Matching tolerances
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Permitted extra section depth when substituting a member, in mm.
        /// </summary>
        public double depthRange { get; set; }

        /// <summary>
        /// Permitted extra strength when substituting a member, in %.
        ///
        /// This is the width of the substitution window, and it is the one setting that decides
        /// whether the tool finds anything at all. Consecutive serial sizes in one section
        /// family differ by roughly 10-18% in elastic modulus - a 305x165x40 UB to a
        /// 305x165x46 UB is 15% - so a 10% window admits almost nothing but the exact section,
        /// which is why the default is 25%: wide enough to reach the next size or two up, tight
        /// enough that the substitute is still recognisably the right member.
        /// </summary>
        public double strengthRange { get; set; }

        /// <summary>
        /// Permitted extra section width when substituting a member, in mm.
        ///
        /// The companion to <see cref="depthRange"/>. Depth alone does not describe a section:
        /// without this a 254 mm wide UC would be offered for a 165 mm wide UB purely because
        /// it is shallow enough, and it would not fit the detail it has to sit in.
        /// </summary>
        public double widthRange { get; set; }

        /// <summary>
        /// The shortest offcut worth putting back into stock, in mm.
        ///
        /// A remnant below this is reported as waste rather than offered again, because the
        /// handling and the second saw cut cost more than the steel is worth.
        /// </summary>
        public double minOffcutLength { get; set; }

        /// <summary>
        /// Whether a column section may be offered for a beam requirement.
        ///
        /// The one cross-family substitution the tool will consider: family H stock - UC, UKC,
        /// HE - serving a family I requirement - UB, UKB, IPE. A UC of equal bending capacity
        /// is shallower and much stiffer about the minor axis, so it is a sound beam; the
        /// reverse is not true, and no other pairing of families is offered in either
        /// direction.
        ///
        /// Off by default. It changes the shape of the member, so every connection on it has to
        /// be re-detailed, and that is a decision for the engineer to opt
        /// into rather than one for the tool to assume.
        /// </summary>
        public bool allowCrossFamilySubstitution { get; set; }

        //--------------------------------------------------------------------------------
        // Visualisation colours
        //--------------------------------------------------------------------------------

        /// <summary>Mined element that was matched to a requirement.</summary>
        public CarboColour colour_ReusedMinedData { get; set; }

        /// <summary>Mined element that found no taker.</summary>
        public CarboColour colour_NotReused { get; set; }

        /// <summary>Required element satisfied from reused stock.</summary>
        public CarboColour colour_FromReusedData { get; set; }

        /// <summary>Required element that needs new material.</summary>
        public CarboColour colour_NotFromReused { get; set; }

        /// <summary>Mined volume element available for reuse.</summary>
        public CarboColour colour_ReusedMinedVolumes { get; set; }

        /// <summary>
        /// Required volume satisfied from reused stock. Persisted and defaulted, but the
        /// current visualisation has no separate list for it.
        /// </summary>
        public CarboColour colour_FromReusedVolumes { get; set; }

        public carboCircleSettings()
        {
            MineParameterName = string.Empty;
            RequiredParameterName = string.Empty;
            reuseIdParameter = DefaultReuseIdParameter;
            gradeParameter = string.Empty;
            timberWidthParameter = string.Empty;
            timberDepthParameter = string.Empty;

            //Named constants rather than repeated literals: these are the same values the
            //combo boxes offer and the collector switches on.
            MineExtractionMethod = carboCircleExtractionMethod.AllDemolishedInView;
            RequiredExtractionMethod = carboCircleExtractionMethod.AllNewInView;

            ConsiderWalls = false;
            ConsiderSlabs = false;
            ConsiderColumnBeams = true;

            cutoffbeamLength = 600;
            timberCutoffLength = 300;
            VolumeLoss = 25;
            MasonryLoss = 25;

            dataBasePath = string.Empty;
            materialDataBasePath = string.Empty;

            depthRange = 50;
            strengthRange = 25;
            widthRange = 50;
            minOffcutLength = 1000;
            allowCrossFamilySubstitution = false;

            colour_ReusedMinedData = new CarboColour(255, 25, 160, 235);
            colour_NotReused = new CarboColour(255, 235, 235, 235);
            colour_FromReusedData = new CarboColour(255, 80, 220, 80);
            colour_NotFromReused = new CarboColour(255, 250, 220, 220);
            colour_ReusedMinedVolumes = new CarboColour(255, 50, 50, 255);
            colour_FromReusedVolumes = new CarboColour(255, 255, 50, 255);
        }

        //--------------------------------------------------------------------------------
        // Load / Save / Copy
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Reads the settings file. Never returns null: a missing file is created from
        /// defaults, and an unreadable one is replaced by defaults.
        /// </summary>
        public carboCircleSettings Load()
        {
            string path = getCircleSettingsFilePath();

            if (path == null)
                return new carboCircleSettings();

            if (!File.Exists(path))
            {
                carboCircleSettings fresh = new carboCircleSettings();
                fresh.Save();
                return fresh;
            }

            string failure = null;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(carboCircleSettings));

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    carboCircleSettings loaded = ser.Deserialize(fs) as carboCircleSettings;

                    if (loaded != null)
                        return loaded;
                }
            }
            catch (Exception ex)
            {
                failure = ex.Message;
            }

            //Unreadable or empty: repair with defaults rather than handing back null.
            //Repair first, then tell the user - so the file is already good by the time
            //anyone dismisses the message.
            carboCircleSettings repaired = new carboCircleSettings();
            repaired.Save();

            if (failure != null)
            {
                System.Windows.MessageBox.Show(
                    "The CarboCircle settings file could not be read and has been reset to defaults." +
                    Environment.NewLine + Environment.NewLine + failure,
                    "CarboCircle settings", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }

            return repaired;
        }

        /// <summary>
        /// Writes the settings file. Returns true only if the file was actually written.
        /// </summary>
        public bool Save()
        {
            string path = getCircleSettingsFilePath();

            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(carboCircleSettings));

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    ser.Serialize(fs, this);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "CarboCircle settings",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
        }

        /// <summary>
        /// Independent snapshot, used to give the settings dialog something it can edit and
        /// throw away on Cancel.
        ///
        /// Cloning through the serializer rather than listing members by hand is
        /// deliberate: the previous hand-written version silently dropped
        /// <see cref="gradeParameter"/> and copied the wrong source into
        /// <see cref="colour_NotFromReused"/>, so every load and every import quietly
        /// corrupted the file. Anything that survives Save/Load now survives Copy, by
        /// construction.
        ///
        /// Every member is now a persisted preference, so a serializer round trip is a
        /// complete clone and nothing has to be carried by hand afterwards.
        /// </summary>
        internal carboCircleSettings Copy()
        {
            carboCircleSettings clone;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(carboCircleSettings));

                using (MemoryStream ms = new MemoryStream())
                {
                    ser.Serialize(ms, this);
                    ms.Position = 0;
                    clone = ser.Deserialize(ms) as carboCircleSettings ?? new carboCircleSettings();
                }
            }
            catch
            {
                //A clone that throws must not take the window down with it.
                clone = new carboCircleSettings();
            }

            return clone;
        }

        //--------------------------------------------------------------------------------
        // Paths
        //--------------------------------------------------------------------------------

        /// <summary>
        /// Location of the CarboCircle settings file, creating the folder if needed.
        /// The file itself does not have to exist - <see cref="Load"/> creates it.
        /// </summary>
        internal static string getCircleSettingsFilePath()
        {
            try
            {
                string folder = Path.Combine(Utils.getAssemblyPath(), "circledb");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                return Path.Combine(folder, "CarboCircleSettings.xml");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Steel section mapping table: the configured file if it exists, otherwise the
        /// copy shipped in circledb.
        /// </summary>
        internal string getSectionDatabasePath()
        {
            return resolveAgainstCircleDb(dataBasePath, "CarboCircleMasterSections.csv");
        }

        /// <summary>
        /// Reuse material file: the configured file if it exists, otherwise the copy
        /// shipped in circledb.
        /// </summary>
        internal string getMaterialDatabasePath()
        {
            return resolveAgainstCircleDb(materialDataBasePath, "carboCircleMaterials.cxml");
        }

        private static string resolveAgainstCircleDb(string configuredPath, string shippedFileName)
        {
            if (!string.IsNullOrEmpty(configuredPath) && File.Exists(configuredPath))
                return configuredPath;

            try
            {
                return Path.Combine(Utils.getAssemblyPath(), "circledb", shippedFileName);
            }
            catch
            {
                return null;
            }
        }
    }
}
