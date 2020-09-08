// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceParameter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataSource
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Abstract base class from which all columns for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    internal abstract class ReportingDataSourceColumn<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Gets the <see cref="DefinedThingShortNameAttribute"/> decorating the class described by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Describes the current parameter class.
        /// </param>
        /// <returns>
        /// The <see cref="DefinedThingShortNameAttribute"/> decorating the current parameter class.
        /// </returns>
        protected static DefinedThingShortNameAttribute GetParameterAttribute(MemberInfo type)
        {
            var attr = Attribute
                .GetCustomAttributes(type)
                .SingleOrDefault(attribute => attribute is DefinedThingShortNameAttribute);

            return attr as DefinedThingShortNameAttribute;
        }

        /// <summary>
        /// The <see cref="ReportingDataSourceNode{T}"/> associated to this parameter.
        /// </summary>
        protected ReportingDataSourceNode<T> Node;

        /// <summary>
        /// Initializes a reported column based on the corresponding <see cref="ReportingDataSourceNode{T}"/>
        /// associated with the current <see cref="ReportingDataSourceRow"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </param>
        internal abstract void Initialize(ReportingDataSourceNode<T> node);

        /// <summary>
        /// Populates with data the <see cref="DataTable.Columns"/> associated with this object
        /// in the given <paramref name="row"/>.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> to which the <paramref name="row"/> belongs to.
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to be populated.
        /// </param>
        internal abstract void Populate(DataTable table, DataRow row);
    }
}
