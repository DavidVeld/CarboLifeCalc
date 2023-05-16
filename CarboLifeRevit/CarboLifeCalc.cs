using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeRevit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    class CarboLifeCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;    

            CarboSettings settings = new CarboSettings();
            settings = settings.Load();
            CarboGroupSettings importSettings = settings.defaultCarboGroupSettings;

            CarboGroupingSettingsDialog settingsWindow = new CarboGroupingSettingsDialog(importSettings);
            settingsWindow.ShowDialog();

            if (settingsWindow.dialogOk == System.Windows.MessageBoxResult.Yes)
            {
                string approvedPath = "";
                string path = settingsWindow.projectPath;
                importSettings = settingsWindow.importSettings;

                if (File.Exists(path))
                    approvedPath = path;

                CarboLifeRevitImport.ImportElements(app, importSettings, approvedPath, settingsWindow.selectedTemplateFile);
            }
            else
            {
                return Result.Succeeded;
            }



            return Result.Succeeded;
        }
    }
}
