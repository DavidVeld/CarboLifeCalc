using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CarboLifeRevit
{
    public class CarboLifeMenu : Autodesk.Revit.UI.IExternalApplication
    {
        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

        public Result OnStartup(UIControlledApplication application)
        {
            //Create  A button
            RibbonPanel CarboCalcPanel = application.CreateRibbonPanel("CarboLifeCalc");

            //Info
            string HelpURL = "https://github.com/DavidVeld/CarboLifeCalc/wiki";
            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, HelpURL);

            //Create  A button
            PushButton pB_CarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalc", "New Project", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalc")) as PushButton;

            Uri img_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife32.jpg");
            BitmapImage limg_CarboCalc = new BitmapImage(img_CarboCalc);
            pB_CarboCalc.LargeImage = limg_CarboCalc;
            pB_CarboCalc.Image = limg_CarboCalc;
            pB_CarboCalc.SetContextualHelp(contextualHelp);
            pB_CarboCalc.ToolTip = "Create a new Carbo Calc Project using the BIM model";

            PushButton pB_ImportCarboCalc = CarboCalcPanel.AddItem(new PushButtonData("Import CarboLife Calc", "Update Project", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcUpdate")) as PushButton;

            Uri img_CarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_UpdateCarboLife32.jpg");
            BitmapImage limg_CarboCalc2 = new BitmapImage(img_CarboCalc2);
            pB_ImportCarboCalc.LargeImage = limg_CarboCalc2;
            pB_ImportCarboCalc.Image = limg_CarboCalc2;
            pB_ImportCarboCalc.SetContextualHelp(contextualHelp);
            pB_ImportCarboCalc.ToolTip = "Update an existing Carbo Life Calc Project using the BIM model";

            PushButton pB_CarboCalcPlus = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalcSettings", "Settings", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcPlus")) as PushButton;

            Uri img_CarboImport = new Uri(MyAssemblyDir + @"\img\ico_CarboLifeSettings32.jpg");
            BitmapImage limg_CarboImport = new BitmapImage(img_CarboImport);
            pB_CarboCalcPlus.LargeImage = limg_CarboImport;
            pB_CarboCalcPlus.Image = limg_CarboImport;
            pB_CarboCalc.SetContextualHelp(contextualHelp);
            pB_CarboCalc.ToolTip = "Advanced export settings";


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }





    }
}
