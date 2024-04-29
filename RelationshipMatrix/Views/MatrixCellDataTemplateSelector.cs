﻿// -------------------------------------------------------------------------------------------------
// <copyright file="MatrixCellDataTemplateSelector.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
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
            if (!(item is EditGridCellData gridData))
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

            if (!(gridData.RowData.Row is IDictionary<string, MatrixCellViewModel> row))
            {
                return base.SelectTemplate(item, container);
            }

            var currentCellValue = row[currentColumn];

            if (currentCellValue == null || currentCellValue.RelationshipDirection == RelationshipDirectionKind.None)
            {
                return this.NoTemplate;
            }

            if (gridData.View.DataContext is RelationshipMatrixViewModel matrixvm && !matrixvm.ShowDirectionality)
            {
                return this.NoDirectionalTemplate;
            }

            switch (currentCellValue.RelationshipDirection)
            {
                case RelationshipDirectionKind.RowThingToColumnThing:
                    return this.SourceYToSourceXTemplate;
                case RelationshipDirectionKind.ColumnThingToRowThing:
                    return this.SourceXToSourceYTemplate;
                default:
                    return this.BiDirectionalTemplate;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show that there is a relationship between a source 1 to a source 2
        /// </summary>
        public DataTemplate SourceYToSourceXTemplate { get; set; }

        /// <summary>
        ///  Gets or sets the <see cref="DataTemplate"/> to show that there is a relationship between a source 2 to a source 1
        /// </summary>
        public DataTemplate SourceXToSourceYTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show that there is a bi-directional relationship between a source 1 and a source 2
        /// </summary>
        public DataTemplate BiDirectionalTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show a relationship with no directionality information
        /// </summary>
        public DataTemplate NoDirectionalTemplate { get; set; }

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
