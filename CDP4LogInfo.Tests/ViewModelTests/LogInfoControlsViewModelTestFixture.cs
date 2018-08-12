// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoControlsViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Tests.ViewModelTests
{
    using System.Reactive.Concurrency;

    using CDP4Composition;
    using CDP4Composition.Log;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4LogInfo.ViewModels;
    using Microsoft.Practices.ServiceLocation;
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

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

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
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatCommandWorks()
        {
            var vm = new LogInfoControlsViewModel(this.dialogNavigationService.Object);

            vm.IsChecked = true;
            vm.OpenClosePanelCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));

            vm.IsChecked = false;
            vm.OpenClosePanelCommand.Execute(null);
            this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true));

            // Verify PanelEVentClosed
            vm.IsChecked = true;
            CDPMessageBus.Current.SendMessage(new NavigationPanelEvent(vm.LogInfoPanel, this.panelView.Object, PanelStatus.Closed));
            Assert.IsFalse(vm.IsChecked);
        }
    }
}