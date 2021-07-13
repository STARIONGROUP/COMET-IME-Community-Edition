// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassKindConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using CDP4Common.CommonData;

    using CDP4Composition.Converters;

    /// <summary>
    /// The purpose of the <see cref="ClassKindConverter"/> is to convert the ClassKind to a 
    /// string including <![CDATA[<< and >> ]]> characters to make them appear like stereotypes in UML
    /// </summary>
    public class ClassKindConverter : IValueConverter
    {
        /// <summary>
        /// Converts the <see cref="ClassKind"/> into a split string and preprends and appends <![CDATA[<< >>]]>
        /// </summary>
        /// <param name="value">An instance of an object which needs to be converted.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A split string preprended and appended with <![CDATA[<< >>]]>
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new CamelCaseToSpaceConverter();
            return $"<<{converter.Convert(value, targetType, parameter, culture)}>>";
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
