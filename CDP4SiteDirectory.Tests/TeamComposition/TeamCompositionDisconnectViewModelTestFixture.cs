// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="TeamCompositionBrowserViewModel"/>
    /// </summary>
    public class TeamCompositionDisconnectViewModelTestFixture
    {
        private SiteDirectory siteDirectory;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.session.Setup(x => x.IsVersionSupported(new Version(1, 0, 0))).Returns(true);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            dal.Setup(x => x.Session).Returns(this.session.Object);
        }

        [Test]
        public void VerifyThatIfSessionIsRemovedItIsRemovedFromMenu()
        {
            var vm = new TeamCompositionBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.AreEqual(1, vm.Sessions.Count);
           
            var menuItem = new RibbonMenuItemEngineeringModelSetupDependentViewModel(new EngineeringModelSetup(), this.session.Object,null);
            var itm = new SessionEngineeringModelSetupMenuGroupViewModel(this.siteDirectory, this.session.Object);
            itm.EngineeringModelSetups.Add(menuItem);
            vm.EngineeringModelSetups.Add(itm);
            
            int cnt = vm.EngineeringModelSetups.Count;
            Assert.IsTrue(cnt > 0);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, vm.Sessions.Count);

            Assert.IsTrue(cnt > vm.EngineeringModelSetups.Count);
        }
    }
}
