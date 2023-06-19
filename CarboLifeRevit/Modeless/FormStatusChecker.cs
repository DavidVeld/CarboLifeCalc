using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeRevit.Modeless
{
    /// <summary>
    /// This stores the status of a modeless form, should be written as a singleton
    /// </summary>
    public static class FormStatusChecker
    {
        public static bool isWindowOpen;
    }
}
