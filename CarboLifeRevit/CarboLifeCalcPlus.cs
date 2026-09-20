using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
using CarboLifeRevitCompat;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeRevit
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    class CarboLifeCalcPlus : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SkiaSharp.Views.WPF.dll"));
            }
            catch
            {

            }

            UIApplication app = commandData.Application;
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            //This command does not use the settings dialog that checks them, but it does end in
            //the main window, where the reinforcement mapper can still be opened. Harvesting here
            //keeps the names it checks against belonging to the model that is actually open,
            //rather than to whichever one was imported earlier in this Revit session.
            CarboModelNameHarvest.Collect(app);

            string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);

            CarboProject myProject = new CarboProject();

            ICollection<ElementId> selectionList = uidoc.Selection.GetElementIds();


            //Get levels
            List<CarboLevel> levellist = new List<CarboLevel>();
            List<Level> levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
            foreach (Level lvl in levels)
            {
                CarboLevel newlvl = new CarboLevel();
                //Narrowed deliberately: CarboLevel.Id is an int and a level id has never been large
                //enough to overflow one. ElementId.IntegerValue itself no longer exists in Revit
                //2026, so the value has to come through the compat helper.
                newlvl.Id = (int)lvl.Id.LongValue();
                newlvl.Name = lvl.Name;
                newlvl.Level = (lvl.Elevation * 304.8);

                levellist.Add(newlvl);
            }

            myProject.carboLevelList = levellist;

            //Get Settings
            ImportSettingsWindow importGroupWindow = new ImportSettingsWindow(levellist);
            importGroupWindow.ShowDialog();
            CarboRevitImportSettings importsettings = importGroupWindow.importSettings;

            if (importGroupWindow.dialogOk == System.Windows.MessageBoxResult.Yes)
            {
                //Import the elements
            }
            else
            {
                return Result.Succeeded;
            }


            //Launch normal Command
            CarboLifeRevitImport.ImportElements(app, importsettings, "");

            return Result.Succeeded;
        }
    }
}
