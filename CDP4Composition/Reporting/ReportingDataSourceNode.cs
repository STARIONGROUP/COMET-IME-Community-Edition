// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceNode.cs" company="RHEA System S.A.">
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
    /// Class representing a node in the hierarhical tree upon which the data source is based.
    /// Each node corresponds to a row in the data source tabular representation.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRow"/> representing the data source rows.
    /// </typeparam>
    internal class ReportingDataSourceNode<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// A dictionary of all the <see cref="ReportingDataSourceColumn{T}"/>s declared
        /// as <see cref="ReportingDataSourceRow"/> fields.
        /// </summary>
        private static readonly Dictionary<Type, FieldInfo> RowFields = typeof(T).GetFields()
            .Where(f => f.FieldType.IsSubclassOf(typeof(ReportingDataSourceColumn<T>)))
            .ToDictionary(f => f.FieldType, f => f);

        /// <summary>
        /// The <see cref="ReportingDataSourceRow"/> representation of the current node.
        /// </summary>
        private readonly T rowRepresentation;

        /// <summary>
        /// The parent node in the hierarhical tree upon which the data source is based.
        /// </summary>
        private readonly ReportingDataSourceNode<T> parent;

        /// <summary>
        /// The children nodes in the hierarhical tree upon which the data source is based.
        /// </summary>
        internal List<ReportingDataSourceNode<T>> Children { get; } = new List<ReportingDataSourceNode<T>>();

        /// <summary>
        /// The <see cref="ElementBase"/> associated with this node.
        /// </summary>
        internal readonly ElementBase ElementBase;

        /// <summary>
        /// The <see cref="ElementDefinition"/> representing this node.
        /// </summary>
        internal ElementDefinition ElementDefinition =>
            (this.ElementBase as ElementDefinition) ?? (this.ElementBase as ElementUsage)?.ElementDefinition;

        /// <summary>
        /// The <see cref="ElementUsage"/> representing this node, if it exists.
        /// </summary>
        internal ElementUsage ElementUsage =>
            this.ElementBase as ElementUsage;

        /// <summary>
        /// The fully qualified (to the tree root) name of this <see cref="ElementBase"/>.
        /// </summary>
        private string FullyQualifiedName => (this.parent != null)
            ? this.parent.FullyQualifiedName + "." + this.ElementUsage.ShortName
            : this.ElementDefinition.ShortName;

        /// <summary>
        /// The filtering <see cref="Category"/> that must be matched on the current <see cref="ElementBase"/>.
        /// </summary>
        private readonly Category filterCategory;

        /// <summary>
        /// Boolean flag indicating whether the current <see cref="ElementBase"/> matches the <see cref="filterCategory"/>.
        /// </summary>
        private bool IsVisible =>
            this.ElementBase.Category.Contains(this.filterCategory);

        /// <summary>
        /// Boolean flag indicating whether the current node or any of its <see cref="Children"/>
        /// match their associated <see cref="filterCategory"/>.
        /// </summary>
        private bool IsRelevant =>
            this.IsVisible || this.Children.Any(child => child.IsRelevant);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceNode{T}"/> class.
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
        public ReportingDataSourceNode(
            ElementBase elementBase,
            CategoryHierarchy categoryHierarchy,
            ReportingDataSourceNode<T> parent = null)
        {
            this.filterCategory = categoryHierarchy.Category;

            this.parent = parent;

            this.ElementBase = elementBase;

            this.rowRepresentation = new T
            {
                ElementBase = this.ElementBase,
                ElementName = this.FullyQualifiedName,
                IsVisible = this.IsVisible
            };

            if (this.IsVisible)
            {
                foreach (var rowField in RowFields)
                {
                    var column = rowField.Key
                        .GetConstructor(Type.EmptyTypes)
                        .Invoke(new object[] { }) as ReportingDataSourceColumn<T>;

                    column.Initialize(this);

                    rowField.Value.SetValue(this.rowRepresentation, column);
                }
            }

            if (categoryHierarchy.Child == null)
            {
                return;
            }

            foreach (var childUsage in this.ElementDefinition.ContainedElement)
            {
                var childNode = new ReportingDataSourceNode<T>(childUsage, categoryHierarchy.Child, this);

                if (childNode.IsRelevant)
                {
                    this.Children.Add(childNode);
                }
            }
        }

        /// <summary>
        /// Gets the column of type <see cref="TP"/> associated with this node.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired column type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="ReportingDataSourceColumn{T}"/> of type <see cref="TP"/>.
        /// </returns>
        public TP GetColumn<TP>() where TP : ReportingDataSourceColumn<T>
        {
            return RowFields[typeof(TP)].GetValue(this.rowRepresentation) as TP;
        }

        /// <summary>
        /// Gets the tabular representation of this node's subtree.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="ReportingDataSourceRow"/>.
        /// </returns>
        public List<T> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<T>
            {
                this.rowRepresentation
            };

            foreach (var node in this.Children)
            {
                tabularRepresentation.AddRange(node.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }
    }
}
