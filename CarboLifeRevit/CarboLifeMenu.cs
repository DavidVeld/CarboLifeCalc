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
            RibbonPanel CarboCalcPanel = application.CreateRibbonPanel("CarboLifeCalcc");

            //Add button


            //Create  A button
            PushButton pB_CarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalc", "CarboLifeCalc", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalc")) as PushButton;

            Uri img_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife32.jpg");
            BitmapImage limg_CarboCalc = new BitmapImage(img_CarboCalc);
            pB_CarboCalc.LargeImage = limg_CarboCalc;
            pB_CarboCalc.Image = limg_CarboCalc;

            PushButton pB_CarboCalcPlus = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalcPlus", "CarboLifeCalcPlus", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalcPlus")) as PushButton;

            Uri img_CarboImport = new Uri(MyAssemblyDir + @"\img\ico_CarboLifeSettings32.jpg");
            BitmapImage limg_CarboImport = new BitmapImage(img_CarboImport);
            pB_CarboCalcPlus.LargeImage = limg_CarboImport;
            pB_CarboCalcPlus.Image = limg_CarboImport;


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {


            return Result.Succeeded;
        }

    }
}
