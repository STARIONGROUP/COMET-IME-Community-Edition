// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetTreeTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4CrossViewEditor.Generator;

    using NUnit.Framework;

    [TestFixture]
    public class CalculationSheetTreeTestFixture
    {
        [Test]
        public void VerifyKeys()
        {
            // the battery definition is used twice, so its cell usage has the same model code in both places
            var tree = new CalculationSheetTree(
                new[] { "Satellite", "   Sat : bat_a", "      Bat : cell_1", "   Sat : bat_b", "      Bat : cell_1" },
                new[] { "Sat", "Sat.bat_a", "Bat.cell_1", "Sat.bat_b", "Bat.cell_1" });

            Assert.That(tree.Keys, Is.EqualTo(new[] { "Sat", "Sat/bat_a", "Sat/bat_a/cell_1", "Sat/bat_b", "Sat/bat_b/cell_1" }));
            Assert.That(tree.Levels, Is.EqualTo(new[] { 0, 1, 2, 1, 2 }));
            Assert.That(tree.IndexOf("Sat/bat_b/cell_1"), Is.EqualTo(4));
            Assert.That(tree.IndexOf("missing"), Is.EqualTo(-1));
            Assert.That(tree.IndexOf(null), Is.EqualTo(-1));

            // a row that really does repeat a path still gets a key of its own
            var repeated = new CalculationSheetTree(new[] { "Sat", "   x", "   x" }, new[] { "Sat", "Sat.x", "Sat.x" });

            Assert.That(repeated.Keys, Is.EqualTo(new[] { "Sat", "Sat/x", "Sat/x#2" }));
        }

        [Test]
        public void VerifyParseLevel()
        {
            Assert.That(CalculationSheetTree.ParseLevel("Satellite"), Is.EqualTo(0));
            Assert.That(CalculationSheetTree.ParseLevel("   Sat : bat_a"), Is.EqualTo(1));
            Assert.That(CalculationSheetTree.ParseLevel("      Bat : cell_1"), Is.EqualTo(2));
            Assert.That(CalculationSheetTree.ParseLevel(null), Is.EqualTo(0));
            Assert.That(CalculationSheetTree.ParseLevel(string.Empty), Is.EqualTo(0));
        }

        [Test]
        public void VerifyDescendantsAndDirectChildren()
        {
            // the cell sits two levels below the satellite, because the intermediate battery was not selected
            var tree = new CalculationSheetTree(
                new[] { "Satellite", "   Sat : bat_a", "      Bat : cell_1", "      Bat : cell_2", "         x : deep", "   Sat : pan_a", "      Bat : orphan" },
                new[] { "Sat", "Sat.bat_a", "Bat.cell_1", "Bat.cell_2", "Cell.deep", "Sat.pan_a", "Bat.orphan" });

            Assert.That(tree.Descendants(1).ToList(), Is.EqualTo(new[] { 2, 3, 4 }));
            Assert.That(tree.DirectChildren(1).ToList(), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(tree.DirectChildren(0).ToList(), Is.EqualTo(new[] { 1, 5 }));
            Assert.That(tree.DirectChildren(4).ToList(), Is.Empty);

            var skipped = new CalculationSheetTree(new[] { "Satellite", "      Bat : cell_1" }, new[] { "Sat", "Bat.cell_1" });

            Assert.That(skipped.DirectChildren(0).ToList(), Is.EqualTo(new[] { 1 }));
        }
    }
}
