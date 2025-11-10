using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarboLifeRevit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    class CarboLifeCalcUpdate : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ///This command is Obsolete
            UIApplication app = commandData.Application;

            try
            {
                string projectOpenPath = Utils.OpenCarboProject();

                if (projectOpenPath != "")
                {
                    //Import the Files
                    CarboLifeRevitImport.ImportElements(app, null, projectOpenPath, "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Result.Succeeded;
        }
    }
}
