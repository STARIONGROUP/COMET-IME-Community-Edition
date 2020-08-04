// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceClass.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using System.Data;
    using System.Linq;

    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Class representing a reporting data source.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRow"/> representing the data source rows.
    /// </typeparam>
    public class ReportingDataSourceClass<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// The <see cref="CategoryHierarchy"/> used for filtering the considered <see cref="ElementBase"/> items.
        /// </summary>
        private readonly CategoryHierarchy categoryHierarchy;

        /// <summary>
        /// The <see cref="ReportingDataSourceNode{T}"/> which is the root of the hierarhical tree.
        /// </summary>
        private readonly ReportingDataSourceNode<T> topNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceClass{T}"/> class.
        /// </summary>
        /// <param name="categoryHierarchy">
        /// The <see cref="CategoryHierarchy"/> used for filtering the considered <see cref="ElementBase"/> items.
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the data source is built.
        /// </param>
        /// <param name="domainOfExpertise">
        /// The <see cref="DomainOfExpertise"/> for which the data source is built.
        /// </param>
        public ReportingDataSourceClass(
            CategoryHierarchy categoryHierarchy,
            Option option,
            DomainOfExpertise domainOfExpertise)
        {
            this.categoryHierarchy = categoryHierarchy;

            var nestedElements = new NestedElementTreeGenerator()
                .Generate(option, domainOfExpertise)
                .ToList();

            var topElement = nestedElements.First(ne => ne.IsRootElement);

            this.topNode = new ReportingDataSourceNode<T>(categoryHierarchy, topElement, nestedElements);
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> representation of the hierarhical tree upon which the data source is based.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public DataTable GetTable()
        {
            var table = ReportingDataSourceNode<T>.GetTable(this.categoryHierarchy);

            this.topNode.AddDataRows(table);

            return table;
        }
    }
}
