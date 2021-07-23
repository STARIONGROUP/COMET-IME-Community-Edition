// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixModuleTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4RelationshipMatrix.Tests
{
    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using Microsoft.Practices.ServiceLocation;

    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipMatrixModule"/>
    /// </summary>
    [TestFixture]
    public class RelationshipMatrixModuleTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private RelationshipMatrixModule relationshipMatrixModule;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            
            this.relationshipMatrixModule = new RelationshipMatrixModule(this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);
            this.pluginSettingsService.Setup(s => s.Read<RelationshipMatrixPluginSettings>(false))
                .Returns(new RelationshipMatrixPluginSettings(true));
        }

        [Test]
        public void Verify_that_when_plugin_service_can_read_settings_no_exception_is_raised()
        {
           Assert.DoesNotThrow(() => this.relationshipMatrixModule.ReadPluginSettings());
            
            this.pluginSettingsService.Verify(x => x.Read<RelationshipMatrixPluginSettings>(false));
        }

        [Test]
        public void Verify_that_when_settings_file_cannot_be_read_the_module_recovers_with_the_default_SettingsClass()
        {
            this.pluginSettingsService
                .Setup(x => x.Read<RelationshipMatrixPluginSettings>(false))
                .Throws<PluginSettingsException>();
            
            Assert.DoesNotThrow(() => this.relationshipMatrixModule.ReadPluginSettings());

            this.pluginSettingsService.Verify(x => x.Write(It.IsAny<RelationshipMatrixPluginSettings>()));
        }
    }
}
