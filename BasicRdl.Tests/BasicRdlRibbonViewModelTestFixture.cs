// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using CDP4Composition.Services.FavoritesService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class BasicRdlRibbonViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private readonly Uri uri = new Uri("http://test.com");
        private Person person;
        private Mock<IFavoritesService> favoritesService;
        private Mock<IFilterStringService> filterStringService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();

            this.filterStringService = new Mock<IFilterStringService>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.favoritesService = new Mock<IFavoritesService>();

            this.favoritesService.Setup(x => x.GetFavoriteItemsCollectionByType(It.IsAny<ISession>(), It.IsAny<Type>()))
                .Returns(new HashSet<Guid>());

            this.favoritesService.Setup(x =>
                x.SubscribeToChanges(It.IsAny<ISession>(), It.IsAny<Type>(), It.IsAny<Action<HashSet<Guid>>>())).Returns(new Mock<IDisposable>().Object);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IFavoritesService>()).Returns(this.favoritesService.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatCommandDontDoAnythingWithoutOpenSession()
        {
            var ribbon1 = new CategoryRibbonViewModel(this.messageBus);
            var ribbon2 = new MeasurementScalesRibbonViewModel(this.messageBus);
            var ribbon3 = new MeasurementUnitsRibbonViewModel(this.messageBus);
            var ribbon4 = new RulesRibbonViewModel(this.messageBus);
            var ribbon5 = new ParameterTypeRibbonViewModel(this.messageBus);

            await ribbon1.OpenSingleBrowserCommand.Execute();
            await ribbon2.OpenSingleBrowserCommand.Execute();
            await ribbon3.OpenSingleBrowserCommand.Execute();
            await ribbon4.OpenSingleBrowserCommand.Execute();
            await ribbon5.OpenSingleBrowserCommand.Execute();

            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Never());
            this.navigation.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()), Times.Never());
        }

        [Test]
        public async Task VerifyThatCommandsCallNavigationService()
        {
            var ribbon1 = new CategoryRibbonViewModel(this.messageBus);
            var ribbon2 = new MeasurementScalesRibbonViewModel(this.messageBus);
            var ribbon3 = new MeasurementUnitsRibbonViewModel(this.messageBus);
            var ribbon4 = new RulesRibbonViewModel(this.messageBus);
            var ribbon5 = new ParameterTypeRibbonViewModel(this.messageBus);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            await ribbon1.OpenSingleBrowserCommand.Execute();
            await ribbon2.OpenSingleBrowserCommand.Execute();
            await ribbon3.OpenSingleBrowserCommand.Execute();
            await ribbon4.OpenSingleBrowserCommand.Execute();
            await ribbon5.OpenSingleBrowserCommand.Execute();

            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(5));

            await ribbon1.OpenSingleBrowserCommand.Execute();
            await ribbon2.OpenSingleBrowserCommand.Execute();
            await ribbon3.OpenSingleBrowserCommand.Execute();
            await ribbon4.OpenSingleBrowserCommand.Execute();
            await ribbon5.OpenSingleBrowserCommand.Execute();

            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(10));
        }

        [Test]
        public void VerifyThatCloseSessionEventAreCaught()
        {
            var ribbon1 = new CategoryRibbonViewModel(this.messageBus);
            var ribbon2 = new MeasurementScalesRibbonViewModel(this.messageBus);
            var ribbon3 = new MeasurementUnitsRibbonViewModel(this.messageBus);
            var ribbon4 = new RulesRibbonViewModel(this.messageBus);
            var ribbon5 = new ParameterTypeRibbonViewModel(this.messageBus);

            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));

            Assert.AreEqual(0, ribbon1.OpenSessions.Count);
            Assert.AreEqual(0, ribbon2.OpenSessions.Count);
            Assert.AreEqual(0, ribbon3.OpenSessions.Count);
            Assert.AreEqual(0, ribbon4.OpenSessions.Count);
            Assert.AreEqual(0, ribbon5.OpenSessions.Count);
        }
    }
}
