// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.Tests
{
    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="RequirementsModule"/>
    /// </summary>
    [TestFixture]
    public class RequirementsModuleTestFixture
    {
        private Mock<IFluentRibbonManager> ribbonManager;
        private Mock<IDialogNavigationService> dialogNAvigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPluginSettingsService> pluginSettingService;

        [SetUp]
        public void Setup()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>();
            this.ribbonManager.Setup(x => x.RegisterRibbonPart(It.IsAny<RequirementRibbonPart>()));
            
            this.dialogNAvigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.pluginSettingService.Setup(x => x.Write(It.IsAny<RequirementsModuleSettings>(), It.IsAny<JsonConverter>()));
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>()).Returns(this.pluginSettingService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyInitialize()
        {
            var module = new RequirementsModule(this.ribbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNAvigationService.Object, this.pluginSettingService.Object, null, new CDPMessageBus());

            this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Returns(new RequirementsModuleSettings());
            module.Initialize();

            this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Returns(default(RequirementsModuleSettings));
            module.Initialize();

            this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Throws<PluginSettingsException>();
            module.Initialize();

            Assert.Throws<JsonException>(() =>
            {
                this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Throws<JsonException>();
                module.Initialize();
            });

            this.pluginSettingService.Verify(x => x.Write(It.IsAny<RequirementsModuleSettings>()), Times.Once);
            this.pluginSettingService.Verify(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>()), Times.Exactly(4));
        }
    }
}
