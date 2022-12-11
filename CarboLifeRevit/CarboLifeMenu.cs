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
            
            /// New Project
            PushButton pB_CarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalc", "New Project", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalc")) as PushButton;
            //LImage
            Uri img_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife32.png");
            BitmapImage limg_CarboCalc = new BitmapImage(img_CarboCalc);
            //SImahe
            Uri imgsmll_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife16.png");
            BitmapImage smllimg_CarboCalc = new BitmapImage(imgsmll_CarboCalc);

            pB_CarboCalc.LargeImage = limg_CarboCalc;
            pB_CarboCalc.Image = smllimg_CarboCalc;
            pB_CarboCalc.SetContextualHelp(contextualHelp);
            pB_CarboCalc.ToolTip = "Create a new Carbo Calc Project using the BIM model";

            /// Update / Open Project
            PushButton pB_ImportCarboCalc = CarboCalcPanel.AddItem(new PushButtonData("Import CarboLife Calc", "Update Project", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcUpdate")) as PushButton;
            //LImage
            Uri img_CarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_UpdateCarboLife32.png");
            BitmapImage limg_CarboCalc2 = new BitmapImage(img_CarboCalc2);
            //SImahe
            Uri imgsmll_CarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_UpdateCarboLife16.png");
            BitmapImage smllimg_CarboCalc2 = new BitmapImage(imgsmll_CarboCalc2);

            pB_ImportCarboCalc.LargeImage = limg_CarboCalc2;
            pB_ImportCarboCalc.Image = smllimg_CarboCalc2;
            pB_ImportCarboCalc.SetContextualHelp(contextualHelp);
            pB_ImportCarboCalc.ToolTip = "Update an existing Carbo Life Calc Project using the BIM model";

            /// Visual Menu
            PushButton pB_ShowCarboCalc = CarboCalcPanel.AddItem(new PushButtonData("Show CarboLife Calc", "Show Project", MyAssemblyPath, "CarboLifeRevit.ShowCarboCalc")) as PushButton;
            //LImage
            Uri pB_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_UpdateCarboLife32.png");
            BitmapImage limg_pB_ShowCarboCalc2 = new BitmapImage(pB_ShowCarboCalc2);
            //SImahe
            Uri imgsmll_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_UpdateCarboLife16.png");
            BitmapImage smllimg_ShowCarboCalc2 = new BitmapImage(imgsmll_ShowCarboCalc2);

            pB_ShowCarboCalc.LargeImage = limg_pB_ShowCarboCalc2;
            pB_ShowCarboCalc.Image = smllimg_ShowCarboCalc2;
            pB_ShowCarboCalc.SetContextualHelp(contextualHelp);
            pB_ShowCarboCalc.ToolTip = "Vizualise your project in Revit";



            PushButton pB_CarboCalcPlus = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalcSettings", "Settings", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcPlus")) as PushButton;
            //LImage
            Uri img_CarboImport = new Uri(MyAssemblyDir + @"\img\ico_CarboLifeSettings32.png");
            BitmapImage limg_CarboImport = new BitmapImage(img_CarboImport);
            //SImahe
            Uri imgsmll_CarboImport = new Uri(MyAssemblyDir + @"\img\ico_CarboLifeSettings16.png");
            BitmapImage smllimg_CarboImport = new BitmapImage(imgsmll_CarboImport);

            pB_CarboCalcPlus.LargeImage = limg_CarboImport;
            pB_CarboCalcPlus.Image = smllimg_CarboImport;
            pB_CarboCalcPlus.SetContextualHelp(contextualHelp);
            pB_CarboCalcPlus.ToolTip = "Advanced export settings";


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }





    }
}
