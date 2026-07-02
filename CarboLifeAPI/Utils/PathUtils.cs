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

                if (!(File.Exists(expectedTemplatePath)))
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

                if (!(File.Exists(expectedMappingPath)))
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
                    // Heal the stored path if it had drifted
                    if (!string.Equals(resolved, settings.templatePath, StringComparison.OrdinalIgnoreCase))
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
        /// Finds the location of the Carbo Life Calculator Template File
        /// </summary>
        /// <returns>Template Path</returns>
        public static IDictionary<string, string> getTemplateFiles()
        {
            CarboSettings settings = new CarboSettings().Load();
            return GetTemplateFiles(settings.templatePath);
            
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string defaultTemplatePath = getTemplateFile();

                if (string.IsNullOrEmpty(defaultTemplatePath)) return result;

                string defaultFileName = Path.GetFileName(defaultTemplatePath);
                result.Add(defaultFileName, defaultTemplatePath);

                // Get the directory containing the default template
                string templateDirectory = Path.GetDirectoryName(defaultTemplatePath);
                if (string.IsNullOrEmpty(templateDirectory) || !Directory.Exists(templateDirectory)) return result;

                // Define extensions to search for
                string[] extensions = { "*.cxml", "*.csv" };

                foreach (var ext in extensions)
                {
                    string[] files = Directory.GetFiles(templateDirectory, ext);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);

                        // Skip if already added (the default template)
                        if (!result.ContainsKey(fileName))
                        {
                            result.Add(fileName, file);
                        }
                    }
                }
            }
            catch
            {
                // Silently fail or log as needed for your implementation
            }

            return result;
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
                foreach (string file in Directory.GetFiles(searchDir, ext))
                {
                    string name = Path.GetFileName(file);
                    if (!result.ContainsKey(name))
                        result.Add(name, file);
                }
            }

            return result;
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
        /// Copies a new file over the existing settings file path.
        /// </summary>
        /// <param name="newFilePath">The path selected by the user.</param>
        /// <param name="targetSettingsPath">The internal Revit settings path.</param>
        public static void OverrideSettingsFile(string newFilePath, string targetSettingsPath)
        {
            if (string.IsNullOrEmpty(newFilePath) || !File.Exists(newFilePath)) return;

            try
            {
                // Ensure the directory for the target exists
                string directory = Path.GetDirectoryName(targetSettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Copy the new file over the old one (true = overwrite)
                File.Copy(newFilePath, targetSettingsPath, true);

                MessageBox.Show("Settings imported and updated successfully.",
                                "Import Complete",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to override settings: {ex.Message}",
                                "Import Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
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