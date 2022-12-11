using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

    class ShowCarboCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;

            try
            {
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

                    HeatMapCreator newWindow = new HeatMapCreator(projectToOpen);
                    newWindow.Show();
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
