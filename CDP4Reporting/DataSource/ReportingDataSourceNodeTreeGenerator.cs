// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceNodeTreeGenerator.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DataSource
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// A builder that builds a forest of <see cref="IEnumerable{T}"/> of <see cref="ReportingDataSourceNode{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRow"/> representing the data source rows.
    /// </typeparam>
    internal class ReportingDataSourceNodeTreeGenerator<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Generates a forest of <see cref="IEnumerable{T}"/> of <see cref="ReportingDataSourceNode{T}"/> instances
        /// </summary>
        /// <param name="categoryHierarchy">The <see cref="CategoryHierarchy"/> instance to use when searching for relevant data.</param>
        /// <param name="nestedElements">A <see cref="List{NestedElement}"/> that contains the <see cref="NestedElement"/>s to search for relevant data.</param>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="ReportingDataSourceNode{T}"/> instances</returns>
        public IEnumerable<ReportingDataSourceNode<T>> Generate(CategoryHierarchy categoryHierarchy, List<NestedElement> nestedElements)
        {
            var topElement = nestedElements.First(ne => ne.IsRootElement);
            return this.GetDataSourceNodes(topElement, categoryHierarchy, nestedElements, null);
        }

        /// <summary>
        /// Gets the tree of <see cref="ReportingDataSourceNode{T}"/>s. This method can call itself recursively.
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/></param>
        /// <param name="categoryHierarchy">The <see cref="CategoryHierarchy"/></param>
        /// <param name="nestedElements">The <see cref="List{NestedElement}"/>s</param>
        /// <param name="parentNode">The <see cref=" ReportingDataSourceNode{T}"/> that is the parent of new nodes.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ReportingDataSourceNode{T}"/></returns>
        private IEnumerable<ReportingDataSourceNode<T>> GetDataSourceNodes(NestedElement nestedElement, CategoryHierarchy categoryHierarchy, List<NestedElement> nestedElements, ReportingDataSourceNode<T> parentNode)
        {
            var resultNodes = new List<ReportingDataSourceNode<T>>();
            ReportingDataSourceNode<T> newNode = null;
            var searchCategory = categoryHierarchy;

            if (nestedElement.IsMemberOfCategory(categoryHierarchy.Category))
            {
                newNode = new ReportingDataSourceNode<T>(categoryHierarchy, nestedElement, parentNode);
                parentNode?.Children.Add(newNode);
                resultNodes.Add(newNode);
            }
            else if (categoryHierarchy.Child != null && (parentNode?.HasCategoryUpTree(categoryHierarchy.Category) ?? false) && nestedElement.IsMemberOfCategory(categoryHierarchy.Child.Category))
            {
                searchCategory = categoryHierarchy.Child;

                newNode = new ReportingDataSourceNode<T>(searchCategory, nestedElement, parentNode);
                parentNode.Children.Add(newNode);
                resultNodes.Add(newNode);
            }

            var children = nestedElement.GetChildren(nestedElements).ToList();

            foreach (var child in children)
            {
                var nodes = this.GetDataSourceNodes(child, searchCategory, nestedElements, newNode ?? parentNode).ToArray();

                if (newNode == null && nodes.Any())
                {
                    resultNodes = resultNodes.Concat(nodes).ToList();
                }
            }

            return resultNodes;
        }
    }
}
