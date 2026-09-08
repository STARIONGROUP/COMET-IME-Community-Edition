// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExcelExporter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.IO;
    using System.Linq;
    using System.Text;

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
        /// <param name="sourceXConfiguration">The x axis configuration</param>
        /// <param name="sourceYConfiguration">The y axis configuration</param>
        /// <param name="relationshipConfiguration">The relationship configuration</param>
        /// <param name="matrix">The matrix to export</param>
        /// <param name="iteration">The <see cref="Iteration"/> the matrix belongs to</param>
        /// <param name="showDirectionality">A value indicating whether the directionality of relationships should be exported</param>
        public MatrixExcelExporter(SourceConfigurationViewModel sourceXConfiguration, SourceConfigurationViewModel sourceYConfiguration, RelationshipConfigurationViewModel relationshipConfiguration, MatrixViewModel matrix, Iteration iteration, bool showDirectionality)
        {
            this.SourceXConfiguration = sourceXConfiguration ?? throw new ArgumentNullException(nameof(sourceXConfiguration));
            this.SourceYConfiguration = sourceYConfiguration ?? throw new ArgumentNullException(nameof(sourceYConfiguration));
            this.RelationshipConfiguration = relationshipConfiguration ?? throw new ArgumentNullException(nameof(relationshipConfiguration));
            this.Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
            this.Iteration = iteration ?? throw new ArgumentNullException(nameof(iteration));
            this.ShowDirectionality = showDirectionality;
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
        /// Gets a value indicating whether the directionality of relationships is exported.
        /// When <c>false</c>, a neutral marker is used for every relationship, matching the matrix display when directionality is hidden.
        /// </summary>
        public bool ShowDirectionality { get; }

        /// <summary>
        /// Exports the matrix into a excel workbook
        /// </summary>
        /// <param name="path">The path to save the file to.</param>
        public void Export(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (var workbook = new XLWorkbook())
            {
                this.ConstructMatrixSheet(workbook);
                this.ConstructRelationshipListSheet(workbook);
                this.ConstructMetaSheet(workbook);
                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// Constructs the matrix sheet
        /// </summary>
        /// <param name="workbook">The workbook</param>
        private void ConstructMatrixSheet(XLWorkbook workbook)
        {
            var worksheetMatrix = workbook.Worksheets.Add("Matrix");

            var firstCell = worksheetMatrix.Cell("A1");
            var lastCell = worksheetMatrix.Cell(this.Matrix.Records.Count + 1, this.Matrix.Columns.Count+1);

            // construct headers
            for (var i = 1; i <= this.Matrix.Columns.Count; i++)
            {
                worksheetMatrix.Cell(1, i).Value = this.Matrix.Columns[i - 1].Header;
            }

            // construct total header
            worksheetMatrix.Cell(1, this.Matrix.Columns.Count + 1).Value = "Traces";

            worksheetMatrix.Row(1).Style.Alignment.TextRotation = 90;
            worksheetMatrix.Row(1).Style.Font.Bold = true;
            worksheetMatrix.Column(1).Style.Font.Bold = true;
            worksheetMatrix.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheetMatrix.Column(this.Matrix.Columns.Count + 1).Style.Font.Bold = false;
            worksheetMatrix.Column(this.Matrix.Columns.Count + 1).Style.Font.Italic = true;

            // construct corner cell
            var sb = new StringBuilder();

            sb.AppendLine($"                 {this.SourceXConfiguration.SelectedClassKind}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"{this.SourceYConfiguration.SelectedClassKind}");

            // style corner cell
            firstCell.Value = sb.ToString();
            firstCell.Style.Alignment.TextRotation = 0;
            firstCell.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;
            firstCell.Style.Border.DiagonalDown = true;

            // construct rows
            for (var i = 1; i <= this.Matrix.Records.Count; i++)
            {
                worksheetMatrix.Cell(i + 1, 1).Value = this.Matrix.Records[i - 1].First().Value.DisplayKind == DisplayKind.Name ? this.Matrix.Records[i - 1].First().Value.SourceY.QueryDisplayName() : this.Matrix.Records[i - 1].First().Value.SourceY.QueryDisplayShortName();
                var relation = this.Matrix.Records[i - 1].Values.ToList();

                var traceCount = 0;

                for (var j = 1; j < this.Matrix.Records[i - 1].Count; j++)
                {
                    var relationshipDirection = relation[j].RelationshipDirection;

                    if (relationshipDirection != RelationshipDirectionKind.None)
                    {
                        traceCount++;
                        
                        worksheetMatrix.Cell(i + 1, j + 1).Value = this.GetDirectionalityMarker(relationshipDirection);
                    }
                }

                worksheetMatrix.Cell(i + 1, this.Matrix.Records[i - 1].Count + 1).Value = traceCount;
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

            matrixTable.Fields.First().TotalsRowLabel = "Traces";
            matrixTable.Fields.First().TotalsRowFunction = XLTotalsRowFunction.None;

            matrixTable.Rows().Last().Style.Font.Italic = true;
            matrixTable.Rows().Last().Style.Font.Bold = false;

            worksheetMatrix.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // color in trace summaries
            matrixTable.Rows().Last().AddConditionalFormat().WhenEquals("Traces").Fill.SetBackgroundColor(XLColor.NoColor);

            var lastRowFirstTraceCell = worksheetMatrix.Cell(matrixTable.Rows().Last().RowNumber(), 2);
            var lastRowLastTraceCell = worksheetMatrix.Cell(matrixTable.Rows().Last().RowNumber(), this.Matrix.Columns.Count);
            var rowTraceRange = worksheetMatrix.Range(lastRowFirstTraceCell, lastRowLastTraceCell);

            rowTraceRange.AddConditionalFormat().WhenEquals(0).Fill.SetBackgroundColor(XLColor.MistyRose);
            rowTraceRange.AddConditionalFormat().WhenGreaterThan(0).Fill.SetBackgroundColor(XLColor.Celadon);

            var lastColumnFirstTraceCell = worksheetMatrix.Cell(2, this.Matrix.Columns.Count + 1);
            var lastColumnLastTraceCell = worksheetMatrix.Cell(matrixTable.Rows().Last().RowNumber() - 1, this.Matrix.Columns.Count + 1);
            var columnTraceRange = worksheetMatrix.Range(lastColumnFirstTraceCell, lastColumnLastTraceCell);

            columnTraceRange.AddConditionalFormat().WhenEquals(0).Fill.SetBackgroundColor(XLColor.MistyRose);
            columnTraceRange.AddConditionalFormat().WhenGreaterThan(0).Fill.SetBackgroundColor(XLColor.Celadon);

            // remove last cell totals
            worksheetMatrix.Cell(matrixTable.Rows().Last().RowNumber(), this.Matrix.Columns.Count + 1).FormulaA1 = string.Empty;

            // adjust width
            worksheetMatrix.Columns().AdjustToContents();

            // set tab color
            worksheetMatrix.SetTabColor(XLColor.DarkSeaGreen);
        }

        /// <summary>
        /// Constructs the sheet that lists, in a tabular form, every <see cref="BinaryRelationship"/> shown in the matrix
        /// together with its attributes (source, relationship name, target, categories and owner).
        /// </summary>
        /// <param name="workbook">The workbook</param>
        private void ConstructRelationshipListSheet(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Relationships");

            worksheet.Cell(1, 1).Value = "Source";
            worksheet.Cell(1, 2).Value = "Relationship";
            worksheet.Cell(1, 3).Value = "Target";
            worksheet.Cell(1, 4).Value = "Categories";
            worksheet.Cell(1, 5).Value = "Owner";

            worksheet.Row(1).Style.Font.Bold = true;
            
            var relationships = this.Matrix.Records
                .SelectMany(record => record.Values)
                .SelectMany(cell => cell.Relationships)
                .GroupBy(relationship => relationship.Iid)
                .Select(group => group.First())
                .OrderBy(relationship => relationship.Source?.UserFriendlyName)
                .ThenBy(relationship => relationship.Target?.UserFriendlyName)
                .ToList();

            var forwardRelationshipName = this.RelationshipConfiguration.SelectedRule?.ForwardRelationshipName ?? string.Empty;

            for (var i = 0; i < relationships.Count; i++)
            {
                var relationship = relationships[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = relationship.Source?.UserFriendlyName ?? string.Empty;
                worksheet.Cell(row, 2).Value = forwardRelationshipName;
                worksheet.Cell(row, 3).Value = relationship.Target?.UserFriendlyName ?? string.Empty;
                worksheet.Cell(row, 4).Value = string.Join(", ", relationship.Category.Select(category => category.Name));
                worksheet.Cell(row, 5).Value = relationship.Owner?.Name ?? string.Empty;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SetTabColor(XLColor.LightSteelBlue);
        }

        /// <summary>
        /// Gets the textual marker that represents the directionality of a relationship for an exported cell
        /// </summary>
        /// <param name="relationshipDirection">The <see cref="RelationshipDirectionKind"/> asserted on the cell</param>
        /// <returns>
        /// An arrow denoting the direction of the relationship between the row and column <see cref="Thing"/> when
        /// <see cref="ShowDirectionality"/> is <c>true</c>; a neutral "X" marker when directionality is hidden;
        /// or an empty string when no relationship exists
        /// </returns>
        private string GetDirectionalityMarker(RelationshipDirectionKind relationshipDirection)
        {
            if (relationshipDirection == RelationshipDirectionKind.None)
            {
                return string.Empty;
            }

            if (!this.ShowDirectionality)
            {
                return "X";
            }

            switch (relationshipDirection)
            {
                case RelationshipDirectionKind.RowThingToColumnThing:
                    return "↑";
                case RelationshipDirectionKind.ColumnThingToRowThing:
                    return "←";
                default:
                    return "↔";
            }
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
