// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.FavoritesService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
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

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

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
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatCommandDontDoAnythingWithoutOpenSession()
        {
            var ribbon1 = new CategoryRibbonViewModel();
            var ribbon2 = new MeasurementScalesRibbonViewModel();
            var ribbon3 = new MeasurementUnitsRibbonViewModel();
            var ribbon4 = new RulesRibbonViewModel();
            var ribbon5 = new ParameterTypeRibbonViewModel();

            ribbon1.OpenSingleBrowserCommand.Execute(null);
            ribbon2.OpenSingleBrowserCommand.Execute(null);
            ribbon3.OpenSingleBrowserCommand.Execute(null);
            ribbon4.OpenSingleBrowserCommand.Execute(null);
            ribbon5.OpenSingleBrowserCommand.Execute(null);

            this.navigation.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Never());
            this.navigation.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true), Times.Never());
        }

        [Test]
        public void VerifyThatCommandsCallNavigationService()
        {
            var ribbon1 = new CategoryRibbonViewModel();
            var ribbon2 = new MeasurementScalesRibbonViewModel();
            var ribbon3 = new MeasurementUnitsRibbonViewModel();
            var ribbon4 = new RulesRibbonViewModel();
            var ribbon5 = new ParameterTypeRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            ribbon1.OpenSingleBrowserCommand.Execute(null);
            ribbon2.OpenSingleBrowserCommand.Execute(null);
            ribbon3.OpenSingleBrowserCommand.Execute(null);
            ribbon4.OpenSingleBrowserCommand.Execute(null);
            ribbon5.OpenSingleBrowserCommand.Execute(null);

            this.navigation.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(5));

            ribbon1.OpenSingleBrowserCommand.Execute(null);
            ribbon2.OpenSingleBrowserCommand.Execute(null);
            ribbon3.OpenSingleBrowserCommand.Execute(null);
            ribbon4.OpenSingleBrowserCommand.Execute(null);
            ribbon5.OpenSingleBrowserCommand.Execute(null);

            this.navigation.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true), Times.Exactly(5));
        }

        [Test]
        public void VerifyThatCloseSessionEventAreCaught()
        {
            var ribbon1 = new CategoryRibbonViewModel();
            var ribbon2 = new MeasurementScalesRibbonViewModel();
            var ribbon3 = new MeasurementUnitsRibbonViewModel();
            var ribbon4 = new RulesRibbonViewModel();
            var ribbon5 = new ParameterTypeRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));

            Assert.AreEqual(0, ribbon1.OpenSessions.Count);
            Assert.AreEqual(0, ribbon2.OpenSessions.Count);
            Assert.AreEqual(0, ribbon3.OpenSessions.Count);
            Assert.AreEqual(0, ribbon4.OpenSessions.Count);
            Assert.AreEqual(0, ribbon5.OpenSessions.Count);
        }
    }
}