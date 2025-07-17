using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCroc
{
    internal static class CarboCrocProcess
    {
        internal static CarboProject ProcessData(List<CarboElement> listOfElements, List<bool> switches, double uncertaintyFacr, string templatePath)
        {
            CarboProject newProject = null;

            if (templatePath != "" && File.Exists(templatePath))
                newProject = new CarboProject(templatePath);
            else
                newProject = new CarboProject();

            newProject.UncertFact = uncertaintyFacr;

            foreach (CarboElement ce in listOfElements)
                {
                    newProject.AddElement(ce);
                }

            if (switches.Count == 8)
            {
                bool a13 = switches[0];
                bool a4 = switches[1];
                bool a5 = switches[2];
                bool b = switches[3];
                bool c = switches[4];
                bool d = switches[5];
                bool s = switches[6];
                bool extra = switches[7];

                newProject.calculateA13 = a13;
                newProject.calculateA4 = a4;
                newProject.calculateA5 = a5;
                newProject.calculateB = b;
                newProject.calculateB67 = b;
                newProject.calculateC = c;
                newProject.calculateD = d;
                newProject.calculateSeq = s;
                newProject.calculateAdd = extra;
            }

            //run once;
            newProject.CreateGroups();

            //MapElements if required
            try
            {
                if (newProject.RevitImportSettings.UseImportedMap == true)
                {
                    CarboMapFile defaultMappingFile = CarboMapFile.LoadFromXml();
                    if (defaultMappingFile != null)
                    {
                        newProject.carboMaterialMap = defaultMappingFile.mappingTable;
                        newProject.mapAllMaterials();
                        newProject.CalculateProject();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //Calculate the values
            newProject.CalculateProject();

            return newProject;
        }

        internal static CarboProject ProcessData(List<CarboGroup> listOfGroups, List<bool> switches)
        {
            CarboProject newProject = new CarboProject();

            if (switches.Count == 8)
            {
                bool a13 = switches[0];
                bool a4 = switches[1];
                bool a5 = switches[2];
                bool b = switches[3];
                bool c = switches[4];
                bool d = switches[5];
                bool s = switches[6];
                bool extra = switches[7];

                newProject.calculateA13 = a13;
                newProject.calculateA4 = a4;
                newProject.calculateA5 = a5;
                newProject.calculateB = b;
                newProject.calculateB67 = b;
                newProject.calculateC = c;
                newProject.calculateD = d;
                newProject.calculateSeq = s;
                newProject.calculateAdd = extra;
            }

            foreach (CarboGroup cg in listOfGroups)
            {
                newProject.AddGroup(cg);

                foreach(CarboElement ce in cg.AllElements)
                {
                    newProject.AddElement(ce);
                }
            }

            //Calculate the values
            newProject.CalculateProject();

            return newProject;
        }
    }
}
