// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;    
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="EngineeringModelModule"/> class
    /// </summary>
    [TestFixture]
    public class EngineeringModelModuleTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
        }

        [Test]
        public void VerifyThatServicesAreSetByConstructor()
        {
            var regionManager = new RegionManager();
            var module = new EngineeringModelModule(regionManager, this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object);
            
            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService );
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
        }
    }
}
