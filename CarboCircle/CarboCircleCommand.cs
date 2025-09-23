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
                if (FormStatusChecker.isWindowOpen)
                {
                    MessageBox.Show("Window is already open. Close it before opening a new one.");
                    return Result.Cancelled;
                }

                CarboCircleApp.thisApp.ShowCarboCircle(commandData.Application);

                // The window is open now
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