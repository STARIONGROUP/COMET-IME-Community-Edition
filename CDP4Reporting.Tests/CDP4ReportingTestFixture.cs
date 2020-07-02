// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4ReportingTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4Reporting"/>
    /// </summary>
    [TestFixture]
    public class CDP4ReportingTestFixture
    {
        private Mock<IRegionManager> regionManager;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.regionManager = new Mock<IRegionManager>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [Test]
        public void VerifyThatServicePropertiesAreSetByConstructor()
        {
            var module = new CDP4ReportingModule(this.regionManager.Object, this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNavigationService.Object);

            Assert.AreEqual(this.regionManager.Object, module.RegionManager);
            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService);
        }
    }
}
