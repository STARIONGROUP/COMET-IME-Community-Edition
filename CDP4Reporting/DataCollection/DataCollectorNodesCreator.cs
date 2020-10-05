// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedElementTreeDataCollector.cs" company="RHEA System S.A.">
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
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Class that can be used to collect data from the NestedElementTree / ProductTree.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DataCollectorRow"/> representing the data collector rows.
    /// </typeparam>
    public class DataCollectorNodesCreator<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> of type <see cref="DataCollectorNode{T}"/> that contains <see cref="DataCollectorNode{T}"/> instances,
        /// using a <see cref="CategoryDecompositionHierarchy"/> and a <see cref="List{NestedElement}"/>
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The <see cref="CategoryDecompositionHierarchy"/> used for filtering the considered <see cref="NestedElement"/> items.
        /// </param>
        /// <param name="nestedElements">
        /// The <see cref="List{NestedElement}"/>s
        /// </param>
        internal IEnumerable<DataCollectorNode<T>> CreateNodes(
            CategoryDecompositionHierarchy categoryDecompositionHierarchy,
            List<NestedElement> nestedElements)
        {
            var topElement = nestedElements.First(ne => ne.IsRootElement);

            return this.GetDataCollectorNodes(topElement, categoryDecompositionHierarchy, nestedElements, null);
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> representation from data in a tree structure of <see cref="NestedElement"/>s based on a <see cref="CategoryDecompositionHierarchy"/>
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The <see cref="CategoryDecompositionHierarchy"/> used for filtering the considered <see cref="NestedElement"/> items.
        /// </param>
        /// <param name="nestedElements">
        /// The <see cref="List{NestedElement}"/>s
        /// </param>
        /// <param name="excludeMissingParameters">
        /// By default all rows are returned by filtering on <see cref="CategoryDecompositionHierarchy"/>.
        /// In case you only want the rows that indeed contain the wanted <see cref="ParameterValueSet"/>s then set this parameter to true.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public DataTable GetTable(
            CategoryDecompositionHierarchy categoryDecompositionHierarchy,
            List<NestedElement> nestedElements, 
            bool excludeMissingParameters = false
            )
        {
            var dataTables =
                this.CreateNodes(categoryDecompositionHierarchy, nestedElements)
                    .Select(x => x.GetTable(excludeMissingParameters))
                    .ToList();

            if (!dataTables.Any())
            {
                return null;
            }

            var dataTable = dataTables.First();

            if (dataTables.Count > 1)
            {
                for (var dt = 1; dt < dataTables.Count; dt++)
                {
                    var mergeTable = dataTables[dt];
                    dataTable.Merge(mergeTable, false, MissingSchemaAction.Add);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Gets the tree of <see cref="DataCollectorNode{T}"/>s. This method can call itself recursively.
        /// </summary>
        /// <param name="nestedElement">
        /// The <see cref="NestedElement"/>
        /// </param>
        /// <param name="categoryDecompositionHierarchy">
        /// The <see cref="CategoryDecompositionHierarchy"/> used for filtering the considered <see cref="NestedElement"/> items.
        /// </param>
        /// <param name="nestedElements">
        /// The <see cref="List{NestedElement}"/>s
        /// </param>
        /// <param name="parentNode">
        /// The <see cref=" DataCollectorNode{T}"/> that is the parent of new nodes.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="DataCollectorNode{T}"/>
        /// </returns>
        private IEnumerable<DataCollectorNode<T>> GetDataCollectorNodes(NestedElement nestedElement, CategoryDecompositionHierarchy categoryDecompositionHierarchy, List<NestedElement> nestedElements, DataCollectorNode<T> parentNode)
        {
            var resultNodes = new List<DataCollectorNode<T>>();
            DataCollectorNode<T> newNode = null;
            var searchCategory = categoryDecompositionHierarchy;

            if (nestedElement.IsMemberOfCategory(categoryDecompositionHierarchy.Category))
            {
                newNode = new DataCollectorNode<T>(categoryDecompositionHierarchy, nestedElement, parentNode);
                parentNode?.Children.Add(newNode);
                resultNodes.Add(newNode);
            }
            else if (categoryDecompositionHierarchy.Child != null && parentNode?.CountCategoryRecursionLevel(categoryDecompositionHierarchy.Category) > 0 && nestedElement.IsMemberOfCategory(categoryDecompositionHierarchy.Child.Category))
            {
                searchCategory = categoryDecompositionHierarchy.Child;

                newNode = new DataCollectorNode<T>(searchCategory, nestedElement, parentNode);
                parentNode.Children.Add(newNode);
                resultNodes.Add(newNode);
            }

            var children = nestedElement.GetChildren(nestedElements).ToList();

            foreach (var child in children)
            {
                var nodes = this.GetDataCollectorNodes(child, searchCategory, nestedElements, newNode ?? parentNode).ToArray();

                if (newNode == null && nodes.Any())
                {
                    resultNodes = resultNodes.Concat(nodes).ToList();
                }
            }

            return resultNodes;
        }
    }
}
