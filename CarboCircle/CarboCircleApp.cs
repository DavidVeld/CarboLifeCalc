using Autodesk.Revit.UI;
using CarboCircle.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
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
                m_CarboCircleWindow = new CarboCircleMain(exEvent, handler, project, VisibleElements);
                FormStatusChecker.isWindowOpen = true;
                m_CarboCircleWindow.Show();
            }
            else
            {
                // A new handler to handle request posting by the dialog
                ColourViewerHandler handler = new ColourViewerHandler(uiapp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_HeatMapCreator = new HeatMapCreator(exEvent, handler, project, VisibleElements);
                FormStatusChecker.isWindowOpen = true;
                m_HeatMapCreator.Show();
            }

        }

    }
}
