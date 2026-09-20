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

                //Show the form.
                //
                //No "window is already open" refusal any more. CarboCircle's own Close
                //button HIDES the window so a mine and a project survive putting the tool
                //away - which left isWindowOpen true, and this check then refused to open
                //the very window it was talking about. Closing with the title bar X cleared
                //the flag and worked; closing with the Close button did not. Same two
                //clicks, two different outcomes, which is what made it look intermittent.
                //
                //Pressing the ribbon button means "show me the window", and all three
                //states have an obvious answer. ShowCarboCircle gives them: build one,
                //un-hide the hidden one, or bring the visible one to the front.
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