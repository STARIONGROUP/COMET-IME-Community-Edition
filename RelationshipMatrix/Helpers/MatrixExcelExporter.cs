// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExcelExporter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Helpers
{
    using System;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4RelationshipMatrix.Settings;
    using CDP4RelationshipMatrix.ViewModels;

    using ClosedXML.Excel;

    /// <summary>
    /// Exporter for relationship matrix to excel
    /// </summary>
    public class MatrixExcelExporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixExcelExporter"/> class
        /// </summary>
        public MatrixExcelExporter(SourceConfigurationViewModel sourceXConfiguration, SourceConfigurationViewModel sourceYConfiguration, RelationshipConfigurationViewModel relationshipConfiguration, MatrixViewModel matrix, Iteration iteration)
        {
            this.SourceXConfiguration = sourceXConfiguration;
            this.SourceYConfiguration = sourceYConfiguration;
            this.RelationshipConfiguration = relationshipConfiguration;
            this.Matrix = matrix;
            this.Iteration = iteration;
        }

        /// <summary>
        /// Gets the x axis configuration
        /// </summary>
        public SourceConfigurationViewModel SourceXConfiguration { get; }

        /// <summary>
        /// Gets the y axis configuration
        /// </summary>
        public SourceConfigurationViewModel SourceYConfiguration { get; }

        /// <summary>
        /// Gets the relationship configuration
        /// </summary>
        public RelationshipConfigurationViewModel RelationshipConfiguration { get; }

        /// <summary>
        /// Gets the matrix
        /// </summary>
        public MatrixViewModel Matrix { get; }

        /// <summary>
        /// Gets the Iteration
        /// </summary>
        public Iteration Iteration { get; }

        /// <summary>
        /// Exports the matrix into a excel workbook
        /// </summary>
        /// <param name="path">The path to save the file to.</param>
        public void Export(string path)
        {
            using (var workbook = new XLWorkbook())
            {
                this.ConstructMetaSheet(workbook);

                this.ConstructMatrixSheet(workbook);

                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// Constructes the matrix sheet
        /// </summary>
        /// <param name="workbook">The workbook</param>
        private void ConstructMatrixSheet(XLWorkbook workbook)
        {
            var worksheetMatrix = workbook.Worksheets.Add("Matrix");

            var firstCell = worksheetMatrix.Cell("A1");
            var lastCell = worksheetMatrix.Cell(this.Matrix.Records.Count + 1, this.Matrix.Columns.Count);

            // construct headers
            for (var i = 1; i < this.Matrix.Columns.Count; i++)
            {
                worksheetMatrix.Cell(1, i).Value = this.Matrix.Columns[i - 1].Header;
            }

            // construct total header
            worksheetMatrix.Cell(1, this.Matrix.Columns.Count).Value = "Traces";

            worksheetMatrix.Row(1).Style.Alignment.TextRotation = 90;
            worksheetMatrix.Row(1).Style.Font.Bold = true;
            worksheetMatrix.Column(1).Style.Font.Bold = true;
            worksheetMatrix.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheetMatrix.Column(this.Matrix.Columns.Count).Style.Font.Bold = false;
            worksheetMatrix.Column(this.Matrix.Columns.Count).Style.Font.Italic = true;

            firstCell.Value = this.SourceYConfiguration?.SelectedClassKind;
            firstCell.Style.Alignment.TextRotation = 0;

            // construct rows
            for (var i = 1; i <= this.Matrix.Records.Count; i++)
            {
                worksheetMatrix.Cell(i + 1, 1).Value = this.Matrix.Records[i - 1].First().Value.DisplayKind == DisplayKind.Name ? this.Matrix.Records[i - 1].First().Value.SourceY.UserFriendlyName : this.Matrix.Records[i - 1].First().Value.SourceY.UserFriendlyShortName;
                var relation = this.Matrix.Records[i - 1].Values.ToList();

                var traceCount = 0;

                for (var j = 1; j < this.Matrix.Records[i - 1].Count; j++)
                {
                    var trace = relation[j].RelationshipDirection != RelationshipDirectionKind.None;

                    if (trace)
                    {
                        traceCount++;
                    }

                    worksheetMatrix.Cell(i + 1, j + 1).Value = trace ? "X" : string.Empty;
                }

                worksheetMatrix.Cell(i + 1, this.Matrix.Records[i - 1].Count).Value = traceCount;
            }

            var matrixTable = worksheetMatrix.Range(firstCell, lastCell).CreateTable("Matrix");
            matrixTable.ShowAutoFilter = false;
            matrixTable.Theme = XLTableTheme.None;

            matrixTable.DataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            matrixTable.ShowTotalsRow = true;

            foreach (var matrixTableField in matrixTable.Fields)
            {
                matrixTableField.TotalsRowFunction = XLTotalsRowFunction.Count;
            }

            matrixTable.Field(this.SourceYConfiguration.SelectedClassKind.ToString()).TotalsRowLabel = "Traces";
            matrixTable.Field(this.SourceYConfiguration.SelectedClassKind.ToString()).TotalsRowFunction = XLTotalsRowFunction.None;

            matrixTable.Rows().Last().Style.Font.Italic = true;
            matrixTable.Rows().Last().Style.Font.Bold = false;

            worksheetMatrix.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // color in trace summaries
            matrixTable.Rows().Last().AddConditionalFormat().WhenEquals("Traces").Fill.SetBackgroundColor(XLColor.NoColor);
            matrixTable.Rows().Last().AddConditionalFormat().WhenEquals(0).Fill.SetBackgroundColor(XLColor.MistyRose);
            matrixTable.Rows().Last().AddConditionalFormat().WhenGreaterThan(0).Fill.SetBackgroundColor(XLColor.Celadon);

            matrixTable.Columns().Last().AddConditionalFormat().WhenEquals("Traces").Fill.SetBackgroundColor(XLColor.NoColor);
            matrixTable.Columns().Last().AddConditionalFormat().WhenEquals(0).Fill.SetBackgroundColor(XLColor.MistyRose);
            matrixTable.Columns().Last().AddConditionalFormat().WhenGreaterThan(0).Fill.SetBackgroundColor(XLColor.Celadon);

            worksheetMatrix.Columns().AdjustToContents();
        }

        /// <summary>
        /// Constructes the meta information sheet
        /// </summary>
        /// <param name="workbook">The workbook</param>
        private void ConstructMetaSheet(XLWorkbook workbook)
        {
            var worksheetMeta = workbook.Worksheets.Add("Configuration");
            worksheetMeta.Cell("A1").Value = "Engineering Model";
            worksheetMeta.Cell("B1").Value = $"{this.Iteration?.IterationSetup?.Container?.UserFriendlyName}";

            worksheetMeta.Cell("A2").Value = "Iteration";
            worksheetMeta.Cell("B2").Value = $"{this.Iteration?.IterationSetup?.IterationNumber}";

            worksheetMeta.Cell("A3").Value = "Generated On";
            worksheetMeta.Cell("B3").Value = $"{DateTime.Now}";

            worksheetMeta.Cell("A4").Value = "X Axis ClassKind";
            worksheetMeta.Cell("B4").Value = $"{this.SourceXConfiguration.SelectedClassKind}";

            worksheetMeta.Column(2).AdjustToContents();

            worksheetMeta.Cell("A5").Value = "X Axis Categories";
            worksheetMeta.Cell("B5").Value = $"{this.SourceXConfiguration.CategoriesString}";

            worksheetMeta.Cell("A6").Value = "Y Axis ClassKind";
            worksheetMeta.Cell("B6").Value = $"{this.SourceYConfiguration.SelectedClassKind}";

            worksheetMeta.Cell("A7").Value = "Y Axis Categories";
            worksheetMeta.Cell("B7").Value = $"{this.SourceYConfiguration.CategoriesString}";

            worksheetMeta.Cell("A8").Value = "Relationship Rule";
            worksheetMeta.Cell("B8").Value = $"{this.RelationshipConfiguration.SelectedRule?.UserFriendlyName}";

            worksheetMeta.Column(1).AdjustToContents();
            worksheetMeta.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheetMeta.Column(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            worksheetMeta.Column(2).Style.Alignment.WrapText = true;

            worksheetMeta.Range("A1:A8").Style.Font.Bold = true;
            worksheetMeta.Range("B1:B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }
    }
}
