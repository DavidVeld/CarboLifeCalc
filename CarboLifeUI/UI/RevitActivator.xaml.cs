using CarboLifeAPI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    public partial class RevitActivator : Window
    {
        internal bool isAccepted;

        private record VersionEntry(
            string Year,
            string AddinFolder,
            CheckBox Checkbox,
            Label Label);

        private VersionEntry[] _versions;

        public RevitActivator()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Which Revit versions this binary can actually be loaded by. The 4.8 build is
            //compiled against the Revit 2023 API and runs on 2023-2024, the last Revit
            //generation on .NET Framework; the NET8 build starts at 2025. Offering a
            //version from the wrong generation would install an add-in Revit cannot load.
#if NETFRAMEWORK
            _versions = new[]
            {
                new VersionEntry("2023",
                    @"C:\ProgramData\Autodesk\Revit\Addins\2023",
                    chx_slot1, lbl_slot1),

                new VersionEntry("2024",
                    @"C:\ProgramData\Autodesk\Revit\Addins\2024",
                    chx_slot2, lbl_slot2),
            };
#else
            _versions = new[]
            {
                new VersionEntry("2025",
                    @"C:\ProgramData\Autodesk\Revit\Addins\2025",
                    chx_slot1, lbl_slot1),

                new VersionEntry("2026",
                    @"C:\ProgramData\Autodesk\Revit\Addins\2026",
                    chx_slot2, lbl_slot2),

                new VersionEntry("2027",
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        @"Autodesk\Revit\Addins\2027"),
                    chx_slot3, lbl_slot3),
            };
#endif

            HideUnusedRows();
            CheckRevitVersions();
        }

        /// <summary>
        /// The window has a fixed set of rows; collapse the ones this build has no version for.
        /// </summary>
        private void HideUnusedRows()
        {
            var rows = new[] { row_slot1, row_slot2, row_slot3 };

            for (int i = _versions.Length; i < rows.Length; i++)
                rows[i].Visibility = Visibility.Collapsed;
        }

        private void CheckRevitVersions()
        {
            foreach (var v in _versions)
                CheckSingleVersion(v);
        }

        private void CheckSingleVersion(VersionEntry v)
        {
            string addinFile = Path.Combine(v.AddinFolder, "CarboLifeCalc.addin");

            if (Directory.Exists(v.AddinFolder))
            {
                v.Checkbox.IsEnabled = true;

                if (File.Exists(addinFile))
                {
                    v.Checkbox.IsChecked = true;
                    v.Label.Foreground = Brushes.Green;
                    v.Label.Content = $"Revit {v.Year} — addin installed";
                }
                else
                {
                    v.Checkbox.IsChecked = false;
                    v.Label.Foreground = SystemColors.ControlTextBrush;
                    v.Label.Content = $"Revit {v.Year} — addin not installed";
                }
            }
            else
            {
                v.Checkbox.IsChecked = false;
                v.Checkbox.IsEnabled = false;
                v.Label.Foreground = Brushes.Gray;
                v.Label.Content = $"Revit {v.Year} — not found";
            }
        }

        private void Btn_Apply(object sender, RoutedEventArgs e)
        {
            string dirPath = Utils.getAssemblyPath();
            string rawAddin = Path.Combine(dirPath, "CarboLifeCalcRaw.addin");
            string builtAddin = Path.Combine(dirPath, "CarboLifeCalc.addin");

            try
            {
                if (!File.Exists(rawAddin))
                {
                    MessageBox.Show("Raw addin file not found:\n" + rawAddin,
                        "Missing file", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UpdateAddinFile(dirPath);

                if (!File.Exists(builtAddin))
                {
                    MessageBox.Show("Failed to create addin file.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                foreach (var v in _versions)
                {
                    string target = Path.Combine(v.AddinFolder, "CarboLifeCalc.addin");

                    if (v.Checkbox.IsChecked == true)
                    {
                        CopyAddin(builtAddin, target);
                    }
                    else if (File.Exists(target))
                    {
                        File.Delete(target);
                    }
                }

                File.Delete(builtAddin);
                CheckRevitVersions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CopyAddin(string sourcePath, string targetPath)
        {
            try
            {
                string targetDir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.Copy(sourcePath, targetPath, overwrite: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy addin to {targetPath}:\n{ex.Message}",
                    "Copy failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void UpdateAddinFile(string dirPath)
        {
            string rawPath = Path.Combine(dirPath, "CarboLifeCalcRaw.addin");
            string outPath = Path.Combine(dirPath, "CarboLifeCalc.addin");

            string text = File.ReadAllText(rawPath);
            text = text.Replace("[PATH]", dirPath);
            File.WriteAllText(outPath, text);
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            Close();
        }
    }
}