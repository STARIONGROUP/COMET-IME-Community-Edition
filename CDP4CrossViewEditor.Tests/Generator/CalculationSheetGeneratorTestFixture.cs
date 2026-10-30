// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetGeneratorTestFixture.cs" company="Starion Group S.A.">
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
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Generator;

    using NUnit.Framework;

    [TestFixture]
    public class CalculationSheetGeneratorTestFixture
    {
        [Test]
        public void VerifyQuerySheets()
        {
            var uri = new Uri("http://test.com");
            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iteration = new Iteration(Guid.NewGuid(), cache, uri);

            var first = new Option(Guid.NewGuid(), cache, uri) { ShortName = "opt_a" };
            var second = new Option(Guid.NewGuid(), cache, uri) { ShortName = "x/y[1]" };
            var third = new Option(Guid.NewGuid(), cache, uri) { ShortName = "a_very_long_option_short_name" };
            var fourth = new Option(Guid.NewGuid(), cache, uri) { ShortName = "a_very_long_option_short_name_too" };
            var fifth = new Option(Guid.NewGuid(), cache, uri) { ShortName = "quoted'" };
            iteration.Option.Add(first);
            iteration.Option.Add(second);
            iteration.Option.Add(third);
            iteration.Option.Add(fourth);
            iteration.Option.Add(fifth);

            var single = CalculationSheetGenerator.QuerySheets(iteration, null).Single();
            Assert.That(single.SheetName, Is.EqualTo("Calculation"), "without a selected option there is one sheet over every option");
            Assert.That(single.Option, Is.Null);

            Assert.That(CalculationSheetGenerator.QuerySheets(iteration, new Guid[0]).Single().SheetName, Is.EqualTo("Calculation"));

            // one sheet per selected option, in the order of the options of the iteration
            var perOption = CalculationSheetGenerator.QuerySheets(iteration, new[] { second.Iid, first.Iid });
            Assert.That(perOption.Select(sheet => sheet.SheetName), Is.EqualTo(new[] { "Calculation opt_a", "Calculation x_y_1_" }));
            Assert.That(perOption.Select(sheet => sheet.Option), Is.EqualTo(new[] { first, second }));

            // Excel does not accept a sheet name over 31 characters
            Assert.That(CalculationSheetGenerator.QuerySheetName(third), Is.EqualTo("Calculation a_very_long_option_"));

            // nor one that ends in an apostrophe
            Assert.That(CalculationSheetGenerator.QuerySheetName(fifth), Is.EqualTo("Calculation quoted"));

            // two options whose names are cut to the same text each keep their own sheet
            var cut = CalculationSheetGenerator.QuerySheets(iteration, new[] { third.Iid, fourth.Iid });

            Assert.That(cut.Select(sheet => sheet.SheetName), Is.EqualTo(new[] { "Calculation a_very_long_option_", "Calculation a_very_long_opt (2)" }));
            Assert.That(cut.Select(sheet => sheet.SheetName.Length), Has.All.LessThanOrEqualTo(31));

            // each sheet names its values with a prefix of its own
            Assert.That(CalculationSheetGenerator.QueryNamePrefix("Calculation"), Is.EqualTo("Calc"));
            Assert.That(CalculationSheetGenerator.QueryNamePrefix("Calculation opt_a"), Is.EqualTo("Calc_opt_a"));
            Assert.That(CalculationSheetGenerator.QueryNamePrefix("Calculation x_y_1_ (2)"), Is.EqualTo("Calc_x_y_1___2_"));
            Assert.That(CalculationSheetGenerator.QueryNamePrefix("Calculation a.b"), Is.EqualTo("Calc_a_b"), "a prefix holds no dot, so it never starts the prefix of another sheet");
        }
    }
}
