using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using CarboCircle;

namespace CarboCircle
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CarboCircleCommand : IExternalCommand
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

                FormStatusChecker.isWindowOpen = true;
                //CarboLifeApp.thisApp.ShowHeatmap(commandData.Application, projectToOpen, VisibleElements);

                CarboCircleApp.thisApp.ShowCarboCircle(commandData.Application);

                
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