using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DBGenerator_FlightManagment_Part2
{
    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double howMuchCreated = (double)value*100;
            if (ViewModel.TotalOfInstances == 0)
                return "0";
            int currentPercent = (int)howMuchCreated / ViewModel.TotalOfInstances;
            return currentPercent.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
