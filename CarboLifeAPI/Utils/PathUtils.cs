using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows;
using System.Xml.Serialization;

namespace CarboLifeAPI
{
    public static class PathUtils
    {

        // ── Root locations ──────────────────────────────────────────────
        public static string GetAssemblyDir() =>
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string GetMaterialsDir() =>
            Path.Combine(GetAssemblyDir(), "db", "materials");

        public static string GetDbSettingsDir() =>
            Path.Combine(GetAssemblyDir(), "db", "settings");

        public static string GetDataDir() =>
            Path.Combine(GetAssemblyDir(), "data");

        // ── Settings file ───────────────────────────────────────────────
        public static string GetSettingsFilePath() =>
            Path.Combine(GetDbSettingsDir(), "CarboSettings.xml");

        // ── Default file locations ──────────────────────────────────────
        public static string GetDefaultTemplatePath() =>
            Path.Combine(GetMaterialsDir(), "UserMaterials.cxml");

        public static string GetDefaultMappingPath() =>
            Path.Combine(GetDbSettingsDir(), "defaultmappingfile.xml");


        /// <summary>
        /// The current location from where the application runs
        /// </summary>
        /// <returns>The application path </returns>
        /* Obsolete
        public static string getAssemblyPath()
        {
            string _path = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(_path);
            return myPath;
        }
        */

        /// <summary>
        /// True when a configured path cannot even be looked at right now, as opposed to simply
        /// not being there.
        ///
        /// This is the difference between "the company share is offline, the VPN is down, the
        /// drive letter is not mapped yet" and "the file was deleted or renamed". When the
        /// containing folder is reachable and the file is not in it, the file is genuinely gone
        /// and repointing at the local default is the right repair. When the folder cannot be
        /// reached at all, repointing is the wrong repair: it silently and permanently detaches
        /// the user from the shared template or mapping file over a momentary network blip.
        /// </summary>
        public static bool IsTemporarilyUnavailable(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (File.Exists(path))
                    return false;

                string folder = Path.GetDirectoryName(path);

                //No folder part means a bare filename, which is resolved locally anyway.
                if (string.IsNullOrEmpty(folder))
                    return false;

                //Folder reachable, file not in it: really missing.
                //Folder unreachable: cannot tell, so assume the location will come back.
                return Directory.Exists(folder) == false;
            }
            catch
            {
                //A path we cannot even inspect is exactly the case this guards.
                return true;
            }
        }

        /// <summary>
        /// Said once per session, not once per project: CheckFileLocations runs in every
        /// CarboProject constructor.
        /// </summary>
        private static bool warnedAboutUnavailablePaths = false;

        /// <summary>
        /// Sets and prepares the usermaterials and settings File
        /// Check Settings file
        /// Check Template file
        /// Check Mapping File
        /// </summary>
        public static void CheckFileLocations()
        {
            string log = "";
            bool errorTemplate = false;
            bool errorMapping = false;

            try
            {
                //Essential Locations:
                string bufferFilePath = Path.Combine(GetMaterialsDir(), "MaterialBuffer.cxml");

                //all error handeling and new setting file creation is done in the Load function.
                string expectedSettingsPath = getSettingsFilePath();

                string defaultTemplatePath = GetDefaultTemplatePath();
                string defaultMappingPath = GetDefaultMappingPath();


                if (File.Exists(bufferFilePath))
                    log += "Material Buffer file found at: " + bufferFilePath + Environment.NewLine;

                if(File.Exists(expectedSettingsPath))
                    log += "Settings file found at: " + expectedSettingsPath + Environment.NewLine;

                if (File.Exists(defaultTemplatePath))
                    log += "Default Template file found at: " + defaultTemplatePath + Environment.NewLine;
                if(File.Exists(defaultMappingPath))
                    log += "Default Mapping file found at: " + defaultMappingPath + Environment.NewLine;

                CarboSettings settings = new CarboSettings().Load();

                string expectedTemplatePath = settings.templatePath;
                string expectedMappingPath = settings.mappingPath;

                //A share that is merely offline must keep its stored path, see
                //IsTemporarilyUnavailable. Falling back for this run is fine, rewriting the
                //setting is not: the user would come back on the network still pointed at their
                //own local copy, with no sign that anything had changed.
                bool templateOffline = IsTemporarilyUnavailable(expectedTemplatePath);
                bool mappingOffline = IsTemporarilyUnavailable(expectedMappingPath);

                if (templateOffline || mappingOffline)
                {
                    string offlineLog = "";

                    if (templateOffline)
                        offlineLog += "Template file: " + expectedTemplatePath + Environment.NewLine;
                    if (mappingOffline)
                        offlineLog += "Mapping file: " + expectedMappingPath + Environment.NewLine;

                    log += "Location unreachable, keeping the setting: " + Environment.NewLine + offlineLog;

                    if (warnedAboutUnavailablePaths == false)
                    {
                        warnedAboutUnavailablePaths = true;

                        MessageBox.Show(
                            "A shared location set in your settings cannot be reached at the moment:" +
                            Environment.NewLine + Environment.NewLine + offlineLog + Environment.NewLine +
                            "The local copy is being used for now and your settings have been left alone, " +
                            "so the shared file will be picked up again once the location is back." +
                            Environment.NewLine + Environment.NewLine +
                            "If this location is gone for good, point at a new one in Settings.",
                            "Shared location unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (!(File.Exists(expectedTemplatePath)) && templateOffline == false)
                {
                    //The selected tenplate file could not be found, refert back to default:

                    if (File.Exists(defaultTemplatePath))
                        //revert back to default
                        settings.templatePath = defaultTemplatePath;
                    else
                    {
                        //If we dont have a template, create one:
                        if (File.Exists(bufferFilePath))
                        {
                            Directory.CreateDirectory(GetMaterialsDir());
                            File.Copy(bufferFilePath, defaultTemplatePath);
                            settings.templatePath = defaultTemplatePath;
                        }
                        else
                        {
                            log += "Error: Could not find or create a template file, please re-install the software." + Environment.NewLine;
                            errorTemplate = true;
                        }
                    }
                }
                else
                {
                    log += "User Template file found at: " + expectedTemplatePath + Environment.NewLine;
                }

                if (!(File.Exists(expectedMappingPath)) && mappingOffline == false)
                {
                    if (File.Exists(defaultMappingPath))
                        //revert back to default
                        settings.mappingPath = defaultMappingPath;
                    else
                    {
                        //If we dont have a mapping file, create an empty one at the default location:
                        try
                        {
                            Directory.CreateDirectory(GetDbSettingsDir());
                            new CarboMapFile().SaveToXml(defaultMappingPath);
                            settings.mappingPath = defaultMappingPath;
                        }
                        catch
                        {
                            log += "Error: Could not find or create a mapping file, please re-install the software." + Environment.NewLine;
                            errorMapping = true;
                        }
                    }
                }
                else
                {
                    log += "User Mapping file found at: " + expectedMappingPath + Environment.NewLine;
                }

                settings.Save();


                //Set the path to the template, ONLY when it's the first launch.
                if(errorMapping == true || errorTemplate == true)
                {
                    string fulllog = "Hi, this is most likely the first time you started Carbo Life Calculator, or a few files could not be found at startup and the app has automatically restored the settings. " + Environment.NewLine +
                        "Template File set as: " + Environment.NewLine + settings.templatePath + Environment.NewLine +
                        "Mapping File set as: " + Environment.NewLine + settings.mappingPath + Environment.NewLine;

                    MessageBox.Show(fulllog);
                }
            }
            catch (Exception ex)
            {
                log += "Error: " + ex.Message + Environment.NewLine;

                MessageBox.Show(log);
            }

        }



        /// <summary>
        /// Finds the location of the Carbo Life Calculator Settings File this is always installdir/db/settings/CarboSettings.xml
        /// If a settings file still exists in the legacy data folder it is migrated over once.
        /// </summary>
        /// <returns>Settings File path</returns>
        public static string getSettingsFilePath()
        {
            string settingsPath = GetSettingsFilePath();

            if (!File.Exists(settingsPath))
            {
                string legacyPath = Path.Combine(GetDataDir(), "CarboSettings.xml");
                if (File.Exists(legacyPath))
                {
                    try
                    {
                        Directory.CreateDirectory(GetDbSettingsDir());
                        File.Copy(legacyPath, settingsPath);
                    }
                    catch
                    {
                        //If migration fails a fresh settings file will be created on the next save.
                    }
                }
            }

            return settingsPath;
        }

        /// <summary>
        /// Returns the mapping file path as stored in the settings.
        /// Falls back to the default db/settings/defaultmappingfile.xml when the stored path is invalid.
        /// </summary>
        public static string GetMappingFilePath()
        {
            try
            {
                CarboSettings settings = new CarboSettings().Load();
                string resolved = ResolveMappingPath(settings.mappingPath);
                if (!string.IsNullOrEmpty(resolved))
                    return resolved;
            }
            catch
            {
                //fall back to the default location below
            }
            return GetDefaultMappingPath();
        }

        /// <summary>
        /// Finds the location of the Carbo Life Calculator Template File
        /// </summary>
        /// <returns>Template Path</returns>
        public static string getTemplateFile()
        {
            try
            {
                CarboSettings settings = new CarboSettings().Load();
                string resolved = PathUtils.ResolveTemplatePath(settings.templatePath);

                if (resolved != null)
                {
                    // Heal the stored path if it had drifted, but never when the stored location
                    // is only unreachable: writing the local fallback back into the settings there
                    // detaches the user from the company share permanently, over a dropped VPN.
                    if (!string.Equals(resolved, settings.templatePath, StringComparison.OrdinalIgnoreCase)
                        && IsTemporarilyUnavailable(settings.templatePath) == false)
                    {
                        settings.templatePath = resolved;
                        settings.Save();
                    }
                    return resolved;
                }

                MessageBox.Show(
                    "Could not find a template file. You may need to reinstall the software.\n" +
                    "Expected location: " + PathUtils.GetMaterialsDir(),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "";
            }
        }

        /// <summary>
        /// The material templates on offer beside the one the settings point at.
        /// The settings are read here; see GetTemplateFiles for the listing itself.
        /// </summary>
        public static IDictionary<string, string> getTemplateFiles()
        {
            CarboSettings settings = new CarboSettings().Load();
            return GetTemplateFiles(settings.templatePath);
        }

        // ── Template resolution ─────────────────────────────────────────
        /// <summary>
        /// Resolves the template file path.
        /// Priority: 1) stored absolute path if valid
        ///           2) stored filename found in local materials folder
        ///           3) default UserMaterials.cxml in materials folder
        /// </summary>
        public static string ResolveTemplatePath(string storedPath)
        {
            // 1. Stored absolute path still valid
            if (!string.IsNullOrEmpty(storedPath) && File.Exists(storedPath))
                return storedPath;

            // 2. Stored value might just be a filename — try local materials folder
            if (!string.IsNullOrEmpty(storedPath))
            {
                string fileName = Path.GetFileName(storedPath);
                string localGuess = Path.Combine(GetMaterialsDir(), fileName);
                if (File.Exists(localGuess))
                    return localGuess;
            }

            // 3. Fall back to default
            string defaultPath = GetDefaultTemplatePath();
            if (File.Exists(defaultPath))
                return defaultPath;

            return null; // caller handles missing file
        }

        // ── Mapping file resolution ─────────────────────────────────────
        public static string ResolveMappingPath(string storedPath)
        {
            if (!string.IsNullOrEmpty(storedPath) && File.Exists(storedPath))
                return storedPath;

            if (!string.IsNullOrEmpty(storedPath))
            {
                string fileName = Path.GetFileName(storedPath);
                string localGuess = Path.Combine(GetDbSettingsDir(), fileName);
                if (File.Exists(localGuess))
                    return localGuess;
            }

            string defaultPath = GetDefaultMappingPath();
            if (File.Exists(defaultPath))
                return defaultPath;

            return null;
        }

        // ── Template file discovery ─────────────────────────────────────
        /// <summary>
        /// The material templates on offer, keyed by file name.
        ///
        /// Sorted by name within each extension: Directory.GetFiles returns whatever order the file
        /// system hands back, which is alphabetical on a local NTFS volume but not on a network
        /// share, so the order of the lists built from this used to depend on where the materials
        /// folder happened to live.
        /// </summary>
        public static IDictionary<string, string> GetTemplateFiles(string storedPath)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string resolved = ResolveTemplatePath(storedPath);
            string searchDir = string.IsNullOrEmpty(resolved)
                ? GetMaterialsDir()
                : Path.GetDirectoryName(resolved);

            if (!Directory.Exists(searchDir)) return result;

            foreach (string ext in new[] { "*.cxml", "*.csv" })
            {
                List<string> files = new List<string>(Directory.GetFiles(searchDir, ext));
                files.Sort(delegate (string a, string b)
                {
                    return string.Compare(Path.GetFileName(a), Path.GetFileName(b),
                                          StringComparison.OrdinalIgnoreCase);
                });

                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    if (!result.ContainsKey(name))
                        result.Add(name, file);
                }
            }

            return result;
        }

        /// <summary>
        /// Which entry of a template list should start off selected.
        ///
        /// In order: the template the settings point at, then UserMaterials.cxml - the user's own
        /// materials, which is the file this application maintains and the one a new project is
        /// meant to be priced with - and only if neither is on the list, its first entry.
        ///
        /// Selecting by position was the whole default before this, so which database a new project
        /// or an import started with came down to the order the file system listed the materials
        /// folder in. That put a reference database like Okobaudat in front of the user's own
        /// materials, and the numbers a project came out with depended on it.
        /// </summary>
        /// <param name="available">The template file names on offer</param>
        /// <returns>The entry to select, null when there is nothing to select</returns>
        public static string GetDefaultTemplateSelection(IEnumerable<string> available)
        {
            if (available == null)
                return null;

            List<string> names = new List<string>(available);

            if (names.Count == 0)
                return null;

            List<string> wanted = new List<string>();

            try
            {
                CarboSettings settings = new CarboSettings().Load();
                string resolved = ResolveTemplatePath(settings.templatePath);

                if (string.IsNullOrEmpty(resolved) == false)
                    wanted.Add(Path.GetFileName(resolved));
            }
            catch
            {
                //An unreadable settings file is no reason to fail to offer a template, the user
                //materials below are the same thing a broken setting is reset to anyway.
            }

            wanted.Add(Path.GetFileName(GetDefaultTemplatePath()));

            foreach (string want in wanted)
            {
                if (string.IsNullOrEmpty(want))
                    continue;

                foreach (string name in names)
                {
                    if (string.Equals(name, want, StringComparison.OrdinalIgnoreCase))
                        return name;
                }
            }

            //Neither is there: the user materials file is missing or the list was built somewhere
            //else entirely, so the first entry is all there is to fall back on.
            return names[0];
        }








        /// <summary>
        /// Finds the location of the Carbo Life Calculator Downloaded Files Path
        /// </summary>
        /// <returns>Download Path</returns>
        [Obsolete]
        public static string getDownloadedPath(bool local = true)
        {
            //sourcePath = PathUtils.getDownloadedPath() + "\\db\\online\\" + selectedItem + ".cxml";


            string myPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CarboLifeCalc\\online\\";
            string myLocalPath = GetAssemblyDir() + "\\db\\online\\";

            if (local == true)
            {
                if (!Directory.Exists(myLocalPath))
                    Directory.CreateDirectory(myLocalPath);

                return myLocalPath;
            }
            else
            { 
                if (Directory.Exists(myPath))
                    return myPath;
                else if (Directory.Exists(myLocalPath))
                    return myLocalPath;
                else
                {
                    MessageBox.Show("Could not find a path reference to the download path, you possibly have to re-install the software" + Environment.NewLine +
                            "Target: " + myPath + Environment.NewLine +
                            "Target: " + myLocalPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }

        public static void CleanOnlineDir()
        {
            try
            {
                //Delete all the files in the online folder:
                //Get all file sin online folder:
                string onlinePath = PathUtils.getDownloadedPath();
                string[] files = Directory.GetFiles(onlinePath);

                foreach (string file in files)
                    File.Delete(file);

                if (Directory.Exists(onlinePath))
                    Directory.Delete(onlinePath);

            }
            catch (Exception ex)
            {
                MessageBox.Show("While trying to delete downloaded files I ran into an error: " + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// Copies a settings file a colleague shared over this machine's settings file.
        ///
        /// The incoming file is read as a CarboSettings first. Overwriting on the strength of the
        /// .xml extension alone let any XML file replace the settings, and the failure only showed
        /// up on the next launch as "settings could not be loaded" with everything back at
        /// defaults - by which point the original was gone.
        /// </summary>
        /// <param name="newFilePath">The path selected by the user.</param>
        /// <param name="targetSettingsPath">The internal Revit settings path.</param>
        /// <returns>True when the settings file was replaced.</returns>
        public static bool OverrideSettingsFile(string newFilePath, string targetSettingsPath)
        {
            if (string.IsNullOrEmpty(newFilePath) || !File.Exists(newFilePath))
                return false;

            if (string.IsNullOrEmpty(targetSettingsPath))
                return false;

            //Reject the file before touching anything, so a wrong pick costs nothing.
            try
            {
                XmlSerializer probe = new XmlSerializer(typeof(CarboSettings));

                using (FileStream fs = new FileStream(newFilePath, FileMode.Open, FileAccess.Read))
                {
                    if (probe.Deserialize(fs) as CarboSettings == null)
                        throw new InvalidOperationException("The file holds no settings.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("That file is not a Carbo Life Calculator settings file, so nothing has been changed." +
                                Environment.NewLine + Environment.NewLine +
                                Path.GetFileName(newFilePath) + Environment.NewLine + Environment.NewLine +
                                "Details: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                                "Import Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }

            try
            {
                // Ensure the directory for the target exists
                string directory = Path.GetDirectoryName(targetSettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //Keep the settings that are being replaced, so a bad import can be undone by hand.
                if (File.Exists(targetSettingsPath))
                    File.Copy(targetSettingsPath, targetSettingsPath + ".bak", true);

                // Copy the new file over the old one (true = overwrite)
                File.Copy(newFilePath, targetSettingsPath, true);

                MessageBox.Show("Settings imported and updated successfully.",
                                "Import Complete",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to override settings: {ex.Message}",
                                "Import Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Returns the full path of a template file given just its filename.
        /// Searches the materials folder and falls back to the resolved default.
        /// </summary>
        /// <param name="fileName">Filename selected by the user e.g. "UserMaterials.cxml"</param>
        /// <returns>Full path to the template file, or null if not found</returns>
        public static string getTemplateFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return ResolveTemplatePath(null); // fall back to default

            // 1. Already a full valid path (e.g. passed in from settings directly)
            if (File.Exists(fileName))
                return fileName;

            // 2. Just a filename — look in the materials folder
            string localPath = Path.Combine(GetMaterialsDir(), fileName);
            if (File.Exists(localPath))
                return localPath;

            // 3. Fall back to default template
            return ResolveTemplatePath(fileName);
        }
    }
}