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



            ImportSettingsWindow settingsWindow = new ImportSettingsWindow();
            settingsWindow.ShowDialog();

            if (settingsWindow.dialogOk == System.Windows.MessageBoxResult.Yes)
            {
                CarboRevitImportSettings importSettings = settingsWindow.importSettings;
                importSettings = importSettings.DeSerializeXML();


                CarboLifeRevitImport.ImportElements(app, importSettings, "");
            }
            else
            {
                return Result.Succeeded;
            }



            return Result.Succeeded;
        }
    }
}
