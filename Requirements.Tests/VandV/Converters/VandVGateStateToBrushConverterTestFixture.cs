// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVGateStateToBrushConverterTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.Converters
{
    using System.Globalization;
    using System.Windows.Media;

    using CDP4Requirements.Converters;
    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Composition.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VandVGateStateToBrushConverter"/> class.
    /// </summary>
    /// <remarks>
    /// The VCRM columns are a project's own stage gates, so the grid generates them from a
    /// <see cref="System.Data.DataTable"/> and the converter has only the cell's text to work from. These tests pin
    /// that it recovers the state from the label rather than from a duplicated literal, which is what stops the
    /// colouring from drifting the moment a state is relabelled.
    /// </remarks>
    [TestFixture]
    public class VandVGateStateToBrushConverterTestFixture
    {
        private VandVGateStateToBrushConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new VandVGateStateToBrushConverter();
        }

        [Test]
        public void VerifyThatTheThreeActionableStatesAreColoured()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Undefined)), Is.EqualTo(Expected(CDP4Color.Inconclusive)));
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Failed)), Is.EqualTo(Expected(CDP4Color.Failed)));
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.ClosedOut)), Is.EqualTo(Expected(CDP4Color.Succeeded)));
            });
        }

        [Test]
        public void VerifyThatNormalProgressIsLeftPlain()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Planned)), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Deferred)), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Verified)), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color(VandVStageGateQuery.Describe(VandVGateState.Complete)), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color(null), Is.EqualTo(Colors.Transparent));
            });
        }

        [Test]
        public void VerifyThatTheStateIsReadFromTheLabelTheCellLeadsWith()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Color("FAILED: VNV-1 (Test, Failed)"), Is.EqualTo(Expected(CDP4Color.Failed)));
                Assert.That(this.Color("UNDEFINED: no V&V item states that it closes this requirement out, so the plan has no end."), Is.EqualTo(Expected(CDP4Color.Inconclusive)));
                Assert.That(this.Color("Closed out: verification and validation complete."), Is.EqualTo(Expected(CDP4Color.Succeeded)));
            });
        }

        [Test]
        public void VerifyThatFreeTextBeginningWithAStateLabelIsNotColoured()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Color("Closed out actions from PDR"), Is.EqualTo(Colors.Transparent), "the style is applied to every column, so a requirement name must never read as a verdict");
                Assert.That(this.Color("UNDEFINED behaviour in the fallback mode"), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color("FAILED-SAFE shutdown of the switchboard"), Is.EqualTo(Colors.Transparent));
                Assert.That(this.Color("Planned maintenance interval"), Is.EqualTo(Colors.Transparent));
            });
        }

        [Test]
        public void VerifyThatAComplianceShortfallIsColouredAsAFailure()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.Color(VandVCompliance.NonCompliant), Is.EqualTo(Expected(CDP4Color.Failed)));
                Assert.That(this.Color(VandVCompliance.PartiallyCompliant), Is.EqualTo(Expected(CDP4Color.Failed)));

                Assert.That(this.Color("Non_Compliant"), Is.EqualTo(Expected(CDP4Color.Failed)));
                Assert.That(this.Color(VandVCompliance.Compliant), Is.EqualTo(Colors.Transparent));
            });
        }

        private Color Color(string text)
        {
            return ((SolidColorBrush)this.converter.Convert(text, typeof(Brush), null, CultureInfo.InvariantCulture)).Color;
        }

        private static Color Expected(CDP4Color color)
        {
            return ((SolidColorBrush)color.GetBrush()).Color;
        }
    }
}
