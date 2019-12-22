// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderedDomainOfExpertiseListConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="OrderedDomainOfExpertiseListConverter"/> is to convert
    /// a <see cref="List{DomainOfExpertise}"/> to a sorted <see cref="List{DomainOfExpertise}"/> by Name 
    /// </summary>
    public class OrderedDomainOfExpertiseListConverter : IValueConverter
    {
        /// <summary>
        /// The conversion method converts a <see cref="List{T}"/> of <see cref="DomainOfExpertise"/> to a sorted <see cref="List{DomainOfExpertise}"/> by Name.
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IList)value)?.Cast<DomainOfExpertise>().OrderBy(c => c.Name).ToList() ?? new List<DomainOfExpertise>();
        }

        /// <summary>
        /// The conversion method converts a <see cref="List{T}"/> of <see cref="DomainOfExpertise"/> to a sorted <see cref="List{DomainOfExpertise}"/> by Name.
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IList)value)?.Cast<DomainOfExpertise>().OrderBy(c => c.Name).ToList() ?? new List<DomainOfExpertise>();
        }
    }
}