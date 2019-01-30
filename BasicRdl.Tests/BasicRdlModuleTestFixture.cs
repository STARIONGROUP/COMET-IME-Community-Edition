// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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

            var module = new BasicRdlModule(this.regionManager.Object, ribbonManager, this.panelNavigationService.Object, null, null, null);
            //module.Initialize();
            Assert.AreEqual(this.regionManager.Object, module.RegionManager);            
        }
    }
}
