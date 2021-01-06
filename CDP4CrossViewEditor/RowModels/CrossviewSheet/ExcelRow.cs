// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelRow.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels.CrossviewSheet
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// An abstract super class from which the Excel rows derive
    /// </summary>
    /// <typeparam name="T">
    /// The type of <see cref="Thing"/> that is represented by the row
    /// </typeparam>
    public abstract class ExcelRow<T> : IExcelRow<T> where T : Thing
    {
        /// <summary>
        /// Backing field for the <see cref="containedRows"/> property.
        /// </summary>
        private readonly List<IExcelRow<Thing>> containedRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelRow{T}"/> class.
        /// </summary>
        /// <param name="thing">
        /// The <see cref="thing"/> that is represented by the current row.
        /// </param>
        protected ExcelRow(T thing)
        {
            this.Thing = thing;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by row.
        /// </summary>
        public T Thing { get; private set; }

        /// <summary>
        /// Gets or sets the human readable name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the human readable short name
        /// </summary>
        public string ShortName { get; protected set; }

        /// <summary>
        /// Gets or sets the model code of the <see cref="Thing"/> that is represented by the current row.
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// Gets or sets the level that this row is located at.
        /// </summary>
        /// <remarks>
        /// The Level property is used to apply grouping in Excel
        /// </remarks>
        public int Level { get; protected set; }

        /// <summary>
        /// Gets or sets the type of <see cref="Thing"/> that this row represents
        /// </summary>
        public string Type { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="Container"/> property.
        /// </summary>
        public IExcelRow<Thing> Container { get; set; }

        /// <summary>
        /// Queries the current row for the contained rows
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row
        /// </returns>
        public IEnumerable<IExcelRow<Thing>> GetContainedRows()
        {
            return this.containedRows;
        }

        /// <summary>
        /// Queries the current row for all the rows in the subtree
        /// </summary>
        /// <returns>
        /// the rows that are contained by the current row and its subtree
        /// </returns>
        public IEnumerable<IExcelRow<Thing>> GetContainedRowsDeep()
        {
            var resultList = new List<IExcelRow<Thing>> { this };
            var elementUsageRows = this.containedRows.OfType<ElementUsageExcelRow>().ToList();

            foreach (var containedRow in elementUsageRows.OrderBy(x => x.Name))
            {
                resultList.AddRange(containedRow.GetContainedRowsDeep());
            }

            var leftOverRows = this.containedRows.Except(elementUsageRows);

            foreach (var containedRow in leftOverRows)
            {
                resultList.AddRange(containedRow.GetContainedRowsDeep());
            }

            return resultList;
        }

        /// <summary>
        /// Gets or sets the short-name of the owning <see cref="DomainOfExpertise"/>
        /// </summary>
        public string Owner { get; protected set; }

        /// <summary>
        /// Gets or sets the unique id if the <see cref="Thing"/> that is represented by the row
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Gets or sets the short-name of the <see cref="Category"/>s that the <see cref="ICategorizableThing"/> the current
        /// row represents is a member of.
        /// </summary>
        public string Categories { get; protected set; }
    }
}
