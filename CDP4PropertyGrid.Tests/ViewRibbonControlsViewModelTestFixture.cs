// -------------------------------------------------------------------------------------------------
// <copyright file="ViewRibbonControlsViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4PropertyGrid.ViewModels;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class ViewRibbonControlsViewModelTestFixture
    {
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IServiceLocator> servicelocator;
        private Mock<IPanelView> panelView;

        [SetUp]
        public void Setup()
        {
            this.navigationService = new Mock<IPanelNavigationService>();
            this.servicelocator = new Mock<IServiceLocator>();
            this.panelView = new Mock<IPanelView>();

            ServiceLocator.SetLocatorProvider(() => this.servicelocator.Object);
            this.servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatCommandWorks()
        {
            var vm = new ViewRibbonControlViewModel();

            vm.IsChecked = true;
            await vm.OpenClosePanelCommand.Execute();

            this.navigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()));

            vm.IsChecked = false;
            await vm.OpenClosePanelCommand.Execute();
            this.navigationService.Verify(x => x.CloseInDock(It.IsAny<Type>()));

            // Verify PanelEVentClosed
            vm.IsChecked = true;
            this.messageBus.SendMessage(new NavigationPanelEvent(new PropertyGridViewModel(false), this.panelView.Object, PanelStatus.Closed));
            Assert.IsFalse(vm.IsChecked);
        }
    }
}