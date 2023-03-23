// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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

namespace CDP4Reporting.Utilities
{
    using System.Data;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Dal;

    using CDP4Reporting.Events;
    using CDP4Reporting.ViewModels;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Static class that contains individual utility methods that can be used in reports
    /// </summary>
    public static class DebugUtilities
    {
        /// <summary>
        /// Shows the data in a <see cref="DataTable"/> in a model <see cref="DXDialog"/>
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/>
        /// </param>
        /// <param name="allowEdit">
        /// Indicates if editting data in the table is allowed
        /// </param>
        public static void ShowDataTable(DataTable table, bool allowEdit = false)
        {
            var dialog = new DXDialog(table.TableName);
            var dataGrid = new DataGrid();
            dataGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            dataGrid.VerticalAlignment = VerticalAlignment.Stretch;
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.IsReadOnly = !allowEdit;
            dialog.Content = dataGrid;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Sends output from a report as a text to message bus.
        /// Typically a listener is added to the report <see cref="ReportDesignerViewModel"/> that handles showing this output in the UI.
        /// </summary>
        /// <param name="output">
        /// The output as a <see cref="string"/>
        /// </param>
        public static void AddOutput(string output)
        {
            CDPMessageBus.Current.SendMessage(new ReportOutputEvent(output));
        }
    }
}
