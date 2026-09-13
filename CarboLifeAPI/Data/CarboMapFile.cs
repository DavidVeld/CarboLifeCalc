using CarboLifeAPI.Data.Superseded;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboMapFile
    {
        public List<CarboMapElement> mappingTable { get; set; }

        public CarboMapFile()
        {
            mappingTable = new List<CarboMapElement>();
        }

        /// <summary>
        /// Writes the mapping table.
        ///
        /// This is the file the whole company shares, so it is written the careful way: the new
        /// content goes to a temporary file next to the target first and only replaces the real
        /// one once it is complete, and a lock held by a colleague is retried for a moment before
        /// giving up. Serialising straight onto the shared path would leave everyone with a
        /// half written mapping file if the network dropped mid-write.
        ///
        /// The old version returned void, skipped the write entirely when the file was locked and
        /// swallowed every exception, so a user whose colleague had the shared file open lost
        /// their mapping work with no message at all.
        /// </summary>
        /// <param name="path">Target file, empty uses the configured mapping file.</param>
        /// <returns>True when the file was written.</returns>
        public bool SaveToXml(string path = "")
        {
            string error;
            return SaveToXml(path, out error);
        }

        /// <summary>
        /// As SaveToXml, with the reason it failed so the caller can tell the user.
        /// </summary>
        /// <param name="path">Target file, empty uses the configured mapping file.</param>
        /// <param name="error">Empty on success, otherwise a sentence fit to show.</param>
        public bool SaveToXml(string path, out string error)
        {
            error = "";

            string myPath = string.IsNullOrEmpty(path) ? PathUtils.GetMappingFilePath() : path;

            if (string.IsNullOrEmpty(myPath))
            {
                error = "No mapping file location is configured.";
                return false;
            }

            //A colleague saving at the same moment holds the file for a fraction of a second.
            //Worth waiting out; a file someone has genuinely left open is not.
            const int attempts = 3;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                if (File.Exists(myPath) && DataExportUtils.IsFileLocked(myPath))
                {
                    if (attempt < attempts)
                    {
                        System.Threading.Thread.Sleep(250);
                        continue;
                    }

                    error = "The mapping file is open or locked by another program:" + Environment.NewLine +
                            myPath + Environment.NewLine + Environment.NewLine +
                            "It may be open on a colleague's machine. Your mapping has NOT been saved.";
                    return false;
                }

                try
                {
                    string folder = Path.GetDirectoryName(myPath);
                    if (string.IsNullOrEmpty(folder) == false && Directory.Exists(folder) == false)
                        Directory.CreateDirectory(folder);

                    //Re-read whatever is on disk NOW and merge these rows into it, rather than
                    //writing the copy that was loaded when the dialog opened.
                    //
                    //Two technicians mapping at the same time both did
                    //LoadFromXml -> Merge -> SaveToXml, and the second save wrote the file as it
                    //had been BEFORE the first one, so the first person's mappings vanished with
                    //no error. The lock check below only catches the much narrower case of the
                    //file being held open at the moment of writing. Merging here makes two
                    //concurrent saves additive instead of destructive.
                    CarboMapFile toWrite;

                    if (TryMergeWithFileOnDisk(myPath, out toWrite) == false)
                    {
                        //The file is there but cannot be parsed. Writing these rows over it would
                        //throw away every other mapping in it, which is precisely what must not
                        //happen to a file the whole team shares.
                        error = "The mapping file exists but could not be read, so it has been left alone:" +
                                Environment.NewLine + myPath + Environment.NewLine + Environment.NewLine +
                                "Your mapping has NOT been saved. Overwriting it would have discarded " +
                                "everyone else's mappings. Repair or replace that file, then map again.";
                        return false;
                    }

                    //Write beside the target, then swap, so a failure never truncates the shared file.
                    string tempPath = myPath + ".tmp";

                    XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                    using (StreamWriter writer = new StreamWriter(tempPath, false, Encoding.UTF8))
                    {
                        serializer.Serialize(writer, toWrite);
                    }

                    if (File.Exists(myPath))
                        File.Delete(myPath);

                    File.Move(tempPath, myPath);

                    return true;
                }
                catch (Exception ex)
                {
                    if (attempt < attempts)
                    {
                        System.Threading.Thread.Sleep(250);
                        continue;
                    }

                    error = "The mapping file could not be written:" + Environment.NewLine +
                            myPath + Environment.NewLine + Environment.NewLine +
                            ex.Message + Environment.NewLine + Environment.NewLine +
                            "Your mapping has NOT been saved.";
                    return false;
                }
            }

            return false;
        }

        public static CarboMapFile LoadFromXml()
        {
            string myPath = PathUtils.GetMappingFilePath();

            if (File.Exists(myPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                    using (StreamReader reader = new StreamReader(myPath))
                    {
                        return (CarboMapFile)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error during deserialization: " + ex.Message);
                    return null;
                }
            }
            else
            {
                return new CarboMapFile();
            }
        }

        public static CarboMapFile ExportTo(string Filepath)
        {
            string myPath = Filepath;

            if (!File.Exists(myPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                    using (StreamReader reader = new StreamReader(myPath))
                    {
                        return (CarboMapFile)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error during deserialization: " + ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds what should actually be written: the file currently on disk with this
        /// instance's rows merged into it.
        ///
        /// Rows held here win where the keys match; anything on disk that is not held here is
        /// carried over untouched. That is what keeps a concurrent save from deleting a
        /// colleague's work, and what keeps other templates' rows out of harm's way.
        /// </summary>
        /// <param name="merged">What to write. Only meaningful when this returns true.</param>
        /// <returns>
        /// False only when a file exists and cannot be read - the one case where writing would
        /// destroy mappings rather than add to them. No file yet is a success: there is simply
        /// nothing to merge with.
        /// </returns>
        private bool TryMergeWithFileOnDisk(string path, out CarboMapFile merged)
        {
            merged = this;

            if (File.Exists(path) == false)
                return true;

            CarboMapFile onDisk;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                using (StreamReader reader = new StreamReader(path))
                {
                    onDisk = serializer.Deserialize(reader) as CarboMapFile;
                }
            }
            catch
            {
                return false;
            }

            if (onDisk == null || onDisk.mappingTable == null)
                return false;

            onDisk.Merge(this.mappingTable);
            merged = onDisk;
            return true;
        }

        /// <summary>
        /// The rows that belong to one material template.
        ///
        /// A lookup already requires the template to match, so rows for other templates can never
        /// be used by this project - they were simply carried around in memory and scanned past.
        /// Filtering on load makes that explicit and keeps a project working only with its own.
        /// </summary>
        public List<CarboMapElement> RowsForTemplate(string templateName)
        {
            List<CarboMapElement> result = new List<CarboMapElement>();

            if (mappingTable == null)
                return result;

            foreach (CarboMapElement row in mappingTable)
            {
                if (row == null)
                    continue;

                if (string.Equals(row.templateName, templateName, StringComparison.OrdinalIgnoreCase))
                    result.Add(row);
            }

            return result;
        }

        public void Merge(List<CarboMapElement> newMappingTable)
        {
            foreach (var newElement in newMappingTable)
            {
                var existingElement = mappingTable.Find(e =>
                    string.Equals(e.revitName, newElement.revitName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.category, newElement.category, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.templateName, newElement.templateName, StringComparison.OrdinalIgnoreCase)
                );

                if (existingElement != null)
                {
                    // Update the carboNAME of the matching element
                    existingElement.carboNAME = newElement.carboNAME;
                }
                else
                {
                    // Add new element to the mapping table
                    mappingTable.Add(newElement);
                }
            }

            CleanUp();
        }

        private void CleanUp()
        {
            // Remove duplicates based on revitName, category, and templateName.
            //
            // Case-insensitively, to agree with Merge above and with the lookup in
            // CarboProject.GetMapItem. Grouping on an anonymous type used the default string
            // comparer, which is case sensitive, so "Concrete" and "CONCRETE" both survived a
            // cleanup while the lookup - being case insensitive - could only ever reach the
            // first. The second row was dead weight that nothing could use.
            mappingTable = mappingTable
                .Where(e => e != null)
                .GroupBy(e => new MapKey(e.revitName, e.category, e.templateName))
                .Select(g => g.First())
                .ToList();
        }

        /// <summary>
        /// The identity of a mapping row: the Revit material, the element category it was seen
        /// in, and the material template it maps into. Compared the way every other part of the
        /// mapping path compares them, without regard to case.
        /// </summary>
        private sealed class MapKey : IEquatable<MapKey>
        {
            private readonly string revitName;
            private readonly string category;
            private readonly string templateName;

            public MapKey(string revitName, string category, string templateName)
            {
                this.revitName = revitName ?? "";
                this.category = category ?? "";
                this.templateName = templateName ?? "";
            }

            public bool Equals(MapKey other)
            {
                if (other == null)
                    return false;

                return string.Equals(revitName, other.revitName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(category, other.category, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(templateName, other.templateName, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as MapKey);
            }

            public override int GetHashCode()
            {
                //Must hash case insensitively or equal keys could land in different buckets.
                int h = 17;
                h = unchecked(h * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(revitName));
                h = unchecked(h * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(category));
                h = unchecked(h * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(templateName));
                return h;
            }
        }
    }
}
