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
        /// <summary>
        /// Owns the one CarboCircle window and decides, per button press, whether to build
        /// one or bring back the one that is already there. See carboCircleWindowGate for
        /// what pressing the button used to do instead.
        /// </summary>
        private readonly carboCircleWindowGate m_CarboCircleWindow = new carboCircleWindowGate();

        private CarboCircleHandler handler;
        private ExternalEvent exEvent;

        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

        public Result OnStartup(UIControlledApplication application)
        {
            //Charts need the native SkiaSharp library, which Windows will not find in an
            //add-in folder on its own. No-op on the NET8 build.
            CarboLifeUI.NativeDependencies.Preload();

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
            //Closed whether or not it is visible. A hidden window is still a live window,
            //and the Close button hides rather than closes - so the old test on Visibility
            //left exactly the window the user thought they had shut behind at shutdown,
            //still subscribed to the external event handler.
            if (m_CarboCircleWindow.Window != null)
            {
                m_CarboCircleWindow.Window.Close();
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

            //Harvested here as well as on every external event, because this is the only
            //moment in API context before the window opens. Without it the settings dialog
            //would offer nothing to pick from until the user had mined a view once - and
            //choosing the parameter is something they would reasonably do first.
            try
            {
                if (uiapp.ActiveUIDocument != null)
                    carboCircleRevitCommands.collectParameterNames(uiapp.ActiveUIDocument.Document);
            }
            catch (Exception)
            {
                //No document open, or a model that will not answer. The dialog copes with
                //an empty list, and nothing else here depends on it.
            }

            //One window per session: built when there is none, brought back when there is.
            //The gate owns that decision and the Closed bookkeeping behind it.
            m_CarboCircleWindow.show(delegate { return new CarboCircleMain(exEvent, handler); });
        }
    }
}
