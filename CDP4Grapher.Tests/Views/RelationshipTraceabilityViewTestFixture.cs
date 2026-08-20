// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipTraceabilityViewTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Tests.Views
{
    using System.Threading;
    using System.Windows;

    using CDP4Grapher.Views;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipTraceability"/> view. Loading the view headless catches XAML
    /// errors that only surface at runtime, such as behaviors added to an incompatible behavior collection.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RelationshipTraceabilityViewTestFixture
    {
        [Test]
        public void VerifyThatTheViewLoadsAndMeasures()
        {
            RelationshipTraceability view = null;

            Assert.DoesNotThrow(() => view = new RelationshipTraceability(true));

            // deferred template content, such as the diagram behaviors, only loads on measure
            Assert.DoesNotThrow(() =>
            {
                view.Measure(new Size(800, 600));
                view.Arrange(new Rect(0, 0, 800, 600));
            });
        }
    }
}
