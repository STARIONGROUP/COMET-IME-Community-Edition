// --------------------------------------------------------------------------------------------------------------------
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
            foreach (var table in report.AllControls<XRTable>())
            {
                if (dataCollector.DynamicTableFields.ContainsKey(table.Name))
                {
                    table.Rows.Clear();
                    var newRow = new XRTableRow();
                    table.Rows.Add(newRow);

                    foreach (var keyValuePair in dataCollector.DynamicTableFields[table.Name])
                    {
                        var newCell = new XRTableCell();
                        var dataMemberPrefix = !string.IsNullOrWhiteSpace(report.DataMember) ? "" : report.DataMember + ".";
                        var dataMemberName = $"{dataMemberPrefix}{keyValuePair.Key}";

                        newCell.DataBindings.Add(new XRBinding("Text", report.DataSource, dataMemberName));
                        newRow.Cells.Add(newCell);
                    }
                }

                if (table.Name.Contains("Header"))
                {
                    var tableNameHeader = table.Name.Replace("Header", "");

                    if (dataCollector.DynamicTableFields.ContainsKey(tableNameHeader))
                    {
                        table.Rows.Clear();
                        var newRow = new XRTableRow();
                        table.Rows.Add(newRow);

                        foreach (var keyValuePair in dataCollector.DynamicTableFields[tableNameHeader])
                        {
                            var newCell = new XRTableCell();
                            newCell.Text = keyValuePair.Value;
                            newRow.Cells.Add(newCell);
                        }
                    }
                }
            }
        }
    }
}
