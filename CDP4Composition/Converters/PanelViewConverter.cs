using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace CDP4Composition.Converters
{
    /// <summary>
    /// Converter that resolves a panel view model to its view by name convention. 
    /// The view is assumed to be in the same assembly as the view model.
    /// </summary>
    public class PanelViewConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            var fullyQualifiedName = value.GetType().FullName.Replace(".ViewModels.", ".Views.");
            var viewName = Regex.Replace(fullyQualifiedName, "ViewModel$", string.Empty);

            //View should be in the same assembly as the view model
            var assembly = value.GetType().Assembly;

            //Instantiate the view from the view model assembly by name convention
            var view = (FrameworkElement)assembly.CreateInstance(viewName, false, BindingFlags.Default, null, new object[] { true }, null, null );
            view.DataContext = value;
            
            return view;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
