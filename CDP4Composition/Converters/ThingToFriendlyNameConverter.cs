// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingToFriendlyNameConverter.cs" company="Starion Group S.A.">
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
    using System.Windows.Data;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Extensions;

    /// <summary>
    /// The purpose of the <see cref="ThingToFriendlyNameConverter"/> is to convert a <see cref="Thing"/> to a
    /// human-readable display string. It exists because some <see cref="Thing"/>s do not implement
    /// <see cref="Thing.UserFriendlyName"/> / <see cref="Thing.UserFriendlyShortName"/> and would otherwise render the
    /// SDK fallback text (e.g. "User-friendly short-name not implemented."). A <see cref="ParametricConstraint"/> has no
    /// name or short-name, so its tree of <see cref="BooleanExpression"/>s is shown instead.
    /// </summary>
    public class ThingToFriendlyNameConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Thing"/> to a human-readable display string.
        /// </summary>
        /// <param name="value">The <see cref="Thing"/> to convert.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">
        /// When equal to "Name" (case-insensitive) the <see cref="Thing.UserFriendlyName"/> is used for non
        /// <see cref="ParametricConstraint"/> things; otherwise the <see cref="Thing.UserFriendlyShortName"/> is used.
        /// </param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="string"/> that represents the <see cref="Thing"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ParametricConstraint parametricConstraint)
            {
                return parametricConstraint.GetDisplayName();
            }

            if (value is Thing thing)
            {
                return string.Equals(parameter as string, "Name", StringComparison.OrdinalIgnoreCase)
                    ? thing.UserFriendlyName
                    : thing.UserFriendlyShortName;
            }

            return string.Empty;
        }

        /// <summary>
        /// not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
