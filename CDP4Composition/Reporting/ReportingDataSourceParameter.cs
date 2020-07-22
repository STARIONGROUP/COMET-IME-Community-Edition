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

namespace CDP4Composition.Reporting
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all parameter columns for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    internal abstract class ReportingDataSourceParameter<T> : ReportingDataSourceColumn<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// The associated <see cref="ParameterOrOverrideBase"/>.
        /// </summary>
        protected ParameterOrOverrideBase ParameterOrOverrideBase { get; private set; }

        /// <summary>
        /// The associated <see cref="ParameterType"/> short name.
        /// </summary>
        internal readonly string ShortName;

        /// <summary>
        /// The value of the associated <see cref="ParameterOrOverrideBase"/>.
        /// </summary>
        protected string Value { get; private set; }

        /// <summary>
        /// The owner <see cref="DomainOfExpertise"/> of the associated <see cref="ParameterOrOverrideBase"/>.
        /// </summary>
        protected DomainOfExpertise Owner => this.ParameterOrOverrideBase.Owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceParameter{T}"/> class.
        /// </summary>
        protected ReportingDataSourceParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes a reported parameter column based on the corresponding <see cref="ParameterOrOverrideBase"/>
        /// within the associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </param>
        internal override void Initialize(ReportingDataSourceNode<T> node)
        {
            this.Node = node;

            var parameter = this.Node.ElementDefinition.Parameter
                .SingleOrDefault(x => x.ParameterType.ShortName == this.ShortName);

            if (parameter != null)
            {
                this.ParameterOrOverrideBase = parameter;
                this.Value = parameter.ValueSet.First().ActualValue.First();
            }

            var parameterOverride = this.Node.ElementUsage?.ParameterOverride
                .SingleOrDefault(x => x.Parameter.ParameterType.ShortName == this.ShortName);

            if (parameterOverride != null)
            {
                this.ParameterOrOverrideBase = parameterOverride;
                this.Value = parameterOverride.ValueSet.First().ActualValue.First();
            }
        }

        /// <summary>
        /// Gets the parameter of type <see cref="TP"/> on the same level in the
        /// hierarhical tree upon which the data source is based.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired parameter type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="ReportingDataSourceParameter{T}"/> of type <see cref="TP"/>.
        /// </returns>
        public TP GetSibling<TP>() where TP : ReportingDataSourceParameter<T>
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
        /// A list of <see cref="ReportingDataSourceParameter{T}"/>s of type <see cref="TP"/>.
        /// </returns>
        public IEnumerable<TP> GetChildren<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.Node.Children.Select(child => child.GetColumn<TP>());
        }
    }
}
