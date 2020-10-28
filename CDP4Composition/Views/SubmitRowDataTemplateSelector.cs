// ------------------------------------------------------------------------------------------------
// <copyright file="SubmitRowDataTemplateSelector.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Composition.ViewModels;

    using DevExpress.Xpf.Grid;

    /// <summary>
    /// The Template Selector
    /// </summary>
    public class SubmitRowDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the validation error <see cref="DataTemplate"/> 
        /// </summary>
        public DataTemplate ErrorImageTemplate { get; set; }

        /// <summary>
        /// Gets or sets the validation success <see cref="DataTemplate"/>
        /// </summary>
        public DataTemplate SuccessImageTemplate { get; set; }

        /// <summary>
        /// The select template.
        /// </summary>
        /// <param name="item">
        /// The cell item.
        /// </param>
        /// <param name="container">
        /// The framework element.
        /// </param>
        /// <returns>
        /// The <see cref="DataTemplate"/> for the cell.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cellData = (GridCellData)item;

            if (!(cellData?.RowData.Row is SubmitParameterRowViewModel rowObject))
            {
                return this.ErrorImageTemplate;
            }

            return rowObject.HasValidationError ? this.ErrorImageTemplate : this.SuccessImageTemplate;
        }
    }
}
