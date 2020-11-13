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
        private IEnumerable<KeyValuePair<PropertyInfo, Type>> allColumns => this.normalColumns.AsEnumerable().Union(this.stateDependentColumns);

        /// <summary>
        /// Gets or sets a <see cref="Dictionary{TKey,TValue}"/> of all the <see cref="DataCollectorColumn{T}"/>s
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private Dictionary<PropertyInfo, Type> normalColumns { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Dictionary{TKey,TValue}"/> of all classes that implement <see cref="IDataCollectorStateDependentPerRowParameter"/>
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private Dictionary<PropertyInfo, Type> stateDependentColumns { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="IEnumerable{T}"/> of all the public getters on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        private IEnumerable<PropertyInfo> publicGetterProperties { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataCollectorRow"/> representation of the current node.
        /// </summary>
        private T rowRepresentation { get; set; }

        /// <summary>
        /// Gets the parent node in the hierarhical tree upon which the data object is based.
        /// </summary>
        private DataCollectorNode<T> parent { get; }

        /// <summary>
        /// Gets or sets The filtering <see cref="CategoryDecompositionHierarchy"/> that must be matched on the current <see cref="ElementBase"/>.
        /// </summary>
        private CategoryDecompositionHierarchy categoryDecompositionHierarchy { get; set; }

        /// <summary>
        /// Gets or sets the name of the field/column when the data is transfered to a <see cref="DataRow"/>.
        /// </summary>
        private string FieldName { get; set; }

        /// <summary>
        /// The children nodes in the hierarhical tree upon which the data object is based.
        /// </summary>
        internal List<DataCollectorNode<T>> Children { get; } = new List<DataCollectorNode<T>>();

        /// <summary>
        /// Gets the <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </summary>
        internal NestedElement NestedElement { get; }

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
        /// GEts or sets an <see cref="IReadOnlyList{Category}"/> that contains all <see cref="Category"/>s in scope of this node.
        /// </summary>
        public IReadOnlyList<Category> CategoriesInRequiredRdl { get; set; }

        /// <summary>
        /// Boolean flag indicating whether the current <see cref="ElementBase"/> matches the <see cref="categoryDecompositionHierarchy"/>.
        /// </summary>
        private bool IsVisible => this.NestedElement.IsMemberOfCategory(this.categoryDecompositionHierarchy.Category) && !this.Children.Any(x => x.IsVisible);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorNode{T}"/> class.
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The <see cref="CategoryDecompositionHierarchy"/> associated with this node's subtree.
        /// </param>
        /// <param name="topElement">
        /// The <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </param>
        /// <param name="parent">
        /// The parent node in the hierarhical tree upon which the data collector is based.
        /// </param>
        public DataCollectorNode(
            CategoryDecompositionHierarchy categoryDecompositionHierarchy,
            NestedElement topElement,
            DataCollectorNode<T> parent = null)
        {
            this.Initialize();
            this.NestedElement = topElement;
            this.parent = parent;
            this.InitializeCategoryDecompositionHierarchy(categoryDecompositionHierarchy);
        }

        /// <summary>
        /// Sets all properties according to data in the related <see cref="CategoryDecompositionHierarchy"/>
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The related <see cref="CategoryDecompositionHierarchy"/>
        /// </param>
        private void InitializeCategoryDecompositionHierarchy(CategoryDecompositionHierarchy categoryDecompositionHierarchy)
        {
            this.categoryDecompositionHierarchy = categoryDecompositionHierarchy;
            this.CategoriesInRequiredRdl = categoryDecompositionHierarchy.CategoriesInRequiredRdl;

            if (categoryDecompositionHierarchy.IsRecursive)
            {
                var level = this.CountCategoryRecursionLevel(this.categoryDecompositionHierarchy.Category);
                this.FieldName = $"{categoryDecompositionHierarchy.FieldName}_{Math.Min(level, categoryDecompositionHierarchy.MaximumRecursiveLevels)}";
            }
            else
            {
                this.FieldName = categoryDecompositionHierarchy.FieldName;
            }
        }

        /// <summary>
        /// Initializes this instance of <see cref="DataCollectorNode{T}"/>
        /// </summary>
        private void Initialize()
        {
            this.stateDependentColumns = typeof(T).GetProperties()
                .Where(f => f.PropertyType.GetInterfaces().Any(i => i == typeof(IDataCollectorStateDependentPerRowParameter)))
                .ToDictionary(f => f, f => f.PropertyType);

            if (this.stateDependentColumns.Select(x => x.Value).Distinct().Count() > 1)
            {
                throw new InvalidOperationException("DataCollectorStateDependentPerRowParameter properties need to be of the same type.");
            }

            this.normalColumns = typeof(T).GetProperties()
                .Where(f => f.PropertyType.IsSubclassOf(typeof(DataCollectorColumn<T>)))
                .Except(this.stateDependentColumns.Keys)
                .ToDictionary(f => f, f => f.PropertyType);

            this.publicGetterProperties = typeof(T).GetProperties()
                .Where(p => p.GetMethod?.IsPublic == true)
                .Except(this.allColumns.Select(x =>x.Key));
        }

        /// <summary>
        /// Checks if there are any columns that implement <see cref="IDataCollectorParameter"/> and have their <see cref="IDataCollectorParameter.HasValueSets"/>
        /// property set to true.
        /// </summary>
        /// <returns>true if there is a column that implements <see cref="IDataCollectorParameter"/> and has its <see cref="IDataCollectorParameter.HasValueSets"/>
        /// property set to true. Otherwise false.</returns>
        private bool ParameterColumnsHaveValueSets()
        {
            var rowPresentation = this.GetRowRepresentation();

            foreach (var rowField in this.allColumns)
            {
                if (rowField.Key.GetValue(rowPresentation) is IDataCollectorParameter column && column.HasValueSets)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the times a category was (recusrively) found in this instance or up its parent tree.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/>.
        /// </param>
        /// <returns>
        /// True if found, otherwise false.
        /// </returns>
        internal int CountCategoryRecursionLevel(Category category)
        {
            var count = this.parent?.CountCategoryRecursionLevel(category) ?? 0;

            if (this.NestedElement.IsMemberOfCategory(category))
            {
                count += 1;
            }

            return count;
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> representation based on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        /// <param name="excludeMissingParameters">
        /// By default all rows are returned by filtering on <see cref="CategoryDecompositionHierarchy"/>.
        /// In case you only want the rows that indeed contain the wanted <see cref="ParameterValueSet"/>s then set this parameter to true.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/> representation.
        /// </returns>
        public DataTable GetTable(bool excludeMissingParameters = false)
        {
            var table = new DataTable();

            this.CreateDataColumnsForCategoryDecompositionHierarchy(table);
            this.CreateDataColumnsForPublicGetters(table);
            this.CheckDataColumnsForStateDependentPerRowParameters(table);

            this.AddDataRows(table, excludeMissingParameters);

            return table;
        }

        /// <summary>
        /// Create <see cref="DataColumn"/>s for properties of <see cref="T"/> whose <see cref="System.Type"/> implements <see cref="IDataCollectorStateDependentPerRowParameter"/>.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> where to add the columns to.
        /// </param>
        private void CheckDataColumnsForStateDependentPerRowParameters(DataTable table)
        {
            foreach (var rowField in this.stateDependentColumns)
            {
                var column = rowField.Key.GetValue(this.GetRowRepresentation()) as IDataCollectorStateDependentPerRowParameter;
                column?.InitializeColumns(table);
            }
        }

        /// <summary>
        /// Create <see cref="DataColumn"/>s for properties of <see cref="T"/> that have public getters.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> where to add the columns to.
        /// </param>
        private void CreateDataColumnsForPublicGetters(DataTable table)
        {
            foreach (var publicGetter in this.publicGetterProperties)
            {
                table.Columns.Add(publicGetter.Name, publicGetter.GetMethod.ReturnType);
            }
        }

        /// <summary>
        /// Create <see cref="DataColumn"/>s that ned to be created from the <see cref="CategoryDecompositionHierarchy"/>'s point of view.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> where to add the columns to.
        /// </param>
        private void CreateDataColumnsForCategoryDecompositionHierarchy(DataTable table)
        {
            for (var hierarchy = this.categoryDecompositionHierarchy; hierarchy != null; hierarchy = hierarchy.Child)
            {
                if (hierarchy.IsRecursive)
                {
                    for (var recursiveCounter = 1; recursiveCounter <= hierarchy.MaximumRecursiveLevels; recursiveCounter++)
                    {
                        table.Columns.Add($"{hierarchy.FieldName}_{recursiveCounter}", typeof(string));
                    }
                }
                else
                {
                    table.Columns.Add(hierarchy.FieldName, typeof(string));
                }
            }
        }

        /// <summary>
        /// Gets the row representation of this node.
        /// </summary>
        /// <param name="forceNew">
        /// If set to true, a new <see cref="T"/> will be created and the cached field <see cref="rowRepresentation"/> will not be set.
        /// </param>
        /// <returns>
        /// A <see cref="DataCollectorRow"/>.
        /// </returns>
        private T GetRowRepresentation(bool forceNew = false)
        {
            if (!forceNew && this.rowRepresentation != null)
            {
                return this.rowRepresentation;
            }

            var row = new T
            {
                ElementBase = this.ElementBase,
                IsVisible = this.IsVisible
            };

            foreach (var rowField in this.allColumns)
            {
                var newObject = Activator.CreateInstance(rowField.Value);
                var column = newObject as DataCollectorColumn<T>;

                if (newObject is DataCollectorCategory<T> categoryColumn)
                {
                    categoryColumn.CategoriesInRequiredRdl = this.CategoriesInRequiredRdl;
                }

                column?.Initialize(this, rowField.Key);
                rowField.Key.SetValue(row, column);
            }

            if (forceNew)
            {
                return row;
            }

            return this.rowRepresentation = row;
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
            return this.allColumns.Where(x => x.Value == typeof(TP) || x.Value.IsSubclassOf(typeof(TP)))
                .Select(x => x.Key.GetValue(this.GetRowRepresentation()) as TP);
        }

        /// <summary>
        /// Adds to the <paramref name="table"/> the <see cref="DataRow"/> representations
        /// of this node's subtree.
        /// </summary>
        /// <param name="table">
        /// The associated <see cref="DataTable"/>.
        /// </param>
        /// <param name="excludeMissingParameters">
        /// By default all rows are returned by filtering on <see cref="CategoryDecompositionHierarchy"/>.
        /// In case you only want the rows that indeed contain the wanted <see cref="ParameterValueSet"/>s then set this parameter to true.
        /// </param>
        internal void AddDataRows(DataTable table, bool excludeMissingParameters)
        {
            if (this.IsVisible 
                && (!excludeMissingParameters || this.ParameterColumnsHaveValueSets())
                && this.categoryDecompositionHierarchy.Child == null)
            {
                this.AddDataRow(table);
            }

            foreach (var child in this.Children)
            {
                child.AddDataRows(table, excludeMissingParameters);
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
        private void AddDataRow(DataTable table)
        {
            var rowPresentationTuples = this.FillStateDependentParameterColumns(table);

            if (!rowPresentationTuples.Any())
            {
                rowPresentationTuples.Add((this.GetRowRepresentation(), table.NewRow()));
            }

            foreach (var (rowPresentation, dataRow) in rowPresentationTuples)
            {
                table.Rows.Add(dataRow);

                this.FillCategoryDecompositionHierarchyColumns(dataRow);
                this.FillNormalColumns(rowPresentation, dataRow);
                this.FillPublicGetterColumns(rowPresentation, dataRow);
            }
        }

        /// <summary>
        /// Fills the <see cref="DataColumn"/>s with with values from the current node for properties of an instance of
        /// <see cref="T"/> whose <see cref="Type"/> implement <see cref="IDataCollectorStateDependentPerRowParameter"/>
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> to fill.
        /// </param>
        private List<(T rowPresentation, DataRow dataRow)> FillStateDependentParameterColumns(DataTable table)
        {
            var mainRowPresentation = this.GetRowRepresentation();
            var rowPresentationTuples = new List<(T rowPresentation, DataRow dataRow)>();

            if (mainRowPresentation != null && this.stateDependentColumns.Any())
            {
                foreach (var rowField in this.stateDependentColumns)
                {
                    var column = rowField.Key.GetValue(mainRowPresentation) as DataCollectorColumn<T>;

                    if (!(column is IDataCollectorParameter dataCollectorParameter) || !dataCollectorParameter.HasValueSets)
                    {
                        continue;
                    }

                    foreach (var valueSet in dataCollectorParameter.ValueSets)
                    {
                        var stateRowPresentation = this.GetRowRepresentation(true);
                        var stateColumn = rowField.Key.GetValue(stateRowPresentation) as DataCollectorColumn<T>;

                        if (!(stateColumn is IDataCollectorParameter stateDataCollectorParameter))
                        {
                            continue;
                        }

                        stateDataCollectorParameter.ValueSets = new List<IValueSet> { valueSet };
                        var stateDataRow = table.NewRow();

                        stateColumn.Populate(table, stateDataRow);

                        rowPresentationTuples.Add((stateRowPresentation, stateDataRow));
                    }
                }
            }

            return rowPresentationTuples;
        }

        /// <summary>
        /// Fills the <see cref="categoryDecompositionHierarchy"/> related <see cref="DataColumn"/>s for the given <paramref name="row"/>
        /// with values from the current node.
        /// </summary>
        /// <param name="row">
        /// The <see cref="DataRow"/> to fill.
        /// </param>
        private void FillCategoryDecompositionHierarchyColumns(DataRow row)
        {
            this.parent?.FillCategoryDecompositionHierarchyColumns(row);

            row[this.FieldName] = this.ElementBase.Name;

            var rowPresentation = this.GetRowRepresentation();

            foreach (var rowField in this.normalColumns)
            {
                var column = rowField.Key.GetValue(rowPresentation) as DataCollectorColumn<T>;

                if (this.categoryDecompositionHierarchy.Child != null 
                    && column is IDataCollectorParameter dataCollectorColumn 
                    && dataCollectorColumn.CollectParentValues)
                {
                    dataCollectorColumn.ParentValuePrefix = $"{this.FieldName}_";
                    column.Populate(row.Table, row);
                }
            }
        }

        /// <summary>
        /// Fill the related <see cref="DataColumn"/>s with values from the current node for properties of an instance of
        /// <see cref="T"/> that have public getters for the given <paramref name="row"/>
        /// </summary>
        /// <param name="rowPresentation">
        /// The instance of <see cref="T"/>
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to fill.
        /// </param>
        private void FillPublicGetterColumns(T rowPresentation, DataRow row)
        {
            foreach (var publicGetter in this.publicGetterProperties)
            {
                row[publicGetter.Name] = publicGetter.GetMethod.Invoke(
                    rowPresentation,
                    new object[] { });
            }
        }

        /// <summary>
        /// Fills the <see cref="DataColumn"/>s with with values from the current node for properties of an instance of <see cref="T"/>
        /// whose <see cref="Type"/> inherits from <see cref="DataCollectorColumn{T}"/>.
        /// </summary>
        /// <param name="rowPresentation">
        /// The instance of <see cref="T"/>
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to fill.
        /// </param>
        private void FillNormalColumns(T rowPresentation, DataRow row)
        {
            foreach (var rowField in this.normalColumns)
            {
                var column = rowField.Key.GetValue(rowPresentation) as DataCollectorColumn<T>;
                column?.Populate(row.Table, row);
            }
        }
    }
}
