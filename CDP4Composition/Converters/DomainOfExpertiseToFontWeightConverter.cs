// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseToFontWeightConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Converts a <see cref="DomainOfExpertise"/> item and the current session <see cref="DomainOfExpertise"/> to a
    /// <see cref="FontWeight"/>, returning <see cref="FontWeights.Bold"/> when the item is the current domain. This is
    /// used to visually highlight the user's current <see cref="DomainOfExpertise"/> in owner selection combo-boxes.
    /// </summary>
    public class DomainOfExpertiseToFontWeightConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts the bound <see cref="DomainOfExpertise"/> item and the current <see cref="DomainOfExpertise"/> to a
        /// <see cref="FontWeight"/>.
        /// </summary>
        /// <param name="values">
        /// An array where the first value is the <see cref="DomainOfExpertise"/> item and the second value is the
        /// current session <see cref="DomainOfExpertise"/>.
        /// </param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// <see cref="FontWeights.Bold"/> when the item is the current <see cref="DomainOfExpertise"/>;
        /// otherwise <see cref="FontWeights.Normal"/>.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is DomainOfExpertise item && values[1] is DomainOfExpertise current && item.Iid == current.Iid)
            {
                return FontWeights.Bold;
            }

            return FontWeights.Normal;
        }

        /// <summary>
        /// not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
