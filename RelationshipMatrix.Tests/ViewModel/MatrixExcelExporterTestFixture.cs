// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExcelExporterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4RelationshipMatrix.Helpers;
    using CDP4RelationshipMatrix.Settings;
    using CDP4RelationshipMatrix.ViewModels;

    using ClosedXML.Excel;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="MatrixExcelExporter"/> class.
    /// </summary>
    [TestFixture]
    public class MatrixExcelExporterTestFixture : ViewModelTestBase
    {
        /// <summary>
        /// The arrow that denotes a relationship from the row <see cref="Thing"/> to the column <see cref="Thing"/>
        /// </summary>
        private const string RowToColumnMarker = "↑";

        /// <summary>
        /// The arrow that denotes a relationship from the column <see cref="Thing"/> to the row <see cref="Thing"/>
        /// </summary>
        private const string ColumnToRowMarker = "←";

        /// <summary>
        /// The arrow that denotes a bi-directional relationship
        /// </summary>
        private const string BiDirectionalMarker = "↔";

        /// <summary>
        /// The neutral marker that is used for any relationship when directionality is hidden
        /// </summary>
        private const string NeutralMarker = "X";

        private RelationshipMatrixViewModel viewModel;
        private string exportPath;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.viewModel = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            this.viewModel.SourceYConfiguration.SelectedClassKind = ClassKind.ElementDefinition;
            this.viewModel.SourceXConfiguration.SelectedClassKind = ClassKind.ElementDefinition;

            this.exportPath = Path.Combine(Path.GetTempPath(), $"relationshipmatrix_{Guid.NewGuid():N}.xlsx");
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();

            if (System.IO.File.Exists(this.exportPath))
            {
                System.IO.File.Delete(this.exportPath);
            }
        }

        [Test]
        public void VerifyThatExportMaintainsRelationshipDirectionalityWhenDirectionalityIsShown()
        {
            var matrix = this.BuildSingleRowMatrix();

            var exporter = new MatrixExcelExporter(
                this.viewModel.SourceXConfiguration,
                this.viewModel.SourceYConfiguration,
                this.viewModel.RelationshipConfiguration,
                matrix,
                this.iteration,
                true);

            Assert.DoesNotThrow(() => exporter.Export(this.exportPath));

            using (var workbook = new XLWorkbook(this.exportPath))
            {
                var worksheet = workbook.Worksheet("Matrix");

                Assert.That(worksheet.Cell(2, 2).GetString(), Is.EqualTo(RowToColumnMarker));
                Assert.That(worksheet.Cell(2, 3).GetString(), Is.EqualTo(ColumnToRowMarker));
                Assert.That(worksheet.Cell(2, 4).GetString(), Is.EqualTo(BiDirectionalMarker));

                // a cell without a relationship is left blank (not an empty string) so it is not counted in the column totals
                Assert.That(worksheet.Cell(2, 5).Value.IsBlank, Is.True);

                // the trace count still reflects every cell that holds a relationship, regardless of its direction
                Assert.That(worksheet.Cell(2, 6).GetValue<int>(), Is.EqualTo(3));

                // the bottom totals row counts only the cells that actually hold a relationship in each column
                Assert.That(worksheet.Cell(3, 2).GetValue<int>(), Is.EqualTo(1));
                Assert.That(worksheet.Cell(3, 3).GetValue<int>(), Is.EqualTo(1));
                Assert.That(worksheet.Cell(3, 4).GetValue<int>(), Is.EqualTo(1));
                Assert.That(worksheet.Cell(3, 5).GetValue<int>(), Is.EqualTo(0));
            }
        }

        [Test]
        public void VerifyThatExportUsesNeutralMarkerWhenDirectionalityIsHidden()
        {
            var matrix = this.BuildSingleRowMatrix();

            var exporter = new MatrixExcelExporter(
                this.viewModel.SourceXConfiguration,
                this.viewModel.SourceYConfiguration,
                this.viewModel.RelationshipConfiguration,
                matrix,
                this.iteration,
                false);

            Assert.DoesNotThrow(() => exporter.Export(this.exportPath));

            using (var workbook = new XLWorkbook(this.exportPath))
            {
                var worksheet = workbook.Worksheet("Matrix");

                // every relationship is marked neutrally, irrespective of its direction
                Assert.That(worksheet.Cell(2, 2).GetString(), Is.EqualTo(NeutralMarker));
                Assert.That(worksheet.Cell(2, 3).GetString(), Is.EqualTo(NeutralMarker));
                Assert.That(worksheet.Cell(2, 4).GetString(), Is.EqualTo(NeutralMarker));
                Assert.That(worksheet.Cell(2, 5).Value.IsBlank, Is.True);

                Assert.That(worksheet.Cell(2, 6).GetValue<int>(), Is.EqualTo(3));
            }
        }

        [Test]
        public void VerifyThatExportWritesRelationshipsListingSheet()
        {
            this.rule.ForwardRelationshipName = "traces";
            this.viewModel.RelationshipConfiguration.SelectedRule = this.rule;

            var matrix = this.BuildSingleRowMatrix();

            var exporter = new MatrixExcelExporter(
                this.viewModel.SourceXConfiguration,
                this.viewModel.SourceYConfiguration,
                this.viewModel.RelationshipConfiguration,
                matrix,
                this.iteration,
                true);

            Assert.DoesNotThrow(() => exporter.Export(this.exportPath));

            using (var workbook = new XLWorkbook(this.exportPath))
            {
                var worksheet = workbook.Worksheet("Relationships");

                // header row
                Assert.That(worksheet.Cell(1, 1).GetString(), Is.EqualTo("Source"));
                Assert.That(worksheet.Cell(1, 2).GetString(), Is.EqualTo("Relationship"));
                Assert.That(worksheet.Cell(1, 3).GetString(), Is.EqualTo("Target"));
                Assert.That(worksheet.Cell(1, 4).GetString(), Is.EqualTo("Categories"));
                Assert.That(worksheet.Cell(1, 5).GetString(), Is.EqualTo("Owner"));

                var dataRows = worksheet.RowsUsed().Skip(1).ToList();

                // the four distinct relationships shown in the matrix are listed
                Assert.That(dataRows.Count, Is.EqualTo(4));

                var exportedPairs = dataRows
                    .Select(row => (Source: row.Cell(1).GetString(), Target: row.Cell(3).GetString()))
                    .ToList();

                Assert.That(exportedPairs, Does.Contain((this.elementDef11.UserFriendlyName, this.elementDef12.UserFriendlyName)));
                Assert.That(exportedPairs, Does.Contain((this.elementDef21.UserFriendlyName, this.elementDef11.UserFriendlyName)));
                Assert.That(exportedPairs, Does.Contain((this.elementDef11.UserFriendlyName, this.elementDef22.UserFriendlyName)));
                Assert.That(exportedPairs, Does.Contain((this.elementDef22.UserFriendlyName, this.elementDef11.UserFriendlyName)));

                // every listed relationship carries the rule forward name, its categories and its owner
                foreach (var row in dataRows)
                {
                    Assert.That(row.Cell(2).GetString(), Is.EqualTo("traces"));
                    Assert.That(row.Cell(4).GetString(), Is.EqualTo(this.catRel.Name));
                    Assert.That(row.Cell(5).GetString(), Is.EqualTo(this.domain.Name));
                }
            }
        }

        /// <summary>
        /// Builds a single-row matrix that contains one cell of each <see cref="RelationshipDirectionKind"/>
        /// </summary>
        /// <returns>The populated <see cref="MatrixViewModel"/></returns>
        private MatrixViewModel BuildSingleRowMatrix()
        {
            var matrix = this.viewModel.Matrix;

            matrix.Records.Clear();
            matrix.Columns.Clear();

            matrix.Columns.Add(new ColumnDefinition(MatrixViewModel.CDP4_NAME_HEADER, MatrixViewModel.ROW_NAME_COLUMN, true));
            matrix.Columns.Add(new ColumnDefinition(this.elementDef12, DisplayKind.ShortName));
            matrix.Columns.Add(new ColumnDefinition(this.elementDef21, DisplayKind.ShortName));
            matrix.Columns.Add(new ColumnDefinition(this.elementDef22, DisplayKind.ShortName));
            matrix.Columns.Add(new ColumnDefinition(this.elementDef31, DisplayKind.ShortName));

            // relationship from the row Thing (ed11) to the column Thing (ed12)
            var rowToColumn = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = this.elementDef11, Target = this.elementDef12, Owner = this.domain };
            rowToColumn.Category.Add(this.catRel);

            // relationship from the column Thing (ed21) to the row Thing (ed11)
            var columnToRow = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = this.elementDef21, Target = this.elementDef11, Owner = this.domain };
            columnToRow.Category.Add(this.catRel);

            // bi-directional relationship between the row Thing (ed11) and the column Thing (ed22)
            var bidirectionalForward = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = this.elementDef11, Target = this.elementDef22, Owner = this.domain };
            bidirectionalForward.Category.Add(this.catRel);

            var bidirectionalReverse = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = this.elementDef22, Target = this.elementDef11, Owner = this.domain };
            bidirectionalReverse.Category.Add(this.catRel);

            var record = new Dictionary<string, MatrixCellViewModel>
            {
                { MatrixViewModel.ROW_NAME_COLUMN, new MatrixCellViewModel(this.elementDef11, null, null, this.rule, DisplayKind.ShortName) },
                { this.elementDef12.ShortName, new MatrixCellViewModel(this.elementDef11, this.elementDef12, new[] { rowToColumn }, this.rule, DisplayKind.ShortName) },
                { this.elementDef21.ShortName, new MatrixCellViewModel(this.elementDef11, this.elementDef21, new[] { columnToRow }, this.rule, DisplayKind.ShortName) },
                { this.elementDef22.ShortName, new MatrixCellViewModel(this.elementDef11, this.elementDef22, new[] { bidirectionalForward, bidirectionalReverse }, this.rule, DisplayKind.ShortName) },
                { this.elementDef31.ShortName, new MatrixCellViewModel(this.elementDef11, this.elementDef31, null, this.rule, DisplayKind.ShortName) }
            };

            matrix.Records.Add(record);

            return matrix;
        }
    }
}
