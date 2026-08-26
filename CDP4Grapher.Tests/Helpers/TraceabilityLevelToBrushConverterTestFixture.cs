// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityLevelToBrushConverterTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Tests.Helpers
{
    using System.Globalization;
    using System.Windows.Media;

    using CDP4Grapher.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="TraceabilityLevelToBrushConverter"/> class
    /// </summary>
    [TestFixture]
    public class TraceabilityLevelToBrushConverterTestFixture
    {
        [Test]
        public void VerifyThatTheLevelSelectsTheBrush()
        {
            var converter = new TraceabilityLevelToBrushConverter();

            var root = converter.Convert(0, typeof(Brush), null, CultureInfo.InvariantCulture);
            var down = converter.Convert(2, typeof(Brush), null, CultureInfo.InvariantCulture);
            var up = converter.Convert(-2, typeof(Brush), null, CultureInfo.InvariantCulture);

            Assert.That(root, Is.Not.EqualTo(down));
            Assert.That(down, Is.Not.EqualTo(up));

            // a non-integer level still yields a usable brush rather than throwing on the diagram
            Assert.That(converter.Convert(null, typeof(Brush), null, CultureInfo.InvariantCulture), Is.EqualTo(down));
        }

        [Test]
        public void VerifyThatTheHexColorMatchesTheBrushOfTheSameLevel()
        {
            // the SVG export reads the hex colors, so they must not drift from the on-screen brushes
            foreach (var level in new[] { -2, -1, 0, 1, 2 })
            {
                var color = ((SolidColorBrush)TraceabilityLevelToBrushConverter.GetBrush(level)).Color;

                Assert.That(TraceabilityLevelToBrushConverter.GetHexColor(level), Is.EqualTo($"#{color.R:X2}{color.G:X2}{color.B:X2}"));
            }

            Assert.That(TraceabilityLevelToBrushConverter.GetHexColor(0), Is.EqualTo("#FFE082"));
            Assert.That(TraceabilityLevelToBrushConverter.GetHexColor(1), Is.EqualTo("#BBDEFB"));
            Assert.That(TraceabilityLevelToBrushConverter.GetHexColor(-1), Is.EqualTo("#C8E6C9"));
        }
    }
}
