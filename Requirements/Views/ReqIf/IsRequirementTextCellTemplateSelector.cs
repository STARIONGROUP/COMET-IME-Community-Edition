// ------------------------------------------------------------------------------------------------
// <copyright file="IsRequirementTextCellTemplateSelector.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Requirements.ViewModels;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// A <see cref="DataTemplateSelector"/> for a <see cref="AttributeDefinitionMappingRowViewModel"/>
    /// </summary>
    public class IsRequirementTextCellTemplateSelector : DataTemplateSelector
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

            var rowObject = cellData.RowData.Row as AttributeDefinitionMappingRowViewModel;
            if (rowObject != null)
            {
                return this.IsRequirementCellTemplate;
            }

            return this.InactiveTemplate;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for spec-object row
        /// </summary>
        public DataTemplate IsRequirementCellTemplate { get; set; }

        /// <summary>
        /// Gets or sets the inactive <see cref="DataTemplate"/>
        /// </summary>
        public DataTemplate InactiveTemplate { get; set; }
    }
}
