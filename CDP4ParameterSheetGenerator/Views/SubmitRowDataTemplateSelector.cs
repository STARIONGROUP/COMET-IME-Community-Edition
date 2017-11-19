// ------------------------------------------------------------------------------------------------
// <copyright file="SubmitRowDataTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using CDP4ParameterSheetGenerator.ViewModels;
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
            var containerElement = (FrameworkElement)container;
            var cellData = (GridCellData)item;

            var rowObject = cellData.RowData.Row as WorkbookRebuildRowViewModel;
            if (rowObject == null)
            {
                return this.ErrorImageTemplate;
            }

            return rowObject.HasValidationError ? this.ErrorImageTemplate : this.SuccessImageTemplate;
        }
    }
}
