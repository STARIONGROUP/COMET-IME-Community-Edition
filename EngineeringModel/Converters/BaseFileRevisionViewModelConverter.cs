// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseFileRevisionViewModelConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using CDP4CommonView;

    using FileDialogViewModel = CDP4EngineeringModel.ViewModels.FileDialogViewModel;

    /// <summary>
    /// The converter implements the default way to retrieve specific data for a cell to be used.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be returned by the GetValue method.
    /// In other words, the type of the property it's bound to.
    /// </typeparam>
    public abstract class BaseFileRevisionViewModelConverter<T> : IValueConverter
    {
        /// <summary>
        /// The conversion method returns the color to use for cells having deprecated parents.
        /// </summary>
        /// <param name="value">
        /// The incoming value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The <see cref="object"/> containing the same objects as the input collection.
        /// </returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileRevisionRowViewModel viewModel)
            {
                if (viewModel.ContainerViewModel is FileDialogViewModel fileDialogViewModel)
                {
                    return this.GetValue(fileDialogViewModel.CurrentFileRevision == viewModel.Thing);
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns a specific value 
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned</typeparam>
        /// <param name="isFrozen">The <see cref="bool"/> that helps to return the right value</param>
        /// <returns>Value of type T</returns>
        protected abstract T GetValue(bool isFrozen);

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The result 
        /// </returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
