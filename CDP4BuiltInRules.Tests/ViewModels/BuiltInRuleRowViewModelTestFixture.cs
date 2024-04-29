// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using CDP4BuiltInRules.ViewModels;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRuleRowViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class BuiltInRuleRowViewModelTestFixture
    {
        private Mock<IDragInfo> draginfo;
        private BuiltInRule builtInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;
            
        [SetUp]
        public void SetUp()
        {
            this.draginfo = new Mock<IDragInfo>();

            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("STARION");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns("shortnamerule");
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.builtInRule = new TestBuiltInRule();
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var viewmodel = new BuiltInRuleRowViewModel(this.builtInRule, this.iBuiltInRuleMetaData.Object);

            Assert.AreEqual("STARION", viewmodel.Author);
            Assert.AreEqual("shortnamerule", viewmodel.Name);
            Assert.AreEqual("verifies that the shortnames are correct", viewmodel.Description);

            Assert.AreEqual(this.builtInRule, viewmodel.Rule);
        }

        [Test]
        public void VerifyThatPayloadAndDraggEffectAreSetOnStartDrag()
        {
            var viewmodel = new BuiltInRuleRowViewModel(this.builtInRule, this.iBuiltInRuleMetaData.Object);

            this.draginfo.SetupProperty(x => x.Effects);
            this.draginfo.SetupProperty(x => x.Payload);

            viewmodel.StartDrag(this.draginfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, this.draginfo.Object.Effects);
            Assert.AreEqual(this.iBuiltInRuleMetaData.Object, this.draginfo.Object.Payload);
        }

        /// <summary>
        /// test class
        /// </summary>
        private class TestBuiltInRule : BuiltInRule
        {
            public override IEnumerable<RuleViolation> Verify(Iteration iteration)
            {
                throw new NotSupportedException();
            }
        }
    }
}
