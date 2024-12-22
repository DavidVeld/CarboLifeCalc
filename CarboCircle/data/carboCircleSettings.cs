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

        public string MineStyle { get; set; } //Always By Type, or type name if empty
        public string RequiredStyle { get; set; } //Always By Type, or type name if empty


        //existingImportSettings
        public double cutoffbeamLength { get; set; } //600 defaiult
        public double cutoffColumnLength { get; set; } //600 defaiult
        public int VolumeLoss { get; set; }
        public int MasonryLoss { get; set; }

        public bool ConsiderWalls { get; set; }
        public bool ConsiderSlabs { get; set; }
        public bool ConsiderColumnBeams { get; set; }


        //matchSettings
        public double depthRange { get; set; }
        public double strengthRange { get; set; }

        //colours
        public CarboColour colour_ReusedMinedData { get; set; }
        public CarboColour colour_ReusedMinedVolumes { get; set; }
        public CarboColour colour_NotReused { get; set; }
        public CarboColour colour_FromReusedData { get; set; }
        public CarboColour colour_FromReusedVolumes { get; set; }
        public CarboColour colour_NotFromReused { get; set; }

        public carboCircleSettings()
        {

            MineParameterName = string.Empty;
            RequiredParameterName = string.Empty;

            MineStyle = string.Empty;
            RequiredStyle = string.Empty;

            cutoffbeamLength = 600;
            cutoffColumnLength = 600;
            VolumeLoss = 25;
            MasonryLoss = 25;

            depthRange = 50;
            strengthRange = .10;

            ConsiderWalls = false;
            ConsiderSlabs = false;
            ConsiderColumnBeams = true;

            colour_ReusedMinedData = new CarboColour();
            colour_ReusedMinedVolumes = new CarboColour();
            colour_NotReused = new CarboColour();
            colour_FromReusedData = new CarboColour();
            colour_FromReusedVolumes = new CarboColour();
            colour_NotFromReused = new CarboColour();
        }

        public CarboSettings Load()
        {
            return DeSerializeXML();
        }
        public bool Save()
        {
            return SerializeXML();
        }
        private CarboSettings DeSerializeXML()
        {
            string mySettingsPath = getCircleSettingsFilePath();

            if (File.Exists(mySettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));
                    CarboSettings bufferproject;

                    using (FileStream fs = new FileStream(mySettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboSettings;
                    }

                    //If the settings exists and all is well use this:
                    return bufferproject;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                CarboSettings newsettings = new CarboSettings();
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
                XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));

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

                MineStyle = this.MineStyle,
                RequiredStyle= this.RequiredStyle,
                cutoffbeamLength = this.cutoffbeamLength,
                cutoffColumnLength = this.cutoffColumnLength,
                VolumeLoss = this.VolumeLoss,
                MasonryLoss = this.MasonryLoss,

                depthRange = this.depthRange,
                strengthRange = this.strengthRange,

                ConsiderWalls = this.ConsiderWalls,
                ConsiderSlabs = this.ConsiderSlabs,
                ConsiderColumnBeams = this.ConsiderColumnBeams,

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
