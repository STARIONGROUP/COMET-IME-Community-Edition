// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlModuleTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.PluginSettingService;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="BasicRdlModule"/>
    /// </summary>
    [TestFixture]
    public class BasicRdlModuleTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IRegionManager> regionManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.regionManager = new Mock<IRegionManager>();
            this.serviceLocator.Setup(x => x.GetInstance<IRegionManager>()).Returns(this.regionManager.Object);
            
            // TODO: figure out how to mock extension methods
            //this.regionManager.Setup(x => x.RegisterViewWithRegion(It.IsAny<string>(), It.IsAny<Type>()));

            this.panelNavigationService = new Mock<IPanelNavigationService>();
        }

        [Test]
        public void VerifyThatRegionManagerIsSet()
        {
            var ribbonManager = new FluentRibbonManager();

            var module = new BasicRdlModule(this.regionManager.Object, ribbonManager, this.panelNavigationService.Object, null, null, null, null);
            //module.Initialize();
            Assert.AreEqual(this.regionManager.Object, module.RegionManager);            
        }
    }
}
