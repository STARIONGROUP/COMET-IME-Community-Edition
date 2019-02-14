// -------------------------------------------------------------------------------------------------
// <copyright file="MatrixCellDataTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Windows;
    using System.Windows.Controls;
    using CDP4CommonView;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The purpose of this class is to select the right template based on the relationship between 2 objects
    /// </summary>
    public class MatrixCellDataTemplateSelector : DataTemplateSelector
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
            var gridData = item as EditGridCellData;
            if (gridData == null)
            {
                return base.SelectTemplate(item, container);
            }

            var currentColumn = gridData.Column.FieldName;
            if (string.IsNullOrWhiteSpace(currentColumn))
            {
                return base.SelectTemplate(item, container);
            }

            if (currentColumn == MatrixViewModel.ROW_NAME_COLUMN)
            {
                return this.NameColumnTemplate;
            }

            var row = gridData.RowData.Row as ExpandoObject;
            if (row == null)
            {
                return base.SelectTemplate(item, container);
            }

            var dic = (IDictionary<string, object>)row;
            var currentCellValue = dic[currentColumn];

            var vm = (MatrixCellViewModel)currentCellValue;
            if (vm == null || vm.RelationshipDirection == RelationshipDirectionKind.None)
            {
                return this.NoTemplate;
            }

            if (vm.RelationshipDirection == RelationshipDirectionKind.RowThingToColumnThing)
            {
                return this.Source1ToSource2Template;
            }

            if (vm.RelationshipDirection == RelationshipDirectionKind.ColumnThingToRowThing)
            {
                return this.Source2ToSource1Template;
            }

            return this.BiDirectionalTemplate;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show that there is a relationship between a source 1 to a source 2
        /// </summary>
        public DataTemplate Source1ToSource2Template { get; set; }

        /// <summary>
        ///  Gets or sets the <see cref="DataTemplate"/> to show that there is a relationship between a source 2 to a source 1
        /// </summary>
        public DataTemplate Source2ToSource1Template { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show that there is a bi-directional relationship between a source 1 and a source 2
        /// </summary>
        public DataTemplate BiDirectionalTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template to display nothing
        /// </summary>
        public DataTemplate NoTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the name column
        /// </summary>
        public DataTemplate NameColumnTemplate { get; set; }
    }
}
