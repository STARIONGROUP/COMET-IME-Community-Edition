// ------------------------------------------------------------------------------------------------
// <copyright file="ScaleCellTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Common.SiteDirectoryData;
    using CDP4Requirements.ViewModels;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// A <see cref="DataTemplateSelector"/> for a <see cref="AttributeDefinitionMappingRowViewModel"/> where the <see cref="ParameterType"/> 
    /// is a <see cref="QuantityKind"/>
    /// </summary>
    public class ScaleCellTemplateSelector : DataTemplateSelector
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

            var rowObject = cellData.RowData.Row as SimpleParameterValueRowViewModel;
            if (rowObject != null)
            {
                if (rowObject.ParameterType is QuantityKind)
                {
                    return this.ScaleCellTemplate;
                }
            }

            var reqContainerRowObject = cellData.RowData.Row as RequirementsContainerParameterValueRowViewModel;
            if (reqContainerRowObject != null)
            {
                if (reqContainerRowObject.ParameterType is QuantityKind)
                {
                    return this.ScaleCellTemplate;
                }
            }

            var relationshipRowObject = cellData.RowData.Row as RelationshipParameterValueRowViewModel;
            if (relationshipRowObject != null)
            {
                if (relationshipRowObject.ParameterType is QuantityKind)
                {
                    return this.ScaleCellTemplate;
                }
            }

            return this.InactiveTemplate;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/>
        /// </summary>
        public DataTemplate ScaleCellTemplate { get; set; }

        /// <summary>
        /// Gets or sets the inactive <see cref="DataTemplate"/>
        /// </summary>
        public DataTemplate InactiveTemplate { get; set; }
    }
}
