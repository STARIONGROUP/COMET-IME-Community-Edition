// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4ReportingModuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
