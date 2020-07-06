// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4ReportingModuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Reporting.Views;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Reactive.Concurrency;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4Reporting"/>
    /// </summary>
    [TestFixture]
    public class CDP4ReportingModuleTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IRegionManager> regionManager;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.regionManager = new Mock<IRegionManager>();
            this.serviceLocator.Setup(x => x.GetInstance<IRegionManager>()).Returns(this.regionManager.Object);

            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.panelNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.thingDialogNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);
        }

        [Test]
        public void VerifyThatServicesAreSetByConstructor()
        {
            var module = new Cdp4ReportingModule(this.regionManager.Object, this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.thingDialogNavigationService.Object, this.dialogNavigationService.Object);

            Assert.AreEqual(this.regionManager.Object, module.RegionManager);
            Assert.AreEqual(this.fluentRibbonManager.Object, module.RibbonManager);
            Assert.AreEqual(this.panelNavigationService.Object, module.PanelNavigationService);
            Assert.AreEqual(this.thingDialogNavigationService.Object, module.ThingDialogNavigationService);
            Assert.AreEqual(this.dialogNavigationService.Object, module.DialogNavigationService);
        }
    }
}
