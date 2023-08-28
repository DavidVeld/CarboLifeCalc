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

            //Check if the document is a family & has a 3D View active


            if (app.ActiveUIDocument.Document.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "The command cannot run from the Family Editor, please run the command again in a Project document in an active 3D view.");
                return Result.Failed;
            }

            if (app.ActiveUIDocument.Document.ActiveView.ViewType != ViewType.ThreeD)
            {
                TaskDialog.Show("Error", "The command has to be started in an active 3D view, please run the command again in a Project document in an active 3D view.");
                return Result.Failed;
            }

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
