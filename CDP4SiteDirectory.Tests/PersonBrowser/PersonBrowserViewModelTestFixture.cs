// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
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

    /// <summary>
    /// Suite of tests for the <see cref="PersonBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class PersonBrowserViewModelTestFixture
    {
        private PropertyInfo revision = typeof (Thing).GetProperty("RevisionNumber");
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPanelNavigationService> panelnavigation;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private Person person;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.panelnavigation = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };
            
            this.siteDir.Person.Add(this.person);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyPanelProperties()
        {
            var browser = new PersonBrowserViewModel(this.session.Object, this.siteDir, this.navigation.Object, this.panelnavigation.Object, null, null);
            Assert.AreEqual("Persons, site directory", browser.Caption);
            Assert.AreEqual("site directory\nhttp://www.rheagroup.com/\n ", browser.ToolTip);
            Assert.AreEqual(1, browser.PersonRowViewModels.Count);
            Assert.AreEqual(this.session.Object, browser.Session);
        }

        [Test]
        public void VerifyThatEventAreCaught()
        {
            var vm = new PersonBrowserViewModel(this.session.Object, this.siteDir, this.navigation.Object, this.panelnavigation.Object, null, null);
            var pers = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "new" };
            this.siteDir.Person.Add(pers);

            this.revision.SetValue(this.siteDir, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(2, vm.PersonRowViewModels.Count);

            this.revision.SetValue(this.siteDir, 3);
            this.siteDir.Person.Remove(pers);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(1, vm.PersonRowViewModels.Count);
        }

        [Test]
        public async Task VerifyThatExecuteCreateWorks()
        {
            var vm = new PersonBrowserViewModel(this.session.Object, this.siteDir, this.navigation.Object, this.panelnavigation.Object, null, null);
            await vm.CreateCommand.Execute();

            this.navigation.Verify(x => x.Navigate(It.IsAny<Person>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public async Task VerifyThatEditCommandWorks()
        {
            var vm = new PersonBrowserViewModel(this.session.Object, this.siteDir, this.navigation.Object, this.panelnavigation.Object, null, null);
            vm.SelectedThing = vm.PersonRowViewModels.First();

            await vm.UpdateCommand.Execute();

            this.navigation.Verify(x => x.Navigate(It.IsAny<Person>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Update, this.navigation.Object, It.IsAny<Thing>(), null));
        }
    }
}