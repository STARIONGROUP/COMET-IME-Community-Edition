// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionEngineeringModelSetupMenuGroupViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Mvvm.MenuItems
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

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
        private CDPMessageBus messageBus;

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

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.viewModel = new TestClass(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionChangeEventsAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.Sessions);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);

            this.messageBus.SendMessage(openSessionEvent);

            Assert.Contains(this.session.Object, this.viewModel.Sessions);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(closeSessionEvent);

            Assert.IsEmpty(this.viewModel.Sessions);
        }

        [Test]
        public void VerifyThatAddAndRemoveEngineeringModelSetupMessagesAreProcessed()
        {
            Assert.IsEmpty(this.viewModel.EngineeringModelSetups);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);
            Assert.AreEqual(1, this.viewModel.Sessions.Count);

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            siteDirectory.Model.Add(engineeringModelSetup);

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            engineeringModel.EngineeringModelSetup = engineeringModelSetup;

            this.messageBus.SendObjectChangeEvent(engineeringModelSetup, EventKind.Added);
            Assert.AreEqual(1, this.viewModel.EngineeringModelSetups.Count);

            this.messageBus.SendObjectChangeEvent(engineeringModelSetup, EventKind.Removed);
            Assert.AreEqual(0, this.viewModel.EngineeringModelSetups.Count);
        }

        [Test]
        public void VerifyThatMenuItemsAreSortedAlphabetically()
        {
            Assert.IsEmpty(this.viewModel.EngineeringModelSetups);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(openSessionEvent);

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            siteDirectory.Model.Add(engineeringModelSetup);

            var engineeringModelOne = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            engineeringModelOne.EngineeringModelSetup = engineeringModelSetup;

            this.messageBus.SendObjectChangeEvent(engineeringModelSetup, EventKind.Added);
            var sessionEngineeringModelSetupMenuGroupViewModel = this.viewModel.EngineeringModelSetups.SingleOrDefault(x => x.Thing == engineeringModelSetup.Container);
            var menuItem = new RibbonMenuItemEngineeringModelSetupDependentViewModel(engineeringModelSetup, this.session.Object, null);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Add(menuItem);

            var menuItemTwo = new RibbonMenuItemEngineeringModelSetupDependentViewModel(engineeringModelSetup, this.session.Object, null);
            sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Add(menuItemTwo);

            Assert.AreEqual(3, sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.Count);
            var sortedList = sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups.OrderBy(em => em.MenuItemContent);
            Assert.AreEqual(sortedList, sessionEngineeringModelSetupMenuGroupViewModel.EngineeringModelSetups);
        }

        private class TestClass : RibbonButtonEngineeringModelSetupDependentViewModel
        {
            public TestClass(ICDPMessageBus messageBus) : base(null, messageBus)
            {
            }
        }
    }
}
