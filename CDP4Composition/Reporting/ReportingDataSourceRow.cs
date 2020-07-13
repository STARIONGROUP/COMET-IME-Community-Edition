// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceRow.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReportingDataSourceRow<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        #region Hierarchy

        private readonly ReportingDataSourceRow<T> parent;

        internal List<ReportingDataSourceRow<T>> Children { get; } = new List<ReportingDataSourceRow<T>>();

        #endregion

        #region Associated Element

        private readonly ElementBase elementBase;

        private ElementDefinition ElementDefinition =>
            (this.elementBase as ElementDefinition) ?? (this.elementBase as ElementUsage)?.ElementDefinition;

        private ElementUsage ElementUsage =>
            this.elementBase as ElementUsage;

        private string FullyQualifiedName => (this.parent != null)
            ? this.parent.FullyQualifiedName + "." + this.ElementUsage.ShortName
            : this.ElementDefinition.ShortName;

        #endregion

        #region Parameters

        private static readonly IEnumerable<FieldInfo> ParameterFields = typeof(T).GetFields()
            .Where(f => f.FieldType.IsSubclassOf(typeof(ReportingDataSourceParameter<T>)));

        private readonly Dictionary<Type, ReportingDataSourceParameter<T>> reportedParameters =
            new Dictionary<Type, ReportingDataSourceParameter<T>>();

        #endregion

        #region Category filtering

        private readonly Category filterCategory;

        private bool IsVisible =>
            this.elementBase.Category.Contains(this.filterCategory);

        private bool IsRelevant =>
            this.IsVisible || this.Children.Any(child => child.IsRelevant);

        #endregion

        public ReportingDataSourceRow(
            ElementBase elementBase,
            CategoryHierarchy categoryHierarchy,
            ReportingDataSourceRow<T> parent = null)
        {
            this.filterCategory = categoryHierarchy.Category;

            this.parent = parent;

            this.elementBase = elementBase;

            foreach (var type in ParameterFields.Select(f => f.FieldType))
            {
                var parameter = type
                    .GetConstructor(Type.EmptyTypes)
                    .Invoke(new object[] { }) as ReportingDataSourceParameter<T>;

                // set parameter row from here so no constructor declaration is needed
                typeof(ReportingDataSourceParameter<T>)
                    .GetField("row", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(parameter, this);

                this.reportedParameters[type] = parameter;

                this.InitializeParameter(parameter);
            }

            if (categoryHierarchy.Child == null)
            {
                return;
            }

            foreach (var childUsage in this.ElementDefinition.ContainedElement)
            {
                var childRow = new ReportingDataSourceRow<T>(childUsage, categoryHierarchy.Child, this);

                if (childRow.IsRelevant)
                {
                    this.Children.Add(childRow);
                }
            }
        }

        private void InitializeParameter(ReportingDataSourceParameter<T> reportedParameter)
        {
            var parameter = this.ElementDefinition.Parameter
                .SingleOrDefault(x => x.ParameterType.ShortName == reportedParameter.ShortName);

            if (parameter != null)
            {
                reportedParameter.Initialize(parameter.ValueSet);
            }

            var parameterOverride = this.ElementUsage?.ParameterOverride
                .SingleOrDefault(x => x.Parameter.ParameterType.ShortName == reportedParameter.ShortName);

            if (parameterOverride != null)
            {
                reportedParameter.Initialize(parameterOverride.ValueSet);
            }
        }

        public TP GetParameter<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.reportedParameters[typeof(TP)] as TP;
        }

        public List<T> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<T>();

            tabularRepresentation.Add(this.GetRowTabularRepresentation());

            foreach (var row in this.Children)
            {
                tabularRepresentation.AddRange(row.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }

        private T GetRowTabularRepresentation()
        {
            var row = new T();

            typeof(T).GetField("ElementName")
                .SetValue(row, this.FullyQualifiedName);

            typeof(T).GetField("IsVisible")
                .SetValue(row, this.IsVisible);

            if (!this.IsVisible)
            {
                return row;
            }

            foreach (var field in ParameterFields)
            {
                field.SetValue(row, this.reportedParameters[field.FieldType]);
            }

            return row;
        }
    }
}
