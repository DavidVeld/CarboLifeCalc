using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CarboCircle.data
{
    public class carboCircleSettings
    {
        //Project Parameters


        //generalImportSettings
        public string MineParameterName { get; set; } //Always By Type, or type name if empty
        public string RequiredParameterName { get; set; } //Always By Type, or type name if empty
        public string gradeParameter { get; set; } //Always By Type, or type name if empty

        /// <summary>
        /// "All Visible in View",
        /// "All New in View",
        /// "All Demolished in View",
        /// "Selected"
        /// </summary>
        public string extractionMethod { get; set; } //Always By Type, or type name if empty

        [Obsolete]

        /// <summary>
        /// "All Visible in View",
        /// "All New in View",
        /// "All Demolished in View",
        /// "Selected"
        /// </summary>
        public string MineExtractionMethod { get; set; } //Always By Type, or type name if empty

        [Obsolete]

        /// <summary>
        /// "All Visible in View",
        /// "All New in View",
        /// "All Demolished in View",
        /// "Selected"
        /// </summary>
        public string RequiredExtractionMethod { get; set; } //Always By Type, or type name if empty

        //existingImportSettings
        public double cutoffbeamLength { get; set; } //600 defaiult

        [Obsolete]
        public double cutoffColumnLength { get; set; } //600 defaiult

        /// <summary>
        /// in %
        /// </summary>
        public int VolumeLoss { get; set; }

        /// <summary>
        /// in %
        /// </summary>
        public int MasonryLoss { get; set; }

        public bool ConsiderWalls { get; set; }
        public bool ConsiderSlabs { get; set; }
        public bool ConsiderColumnBeams { get; set; }

        /// <summary>
        /// "Path to database",
        /// </summary>
        public string dataBasePath { get; set; } //Always By Type, or type name if empty

        //matchSettings
        /// <summary>
        /// in mm
        /// </summary>
        public double depthRange { get; set; }
        /// <summary>
        /// in %
        /// </summary>
        public double strengthRange { get; set; }

        //colours
        /// <summary>
        /// Existing reused beams
        /// </summary>
        public CarboColour colour_ReusedMinedData { get; set; }

        public CarboColour colour_NotReused { get; set; }
        public CarboColour colour_FromReusedData { get; set; }
        public CarboColour colour_NotFromReused { get; set; }

        /// <summary>
        /// Reused Volumes Not Uses
        /// </summary>
        public CarboColour colour_ReusedMinedVolumes { get; set; }

        /// <summary>
        /// Reused Volumes Not Uses
        /// </summary>
        public CarboColour colour_FromReusedVolumes { get; set; }

        public carboCircleSettings()
        {

            MineParameterName = string.Empty;
            RequiredParameterName = string.Empty;

            extractionMethod = string.Empty;
            MineExtractionMethod = string.Empty;
            RequiredExtractionMethod = string.Empty;

            cutoffbeamLength = 600;
            cutoffColumnLength = 600;
            VolumeLoss = 25;
            MasonryLoss = 25;

            depthRange = 50;
            strengthRange = 10;

            ConsiderWalls = false;
            ConsiderSlabs = false;
            ConsiderColumnBeams = true;

            colour_ReusedMinedData = new CarboColour(255,25,160,235);
            colour_ReusedMinedVolumes = new CarboColour(255, 50, 50, 255);
            colour_NotReused = new CarboColour(255, 235, 235, 235);
            colour_FromReusedData = new CarboColour(255, 80, 220, 80);
            colour_FromReusedVolumes = new CarboColour(255, 255, 50, 255);
            colour_NotFromReused = new CarboColour(255, 250, 220, 220);

            dataBasePath = "";
        }

        public carboCircleSettings Load()
        {
            return DeSerializeXML();
        }
        public bool Save()
        {
            return SerializeXML();
        }
        private carboCircleSettings DeSerializeXML()
        {
            string mySettingsPath = getCircleSettingsFilePath();
            carboCircleSettings bufferproject = new carboCircleSettings();

            if (File.Exists(mySettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(carboCircleSettings));

                    using (FileStream fs = new FileStream(mySettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as carboCircleSettings;
                    }

                    //If the settings exists and all is well use this:
                    return bufferproject;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    //override the current file with a new setting file as repair;
                    bufferproject.Save();
                    return null;
                }
            }
            else
            {
                carboCircleSettings newsettings = new carboCircleSettings();
                newsettings.Save();
                return newsettings;
            }
        }
        private bool SerializeXML()
        {
            bool result = false;

            string mySettingsPath = getCircleSettingsFilePath();
            if (mySettingsPath == null || mySettingsPath == "")
                return false;

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(carboCircleSettings));

                using (FileStream fs = new FileStream(mySettingsPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }

            return result;
        }

        /// <summary>
        /// Finds the location of the Carbo Life Calculator Settings File
        /// </summary>
        /// <returns>Settings File path</returns>
        internal static string getCircleSettingsFilePath()
        {
            //string fileName = "db\\CarboSettings.xml";
            string myLocalPath = Utils.getAssemblyPath() + "\\db\\" + "CarboCircleSettings.xml";
            try
            {
                if (File.Exists(myLocalPath))
                    return myLocalPath;
                else
                {
                    MessageBox.Show("Could not find a path reference to the CarboLifeCircle Setting File, you possibly have to re-install the software" + Environment.NewLine +
                            "Target: " + myLocalPath + Environment.NewLine +
                            "Target: " + myLocalPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return myLocalPath;
                }
            }
            catch 
            {
                return null;
            }

        }

        internal carboCircleSettings Copy()
        {
            carboCircleSettings clone = new carboCircleSettings
            {
                //Import settings
                MineParameterName = this.MineParameterName,
                RequiredParameterName = this.RequiredParameterName,

                extractionMethod = this.extractionMethod,
                //MineExtractionMethod = this.MineExtractionMethod,
                //RequiredExtractionMethod= this.RequiredExtractionMethod,

                cutoffbeamLength = this.cutoffbeamLength,
                cutoffColumnLength = this.cutoffColumnLength,
                VolumeLoss = this.VolumeLoss,
                MasonryLoss = this.MasonryLoss,

                depthRange = this.depthRange,
                strengthRange = this.strengthRange,

                ConsiderWalls = this.ConsiderWalls,
                ConsiderSlabs = this.ConsiderSlabs,
                ConsiderColumnBeams = this.ConsiderColumnBeams,

                dataBasePath = this.dataBasePath,

                //Colours
                colour_ReusedMinedData = this.colour_ReusedMinedData.Copy(),
                colour_ReusedMinedVolumes = this.colour_ReusedMinedVolumes.Copy(),
                colour_NotReused = this.colour_NotReused.Copy(),
                colour_FromReusedData = this.colour_FromReusedData.Copy(),
                colour_FromReusedVolumes = this.colour_FromReusedVolumes.Copy(),
                colour_NotFromReused= this.colour_NotReused.Copy()

            };

            return clone;


        }


    }
}
