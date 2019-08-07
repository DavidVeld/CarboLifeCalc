using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarboLifeRevit
{
    public class CarboLifeMenu : Autodesk.Revit.UI.IExternalApplication
    {
        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);
        public Result OnStartup(UIControlledApplication application)
        {
            //Create  A button
            RibbonPanel CarboCalcPanel = application.CreateRibbonPanel("CarboLifeCalcc");

            //Create  A button
            PushButton pB_CarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalc", "CarboLifeCalc", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalc")) as PushButton;
            PushButton pB_CarboCalcPlus = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalcPlus", "CarboLifeCalcPlus", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcPlus")) as PushButton;

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }

    }
}
