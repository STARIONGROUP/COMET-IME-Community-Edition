// --------------------------------------------------------------------------------------------------------------------
// <copyright file="R1C1ReferencesTestFixture.cs" company="Starion Group S.A.">
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
    using System;

    using CDP4CrossViewEditor.Generator;

    using NUnit.Framework;

    [TestFixture]
    public class R1C1ReferencesTestFixture
    {
        private static readonly Func<string, int, int?> Unchanged = (sheet, position) => position;

        // a row was inserted after row 10 of this sheet
        private static readonly Func<string, int, int?> RowInsertedAfterTen = (sheet, row) => sheet == null && row > 10 ? row + 1 : row;

        // row 7 of this sheet was deleted
        private static readonly Func<string, int, int?> RowSevenDeleted = (sheet, row) =>
            sheet != null ? row : row == 7 ? (int?)null : row > 7 ? row - 1 : row;

        [Test]
        public void VerifyRemap()
        {
            // a sum over the children of a row grows when a child is inserted between them
            var grown = R1C1References.Remap("=SUM(R[1]C:R[12]C)", 6, 3, 6, 3, RowInsertedAfterTen, Unchanged);

            Assert.That(grown.Formula, Is.EqualTo("=SUM(R[1]C:R[13]C)"));
            Assert.That(grown.HasBrokenReference, Is.False);
            Assert.That(grown.MultiRowRanges, Has.Count.EqualTo(1));
            Assert.That(grown.MultiRowRanges[0], Is.EqualTo(((string)null, 7, 18, 7, 19)));

            // a reference follows its target when the target moves, and keeps its relative or absolute style
            Assert.That(R1C1References.Remap("=R[6]C*2+R12C3", 6, 3, 6, 3, RowInsertedAfterTen, Unchanged).Formula, Is.EqualTo("=R[7]C*2+R13C3"));

            // a reference into a row that no longer exists is broken, as Excel does
            var broken = R1C1References.Remap("=R[1]C*2", 6, 3, 6, 3, RowSevenDeleted, Unchanged);

            Assert.That(broken.Formula, Is.EqualTo("=#REF!*2"));
            Assert.That(broken.HasBrokenReference, Is.True);

            // a range shrinks when the row at its edge is deleted, again as Excel does
            var shrunk = R1C1References.Remap("=SUM(R[1]C:R[3]C)", 4, 3, 4, 3, RowSevenDeleted, Unchanged);

            Assert.That(shrunk.Formula, Is.EqualTo("=SUM(R[1]C:R[2]C)"));
            Assert.That(shrunk.HasBrokenReference, Is.False);

            // a range of which nothing remains is lost as a whole
            var gone = R1C1References.Remap("=SUM(R[3]C:R[3]C)+1", 4, 3, 4, 3, RowSevenDeleted, Unchanged);

            Assert.That(gone.Formula, Is.EqualTo("=SUM(#REF!)+1"));
            Assert.That(gone.HasBrokenReference, Is.True);
        }

        [Test]
        public void VerifyRemapLeavesTextAlone()
        {
            Func<string, int, int?> columnInsertedBeforeThree = (sheet, column) => sheet == null && column >= 3 ? column + 1 : column;

            // a string literal that happens to read like a reference is not a reference
            Assert.That(R1C1References.Remap("=\"RC\"&RC[1]", 2, 2, 2, 2, Unchanged, columnInsertedBeforeThree).Formula, Is.EqualTo("=\"RC\"&RC[2]"));
            Assert.That(R1C1References.Remap("=\"say \"\"R1C1\"\"\"&RC[1]", 2, 2, 2, 2, Unchanged, columnInsertedBeforeThree).Formula, Is.EqualTo("=\"say \"\"R1C1\"\"\"&RC[2]"));

            // nor is the name of a function
            Assert.That(R1C1References.Remap("=ROUND(RC[1],2)+COUNT(R1C1:R2C2)+TRUE", 3, 3, 3, 3, Unchanged, Unchanged).Formula, Is.EqualTo("=ROUND(RC[1],2)+COUNT(R1C1:R2C2)+TRUE"));
        }

        [Test]
        public void VerifyRemapOfOtherSheets()
        {
            Func<string, int, int?> crossviewRowMoved = (sheet, row) => sheet == "Crossview" && row == 18 ? 19 : row;

            var unquoted = R1C1References.Remap("=Crossview!R18C6*2", 3, 3, 3, 3, crossviewRowMoved, Unchanged);

            Assert.That(unquoted.Formula, Is.EqualTo("=Crossview!R19C6*2"));
            Assert.That(unquoted.ReferencedSheets, Does.Contain("Crossview"));

            Assert.That(R1C1References.Remap("='Crossview'!R18C6", 3, 3, 3, 3, crossviewRowMoved, Unchanged).Formula, Is.EqualTo("='Crossview'!R19C6"));

            // the far end of a range on another sheet carries no sheet of its own, and keeps carrying none
            Assert.That(R1C1References.Remap("=SUM(Crossview!R17C6:R18C6)", 3, 3, 3, 3, crossviewRowMoved, Unchanged).Formula, Is.EqualTo("=SUM(Crossview!R17C6:R19C6)"));
        }

        [Test]
        public void VerifyFromA1()
        {
            Assert.That(R1C1References.FromA1("=SUM(C7:C18)", 6, 3), Is.EqualTo("=SUM(R[1]C:R[12]C)"));
            Assert.That(R1C1References.FromA1("=D7*E7", 7, 6), Is.EqualTo("=RC[-2]*RC[-1]"));
            Assert.That(R1C1References.FromA1("=$B$2+B$2+$B2", 4, 4), Is.EqualTo("=R2C2+R2C[-2]+R[-2]C2"));
            Assert.That(R1C1References.FromA1("=Crossview!F18", 7, 3), Is.EqualTo("=Crossview!R[11]C[3]"));
            Assert.That(R1C1References.FromA1("=\"C7\"&C7", 7, 3), Is.EqualTo("=\"C7\"&RC"));
            Assert.That(R1C1References.FromA1("=LOG10(C7)", 7, 3), Is.EqualTo("=LOG10(RC)"));
        }

        [Test]
        public void VerifyColumnNumber()
        {
            Assert.That(R1C1References.ColumnNumber("A"), Is.EqualTo(1));
            Assert.That(R1C1References.ColumnNumber("Z"), Is.EqualTo(26));
            Assert.That(R1C1References.ColumnNumber("AA"), Is.EqualTo(27));
            Assert.That(R1C1References.ColumnNumber("XFD"), Is.EqualTo(16384));
        }
    }
}
