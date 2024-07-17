// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4BuiltInRulesModuleTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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

namespace CDP4BuiltInRules.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4BuiltInRulesModule"/>
    /// </summary>
    [TestFixture]
    public class CDP4BuiltInRulesModuleTestFixture
    {
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [Test]
        public void VerifyThatServicePropertiesAreSetByConstrcuctor()
        {
            var module = new CDP4BuiltInRulesModule(this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNavigationService.Object);

            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService);
        }
    }
}
