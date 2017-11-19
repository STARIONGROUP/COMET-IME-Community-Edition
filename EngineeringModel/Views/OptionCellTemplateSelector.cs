// ------------------------------------------------------------------------------------------------
// <copyright file="OptionCellTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// A <see cref="DataTemplateSelector"/> for a <see cref="ElementUsageRowViewModel"/>
    /// </summary>
    public class OptionCellTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Returns a <see cref="DataTemplate"/> based on custom logic.
        /// </summary>
        /// <param name="item">
        /// The data object for which to select the template.
        /// </param>
        /// <param name="container">
        /// The data-bound object.
        /// </param>
        /// <returns>
        /// Returns a <see cref="DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var containerElement = (FrameworkElement)container;
            var cellData = (GridCellData)item;

            var rowObject = cellData.RowData.Row;
            if (rowObject is ElementUsageRowViewModel)
            {
                return (DataTemplate)containerElement.FindResource("elementUsageOptionTemplate");
            }

            return base.SelectTemplate(item, container);
        }
    }
}
