// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorColumn.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DataCollection
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Abstract base class from which all columns for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    public abstract class DataCollectorColumn<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// Gets the <see cref="DefinedThingShortNameAttribute"/> decorating the property described by <paramref name="propertyType"/>.
        /// </summary>
        /// <param name="propertyType">
        /// Describes the current property.
        /// </param>
        /// <returns>
        /// The <see cref="DefinedThingShortNameAttribute"/> decorating the current parameter class.
        /// </returns>
        protected static DefinedThingShortNameAttribute GetParameterAttribute(PropertyInfo propertyType)
        {
            var attr = Attribute
                .GetCustomAttributes(propertyType)
                .SingleOrDefault(attribute => attribute is DefinedThingShortNameAttribute);

            return attr as DefinedThingShortNameAttribute;
        }

        /// <summary>
        /// The <see cref="DataCollectorNode{T}"/> associated to this parameter.
        /// </summary>
        internal DataCollectorNode<T> Node;

        /// <summary>
        /// Initializes a reported column based on the corresponding <see cref="DataCollectorNode{T}"/>
        /// associated with the current <see cref="DataCollectorRow"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="DataCollectorNode{T}"/>.
        /// </param>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> object for this <see cref="DataCollectorCategory{T}"/>'s usage in a class.
        /// </param>
        internal abstract void Initialize(DataCollectorNode<T> node, PropertyInfo propertyInfo);

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
        public abstract void Populate(DataTable table, DataRow row);
    }
}
