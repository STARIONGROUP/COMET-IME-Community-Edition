// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorParameterColumn.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all parameter columns for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    public abstract class DataCollectorParameter<TRow, TValue> : DataCollectorColumn<TRow>, IDataCollectorParameter where TRow : DataCollectorRow, new()
    {
        /// <summary>
        /// The associated <see cref="CDP4Common.EngineeringModelData.ParameterBase"/>.
        /// </summary>
        protected ParameterBase ParameterBase { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="ParameterType"/> short name.
        /// </summary>
        protected string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="ParameterType"/> field name in the result Data Object.
        /// </summary>
        internal string FieldName { get; set; }

        /// <summary>
        /// The value of the associated <see cref="ParameterOrOverrideBase"/>.
        /// The <see cref="IValueSet"/>s of the associated <see cref="ParameterBase"/>.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (this.ValueSets?.Any() ?? false)
                { 
                    return this.GetValueSetValue(this.ValueSets.FirstOrDefault());
                }

                return default;
            }
        }

        /// <summary>
        /// Get the correct value of a <see cref="IValueSet"/>.
        /// </summary>
        /// <param name="valueSet">The <see cref="IValueSet"/></param>
        /// <returns>The correct value of the <see cref="IValueSet"/></returns>
        protected TValue GetValueSetValue(IValueSet valueSet)
        {
            if (valueSet is ParameterSubscriptionValueSet)
            {
                return this.Parse(valueSet.Computed.FirstOrDefault()) ?? default;
            }

            if (valueSet is ParameterValueSetBase valueSetBase)
            {
                return this.Parse(valueSetBase.Published.FirstOrDefault()) ?? default;
            }

            return default;
        }

        /// <summary>
        /// The ValueSets of the associated object.
        /// The <see cref="IEnumerable{IValueSet}"/>s of the associated object/>.
        /// </summary>
        public IEnumerable<IValueSet> ValueSets { get; set; }

        /// <summary>
        /// Gets the owner <see cref="DomainOfExpertise"/> of the associated <see cref="ParameterBase"/>.
        /// </summary>
        public DomainOfExpertise Owner { get; set; }

        /// <summary>
        /// Initializes a reported parameter column based on the corresponding
        /// <see cref="CDP4Common.EngineeringModelData.ParameterBase"/> within the associated
        /// <see cref="DataCollectorNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="DataCollectorNode{TRow}"/>.
        /// </param>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> object for this <see cref="DataCollectorCategory{TRow}"/>'s usage in a class.
        /// </param>
        internal override void Initialize(DataCollectorNode<TRow> node, PropertyInfo propertyInfo)
        {
            var attribute = GetParameterAttribute(propertyInfo);

            this.ShortName = attribute?.ShortName;
            this.FieldName = attribute?.FieldName;
            this.Node = node;

            var firstNestedParameter = this.Node.NestedElement.NestedParameter.FirstOrDefault(
                x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName);

            this.ParameterBase = firstNestedParameter?.AssociatedParameter;

            this.Owner = (firstNestedParameter?.ValueSet as IOwnedThing)?.Owner ?? firstNestedParameter?.Owner;

            var nestedParameterData =
                this.Node.NestedElement.NestedParameter
                .Where(x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName)
                .Select(x => x.ValueSet)
                .ToList();

            if (nestedParameterData.Any())
            {
                this.ValueSets = nestedParameterData;
            }
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
            if (this.HasValueSets)
            {
                foreach (var valueSet in this.ValueSets)
                {
                    var columnName = $"{this.FieldName}{valueSet.ActualState?.ShortName ?? ""}";

                    if (!table.Columns.Contains(columnName))
                    {
                        table.Columns.Add(columnName, typeof(TValue));
                    }

                    row[columnName] = this.Value;
                }
            }
        }

        /// <summary>
        /// Parses a parameter value as the type that will be used for the row representation.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed. This needs to be specified as a state dependent
        /// <see cref="DataCollectorParameter{TRow,TValue}"/> can have multiple values.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        public abstract TValue Parse(string value);

        /// <summary>
        /// Gets a flag that indicates whether this instance has <see cref="IValueSet"/>s.
        /// </summary>
        public bool HasValueSets => this.ValueSets?.Any() ?? false;
    }
}
