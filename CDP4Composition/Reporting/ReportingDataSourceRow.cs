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

    /// <summary>
    /// Class representing a row associated with a node in the hierarhical tree upon which the data source is based.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRowRepresentation"/> representing the data source rows.
    /// </typeparam>
    internal class ReportingDataSourceRow<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        /// <summary>
        /// The parent node in the hierarhical tree upon which the data source is based.
        /// </summary>
        private readonly ReportingDataSourceRow<T> parent;

        /// <summary>
        /// The children nodes in the hierarhical tree upon which the data source is based.
        /// </summary>
        internal List<ReportingDataSourceRow<T>> Children { get; } = new List<ReportingDataSourceRow<T>>();

        /// <summary>
        /// The <see cref="ElementBase"/> associated with this node.
        /// </summary>
        private readonly ElementBase elementBase;

        /// <summary>
        /// The <see cref="ElementDefinition"/> representing this node.
        /// </summary>
        private ElementDefinition ElementDefinition =>
            (this.elementBase as ElementDefinition) ?? (this.elementBase as ElementUsage)?.ElementDefinition;

        /// <summary>
        /// The <see cref="ElementUsage"/> representing this node, if it exists.
        /// </summary>
        private ElementUsage ElementUsage =>
            this.elementBase as ElementUsage;

        /// <summary>
        /// The fully qualified (to the tree root) name of this <see cref="elementBase"/>.
        /// </summary>
        private string FullyQualifiedName => (this.parent != null)
            ? this.parent.FullyQualifiedName + "." + this.ElementUsage.ShortName
            : this.ElementDefinition.ShortName;

        /// <summary>
        /// The list of declared <see cref="ReportingDataSourceParameter{T}"/> types on
        /// the associated <see cref="ReportingDataSourceRowRepresentation"/>.
        /// </summary>
        private static readonly IEnumerable<FieldInfo> ParameterFields = typeof(T).GetFields()
            .Where(f => f.FieldType.IsSubclassOf(typeof(ReportingDataSourceParameter<T>)));

        /// <summary>
        /// The <see cref="ReportingDataSourceParameter{T}"/>s associated with this row.
        /// </summary>
        private readonly Dictionary<Type, ReportingDataSourceParameter<T>> reportedParameters =
            new Dictionary<Type, ReportingDataSourceParameter<T>>();

        /// <summary>
        /// The filtering <see cref="Category"/> that must be matched on the current <see cref="elementBase"/>.
        /// </summary>
        private readonly Category filterCategory;

        /// <summary>
        /// Boolean flag indicating whether the current <see cref="elementBase"/> matches the <see cref="filterCategory"/>.
        /// </summary>
        private bool IsVisible =>
            this.elementBase.Category.Contains(this.filterCategory);

        /// <summary>
        /// Boolean flag indicating whether the current node or any of its <see cref="Children"/>
        /// match their associated <see cref="filterCategory"/>.
        /// </summary>
        private bool IsRelevant =>
            this.IsVisible || this.Children.Any(child => child.IsRelevant);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceRow{T}"/> class.
        /// </summary>
        /// <param name="elementBase">
        /// The <see cref="ElementBase"/> associated with this node.
        /// </param>
        /// <param name="categoryHierarchy">
        /// The <see cref="CategoryHierarchy"/> associated with this node's subtree.
        /// </param>
        /// <param name="parent">
        /// The parent node in the hierarhical tree upon which the data source is based.
        /// </param>
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

                parameter.Row = this;

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

        /// <summary>
        /// Initializes a reported parameter based on the corresponding <see cref="ParameterOrOverrideBase"/>
        /// associated with the current <see cref="elementBase"/>.
        /// </summary>
        /// <param name="reportedParameter">
        /// The reported parameter to be initialized.
        /// </param>
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

        /// <summary>
        /// Gets the parameter of type <see cref="TP"/> associated with this node.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired parameter type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="ReportingDataSourceParameter{T}"/> of type <see cref="TP"/>.
        /// </returns>
        public TP GetParameter<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.reportedParameters[typeof(TP)] as TP;
        }

        /// <summary>
        /// Gets the tabular representation of this node's subtree.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="ReportingDataSourceRowRepresentation"/>.
        /// </returns>
        public List<T> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<T>
            {
                this.GetRowTabularRepresentation()
            };

            foreach (var row in this.Children)
            {
                tabularRepresentation.AddRange(row.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }

        /// <summary>
        /// Gets the tabular representation of this node.
        /// </summary>
        /// <returns>
        /// A <see cref="ReportingDataSourceRowRepresentation"/>.
        /// </returns>
        private T GetRowTabularRepresentation()
        {
            var row = new T
            {
                ElementName = this.FullyQualifiedName,
                IsVisible = this.IsVisible
            };

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
