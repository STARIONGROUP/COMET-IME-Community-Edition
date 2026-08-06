// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WhatIfEditRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Reporting.Tests.ViewModels
{
    using System.Collections.Generic;

    using CDP4Reporting.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WhatIfEditRowViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class WhatIfEditRowViewModelTestFixture
    {
        [Test]
        public void VerifyThatConstructorInitialisesProperties()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", @"Sat.bat_a\m\\OPT_A", 12d, null);

            Assert.That(row.Element, Is.EqualTo("Sat.bat_a"));
            Assert.That(row.Parameter, Is.EqualTo("Mass"));
            Assert.That(row.Path, Is.EqualTo(@"Sat.bat_a\m\\OPT_A"));
            Assert.That(row.Original, Is.EqualTo(12d));
            Assert.That(row.Value, Is.EqualTo(12d), "the what-if value starts at the original value");
            Assert.That(row.IsOverride, Is.False);
            Assert.That(row.IsEdited, Is.False);
        }

        [Test]
        public void VerifyThatCurrentDisplayShowsTheFormattedValue()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 12.3456d, null);

            Assert.That(row.CurrentDisplay, Is.EqualTo(12.3456d.ToString("0.####")));
        }

        [Test]
        public void VerifyThatCurrentDisplayIsEmptyForAnUnvaluedParameter()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", null, null);

            Assert.That(row.CurrentDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void VerifyThatCurrentDisplayMarksAnOverrideWithAnAsterisk()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 12d, null) { IsOverride = true };

            Assert.That(row.CurrentDisplay, Is.EqualTo(12d.ToString("0.####") + " *"));
        }

        [Test]
        public void VerifyThatCurrentDisplayHasNoAsteriskForASharedValue()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 12d, null) { IsOverride = false };

            Assert.That(row.CurrentDisplay, Does.Not.Contain("*"));
        }

        [Test]
        public void VerifyThatIsEditedReflectsAChangeFromTheOriginal()
        {
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 5d, null);

            Assert.That(row.IsEdited, Is.False, "unchanged value is not edited");

            row.Value = 6d;
            Assert.That(row.IsEdited, Is.True, "a different value is edited");

            row.Value = 5d;
            Assert.That(row.IsEdited, Is.False, "restoring the original value is not edited");
        }

        [Test]
        public void VerifyThatIsEditedTreatsSettingOrClearingAValueAsAnEdit()
        {
            var unvalued = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", null, null);
            unvalued.Value = 5d;
            Assert.That(unvalued.IsEdited, Is.True, "giving an unvalued parameter a value is an edit");

            var valued = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 5d, null);
            valued.Value = null;
            Assert.That(valued.IsEdited, Is.True, "clearing a value is an edit");
        }

        [Test]
        public void VerifyThatSettingTheValueInvokesTheCallback()
        {
            var callbacks = new List<WhatIfEditRowViewModel>();
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 5d, r => callbacks.Add(r));

            row.Value = 9d;

            Assert.That(callbacks, Has.Count.EqualTo(1));
            Assert.That(callbacks[0], Is.SameAs(row));
            Assert.That(row.Value, Is.EqualTo(9d));
        }

        [Test]
        public void VerifyThatSetValueSilentlyUpdatesTheValueWithoutInvokingTheCallback()
        {
            var callbacks = new List<WhatIfEditRowViewModel>();
            var row = new WhatIfEditRowViewModel("Sat.bat_a", "Mass", "path", 5d, r => callbacks.Add(r));

            row.SetValueSilently(42d);

            Assert.That(row.Value, Is.EqualTo(42d), "the value is updated");
            Assert.That(callbacks, Is.Empty, "the callback is suppressed so it does not re-record an override");

            // a subsequent normal edit must still invoke the callback (the suppression is per-call).
            row.Value = 7d;
            Assert.That(callbacks, Has.Count.EqualTo(1));
        }
    }
}
