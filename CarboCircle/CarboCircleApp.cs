using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboCircle;
using CarboCircle.UI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CarboCircle
{
    public class CarboCircleApp : Autodesk.Revit.UI.IExternalApplication
    {
        public static CarboCircleApp thisApp = null;
        private CarboCircleMain m_CarboCircleWindow;

        private CarboCircleHandler handler;
        private ExternalEvent exEvent;

        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

        public Result OnStartup(UIControlledApplication application)
        {
            //Placeholders we will call later
            m_CarboCircleWindow = null;
            thisApp = this;

            //Check if user wants the circle app
            CarboSettings carboSettings = new CarboSettings();
            carboSettings = new CarboSettings().Load();

            if (carboSettings.launchCircle == false)
            {
                return Result.Succeeded;
            }

            //Create  A button
            RibbonPanel CarboCalcPanel = application.CreateRibbonPanel("CarboLifeCircle");

            //Info
            string HelpURL = "https://github.com/DavidVeld/CarboLifeCalc/wiki";
            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, HelpURL);

            /// Visual Menu
            PushButton pB_ShowCarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCircle", "CarboLifeCircle", MyAssemblyPath, "CarboCircle.CarboCircleCommand")) as PushButton;
            //LImage
            Uri pB_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_CarboCircle2D32.png");
            BitmapImage limg_pB_ShowCarboCalc2 = new BitmapImage(pB_ShowCarboCalc2);
            //SImahe
            Uri imgsmll_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_CarboCircle16.png");
            BitmapImage smllimg_ShowCarboCalc2 = new BitmapImage(imgsmll_ShowCarboCalc2);

            pB_ShowCarboCalc.LargeImage = limg_pB_ShowCarboCalc2;
            pB_ShowCarboCalc.Image = smllimg_ShowCarboCalc2;
            pB_ShowCarboCalc.SetContextualHelp(contextualHelp);
            pB_ShowCarboCalc.ToolTip = "Reuse beams and columns";

            FormStatusChecker.isWindowOpen = false;

            return Result.Succeeded;

        }

        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_CarboCircleWindow != null && m_CarboCircleWindow.Visibility == System.Windows.Visibility.Visible)
            {
                m_CarboCircleWindow.Close();
                FormStatusChecker.isWindowOpen = false;
            }

            return Result.Succeeded;
        }

        public void ShowCarboCircle(UIApplication uiapp)
        {
            if (handler == null || exEvent == null)
            {
                handler = new CarboCircleHandler(uiapp);
                exEvent = ExternalEvent.Create(handler);
            }

            if (m_CarboCircleWindow == null || !FormStatusChecker.isWindowOpen)
            {
                m_CarboCircleWindow = new CarboCircleMain(exEvent, handler);
                FormStatusChecker.isWindowOpen = true;
                m_CarboCircleWindow.Show();
            }
            else
            {
                m_CarboCircleWindow.Activate();
                m_CarboCircleWindow.Show(); // ensure it's visible again
            }
        }
    }
}
