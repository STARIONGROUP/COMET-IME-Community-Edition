// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogInfoControlsViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests.ViewModelTests
{
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;

    using CDP4LogInfo.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NLog;
    using NLog.Config;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class LogInfoControlsViewModelTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Mock<IPanelNavigationService> navigationService;

        private Mock<IServiceLocator> servicelocator;

        private Mock<IPanelView> panelView;

        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.servicelocator = new Mock<IServiceLocator>();
            this.panelView = new Mock<IPanelView>();

            ServiceLocator.SetLocatorProvider(() => this.servicelocator.Object);

            this.servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            LogManager.Configuration = new LoggingConfiguration();
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatCommandWorks()
        {
            var vm = new LogInfoControlsViewModel(this.dialogNavigationService.Object, this.messageBus);

            vm.IsChecked = true;
            await vm.OpenClosePanelCommand.Execute();

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));

            vm.IsChecked = false;
            await vm.OpenClosePanelCommand.Execute();
            this.navigationService.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()));

            // Verify PanelEVentClosed
            vm.IsChecked = true;
            this.messageBus.SendMessage(new NavigationPanelEvent(vm.LogInfoPanel, this.panelView.Object, PanelStatus.Closed));
            Assert.IsFalse(vm.IsChecked);
        }
    }
}
