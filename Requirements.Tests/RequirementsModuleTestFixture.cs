// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
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

        [Test]
        public void VerifyThatRegionManagerIsSet()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>();
            this.dialogNAvigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService  = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            var regionManager = new RegionManager();
            var module = new RequirementsModule(regionManager, this.ribbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNAvigationService.Object);
            Assert.AreEqual(regionManager, module.RegionManager);
        }
    }
}
