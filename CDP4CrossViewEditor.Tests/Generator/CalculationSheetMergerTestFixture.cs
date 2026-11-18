// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetMergerTestFixture.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4CrossViewEditor.Generator;

    using NUnit.Framework;

    [TestFixture]
    public class CalculationSheetMergerTestFixture
    {
        // rows of the satellite layout: header 0, Sat 1, bat_a 2, cell_1 3, cell_2 4, pan_a 5
        // columns with mass only: Name 0, Model Code 1, mass 2, then the user's own columns from 3
        private const int MassColumn = 2;

        private const int UserColumn = 3;

        private const string SumOfBatteryCells = "=SUM(R[1]C:R[2]C)";

        private CalculationSheetLayout previous;

        private object[,] sheet;

        [SetUp]
        public void SetUp()
        {
            this.previous = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass);
            this.sheet = CalculationSheetTestModel.AsSheet(this.previous, userColumns: 1, userRows: 1);

            // the user sums the battery cells by hand, adds a column of their own, and a grand total below the tree
            this.sheet[2, MassColumn] = SumOfBatteryCells;
            this.sheet[0, UserColumn] = "with margin";
            this.sheet[3, UserColumn] = "=RC[-1]*1.2";
            this.sheet[4, UserColumn] = "=RC[-1]*1.2";
            this.sheet[6, 0] = "Total";
            this.sheet[6, MassColumn] = "=SUM(R[-5]C:R[-1]C)";
        }

        [Test]
        public void VerifyMergeWithoutPreviousSheet()
        {
            var result = CalculationSheetMerger.Merge(null, this.previous);

            Assert.That(result.Cells, Is.EqualTo(this.previous.Cells));
            Assert.That(result.Flags, Is.Empty);
            Assert.That(result.Dropped, Is.Empty);
            Assert.That(result.PreservedCellCount, Is.Zero);
        }

        [Test]
        public void VerifyMergeOfAnUnchangedTree()
        {
            var current = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass);
            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), current);

            // everything of the user is back where it was, and nothing is flagged
            Assert.That(result.Cells[2, MassColumn], Is.EqualTo(SumOfBatteryCells));
            Assert.That(result.Cells[0, UserColumn], Is.EqualTo("with margin"));
            Assert.That(result.Cells[3, UserColumn], Is.EqualTo("=RC[-1]*1.2"));
            Assert.That(result.Cells[6, 0], Is.EqualTo("Total"));
            Assert.That(result.Cells[6, MassColumn], Is.EqualTo("=SUM(R[-5]C:R[-1]C)"));
            Assert.That(result.Flags, Is.Empty);
            Assert.That(result.Dropped, Is.Empty);
            Assert.That(result.PreservedCellCount, Is.EqualTo(6));

            // the generated values of the model are regenerated, not carried over as if they were the user's
            Assert.That(result.Cells[3, MassColumn], Is.EqualTo(current.Cells[3, MassColumn]));
        }

        [Test]
        public void VerifyMergeWhenAChildIsInsertedInsideTheRange()
        {
            var rows = CalculationSheetTestModel.Satellite();
            rows.Insert(3, (2, "Bat.cell_x", new[] { "7", null }));

            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass));

            // like Excel when a row is inserted inside a range, the sum grows to take the new cell in
            Assert.That(result.Cells[2, MassColumn], Is.EqualTo("=SUM(R[1]C:R[3]C)"));

            // and because the meaning of the sum changed, it is flagged
            var flag = result.Flags.Single(candidate => candidate.Row == 2 && candidate.Column == MassColumn);
            Assert.That(flag.Messages.Single(), Does.StartWith("This range covered 2 row(s) of the product tree and now covers 3."));

            // a formula on a single row moves with its row, and is not flagged
            Assert.That(result.Cells[5, UserColumn], Is.EqualTo("=RC[-1]*1.2"), "cell_2 moved from row 4 to row 5");
            Assert.That(result.Flags.Any(candidate => candidate.Column == UserColumn), Is.False);

            // the grand total below the tree moved down with it and now covers the extra row
            Assert.That(result.Cells[7, MassColumn], Is.EqualTo("=SUM(R[-6]C:R[-1]C)"));
        }

        [Test]
        public void VerifyMergeWhenAChildIsAddedAfterTheRange()
        {
            var rows = CalculationSheetTestModel.Satellite();
            rows.Insert(4, (2, "Bat.cell_3", new[] { "3", null }));

            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass));

            // like Excel when a row is added after a range, the sum does not grow: every row it covered is still there
            Assert.That(result.Cells[2, MassColumn], Is.EqualTo(SumOfBatteryCells));

            // it now misses the new cell, which is exactly the case that has to be flagged
            var flag = result.Flags.Single(candidate => candidate.Row == 2 && candidate.Column == MassColumn);
            Assert.That(flag.Messages.Single(), Does.StartWith("The rows under this element changed in this rebuild (2 before, 3 now)"));
        }

        [Test]
        public void VerifyMergeWhenAnElementIsRemoved()
        {
            this.sheet[3, UserColumn] = "=R[2]C[-1]*2";
            this.sheet[5, UserColumn] = "panel note";

            var rows = CalculationSheetTestModel.Satellite();
            rows.RemoveAt(4);

            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass));

            // a reference to the removed panel is broken, as Excel would break it, and flagged
            Assert.That(result.Cells[3, UserColumn], Is.EqualTo("=#REF!*2"));
            Assert.That(result.Flags.Single(candidate => candidate.Row == 3 && candidate.Column == UserColumn).Messages, Does.Contain(CalculationSheetMerger.BrokenReferenceMessage));

            // what the user wrote on the removed row has nowhere to go, and is reported rather than silently lost
            Assert.That(result.Dropped, Has.Count.EqualTo(1));
            Assert.That(result.Dropped[0], Does.Contain("panel note"));
        }

        [Test]
        public void VerifyMergeOfAnEntryOverAValueOfTheModel()
        {
            this.sheet[3, MassColumn] = "9";
            this.sheet[4, MassColumn] = "2";

            var current = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass);
            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), current);

            // the model is the source of truth for its own values
            Assert.That(result.Cells[3, MassColumn], Is.EqualTo(current.Cells[3, MassColumn]));
            Assert.That(result.Cells[4, MassColumn], Is.EqualTo(current.Cells[4, MassColumn]));

            // a different value is flagged with what the user had typed, a pasted copy of the same value is no news
            Assert.That(result.Flags.Single(candidate => candidate.Row == 3 && candidate.Column == MassColumn).Messages.Single(), Does.Contain("9"));
            Assert.That(result.Flags.Any(candidate => candidate.Row == 4 && candidate.Column == MassColumn), Is.False);
        }

        [Test]
        public void VerifyMergeOfSubtotals()
        {
            // a hand written sum that reads exactly like a generated subtotal is the user's when no subtotals were generated
            var keptWhenNeverGenerated = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous, hadSubtotals: false),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            Assert.That(keptWhenNeverGenerated.Cells[2, MassColumn], Is.EqualTo(SumOfBatteryCells));

            // a subtotal the add-in generated goes away when subtotals are switched off
            var withSubtotals = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true);
            var generatedSheet = CalculationSheetTestModel.AsSheet(withSubtotals);

            var switchedOff = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(generatedSheet, withSubtotals, hadSubtotals: true),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            Assert.That(switchedOff.Cells[1, MassColumn], Is.Null);
            Assert.That(switchedOff.Cells[2, MassColumn], Is.Null);
            Assert.That(switchedOff.PreservedCellCount, Is.Zero);

            // a generated subtotal the user changed has become theirs
            generatedSheet[2, MassColumn] = "=SUM(R[1]C:R[2]C)*1.1";

            var changed = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(generatedSheet, withSubtotals, hadSubtotals: true),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            Assert.That(changed.Cells[2, MassColumn], Is.EqualTo("=SUM(R[1]C:R[2]C)*1.1"));

            // with subtotals switched on, a sum of the user wins over the generated one in the same cell
            this.sheet[2, MassColumn] = "=SUM(R[1]C:R[2]C)*0.9";

            var userWins = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true));

            Assert.That(userWins.Cells[2, MassColumn], Is.EqualTo("=SUM(R[1]C:R[2]C)*0.9"));
            Assert.That(userWins.SubtotalCells.Contains((2, MassColumn)), Is.False);
            Assert.That(userWins.SubtotalCells.Contains((1, MassColumn)), Is.True, "the satellite still gets its generated subtotal");
        }

        [Test]
        public void VerifyMergeOfSubtotalsPerColumn()
        {
            // mass is summed, power is not; the user sums the battery cells of power by hand, exactly as a subtotal would read
            var massSummed = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower, summedColumns: new[] { 0 });
            Assert.That(massSummed.SubtotalColumnKeys, Is.EqualTo(new[] { "mass [kg]" }));

            var generatedSheet = CalculationSheetTestModel.AsSheet(massSummed);
            generatedSheet[2, 3] = SumOfBatteryCells;

            // summing is switched off for mass
            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(generatedSheet, massSummed, hadSubtotals: true),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower));

            Assert.That(result.Cells[2, 2], Is.Null, "the generated subtotal of mass goes away");
            Assert.That(result.Cells[2, 3], Is.EqualTo(SumOfBatteryCells), "the sum of the user in a column that was not summed is kept");
        }

        [Test]
        public void VerifyMergeOfABrokenFormulaOnARowThatIsSummed()
        {
            var withSubtotals = CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true);
            var brokenSheet = CalculationSheetTestModel.AsSheet(withSubtotals);

            // an earlier rebuild left the sum on the satellite broken
            brokenSheet[1, MassColumn] = "=SUM(#REF!,R[4]C)";

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(brokenSheet, withSubtotals, hadSubtotals: true),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true));

            // a formula that only shows #REF! does not win over the sum the add-in can put back
            Assert.That(result.Cells[1, MassColumn], Is.EqualTo("=SUM(R[1]C,R[4]C)"));
            Assert.That(result.SubtotalCells.Contains((1, MassColumn)), Is.True);

            // and the formula that was there is not lost without a word
            var flag = result.Flags.Single(candidate => candidate.Row == 1 && candidate.Column == MassColumn);
            Assert.That(flag.Messages.Single(), Does.Contain("=SUM(#REF!,R[4]C)"));

            // a sum of the user that works still wins, as before
            brokenSheet[1, MassColumn] = "=SUM(R[1]C,R[4]C)*1.1";

            var working = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(brokenSheet, withSubtotals, hadSubtotals: true),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass, true));

            Assert.That(working.Cells[1, MassColumn], Is.EqualTo("=SUM(R[1]C,R[4]C)*1.1"));
        }

        [Test]
        public void VerifyMergeWhenAParameterColumnIsAdded()
        {
            // power is selected and lands after mass, pushing the user's column one to the right but leaving mass where it was
            var afterMass = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.MassAndPower));

            Assert.That(afterMass.Cells[0, 4], Is.EqualTo("with margin"));

            // the formula still multiplies the mass, now two columns to its left; copied as it was it would multiply the power
            Assert.That(afterMass.Cells[3, 4], Is.EqualTo("=RC[-2]*1.2"));
            Assert.That(afterMass.Cells[2, MassColumn], Is.EqualTo(SumOfBatteryCells));

            // power lands before mass instead, so mass and the user's column move together and the formula is unchanged
            var beforeMass = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.PowerAndMass));

            Assert.That(beforeMass.Cells[3, 4], Is.EqualTo("=RC[-1]*1.2"));
            Assert.That(beforeMass.Cells[2, 3], Is.EqualTo(SumOfBatteryCells), "the hand written sum follows the mass column");
        }

        [Test]
        public void VerifyMergeCarriesFlagsUntilTheUserRemovesThem()
        {
            var comments = new Dictionary<(int Row, int Column), string>
            {
                [(3, UserColumn)] = CalculationSheetMerger.FlagPrefix + "Check this.",
                [(4, UserColumn)] = "A comment of the user, not a flag",
            };

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous, flagComments: comments),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            Assert.That(result.Flags.Single().Messages.Single(), Is.EqualTo("Check this."));
            Assert.That(result.Flags.Single().Row, Is.EqualTo(3));
        }

        [Test]
        public void VerifyMergeWhenTheUserInsertedARowIntoTheProductTree()
        {
            // the user inserts a blank row between cell_1 and cell_2, which pushes pan_a and the total down
            var shifted = new object[this.sheet.GetLength(0) + 1, this.sheet.GetLength(1)];

            for (var row = 0; row < this.sheet.GetLength(0); row++)
            {
                for (var column = 0; column < this.sheet.GetLength(1); column++)
                {
                    shifted[row < 4 ? row : row + 1, column] = this.sheet[row, column];
                }
            }

            // Excel widens a range the insertion falls inside, and moves the one below it along, so the sheet comes back with
            // the formulas it adjusted rather than the ones that were written
            shifted[2, MassColumn] = "=SUM(R[1]C:R[3]C)";
            shifted[7, MassColumn] = "=SUM(R[-6]C:R[-1]C)";

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(shifted, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            // the rows below the inserted one are still recognised, so nothing of the user is lost
            Assert.That(result.Dropped, Is.Empty);
            Assert.That(result.Cells[2, MassColumn], Is.EqualTo(SumOfBatteryCells), "the hand written sum stays on the battery");
            Assert.That(result.Cells[4, UserColumn], Is.EqualTo("=RC[-1]*1.2"), "the formula of cell_2 follows cell_2");
            Assert.That(result.Cells[6, 0], Is.EqualTo("Total"), "the row of the user below the tree keeps its place");
            Assert.That(result.Cells[6, MassColumn], Is.EqualTo("=SUM(R[-5]C:R[-1]C)"));
        }

        [Test]
        public void VerifyMergeWhenTheUserDeletedARowOfTheProductTree()
        {
            // the user deletes the pan_a row, which pulls their total up into the generated block
            var shortened = new object[this.sheet.GetLength(0) - 1, this.sheet.GetLength(1)];

            for (var row = 0; row < shortened.GetLength(0); row++)
            {
                for (var column = 0; column < shortened.GetLength(1); column++)
                {
                    shortened[row, column] = this.sheet[row < 5 ? row : row + 1, column];
                }
            }

            // Excel narrows the range of the total to the rows that are left
            shortened[5, MassColumn] = "=SUM(R[-4]C:R[-1]C)";

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(shortened, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            // the total is the user's, and is not mistaken for the generated row it now sits on
            Assert.That(result.Cells[6, 0], Is.EqualTo("Total"));
            Assert.That(result.Cells[6, MassColumn], Is.EqualTo("=SUM(R[-5]C:R[-2]C)"), "the range keeps the four rows it covered, which pan_a is not one of");
            Assert.That(result.Cells[5, 1], Is.EqualTo("Sat.pan_a"), "the deleted row is generated afresh");
        }

        [Test]
        public void VerifyMergeFlagsAReferenceIntoAnotherCalculationSheet()
        {
            this.sheet[3, UserColumn] = "='Calculation opt_b'!R4C3*2";

            var rows = CalculationSheetTestModel.Satellite();
            rows.Insert(4, (2, "Bat.cell_3", new[] { "3", null }));

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous),
                CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass));

            var flag = result.Flags.Single(candidate => candidate.Row == 3 && candidate.Column == UserColumn);

            Assert.That(flag.Messages.Any(message => message.Contains("Calculation opt_b")), Is.True);
            Assert.That(result.Cells[3, UserColumn], Is.EqualTo("='Calculation opt_b'!R4C3*2"), "a reference into another sheet is left as it is");
        }

        [Test]
        public void VerifyMergeOfAReferenceIntoTheCrossviewSheetOnTheSheetOfAnOption()
        {
            // the user refers to the mass of pan_a on the Crossview sheet directly, at body row 4
            this.sheet[5, UserColumn] = "='Crossview'!R17C6*2";

            // a cell is added to the battery, which moves pan_a down on the Crossview sheet, while the sheet of the option shows
            // Sat and pan_a only
            var rows = CalculationSheetTestModel.Satellite();
            rows.Insert(4, (2, "Bat.cell_3", new[] { "3", null }));

            var current = CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass, crossviewRows: new[] { 0, 5 });
            var result = CalculationSheetMerger.Merge(CalculationSheetTestModel.Snapshot(this.sheet, this.previous), current);

            // the reference follows pan_a on the Crossview sheet, not the position of pan_a on this sheet
            Assert.That(result.Cells[2, UserColumn], Is.EqualTo("='Crossview'!R18C6*2"));

            var flag = result.Flags.Single(candidate => candidate.Row == 2 && candidate.Column == UserColumn);
            Assert.That(flag.Messages, Does.Contain(CalculationSheetMerger.CrossviewMovedMessage));

            // the sum over the battery cells has no row on the sheet of this option, and is reported rather than lost
            Assert.That(result.Dropped.Any(entry => entry.Contains(SumOfBatteryCells)), Is.True);
        }

        [Test]
        public void VerifyMergeOfAReferenceOutsideTheSheet()
        {
            // a formula may point at a cell beyond what the sheet uses, in a column to the right of the last one or in a row above
            // the header, and a relative reference lands there as soon as the rows move
            this.sheet[3, UserColumn] = "=RC[1]*2";
            this.sheet[4, UserColumn] = "=R[-5]C*2";

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(this.sheet, this.previous),
                CalculationSheetTestModel.Build(CalculationSheetTestModel.Satellite(), CalculationSheetTestModel.Mass));

            // the column to the right of the sheet keeps its distance, as any column of the user does
            Assert.That(result.Cells[3, UserColumn], Is.EqualTo("=RC[1]*2"));

            // a reference above the first row is gone, which Excel shows as #REF! rather than silently moving it
            Assert.That(result.Cells[4, UserColumn], Is.EqualTo("=#REF!*2"));
        }

        [Test]
        public void VerifyMergeWhenTheUserInsertedARowAboveTheHeader()
        {
            // the user inserts a row on top of the sheet and sums the tree there, from Sat on row 3 to pan_a on row 7
            var withTop = new object[this.sheet.GetLength(0) + 1, this.sheet.GetLength(1)];

            for (var row = 0; row < this.sheet.GetLength(0); row++)
            {
                for (var column = 0; column < this.sheet.GetLength(1); column++)
                {
                    withTop[row + 1, column] = this.sheet[row, column];
                }
            }

            withTop[0, 0] = "Total on top";
            withTop[0, MassColumn] = "=SUM(R[2]C:R[6]C)";

            // a cell is added to the battery, which moves pan_a down
            var rows = CalculationSheetTestModel.Satellite();
            rows.Insert(4, (2, "Bat.cell_3", new[] { "3", null }));

            var result = CalculationSheetMerger.Merge(
                CalculationSheetTestModel.Snapshot(withTop, this.previous),
                CalculationSheetTestModel.Build(rows, CalculationSheetTestModel.Mass));

            // the row on top stays on top, above the header, rather than being read as the header
            Assert.That(result.HeaderRow, Is.EqualTo(1));
            Assert.That(result.Cells[0, 0], Is.EqualTo("Total on top"));
            Assert.That(result.Cells[1, 0], Is.EqualTo("Name"));
            Assert.That(result.Cells[2, 1], Is.EqualTo("Sat"));

            // the tree is not shifted, so no row of it is repeated below it
            var modelCodes = Enumerable.Range(0, result.Cells.GetLength(0)).Select(row => result.Cells[row, 1] as string).Where(code => code != null).ToList();
            Assert.That(modelCodes.Count(code => code == "Sat.pan_a"), Is.EqualTo(1));

            // the sum on top grows to take the new cell in, and is flagged because it now covers one row more
            Assert.That(result.Cells[0, MassColumn], Is.EqualTo("=SUM(R[2]C:R[7]C)"));
            Assert.That(result.Flags.Any(flag => flag.Row == 0 && flag.Column == MassColumn), Is.True);

            // what the user had below the tree keeps its place below it
            Assert.That(result.Cells[result.Cells.GetLength(0) - 1, 0], Is.EqualTo("Total"));
            Assert.That(result.Dropped, Is.Empty);
        }

        [Test]
        public void VerifyIsGeneratedReference()
        {
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=IFERROR(VALUE('Crossview'!R15C6),'Crossview'!R15C6)"), Is.True);
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=IFERROR(VALUE(Crossview!R15C6),Crossview!R15C6)"), Is.True, "Excel drops the quotes when it reads the formula back");
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=Crossview!R15C6*2"), Is.False, "arithmetic on a value is the user's");
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=Crossview!R15C6"), Is.False, "a plain reference of the user is not a generated cell");
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=IFERROR(VALUE('Crossview'!R15C6),'Crossview'!R16C6)"), Is.False);
            Assert.That(CalculationSheetMerger.IsGeneratedReference("=SUM(R[1]C:R[2]C)"), Is.False);
            Assert.That(CalculationSheetMerger.IsGeneratedReference(null), Is.False);
        }
    }
}
