using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
using CarboLifeRevit.Modeless;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CarboLifeRevit
{
    public class CarboLifeApp : Autodesk.Revit.UI.IExternalApplication
    {
        public static CarboLifeApp thisApp = null;
        private HeatMapCreator m_HeatMapCreator;

        static string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

        public Result OnStartup(UIControlledApplication application)
        {
            //Placeholders we will call later
            m_HeatMapCreator = null;
            thisApp = this;

            //Create  A button
            RibbonPanel CarboCalcPanel = application.CreateRibbonPanel("CarboLifeCalc");

            //Info
            string HelpURL = "https://github.com/DavidVeld/CarboLifeCalc/wiki";
            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, HelpURL);

            /// New Project
            PushButton pB_CarboCalc = CarboCalcPanel.AddItem(new PushButtonData("CarboLifeCalc", "Launch", MyAssemblyPath, "CarboLifeRevit.CarboLifeCalc")) as PushButton;
            //LImage
            Uri img_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife2D32.png");
            BitmapImage limg_CarboCalc = new BitmapImage(img_CarboCalc);
            //SImahe
            Uri imgsmll_CarboCalc = new Uri(MyAssemblyDir + @"\img\ico_CarboLife2D16.png");
            BitmapImage smllimg_CarboCalc = new BitmapImage(imgsmll_CarboCalc);

            pB_CarboCalc.LargeImage = limg_CarboCalc;
            pB_CarboCalc.Image = smllimg_CarboCalc;
            pB_CarboCalc.SetContextualHelp(contextualHelp);
            pB_CarboCalc.ToolTip = "Create or update a new Carbo Calc Project using the BIM model";

            /// Update / Open Project
            /*
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
            */

            /// Visual Menu
            PushButton pB_ShowCarboCalc = CarboCalcPanel.AddItem(new PushButtonData("Show CarboLife Calc", "Heatmap", MyAssemblyPath, "CarboLifeRevit.CarboViewerCommand")) as PushButton;
            //LImage
            Uri pB_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_CarboLife32.png");
            BitmapImage limg_pB_ShowCarboCalc2 = new BitmapImage(pB_ShowCarboCalc2);
            //SImahe
            Uri imgsmll_ShowCarboCalc2 = new Uri(MyAssemblyDir + @"\img\ico_CarboLife16.png");
            BitmapImage smllimg_ShowCarboCalc2 = new BitmapImage(imgsmll_ShowCarboCalc2);

            pB_ShowCarboCalc.LargeImage = limg_pB_ShowCarboCalc2;
            pB_ShowCarboCalc.Image = smllimg_ShowCarboCalc2;
            pB_ShowCarboCalc.SetContextualHelp(contextualHelp);
            pB_ShowCarboCalc.ToolTip = "Vizualise your project in Revit";

            FormStatusChecker.isWindowOpen = false;


            /*
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
            */

            try
            {
                //The hanldler for the colour viewerr:
                //ColourViewerHandler handler = new ColourViewerHandler();
                // External use this on in the dialog:
                //ExternalEvent exEvent = ExternalEvent.Create(handler);

            }
            catch (Exception eX)
            {
                TaskDialog td = new TaskDialog("Error in handler setup");
                td.ExpandedContent = eX.GetType().Name + ": " + eX.Message + Environment.NewLine + eX.StackTrace;
                td.Show();

                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {

            if (m_HeatMapCreator != null)
            {
                m_HeatMapCreator.Close();
            }

            return Result.Succeeded;
        }

        public void ShowHeatmap(UIApplication uiapp, CarboProject project, List<int> VisibleElements)
        {


            // If we do not have a dialog yet, create and show it
            if (m_HeatMapCreator == null)
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
        public void CloseHeatmap()
        {
            m_HeatMapCreator = null;
        }

        }
}
