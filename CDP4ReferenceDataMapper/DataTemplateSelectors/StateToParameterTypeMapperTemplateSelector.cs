// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperTemplateSelector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.DataTemplateSelectors
{
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4ReferenceDataMapper.Managers;

    using DevExpress.Xpf.Grid;

    using DataGridColumn = GridColumns.DataGridColumn;

    /// <summary>
    /// Custom <see cref="DataTemplateSelector"/> used to dynamically select a editor emplate for a specific cell
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StateToParameterTypeMapperTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Select a specific <see cref="DataTemplate"/> for an item
        /// </summary>
        /// <param name="item">The item (<see cref="EditGridCellData"/>)</param>
        /// <param name="container">The <see cref="DependencyObject"/></param>
        /// <returns>A <see cref="DataTemplate"/>, or null if no <see cref="DataTemplate"/> is necessary</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var data = (EditGridCellData)item;

            if (!(data?.RowData.Row is DataRowView dataRowView))
            {
                return null;
            }

            if (!(data.Column.DataContext is DataGridColumn column))
            {
                return null;
            }

            return dataRowView[DataSourceManager.TypeColumnName].ToString() == DataSourceManager.ParameterMappingType
                   && column.DataSourceManager.IsActualStateColumn(data.Column.FieldName)
                ? (DataTemplate)((FrameworkElement)container).FindResource("comboBoxEditor")
                : null;
        }
    }
}
