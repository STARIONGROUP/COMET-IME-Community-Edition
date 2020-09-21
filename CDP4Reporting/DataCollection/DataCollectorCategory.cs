// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorCategory.cs" company="RHEA System S.A.">
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
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all category columns for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    public class DataCollectorCategory<T> : DataCollectorColumn<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// Gets or sets the associated <see cref="Category"/> short name.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Gets or sets the desired field name.
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// Flag indicating whether the associated <see cref="Category"/> is present.
        /// </summary>
        public bool Value { get; private set; }

        /// <summary>
        /// Initializes a reported category column based on the corresponding <see cref="ElementBase"/>
        /// within the associated <see cref="DataCollectorNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="DataCollectorNode{T}"/>.
        /// </param>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> object for this <see cref="DataCollectorCategory{T}"/>'s usage in a class.
        /// </param>
        internal override void Initialize(DataCollectorNode<T> node, PropertyInfo propertyInfo)
        {
            var definedShortNameAttribute = GetParameterAttribute(propertyInfo);
            this.FieldName = definedShortNameAttribute?.FieldName;
            this.ShortName = definedShortNameAttribute?.ShortName;
            this.Node = node;
            this.Value = this.IsMemberOfCategory();
        }

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
        public override void Populate(DataTable table, DataRow row)
        {
            if (!table.Columns.Contains(this.ShortName))
            {
                table.Columns.Add(this.FieldName, typeof(bool));
            }

            row[this.FieldName] = this.Value;
        }

        /// <summary>
        /// Checks if <see cref="DataCollectorNode{T}.CategorizableThing"/> is member of a specific <see cref="Category"/>
        /// that is found using <see cref="DataCollectorCategory{T}.ShortName"/>.
        /// </summary>
        /// <returns>
        /// True if <see cref="Category.ShortName"/> was found, otherwise false.
        /// </returns>
        private bool IsMemberOfCategory()
        {
            var categorizableThing = this.Node.CategorizableThing as Thing;

            var categories =
                categorizableThing?.Cache
                    .Where(x => x.Value.Value.ClassKind == ClassKind.Category)
                    .Select(x => x.Value.Value)
                    .Cast<Category>()
                    .Where(x => x.ShortName == this.ShortName)
                    .ToList();

            return categories?.Any(x => this.Node.NestedElement.IsMemberOfCategory(x)) ?? false;
        }
    }
}
