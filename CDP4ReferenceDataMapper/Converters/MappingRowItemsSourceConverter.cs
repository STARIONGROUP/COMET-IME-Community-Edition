// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingRowItemsSourceConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using CDP4Common.SiteDirectoryData;

    using CDP4ReferenceDataMapper.Data;
    using CDP4ReferenceDataMapper.Managers;
    using CDP4ReferenceDataMapper.Views;

    /// <summary>
    /// The <see cref="IMultiValueConverter"/> that gets an ItemsSource for a dropdown editor in the <see cref="StateToParameterTypeMapperBrowser"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MappingRowItemsSourceConverter : IMultiValueConverter
    {
        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> of type <see cref="ParameterType"/> that holds a specific dropdown control's ItemsSource.
        /// </summary>
        /// <param name="values">The MultiBinding values coming from the Xaml file.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dataViewRow = values[0] as DataRowView;
            var dataSourceManager = values[1] as DataSourceManager;

            IEnumerable<SourceParameter> newSourceParameters = new List<SourceParameter>();

            if (dataViewRow != null && dataSourceManager != null)
            {
                newSourceParameters = 
                    dataSourceManager.GetSourceParameterTypesForDataRow(dataViewRow.Row)?
                        .Select(x => new SourceParameter(x));
            }

            return newSourceParameters;
        }

        /// <summary>
        /// Not supported
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
