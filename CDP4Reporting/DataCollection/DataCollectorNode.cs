// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorNode.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Class representing a node in the hierarhical tree upon which the data object is based.
    /// Each node corresponds to a row in the data object's tabular representation.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DataCollectorRow"/> representing the data collector rows.
    /// </typeparam>
    internal class DataCollectorNode<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> of all the <see cref="DataCollectorColumn{T}"/>s
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private Dictionary<PropertyInfo, Type> rowFields;

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> of all the public getters on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        private IEnumerable<PropertyInfo> otherPublicGetters;

        /// <summary>
        /// The <see cref="DataCollectorRow"/> representation of the current node.
        /// </summary>
        private readonly T rowRepresentation;

        /// <summary>
        /// The parent node in the hierarhical tree upon which the data object is based.
        /// </summary>
        private readonly DataCollectorNode<T> parent;

        /// <summary>
        /// The children nodes in the hierarhical tree upon which the data object is based.
        /// </summary>
        internal List<DataCollectorNode<T>> Children { get; } = new List<DataCollectorNode<T>>();

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </summary>
        internal readonly NestedElement NestedElement;

        /// <summary>
        /// The <see cref="ICategorizableThing"/> associated with this node.
        /// </summary>
        internal ICategorizableThing CategorizableThing => this.ElementBase;

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.ElementBase"/> associated with this node.
        /// </summary>
        internal ElementBase ElementBase => this.NestedElement.GetElementBase();

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/> representing this node.
        /// </summary>
        internal ElementDefinition ElementDefinition => this.NestedElement.GetElementDefinition();

        /// <summary>
        /// The <see cref="ElementUsage"/> representing this node, if it exists.
        /// </summary>
        internal ElementUsage ElementUsage =>  this.NestedElement.GetElementUsage();

        /// <summary>
        /// The filtering <see cref="CategoryHierarchy"/> that must be matched on the current <see cref="ElementBase"/>.
        /// </summary>
        private readonly CategoryHierarchy categoryHierarchy;

        /// <summary>
        /// Gets the name of the field/column when the data is transfered to a <see cref="DataRow"/>.
        /// </summary>
        private string fieldName => this.categoryHierarchy?.FieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorNode{T}"/> class.
        /// </summary>
        /// <param name="categoryHierarchy">
        /// The <see cref="CategoryHierarchy"/> associated with this node's subtree.
        /// </param>
        /// <param name="topElement">
        /// The <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </param>
        /// <param name="parent">
        /// The parent node in the hierarhical tree upon which the data collector is based.
        /// </param>
        public DataCollectorNode(
            CategoryHierarchy categoryHierarchy,
            NestedElement topElement,
            DataCollectorNode<T> parent = null)
        {
            this.Initialize();
            this.categoryHierarchy = categoryHierarchy;
            this.NestedElement = topElement;
            this.parent = parent;
            this.rowRepresentation = this.GetRowRepresentation();
        }

        /// <summary>
        /// Initializes this instance of <see cref="DataCollectorNode{T}"/>
        /// </summary>
        private void Initialize()
        {
            this.rowFields = typeof(T).GetProperties()
                .Where(f => f.PropertyType.IsSubclassOf(typeof(DataCollectorColumn<T>)))
                .ToDictionary(f => f, f => f.PropertyType);

            this.otherPublicGetters = typeof(T).GetProperties()
                .Where(p => p.GetMethod?.IsPublic == true).Where(x => !this.rowFields.ContainsKey(x));
        }

        /// <summary>
        /// Checks if a category was found in this instance or up its parent tree.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/>.
        /// </param>
        /// <returns>
        /// True if found, otherwise false.
        /// </returns>
        internal bool HasCategoryUpTree(Category category)
        {
            return this.NestedElement.IsMemberOfCategory(category) || (this.parent?.HasCategoryUpTree(category) ?? false);
        }

        /// <summary>
        /// Boolean flag indicating whether the current <see cref="ElementBase"/> matches the <see cref="categoryHierarchy"/>.
        /// </summary>
        private bool IsVisible => this.NestedElement.IsMemberOfCategory(this.categoryHierarchy.Category);

        /// <summary>
        /// Creates a <see cref="DataTable"/> representation based on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> representation.
        /// </returns>
        public DataTable GetTable()
        {
            var table = new DataTable();

            for (var hierarchy = this.categoryHierarchy; hierarchy != null; hierarchy = hierarchy.Child)
            {
                table.Columns.Add(hierarchy.FieldName, typeof(string));
            }

            foreach (var publicGetter in this.otherPublicGetters)
            {
                table.Columns.Add(publicGetter.Name, publicGetter.GetMethod.ReturnType);
            }

            this.AddDataRows(table);

            return table;
        }

        /// <summary>
        /// Gets the row representation of this node.
        /// </summary>
        /// <returns>
        /// A <see cref="DataCollectorRow"/>.
        /// </returns>
        private T GetRowRepresentation()
        {
            if (!this.IsVisible)
            {
                return null;
            }

            var row = new T
            {
                ElementBase = this.ElementBase,
                IsVisible = this.IsVisible
            };

            foreach (var rowField in this.rowFields)
            {
                var column = Activator.CreateInstance(rowField.Value) as DataCollectorColumn<T>;

                column?.Initialize(this, rowField.Key);

                rowField.Key.SetValue(row, column);
            }

            return row;
        }

        /// <summary>
        /// Gets the columns of type <see cref="TP"/> associated with this node.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired column type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable{TP}"/> of <see cref="DataCollectorColumn{T}"/>s of type <see cref="TP"/>.
        /// </returns>
        public IEnumerable<TP> GetColumns<TP>() where TP : DataCollectorColumn<T>
        {
            return this.rowFields.Where(x => x.Value == typeof(TP) || x.Value.IsSubclassOf(typeof(TP)))
                .Select(x => x.Key.GetValue(this.rowRepresentation) as TP);
        }

        /// <summary>
        /// Adds to the <paramref name="table"/> the <see cref="DataRow"/> representations
        /// of this node's subtree.
        /// </summary>
        /// <param name="table">
        /// The associated <see cref="DataTable"/>.
        /// </param>
        internal void AddDataRows(DataTable table)
        {
            if (this.IsVisible && this.categoryHierarchy.Child == null)
            {
                table.Rows.Add(this.GetDataRow(table));
            }

            foreach (var child in this.Children)
            {
                child.AddDataRows(table);
            }
        }

        /// <summary>
        /// Gets the <see cref="DataRow"/> representation of this node.
        /// </summary>
        /// <param name="table">
        /// The associated <see cref="DataTable"/>.
        /// </param>
        /// <returns>
        /// A <see cref="DataRow"/>.
        /// </returns>
        private DataRow GetDataRow(DataTable table)
        {
            var row = table.NewRow();

            this.InitializeCategoryColumns(row);

            foreach (var rowField in this.rowFields)
            {
                var column = rowField.Key.GetValue(this.rowRepresentation) as DataCollectorColumn<T>;

                column?.Populate(table, row);
            }

            foreach (var publicGetter in this.otherPublicGetters)
            {
                row[publicGetter.Name] = publicGetter.GetMethod.Invoke(
                    this.rowRepresentation,
                    new object[] { });
            }

            return row;
        }

        /// <summary>
        /// Initializes the category columns for the given <paramref name="row"/>
        /// with values from the current node.
        /// </summary>
        /// <param name="row">
        /// The <see cref="DataRow"/> to initialize.
        /// </param>
        private void InitializeCategoryColumns(DataRow row)
        {
            this.parent?.InitializeCategoryColumns(row);

            row[this.fieldName] = this.ElementBase.Name;
        }
    }
}
