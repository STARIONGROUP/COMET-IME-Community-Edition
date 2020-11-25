// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4OfficeInfrastructure;

    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossViewEditorModule"/> class.
    /// </summary>
    [TestFixture]
    public class CrossViewEditorModuleTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IOfficeApplicationWrapper> officeApplicationWrapper;
        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.officeApplicationWrapper = new Mock<IOfficeApplicationWrapper>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatModuleIsInitialized()
        {
            var module = new CrossViewEditorModule(
                this.fluentRibbonManager.Object,
                this.panelNavigationService.Object,
                this.thingDialogNavigationService.Object, this.dialogNavigationService.Object, this.officeApplicationWrapper.Object);

            module.Initialize();

            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService);
            Assert.AreEqual(this.officeApplicationWrapper.Object, module.OfficeApplicationWrapper);
        }
    }
}
