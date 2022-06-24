// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.SiteRdlBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class SiteRdlBrowserViewModelTestFixture
    {
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDir;
        private Uri uri;
        private Mock<ISession> session;
        private Person person;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService; 
        private PropertyInfo revInfo = typeof (Thing).GetProperty("RevisionNumber");
            
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };

            var rdl1 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl1", ShortName = "1" };
            var rdl2 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl2", ShortName = "2" };
            var rdl21 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl21", ShortName = "21", RequiredRdl = rdl2 };

            this.siteDir.SiteReferenceDataLibrary.Add(rdl1);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl2);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl21);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsArePopulated()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.AreEqual(3, viewModel.SiteRdls.Count);
            Assert.That(viewModel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewModel.ToolTip, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatEventAreCaught()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "rdl0", ShortName = "0", Container = this.siteDir};
            
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.revInfo.SetValue(this.siteDir, 10);
            
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(4, viewModel.SiteRdls.Count);

            this.siteDir.SiteReferenceDataLibrary.Remove(rdl);
            this.revInfo.SetValue(this.siteDir, 20);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(3, viewModel.SiteRdls.Count);

            var rdl21 = viewModel.SiteRdls.SingleOrDefault(x => x.Name == "rdl21" && x.ShortName == "21" && x.RequiredRdlShortName == "2");
            Assert.IsNotNull(rdl21);
        }

        [Test]
        public void VerifyThatDiposeWorks()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            viewModel.Dispose();

            Assert.IsNull(viewModel.Thing);
        }

        [Test]
        public async Task VerifyThatCreateCommandWorks()
        {
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<SiteDirectory>())).Returns(true);
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, this.thingDialogNavigationService.Object, null, null, null);

            viewModel.ComputePermission();
            Assert.IsTrue(viewModel.CanCreateSiteRdl);

            viewModel.PopulateContextMenu();
            Assert.AreEqual(1, viewModel.ContextMenu.Count);

            await viewModel.CreateCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<SiteReferenceDataLibrary>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<SiteDirectory>(), null));
        }

    }
}