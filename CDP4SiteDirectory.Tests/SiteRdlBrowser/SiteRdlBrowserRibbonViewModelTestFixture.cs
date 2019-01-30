// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.SiteRdlBrowser
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class SiteRdlBrowserRibbonViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator; 
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private SiteDirectory siteDir;
        private Uri uri;
        private Person person;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
            
        [Test]
        public void VerifyThatSessionEventAreCaught()
        {
            var ribbonViewModel = new SiteRdlBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.AreEqual(1, ribbonViewModel.OpenSessions.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, ribbonViewModel.OpenSessions.Count);
        }

        [Test]
        public void VerifyThatInstantiateWorks()
        {
            var vm = SiteRdlBrowserRibbonViewModel.InstantiatePanelViewModel(this.session.Object, null, null, null, null);
            Assert.IsNotNull(vm);
        }
    }
}