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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all parameter columns
    /// for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    /// <typeparam name="TRow">
    /// The type of the associated <see cref="ReportingDataSourceRow"/>.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type that will be used for the parameter value representation.
    /// </typeparam>
    internal abstract class ReportingDataSourceParameter<TRow, TValue> : ReportingDataSourceColumn<TRow>
        where TRow : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// The associated <see cref="ParameterType"/> short name.
        /// </summary>
        internal readonly string ShortName;

        /// <summary>
        /// The associated <see cref="CDP4Common.EngineeringModelData.ParameterBase"/>.
        /// </summary>
        protected ParameterBase ParameterBase { get; private set; }

        /// <summary>
        /// The <see cref="IValueSet"/>s of the associated <see cref="ParameterBase"/>.
        /// </summary>
        protected internal IEnumerable<IValueSet> ValueSets => this.ParameterBase?.ValueSets;

        /// <summary>
        /// The owner <see cref="DomainOfExpertise"/> of the associated <see cref="ParameterBase"/>.
        /// </summary>
        protected DomainOfExpertise Owner => this.ParameterBase?.Owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceParameter{TRow,TValue}"/> class.
        /// </summary>
        protected ReportingDataSourceParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes a reported parameter column based on the corresponding
        /// <see cref="CDP4Common.EngineeringModelData.ParameterBase"/> within the associated
        /// <see cref="ReportingDataSourceNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </param>
        internal override void Initialize(ReportingDataSourceNode<TRow> node)
        {
            this.Node = node;

            this.ParameterBase = this.ParameterBase ??
                                 this.Node.NestedElement.NestedParameter.SingleOrDefault(
                                     x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName)?.AssociatedParameter;

            this.ParameterBase = this.ParameterBase ??
                                 this.Node.ElementUsage?.ParameterOverride.SingleOrDefault(
                                     x => x.Parameter.ParameterType.ShortName == this.ShortName);

            this.ParameterBase = this.ParameterBase ??
                                 this.Node.ElementDefinition.Parameter.SingleOrDefault(
                                     x => x.ParameterType.ShortName == this.ShortName);
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
        internal override void Populate(DataTable table, DataRow row)
        {
            foreach (var valueSet in this.ValueSets)
            {
                var columnName = $"{this.ShortName} {valueSet.ActualState.ShortName}";

                if (!table.Columns.Contains(columnName))
                {
                    table.Columns.Add(columnName, typeof(TValue));
                }

                row[columnName] = this.Parse(valueSet.ActualValue.First());
            }
        }

        /// <summary>
        /// Parses a parameter value as the type that will be used for the row representation.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed. This needs to be specified as a state dependent
        /// <see cref="ReportingDataSourceParameter{TRow,TValue}"/> can have multiple values.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        internal abstract TValue Parse(string value);

        /// <summary>
        /// Gets the parameter of type <see cref="TP"/> on the same level in the
        /// hierarhical tree upon which the data source is based.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired parameter type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="ReportingDataSourceParameter{TRow,TValue}"/> of type <see cref="TP"/>.
        /// </returns>
        public TP GetSibling<TP>() where TP : ReportingDataSourceParameter<TRow, TValue>
        {
            return this.Node.GetColumn<TP>();
        }

        /// <summary>
        /// Gets the parameters of type <see cref="TP"/> on the children levels in the
        /// hierarhical tree upon which the data source is based.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired parameter type.
        /// </typeparam>
        /// <returns>
        /// A list of <see cref="ReportingDataSourceParameter{TRow,TValue}"/>s of type <see cref="TP"/>.
        /// </returns>
        public IEnumerable<TP> GetChildren<TP>() where TP : ReportingDataSourceParameter<TRow, TValue>
        {
            return this.Node.Children.Select(child => child.GetColumn<TP>());
        }
    }
}
