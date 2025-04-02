using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarboLifeUI.UI
{
    public class SumValues : IValueConverter
    {
        [Obsolete]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";
            double totalEC = 0;
            double totalPC = 0;

            try
            {
                var cvg = value as CollectionViewGroup;
                var field = parameter as string;
                if (cvg == null || field == null)
                    return null;

                for (int i = 0; i < cvg.ItemCount; i++)
                {
                    // DataRowView dr = cvg.Items[i] as DataRowView;
                    CarboGroup gr = cvg.Items[i] as CarboGroup;

                    if (gr != null)
                    {
                        totalEC += gr.EC;
                        totalPC += gr.PerCent;
                    }
                }
                //Total: {0} tCO₂e

                string totECstr = Math.Round(totalEC, 4).ToString();
                string totPerCstr = Math.Round(totalPC, 2).ToString();

                result = totECstr + " tCO₂e / " + totPerCstr + " % ";

            }
            catch (Exception ex)
            {
                result = "error: " + ex.Message;
            }

            return result;
            
        }
        
        [Obsolete]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
