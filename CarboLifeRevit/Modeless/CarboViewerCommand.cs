using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using CarboLifeRevit;
using CarboLifeAPI.Data;
using Microsoft.Win32;
using System.IO;
using CarboLifeRevit.Modeless;
using System.Windows;

namespace CarboLifeRevit
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CarboViewerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication app = commandData.Application;

                ///CarboGroupSettings importSettings = new CarboGroupSettings();
                //importSettings = importSettings.DeSerializeXML();

                //Show the form
                if (FormStatusChecker.isWindowOpen == true)
                {
                    //Window is open.
                    MessageBox.Show("Window is already open, make sure other instances are closed and restart the command.");
                    FormStatusChecker.isWindowOpen = true;

                    return Result.Cancelled;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    string projectPath = openFileDialog.FileName;

                    //Open the project
                    CarboProject projectToOpen = new CarboProject();

                    CarboProject projectToUpdate = new CarboProject();
                    CarboProject buffer = new CarboProject();
                    projectToUpdate = buffer.DeSerializeXML(projectPath);

                    projectToUpdate.Audit();
                    projectToUpdate.CalculateProject();

                    projectToOpen = projectToUpdate;

                    CarboProject ElementsVisibleOrSelected = CarboLifeRevitImport.CollectVisibleorSelectedElements(app, projectToOpen.RevitImportSettings, "");

                    List<int> VisibleElements = ElementsVisibleOrSelected.GetElementIdList();
                    CarboLifeApp.thisApp.ShowHeatmap(commandData.Application, projectToOpen, VisibleElements);

                }

                //Return result
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}