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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    public class ReportingDataSourceParameter<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        public static ParameterTypeShortNameAttribute GetParameterAttribute(Type type)
        {
            var attr = Attribute
                .GetCustomAttributes(type)
                .First(attribute => attribute is ParameterTypeShortNameAttribute);

            return attr as ParameterTypeShortNameAttribute;
        }

        // set with reflection to avoid the user-declared constructor having to see it
        private readonly ReportingDataSourceRow<T> row;

        internal readonly string ShortName;

        protected string Value { get; private set; }

        protected ReportingDataSourceParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType()).ShortName;
        }

        internal void Initialize(ContainerList<ParameterValueSet> valueSet)
        {
            // TODO Options, Finite States, and Array parameter types
            this.Value = valueSet.First().ActualValue.First();
        }

        internal void Initialize(ContainerList<ParameterOverrideValueSet> valueSet)
        {
            // TODO Options, Finite States, and Array parameter types
            this.Value = valueSet.First().ActualValue.First();
        }

        public TP GetSibling<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.row.GetParameter<TP>();
        }

        public IEnumerable<TP> GetChildren<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.row.Children.Select(child => child.GetParameter<TP>());
        }
    }
}
