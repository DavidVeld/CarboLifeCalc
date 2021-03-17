using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
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
            UIApplication app = commandData.Application;
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

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
                newlvl.Id = lvl.Id.IntegerValue;
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
