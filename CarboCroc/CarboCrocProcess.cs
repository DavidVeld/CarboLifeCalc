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
        /// <summary>
        /// What the Uncertainty input hands over when nothing was wired into it.
        ///
        /// A factor below zero is not something a user could mean, so it reads as "not supplied"
        /// and the project keeps the figure its own import settings gave it. A sentinel rather
        /// than zero because zero is a real answer: a user who wants no uncertainty allowance
        /// must still be able to ask for one.
        /// </summary>
        internal const double UncertaintyNotSupplied = -1;

        /// <summary>
        /// Puts the uncertainty factor on the project, but only one the user asked for.
        ///
        /// A new CarboProject is already built with the factor out of the user's own import
        /// settings, which is the same 10% the Revit import applies. Overwriting that with an
        /// unwired input's 0 left Grasshopper reading about 9% under the main application on the
        /// same model, with nothing on screen to say why.
        /// </summary>
        private static void ApplyUncertainty(CarboProject project, double uncertaintyFacr)
        {
            if (uncertaintyFacr >= 0)
                project.UncertFact = uncertaintyFacr;
        }

        /// <summary>
        /// Builds the reinforcement and connection allowance groups that have been asked for.
        ///
        /// Both read out of the project's import settings, which is where the allowances
        /// component writes and where the settings a project is constructed with already sit, so
        /// a definition with nothing wired into that input still gets the allowances the main
        /// application would build for the same model.
        /// </summary>
        private static void BuildAllowances(CarboProject project)
        {
            CarboGroupSettings settings = project.RevitImportSettings;

            if (settings == null)
                return;

            //CreateReinforcementGroup puts a modal message box up when the reinforcement material
            //has no density, which in Grasshopper is a dialog nobody asked for in the middle of a
            //solution. The same test is made here so the call is simply not made; the allowances
            //component reports it, where the name is on screen next to the input that set it.
            if (string.IsNullOrWhiteSpace(settings.RCMaterialName) == false
                && string.IsNullOrWhiteSpace(settings.RCMaterialCategory) == false
                && ResolvedDensity(project, settings.RCMaterialName) > 0)
            {
                project.CreateReinforcementGroup();
            }

            //Reads mapSteelConnections and mapTimberConnections itself, and getConnectionGroups
            //gives up on an empty material name or a percentage of zero, so this is safe to call
            //whatever the settings say.
            project.CreateConnectionGroups();
        }

        /// <summary>
        /// The density the reinforcement allowance would actually be converted with, by the same
        /// route CreateReinforcementGroup takes: the exact row first, the matcher as a fallback.
        /// </summary>
        private static double ResolvedDensity(CarboProject project, string materialName)
        {
            try
            {
                CarboMaterial exact = project.CarboDatabase.GetExcactMatch(materialName);
                CarboMaterial material = exact != null
                    ? exact
                    : project.CarboDatabase.getClosestMatch(materialName);

                return material != null ? material.Density : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        internal static CarboProject ProcessData(List<CarboElement> listOfElements, List<bool> switches, double uncertaintyFacr, string templatePath, double gia, CarboCrocAllowanceSet allowances = null)
        {
            CarboProject newProject = null;

            if (templatePath != "" && File.Exists(templatePath))
                newProject = new CarboProject(templatePath);
            else
                newProject = new CarboProject();

            ApplyUncertainty(newProject, uncertaintyFacr);

            //Before the grouping, because the grouping key takes reinforcement rates into account
            //and CreateReinforcementGroup reads its material out of these same settings.
            if (allowances != null)
                allowances.ApplyTo(newProject.RevitImportSettings);

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

            //Only a GIA the user actually gave. An unwired input arrives as 0, and assigning
            //that zeroed A5Global - which CalculateProject works out as AreaNew x A5AreaFactor,
            //20 kgCO2e/m2 - and turned every per m2 figure into a divide by zero, so the
            //headline kgCO2e/m2 and the IStructE rating came out as NaN. The constructor's 1 m2
            //is not the right area either, but it is finite and the solver says so out loud.
            if (gia > 0)
            {
                newProject.Area = gia;
                newProject.AreaNew = gia;
            }

            //The area is settled now, so the A0 allowance can be seeded off it rather than off
            //the 1 m2 the constructor had to assume. This is what CarboLifeRevitImport does once
            //it knows the real GIA; without it A0 stayed on its one tonne floor whatever the size
            //of the project, where the main application had it at roughly 1 kgCO2e/m2.
            newProject.SeedA0FromArea();

            //The saved mapping file is deliberately not applied here.
            //
            //A material name in Revit is whatever the model happens to call it, so the main
            //application has to guess and then remember the guess. In Grasshopper the name is
            //typed by the person building the definition, and the matcher already answers an
            //exact name at full confidence before any scoring runs - see RunCascade tier 1 - so
            //a name taken from the material list or the selector component needs no mapping at
            //all. Running the map over it only reintroduced a rewrite: mapAllMaterials keys on
            //the imported name and swaps the material whatever the match was worth, which is how
            //the same model came out on a different steel here than in the main application.
            //
            //What replaces it is being told. Anything the matcher was not sure about is reported
            //by the solver instead of quietly corrected, and the answer is then whatever the
            //definition asked for.
            BuildAllowances(newProject);

            //Calculate the values
            newProject.CalculateProject();

            return newProject;
        }

        internal static CarboProject ProcessData(List<CarboGroup> listOfGroups, List<bool> switches, double uncertaintyFacr, string templatePath)
        {
            CarboProject newProject = new CarboProject();

            if (templatePath != "" && File.Exists(templatePath))
                newProject = new CarboProject(templatePath);
            else
                newProject = new CarboProject();

            ApplyUncertainty(newProject, uncertaintyFacr);

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
