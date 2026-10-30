// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetBuilderTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4CrossViewEditor.Tests.Generator
{
    using System.Linq;
    using System.Text.RegularExpressions;

    using CDP4CrossViewEditor.Generator;

    using NUnit.Framework;

    [TestFixture]
    public class CalculationSheetBuilderTestFixture
    {
        [Test]
        public void VerifyBuild()
        {
            var layout = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass);

            Assert.That(layout.GeneratedColumnCount, Is.EqualTo(3));
            Assert.That(layout.GeneratedRowCount, Is.EqualTo(5));
            Assert.That(layout.Cells[0, 0], Is.EqualTo("Name"));
            Assert.That(layout.Cells[0, 1], Is.EqualTo("Model Code"));
            Assert.That(layout.Cells[0, 2], Is.EqualTo("mass [kg]"));
            Assert.That(layout.Cells[3, 1], Is.EqualTo("Bat.cell_1"));

            // The Crossview sheet has 6 rows of workbook header and 6 rows of column header, so its body starts at row 13, and
            // its 5 fixed columns put the first parameter in column 6. Body row 2 (cell_1) is therefore Crossview!R15C6. This is
            // the check whose absence once let a calculation sheet ship that referred to nothing at all.
            Assert.That(CalculationSheetBuilder.CrossviewBodyStartRow, Is.EqualTo(13));
            Assert.That(layout.Cells[3, 2], Is.EqualTo("=IFERROR(VALUE('Crossview'!R15C6),'Crossview'!R15C6)"));
            Assert.That(layout.Cells[5, 2], Is.EqualTo("=IFERROR(VALUE('Crossview'!R17C6),'Crossview'!R17C6)"));

            // an element without a value of its own gets no reference, which would otherwise read as a zero
            Assert.That(layout.Cells[1, 2], Is.Null);
            Assert.That(layout.Cells[2, 2], Is.Null);

            Assert.That(layout.ModelValues[(3, 2)], Is.EqualTo("1"));
            Assert.That(layout.ModelValues.ContainsKey((2, 2)), Is.False);
            Assert.That(layout.SubtotalCells, Is.Empty);
        }

        [Test]
        public void VerifyBuildWithSubtotals()
        {
            var layout = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true);

            // the battery sums its two cells, which sit next to each other
            Assert.That(layout.Cells[2, 2], Is.EqualTo("=SUM(R[1]C:R[2]C)"));

            // the satellite sums its direct children, the battery subtotal and the panel, and not the cells again
            Assert.That(layout.Cells[1, 2], Is.EqualTo("=SUM(R[1]C,R[4]C)"));

            Assert.That(layout.SubtotalCells, Is.EquivalentTo(new[] { (1, 2), (2, 2) }));

            // a value of the model is never replaced by a subtotal
            Assert.That(layout.Cells[5, 2], Is.EqualTo("=IFERROR(VALUE('Crossview'!R17C6),'Crossview'!R17C6)"));
        }

        [Test]
        public void VerifyBuildSumsTheChosenColumnsOnly()
        {
            var layout = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower, summedColumns: new[] { 1 });

            Assert.That(layout.Cells[2, 2], Is.Null, "mass is not summed");
            Assert.That(layout.Cells[2, 3], Is.EqualTo("=SUM(R[1]C:R[2]C)"), "power is summed");
            Assert.That(layout.SubtotalColumnKeys, Is.EqualTo(new[] { "power [W]" }));

            Assert.That(CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower).SubtotalColumnKeys, Is.Empty);
        }

        [Test]
        public void VerifyBuildDoesNotSumText()
        {
            var rows = CalculationSheetTestModel.Satellite();
            rows[2] = (2, "Bat.cell_1", new[] { "on", null });
            rows[3] = (2, "Bat.cell_2", new[] { "off", null });
            rows[4] = (1, "Sat.pan_a", new[] { "-", null });

            var layout = CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass, true);

            Assert.That(layout.SubtotalCells, Is.Empty, "a column of text would get a misleading zero on every parent row");

            rows[2] = (2, "Bat.cell_1", new[] { "-", null });
            rows[3] = (2, "Bat.cell_2", new[] { "-", null });

            Assert.That(CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass, true).SubtotalCells, Is.Empty, "a column without any number has nothing to sum");
        }

        [Test]
        public void VerifyBuildOfPartOfTheCrossviewSheet()
        {
            // the sheet of an option shows Sat and pan_a only, and the power column only
            var layout = CalculationSheetTestModel.Build(
                CalculationSheetTestModel.Satellite(),
                CalculationSheetTestModel.MassAndPower,
                crossviewRows: new[] { 0, 4 },
                crossviewColumns: new[] { 1 });

            Assert.That(layout.Cells.GetLength(0), Is.EqualTo(3));
            Assert.That(layout.Cells.GetLength(1), Is.EqualTo(3));
            Assert.That(layout.Cells[0, 2], Is.EqualTo("power [W]"));
            Assert.That(layout.Cells[2, 1], Is.EqualTo("Sat.pan_a"));

            // the reference points at the row and column of pan_a's power on the Crossview sheet, not at the position on this sheet
            Assert.That(layout.Cells[2, 2], Is.EqualTo("=IFERROR(VALUE('Crossview'!R17C7),'Crossview'!R17C7)"));
            Assert.That(layout.Tree.Keys, Is.EqualTo(new[] { "Sat", "Sat/pan_a" }));

            // the structure of the whole Crossview sheet is kept, so that references into it can follow their target
            Assert.That(layout.Crossview.Tree.Count, Is.EqualTo(5));
            Assert.That(layout.Crossview.ColumnKeys, Is.EqualTo(new[] { "mass [kg]", "power [W]" }));
        }

        [Test]
        public void VerifyBuildNamesTheValueCells()
        {
            var layout = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower);

            // the path through the tree and the parameter, which stays the same when rows and columns move
            Assert.That(layout.CellNames[(2, 2)], Is.EqualTo("Sat.bat_a.mass"));
            Assert.That(layout.CellNames[(5, 3)], Is.EqualTo("Sat.pan_a.power"));
            Assert.That(layout.CellNames.Keys.Any(cell => cell.Column < CalculationSheetBuilder.FixedColumns), Is.False, "the name and model code columns are not values");

            // two columns of the same parameter in different scales would share a name, so they get their scale added
            var scales = new[] { ("mass", "kg"), ("mass", "g") };
            var clashing = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), scales);

            Assert.That(clashing.CellNames[(5, 2)], Is.EqualTo("Sat.pan_a.mass.kg"));
            Assert.That(clashing.CellNames[(5, 3)], Is.EqualTo("Sat.pan_a.mass.g"));

            // on the sheet over every option, the columns of one parameter differ by their option only
            var content = CalculationSheetTestModel.Content(CalculationSheetTestModel.Satellite(), new[] { ("mass", "kg"), ("mass", "kg"), ("mass", "kg") });
            content[CrossviewSheetConstants.OptionHeaderRow, CrossviewSheetConstants.FixedColumns] = "opt_a";
            content[CrossviewSheetConstants.OptionHeaderRow, CrossviewSheetConstants.FixedColumns + 1] = "opt_b";

            var names = Enumerable.Range(0, 5).Select(row => new string(' ', 3 * new[] { 0, 1, 2, 2, 1 }[row]) + CalculationSheetTestModel.Satellite()[row].ModelCode).ToList();
            var modelCodes = CalculationSheetTestModel.Satellite().Select(row => row.ModelCode).ToList();
            var combined = CalculationSheetBuilder.Build(names, modelCodes, content, null);

            Assert.That(combined.CellNames[(5, 2)], Is.EqualTo("Sat.pan_a.mass.opt_a"));
            Assert.That(combined.CellNames[(5, 3)], Is.EqualTo("Sat.pan_a.mass.opt_b"));
            Assert.That(combined.CellNames[(5, 4)], Is.Not.EqualTo(combined.CellNames[(5, 2)]), "a name never points at two cells");

            // what Excel does not accept in a name is replaced
            Assert.That(CalculationSheetBuilder.ToName("x/y [1]-z"), Is.EqualTo("x_y__1__z"));
        }

        [Test]
        public void VerifyUniqueKeysAndModelCodeFromShortName()
        {
            Assert.That(CrossviewStructure.UniqueKeys(new[] { "mass", "power", "mass", "mass" }), Is.EqualTo(new[] { "mass", "power", "mass#2", "mass#3" }));
            Assert.That(CrossviewStructure.UniqueKeys(new string[] { null }), Is.EqualTo(new[] { string.Empty }));

            Assert.That(CrossviewStructure.ModelCodeFromShortName("   Bat : cell_1"), Is.EqualTo("Bat.cell_1"));
            Assert.That(CrossviewStructure.ModelCodeFromShortName("Sat"), Is.EqualTo("Sat"));
            Assert.That(CrossviewStructure.ModelCodeFromShortName(null), Is.Empty);
        }

        [Test]
        public void VerifyBuildSubtotalFormula()
        {
            Assert.That(CalculationSheetBuilder.BuildSubtotalFormula(1, new[] { 2, 3, 4, 7 }), Is.EqualTo("=SUM(R[1]C:R[3]C,R[6]C)"));
            Assert.That(CalculationSheetBuilder.BuildSubtotalFormula(5, new[] { 6 }), Is.EqualTo("=SUM(R[1]C)"));

            // Excel takes at most 255 arguments, so a parent with more scattered children is summed in groups
            var many = CalculationSheetBuilder.BuildSubtotalFormula(1, Enumerable.Range(1, 300).Select(index => index * 2));

            Assert.That(many, Does.StartWith("=SUM(SUM("));
            Assert.That(Regex.Matches(many, @"SUM\(").Count, Is.EqualTo(3));
        }
    }
}
