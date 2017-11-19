// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal.Permission;
    using CDP4ProductTree;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="ProductTreeModule"/>
    /// </summary>
    [TestFixture]
    public class ProductTreeModuleTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IDialogNavigationService> dialogNavigationService;
            
        [SetUp]
        public void Setup()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [Test]
        public void VerifyThatRegionManagerIsSet()
        {
            var regionManager = new RegionManager();
            var module = new ProductTreeModule(regionManager, this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object);

            // TODO static method
            // Assert.DoesNotThrow(module.Initialize);
        }
    }
}
