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
        /// Type parameter holding the steel grade. Empty = grade is not read.
        /// </summary>
        public string gradeParameter { get; set; }

        /// <summary>
        /// Type parameter holding the width of a timber section. Empty = use the Revit
        /// type name.
        /// </summary>
        public string timberWidthParameter { get; set; }

        /// <summary>
        /// Type parameter holding the depth of a timber section. Empty = use the Revit
        /// type name.
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
        /// </summary>
        public double strengthRange { get; set; }

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
            strengthRange = 10;

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
