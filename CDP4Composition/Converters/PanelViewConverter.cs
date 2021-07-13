using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CDP4Composition.Converters
{
    public class PanelViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return null;
            }

            var fullyQualifiedName = value.GetType().FullName.Replace(".ViewModels.", ".Views.");
            var viewName = System.Text.RegularExpressions.Regex.Replace(fullyQualifiedName, "ViewModel$", "");

            var assembly = value.GetType().Assembly;

            var view = (FrameworkElement)assembly.CreateInstance(viewName, false, BindingFlags.Default, null, new object[] { true }, null, null );
            view.DataContext = value;
            return view;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
