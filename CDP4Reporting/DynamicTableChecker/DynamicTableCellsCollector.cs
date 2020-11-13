// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataCollector.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DynamicTableChecker
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="DynamicTableCellsCollector"/> manages a list of definitions used to create table cells that are
    /// added dynamically to a exisitng table in a report definition.
    /// </summary>
    public class DynamicTableCellsCollector : IDynamicTableCellsCollector
    {
        /// <summary>
        /// Gets all Dynamic table cell definitions
        /// </summary>
        public Dictionary<string, ICollection<DynamicTableCell>> DynamicTableCells { get; } = new Dictionary<string,  ICollection<DynamicTableCell>>();

        /// <summary>
        /// Add a static value to the <see cref="DynamicTableCells"/> property 
        /// </summary>
        /// <param name="tableName">The name of the Table in the report</param>
        /// <param name="value">The value that should be displayed in the table cell.</param>
        /// A <see cref="DynamicTableCell"/>
        public DynamicTableCell AddValueTableCell(string tableName, string value)
        {
            return this.AddExpressionTableCell(tableName, $"'{value}'");
        }

        /// <summary>
        /// Add a bindable field to the <see cref="DynamicTableCells"/> property
        /// </summary>
        /// <param name="tableName">The name of the Table in the report</param>
        /// <param name="fieldName">The name of the datasource's field that will be displayed in the table cell.</param>
        /// A <see cref="DynamicTableCell"/>
        public DynamicTableCell AddFieldTableCell(string tableName, string fieldName)
        {
            return this.AddExpressionTableCell(tableName, $"[{fieldName}]");
        }

        /// <summary>
        /// Add an expression to the <see cref="DynamicTableCells"/> property
        /// </summary>
        /// <param name="tableName">The name of the Table in the report</param>
        /// <param name="expression">The expression that will be used to display text in the table cell.</param>
        /// A <see cref="DynamicTableCell"/>
        public DynamicTableCell AddExpressionTableCell(string tableName, string expression)
        {
            if (!this.DynamicTableCells.ContainsKey(tableName))
            {
                this.DynamicTableCells.Add(tableName, new List<DynamicTableCell>());
            }

            var dynamicTableCell = new DynamicTableCell(expression);

            this.DynamicTableCells[tableName].Add(dynamicTableCell);

            return dynamicTableCell;
        }
    }
}
