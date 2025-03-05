using Autodesk.Revit.UI;
using CarboCircle;
using CarboCircle.UI;
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

        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

        public Result OnStartup(UIControlledApplication application)
        {
            //Placeholders we will call later
            m_CarboCircleWindow = null;
            thisApp = this;

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
            if (m_CarboCircleWindow != null)
            {
                m_CarboCircleWindow.Close();
            }

            return Result.Succeeded;
        }

        public void ShowCarboCircle(UIApplication uiapp)
        {


            // If we do not have a dialog yet, create and show it
            if (m_CarboCircleWindow == null)
            {
                // A new handler to handle request posting by the dialog
                CarboCircleHandler handler = new CarboCircleHandler(uiapp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                // m_HeatMapCreator = new HeatMapCreator(exEvent, handler, project, VisibleElements);

                m_CarboCircleWindow = new CarboCircleMain(exEvent,handler);
                FormStatusChecker.isWindowOpen = true;
                m_CarboCircleWindow.Show();
            }
            else
            {
                // A new handler to handle request posting by the dialog
                CarboCircleHandler handler = new CarboCircleHandler(uiapp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_CarboCircleWindow = new CarboCircleMain();
                FormStatusChecker.isWindowOpen = true;
                m_CarboCircleWindow.Show();
            }

        }

    }
}
