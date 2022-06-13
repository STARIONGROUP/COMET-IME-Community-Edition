// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphExportModuleTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="GraphExportModule"/>
    /// </summary>
    [TestFixture]
    public class GraphExportModuleTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<IFluentRibbonManager> fluentribbonManager;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void Setup()
        {
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.fluentribbonManager = new Mock<IFluentRibbonManager>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.panelNavigationService.Object);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyInitialize()
        {
            var module = new GraphExportModule(this.fluentribbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object, this.pluginSettingService.Object);
            Assert.IsNotNull(module);
            module.Initialize();
        }
    }
}
