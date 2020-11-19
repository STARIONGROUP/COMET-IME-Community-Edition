﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicTableChecker.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DynamicTableChecker
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;

    using CDP4Reporting.DataCollection;

    using DevExpress.XtraReports.UI;

    /// <summary>
    /// The implementation of the injectable interface <see cref="IDynamicTableChecker"/> that is used to check dynamic tables in a report
    /// </summary>
    [Export(typeof(IDynamicTableChecker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExcludeFromCodeCoverage]
    public class DynamicTableChecker : IDynamicTableChecker
    {
        public void Check(XtraReport report, IDataCollector dataCollector)
        {
            if (report == null || dataCollector == null)
            {
                return;
            }

            var dynamicTableCells = dataCollector.DynamicTableCellsCollector.DynamicTableCells;

            foreach (var table in report.AllControls<XRTable>())
            {
                if (dynamicTableCells.ContainsKey(table.Name))
                {
                    var orgCell = table.Rows[0].Cells[0];

                    table.Rows.Clear();
                    var newRow = new XRTableRow();
                    table.Rows.Add(newRow);

                    var orgWidth = table.Width;
                    var columnCount = Math.Max(dynamicTableCells[table.Name].Count, 1);
                    var newWidth = orgWidth / columnCount;

                    foreach (var dynamicTableCell in dynamicTableCells[table.Name])
                    {
                        var newCell = CreateNewTableCell(orgCell);
                        SetDynamicTableCellProperties(newCell, dynamicTableCell);

                        newRow.Cells.Add(newCell);
                    }

                    foreach (var cell in newRow.Cells)
                    {
                        ((XRTableCell)cell).WidthF = newWidth;
                    }
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="XRTableCell"/> based on an existing <see cref="XRTableCell"/>
        /// </summary>
        /// <param name="baseOnCell">The exisiting <see cref="XRTableCell"/></param>
        /// <returns>A new <see cref="XRTableCell"/></returns>
        private static XRTableCell CreateNewTableCell(XRTableCell baseOnCell)
        {
            var newCell = new XRTableCell();
            newCell.TextFormatString = baseOnCell.TextFormatString;
            newCell.Summary.Running = baseOnCell.Summary.Running;
            newCell.Summary.FormatString = baseOnCell.Summary.FormatString;
            return newCell;
        }

        /// <summary>
        /// Set specific <see cref="XRTableCell"/> properties using a <see cref="DynamicTableCell"/>
        /// </summary>
        /// <param name="newCell">The <see cref="XRTableCell"/></param>
        /// <param name="dynamicTableCell">The <see cref="DynamicTableCell"/></param>
        private static void SetDynamicTableCellProperties(XRTableCell newCell, DynamicTableCell dynamicTableCell)
        {
            newCell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", dynamicTableCell.Expression));

            if (!string.IsNullOrWhiteSpace(dynamicTableCell.ForeColorExpression))
            {
                newCell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "ForeColor", dynamicTableCell.ForeColorExpression));
            }

            if (!string.IsNullOrWhiteSpace(dynamicTableCell.BackColorExpression))
            {
                newCell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "BackColor", dynamicTableCell.BackColorExpression));
            }
        }
    }
}