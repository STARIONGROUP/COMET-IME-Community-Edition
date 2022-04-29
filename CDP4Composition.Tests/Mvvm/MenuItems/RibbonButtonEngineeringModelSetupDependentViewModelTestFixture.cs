// -------------------------------------------------------------------------------------------------
// <copyright file="SessionEngineeringModelSetupMenuGroupViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm.MenuItems
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Composition.Tests.Attributes;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RibbonButtonEngineeringModelSetupDependentViewModel"/> class.
    /// </summary>
    [TestFixture] 
    public class RibbonButtonEngineeringModelSetupDependentViewModelTestFixture
    {
        /// <summary>
        /// The view-model that is being tested
        /// </summary>
        private TestClass viewModel;
        private EngineeringModelSetup engeEngineeringModelSetup;
        private Uri uri;
        private Mock<ISession> session;

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;

        private Assembler assembler;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.dialogNavigation.Object);

            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);

            this.viewModel = new TestClass();
            
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionChangeEventsAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.Sessions);
            
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);

            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.Contains(this.session.Object,this.viewModel.Sessions);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsEmpty(this.viewModel.Sessions);
        }

        [Test]
        public void VerifyThatAddAndRemoveEngineeringModelSetupMessagesAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.EngineeringModelSetups);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);
            Assert.AreEqual(1, this.viewModel.Sessions.Count);

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            siteDirectory.Model.Add(engineeringModelSetup);

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            
            CDPMessageBus.Current.SendObjectChangeEvent(engineeringModelSetup, EventKind.Added);
            Assert.AreEqual(1, this.viewModel.EngineeringModelSetups.Count);
            
            CDPMessageBus.Current.SendObjectChangeEvent(engineeringModelSetup, EventKind.Removed);
            Assert.AreEqual(0, this.viewModel.EngineeringModelSetups.Count);

        }
        
        [Test]
        public void VerifyThatMenuItemsAreSortedAlphabetically()
        {
            Assert.IsEmpty(this.viewModel.EngineeringModelSetups);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            siteDirectory.Model.Add(engineeringModelSetup);

            var engineeringModelOne = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            engineeringModelOne.EngineeringModelSetup = engineeringModelSetup;

            CDPMessageBus.Current.SendObjectChangeEvent(engineeringModelSetup, EventKind.Added);
            var sessionEngineeringModelSetupMenuGroupViewModel = this.viewModel.EngineeringModelSetups.SingleOrDefault(x => x.Thing == engineeringModelSetup.Container);
            var menuItem = new RibbonMenuItemEngineeringModelSetupDependentViewModel(engineeringModelSetup, session.Object, null);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Add(menuItem);
            
            var menuItemTwo = new RibbonMenuItemEngineeringModelSetupDependentViewModel(engineeringModelSetup, session.Object, null);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Add(menuItemTwo);
            
            Assert.AreEqual(3, sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Count);
            var sortedList = sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.OrderBy(em => em.MenuItemContent);
            Assert.AreEqual(sortedList, sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups);
        }

        private class TestClass : RibbonButtonEngineeringModelSetupDependentViewModel
        {
            public TestClass() : base(null)
            {
            }
        }
    }
}
