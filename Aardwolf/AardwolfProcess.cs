﻿using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AardWolf
{
    internal static class AardwolfProcess
    {
        internal static CarboProject ProcessData(List<CarboElement> listOfElements)
        {
            CarboProject newProject = new CarboProject();

            foreach (CarboElement ce in listOfElements)
            {
                newProject.AddElement(ce);
            }

            //run once;
            newProject.CreateGroups();
            //Calculate the values
            newProject.CalculateProject();

            return newProject;
        }
    }
}