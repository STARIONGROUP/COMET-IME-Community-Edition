// -------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserRibbonPageViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4ObjectBrowser.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    using Thing = CDP4Common.CommonData.Thing;

    [TestFixture]
    public class ObjectBrowserRibbonPageViewModelTestFixture
    {
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IServiceLocator> servicelocator;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private Person person;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");

            this.person = new Person { GivenName = "John", Surname = "Doe" };
            this.siteDirectory = new SiteDirectory();
            
            this.session = new Mock<ISession>();
            
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);

            this.navigationService = new Mock<IPanelNavigationService>();
            this.servicelocator = new Mock<IServiceLocator>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            ServiceLocator.SetLocatorProvider(() => this.servicelocator.Object);
            this.servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatCommandWorks()
        {
            var vm = new ObjectBrowserRibbonPageViewModel();
            Assert.IsEmpty(vm.OpenSessions); 
            
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, vm.OpenSessions.Count);

            vm.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));

            vm.OpenSingleBrowserCommand.Execute(null);
            this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true));            
        }
    }
}
