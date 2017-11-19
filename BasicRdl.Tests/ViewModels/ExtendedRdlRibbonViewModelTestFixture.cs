// -------------------------------------------------------------------------------------------------
// <copyright file="BasicRDLRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="BasicRDLRibbonViewModel"/>
    /// </summary>
    [TestFixture]
    public class BasicRDLRibbonViewModelTestFixture
    {
        private Uri uri;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService; 
        private Mock<ISession> session;
        private Person person;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyConstantBrowserRibbon()
        {
            var viewmodel = new ConstantBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            viewmodel.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));
        }

        [Test]
        public void VerifyGlossaryBrowserRibbon()
        {
            var viewmodel = new GlossaryBrowserRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            viewmodel.OpenSingleBrowserCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));
        }
    }
}