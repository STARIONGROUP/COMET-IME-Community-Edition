// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="SiteDirectoryModule"/>
    /// </summary>
    [TestFixture]
    public class SiteDirectoryModuleTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IRegionManager> regionManager;
        private Mock<IPanelNavigationService> panelNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.regionManager = new Mock<IRegionManager>();
            this.serviceLocator.Setup(x => x.GetInstance<IRegionManager>()).Returns(this.regionManager.Object);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
        }

        [Test]
        public void VerifyThatRegionManagerIsSet()
        {
            var ribbonManager = new FluentRibbonManager();

            var module = new SiteDirectoryModule(this.regionManager.Object, ribbonManager, this.panelNavigationService.Object, null, null);
            Assert.AreEqual(this.regionManager.Object, module.RegionManager);
        }
    }
}
