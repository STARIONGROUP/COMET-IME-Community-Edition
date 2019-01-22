// -------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowserRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace BasicRdl.Tests.ViewModels.Ribbons
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="GlossaryBrowserRibbonViewModel"/> class
    /// </summary>
    [TestFixture]
    public class GlossaryBrowserRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IServiceLocator> serviceLocator;
        private Assembler assembler;

        private SiteDirectory sitedir;
        private Person person;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.panelNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);

            this.sitedir.Person.Add(this.person);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
        }

        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRibbonViewModelCanBeConstructed()
        {
            Assert.DoesNotThrow(() => new GlossaryBrowserRibbonViewModel());
        }

        [Test]
        public void VerifyThatInstantiatePanelViewModelReturnsExpectedViewModel()
        {
            var viewmodel = GlossaryBrowserRibbonViewModel.InstantiatePanelViewModel(
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object);

            Assert.IsInstanceOf<GlossaryBrowserViewModel>(viewmodel);
        }
    }
}
