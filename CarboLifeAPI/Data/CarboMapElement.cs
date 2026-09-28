using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboMapElement
    {
        public string revitName { get; set; }
        public string category { get; set; }
        public string carboNAME { get; set; }
        public string templateName { get; set; }

        //The three below only exist while the material mapper is open. They are not written to
        //the shared mapping file or to the project, so a file read back has them at their defaults.

        /// <summary>
        /// The description of the group this row was built from, provenance note included, so the
        /// mapper can show how sure the initial match was and whether a grade was considered.
        /// </summary>
        [XmlIgnore]
        public string description { get; set; }

        /// <summary>
        /// Whether this row goes to the shared mapping file on Accept. A user can correct a
        /// material for this one project without making that correction everyone's default.
        /// </summary>
        [XmlIgnore]
        public bool saveMapping { get; set; }

        /// <summary>
        /// The material the row held when the mapper opened, null for a row that did not come
        /// from the mapper. See <see cref="IsChangedByUser"/>.
        /// </summary>
        [XmlIgnore]
        public string originalCarboName { get; set; }

        public CarboMapElement()
        {
            revitName = "";
            category = "";
            carboNAME = "";
            templateName = "";
            description = "";
            saveMapping = true;
            originalCarboName = null;
        }

        /// <summary>
        /// True when the user picked a different material in the mapper. A row that did not come
        /// from the mapper counts as changed, so a map from any other source applies as before.
        /// </summary>
        public bool IsChangedByUser()
        {
            if (originalCarboName == null)
                return true;

            return string.Equals((originalCarboName ?? "").Trim(), (carboNAME ?? "").Trim(),
                                 StringComparison.OrdinalIgnoreCase) == false;
        }

    }
}
