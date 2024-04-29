// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelViewConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Data;

    /// <summary>
    /// Converter that resolves a panel view model to its view by name convention. 
    /// The view is assumed to be in the same assembly as the view model.
    /// </summary>
    public class PanelViewConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="IPanelViewModel"/> to an <see cref="IPanelView"/>
        /// </summary>
        /// <param name="value">The <see cref="IPanelViewModel"/> to be converted</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The <see cref="IPanelView"/></returns>
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
            var view = (IPanelView)assembly.CreateInstance(viewName, false, BindingFlags.Default, null, new object[] { true }, null, null );
            view.DataContext = value;
            
            return view;
        }

        /// <summary>
        /// Not implemented and should not be called.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
