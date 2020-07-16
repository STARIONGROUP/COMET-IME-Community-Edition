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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// Abstract base class from which all parameters for a <see cref="ReportingDataSourceRowRepresentation"/> need to derive.
    /// </summary>
    public abstract class ReportingDataSourceParameter<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        /// <summary>
        /// Gets the <see cref="ParameterTypeShortNameAttribute"/> decorating the class described by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Describes the current parameter class.
        /// </param>
        /// <returns>
        /// The <see cref="ParameterTypeShortNameAttribute"/> decorating the current parameter class.
        /// </returns>
        private static ParameterTypeShortNameAttribute GetParameterAttribute(MemberInfo type)
        {
            var attr = Attribute
                .GetCustomAttributes(type)
                .SingleOrDefault(attribute => attribute is ParameterTypeShortNameAttribute);

            return attr as ParameterTypeShortNameAttribute;
        }

        /// <summary>
        /// The <see cref="ReportingDataSourceRow{T}"/> associated to this parameter.
        /// </summary>
        internal ReportingDataSourceRow<T> Row;

        /// <summary>
        /// The associated <see cref="ParameterType"/> short name.
        /// </summary>
        internal readonly string ShortName;

        /// <summary>
        /// The value of the associated <see cref="ParameterOrOverrideBase"/>.
        /// </summary>
        protected string Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceParameter{T}"/> class.
        /// </summary>
        protected ReportingDataSourceParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes the <see cref="Value"/> based on a <see cref="Parameter"/>.
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="ParameterValueSet"/>s of the <see cref="Parameter"/>.
        /// </param>
        internal void Initialize(ContainerList<ParameterValueSet> valueSet)
        {
            this.Value = valueSet.First().ActualValue.First();
        }

        /// <summary>
        /// Initializes the <see cref="Value"/> based on a <see cref="ParameterOverride"/>.
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="ParameterOverrideValueSet"/>s of the <see cref="ParameterOverride"/>.
        /// </param>
        internal void Initialize(ContainerList<ParameterOverrideValueSet> valueSet)
        {
            this.Value = valueSet.First().ActualValue.First();
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
            return this.Row.GetParameter<TP>();
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
            return this.Row.Children.Select(child => child.GetParameter<TP>());
        }
    }
}
