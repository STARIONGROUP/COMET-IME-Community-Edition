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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// A builder that builds a tree of <see cref="IEnumerable{ReportingDataSourceNode{T}}"/> instances
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRow"/> representing the data source rows.
    /// </typeparam>
    internal class ReportingDataSourceNodeTreeGenerator<T> where T : ReportingDataSourceRow, new()
    {
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
        /// <param name="nestedElements">The <see cref="List{NestedElement{>}"/>s</param>
        /// <param name="parentNode">The <see cref=" ReportingDataSourceNode{T}"/> that is the parent of new nodes.</param>
        /// <returns>An <see cref="IEnumerable{ReportingDataSourceNode{T}}"/></returns>
        private IEnumerable<ReportingDataSourceNode<T>> GetDataSourceNodes(NestedElement nestedElement, CategoryHierarchy categoryHierarchy, List<NestedElement> nestedElements, ReportingDataSourceNode<T> parentNode)
        {
            var resultNodes = new List<ReportingDataSourceNode<T>>();
            ReportingDataSourceNode<T> newNode = null;
            var searchCategory = categoryHierarchy;

            if (this.HasCategory(nestedElement, categoryHierarchy.Category))
            {
                newNode = new ReportingDataSourceNode<T>(categoryHierarchy, nestedElement, parentNode);
                parentNode?.Children.Add(newNode);
                resultNodes.Add(newNode);
            }
            else if ((parentNode?.HadCategoryUpTree(categoryHierarchy.Category) ?? false) && categoryHierarchy.Child != null && this.HasCategory(nestedElement, categoryHierarchy.Child.Category))
            {
                //if parent was found somewhere up the tree
                searchCategory = categoryHierarchy.Child;

                newNode = new ReportingDataSourceNode<T>(searchCategory, nestedElement, parentNode);
                parentNode?.Children.Add(newNode);
                resultNodes.Add(newNode);
            }

            var children = this.GetChildren(nestedElement, nestedElements).ToList();

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

        /// <summary>
        /// The <see cref="ElementBase"/> representing a <see cref="NestedElement"/>.
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/></param>
        /// <returns>The <see cref="ElementBase"/></returns>
        private ElementBase GetElementBase(NestedElement nestedElement)
        {
            return nestedElement.IsRootElement ? (ElementBase)nestedElement.RootElement
                : nestedElement.ElementUsage.Last();
        }

        /// <summary>
        /// The <see cref="ElementDefinition"/> representing a <see cref="NestedElement"/>, if it exists.
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/></param>
        /// <returns>The <see cref="ElementDefinition"/></returns>
        private ElementDefinition GetElementDefinition(NestedElement nestedElement)
        {
            var elementBase = this.GetElementBase(nestedElement);

            return elementBase as ElementDefinition ?? (elementBase as ElementUsage)?.ElementDefinition;
        }

        /// <summary>
        /// The <see cref="ElementUsage"/> representing a <see cref="NestedElement"/>, if it exists.
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/></param>
        /// <returns>The <see cref="ElementUsage"/></returns>
        private ElementUsage GetElementUsage(NestedElement nestedElement)
        {
            return this.GetElementBase(nestedElement) as ElementUsage;
        }

        /// <summary>
        /// Checks if a <see cref="NestedElement"/> contains a specific <see cref="Category"/>
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/></param>
        /// <param name="category">The <see cref="Category"/></param>
        /// <returns></returns>
        private bool HasCategory(NestedElement nestedElement, Category category)
        {
            return (this.GetElementDefinition(nestedElement)?.Category.Contains(category) ?? false)
                   || (this.GetElementUsage(nestedElement)?.Category.Contains(category) ?? false);
        }

        /// <summary>
        /// Get the children of a <see cref="NestedElement"/>
        /// </summary>
        /// <param name="parentElement">The parent <see cref="NestedElement"/></param>
        /// <param name="nestedElements">A list containing all <see cref="NestedElement"/>s</param>
        /// <returns>An <see cref="IEnumerable{NestedElement}"/> contianing all children <see cref="NestedElement"/>s.</returns>
        private IEnumerable<NestedElement> GetChildren(NestedElement parentElement, List<NestedElement> nestedElements)
        {
            var level = parentElement.ElementUsage.Count;

            var children = nestedElements.Where(ne => ne.ElementUsage.Count == level + 1);

            if (level > 0)
            {
                children = children.Where(ne =>
                    ne.ElementUsage[level - 1] == parentElement.ElementUsage.LastOrDefault());
            }

            children = children.ToList();
            return children;
        }
    }
}
