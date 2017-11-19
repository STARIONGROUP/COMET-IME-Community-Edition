// -------------------------------------------------------------------------------------------------
// <copyright file="IterationMenuItemViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System.Reactive.Concurrency;

    [TestFixture]
    public class IterationMenuItemViewModelTestFixture
    {
        private Uri uri = new Uri("http://test.be");
        private PanelViewModel panel;
        private Mock<IPanelView> panelView;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService; 
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private Mock<ISession> session;
        private EngineeringModel model;
        private Participant participant;
        private EngineeringModelSetup modelSetup;
        private Person perosn;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri) { IterationNumber = 5  };
            this.iteration.IterationSetup = this.iterationSetup;
            this.participant = new Participant(Guid.NewGuid(), null, this.uri);
            this.perosn = new Person(Guid.NewGuid(), null, this.uri);
            this.model.Iteration.Add(this.iteration);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.perosn;
            this.model.EngineeringModelSetup = this.modelSetup;

            this.panelView = new Mock<IPanelView>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);

            this.session.Setup(x => x.ActivePerson).Returns(this.perosn);

            this.permissionService = new Mock<IPermissionService>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatNameIsSet()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            Assert.IsTrue(menu.MenuItemContent.Contains(this.iterationSetup.IterationNumber.ToString()));
        }

        [Test]
        public void VerifyThatCommandWorks()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            menu.IsChecked = true;
            menu.ShowOrClosePanelCommand.Execute(null);

            this.navigation.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));

            menu.IsChecked = false;
            menu.ShowOrClosePanelCommand.Execute(null);

            var modelbrowser = menu.PanelViewModel;
            CDPMessageBus.Current.SendMessage(new NavigationPanelEvent(modelbrowser, this.panelView.Object, PanelStatus.Closed));
            menu.ShowOrClosePanelCommand.Execute(null);
            this.navigation.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true), Times.Exactly(1));
        }

        [Test]
        public void VerifyThatClosePAnelEventHandlerWorks()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            menu.IsChecked = true;
            menu.ShowOrClosePanelCommand.Execute(null);
            CDPMessageBus.Current.SendMessage(new NavigationPanelEvent(menu.PanelViewModel, this.panelView.Object, PanelStatus.Closed));

            Assert.IsFalse(menu.IsChecked);
        }

        private static IPanelViewModel MockInstantiate(Iteration it, ISession ses, IThingDialogNavigationService thingDialogService, IPanelNavigationService panelService, IDialogNavigationService dialogNavigationService)
        {
            var model = it.Container as EngineeringModel;
            return new PanelViewModel(it, model.GetActiveParticipant(ses.ActivePerson), ses, thingDialogService, panelService, dialogNavigationService);
        }

        private class PanelViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
        {
            public PanelViewModel(Iteration iteration, Participant participant, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService)
                : base(iteration, session, dialogNav, panelNav, dialogNavigationService)
            { }

            public string Caption { get; private set; }
            public string ToolTip { get; private set; }
            public string DataSource { get; private set; }

            public override void ComputePermission()
            { }
        }
    }
}