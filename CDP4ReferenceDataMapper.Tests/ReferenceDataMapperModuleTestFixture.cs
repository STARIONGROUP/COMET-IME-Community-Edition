// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataMapperModuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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

namespace CDP4ReferenceDataMapper.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4ReferenceDataMapper;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ReferenceDataMapperModule"/> class
    /// </summary>
    [TestFixture]
    public class ReferenceDataMapperModuleTestFixture
    {
        private Mock<IFluentRibbonManager> ribbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        [SetUp]
        public void SetUp()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>(); 
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
        }

        [Test]
        public void Verify_that_properties_are_set_one_module_instantiation()
        {
            var referenceDataMapperModule = new ReferenceDataMapperModule(
                this.ribbonManager.Object, 
                this.panelNavigationService.Object, 
                this.thingDialogNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(referenceDataMapperModule.RibbonManager, Is.EqualTo(this.ribbonManager.Object));
            Assert.That(referenceDataMapperModule.PanelNavigationService, Is.EqualTo(this.panelNavigationService.Object));
            Assert.That(referenceDataMapperModule.ThingDialogNavigationService, Is.EqualTo(this.thingDialogNavigationService.Object));
            Assert.That(referenceDataMapperModule.DialogNavigationService, Is.EqualTo(this.dialogNavigationService.Object));
            Assert.That(referenceDataMapperModule.PluginSettingsService, Is.EqualTo(this.pluginSettingsService.Object));
        }
    }
}
