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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all parameter columns for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    public abstract class DataCollectorParameter<T> : DataCollectorColumn<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// The associated <see cref="CDP4Common.EngineeringModelData.ParameterBase"/>.
        /// </summary>
        protected ParameterBase ParameterBase { get; private set; }

        /// <summary>
        /// The associated <see cref="ParameterType"/> short name.
        /// </summary>
        internal readonly string ShortName;

        /// <summary>
        /// The value of the associated <see cref="ParameterOrOverrideBase"/>.
        /// </summary>
        protected string Value { get; private set; }

        /// <summary>
        /// The owner <see cref="DomainOfExpertise"/> of the associated <see cref="ParameterBase"/>.
        /// </summary>
        protected DomainOfExpertise Owner => this.ParameterBase?.Owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorParameter{T}"/> class.
        /// </summary>
        protected DataCollectorParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes a reported parameter column based on the corresponding
        /// <see cref="CDP4Common.EngineeringModelData.ParameterBase"/> within the associated
        /// <see cref="DataCollectorNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="DataCollectorNode{T}"/>.
        /// </param>
        internal override void Initialize(DataCollectorNode<T> node)
        {
            this.Node = node;

            this.ParameterBase ??= this.Node.NestedElement.NestedParameter.SingleOrDefault(
                x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName)?.AssociatedParameter;

            this.ParameterBase ??= this.Node.ElementUsage?.ParameterOverride.SingleOrDefault(
                x => x.Parameter.ParameterType.ShortName == this.ShortName);

            this.ParameterBase ??= this.Node.ElementDefinition.Parameter.SingleOrDefault(
                x => x.ParameterType.ShortName == this.ShortName);

            this.Value = this.ParameterBase?.ValueSets.First().ActualValue.First();
        }

        /// <summary>
        /// Gets the parameters of type <see cref="TP"/> on the children levels in the
        /// hierarhical tree upon which the data object is based.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired parameter type.
        /// </typeparam>
        /// <returns>
        /// A list of <see cref="DataCollectorParameter{T}"/>s of type <see cref="TP"/>.
        /// </returns>
        public IEnumerable<TP> GetChildren<TP>() where TP : DataCollectorParameter<T>
        {
            return this.Node.Children.Select(child => child.GetColumn<TP>());
        }
    }
}
