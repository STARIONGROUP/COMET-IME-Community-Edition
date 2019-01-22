// -------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.OrganizationBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
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
    public class OrganizationBrowserViewModelTestFixture
    {
        private readonly PropertyInfo revision = typeof (Thing).GetProperty("RevisionNumber");
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private Organization orga1;
        private Organization orga2;
        private Uri uri;
        private Person person;

        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPermissionService> permissionService;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://test.com");
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri) { Name = "SiteDir" };
            this.orga1 = new Organization(Guid.NewGuid(), this.cache, this.uri) { Name = "1", ShortName = "1" };
            this.orga2 = new Organization(Guid.NewGuid(), this.cache, this.uri) { Name = "1", ShortName = "1" };

            this.siteDir.Organization.Add(this.orga1);

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "Test" };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.person.Organization = this.orga2;
            this.siteDir.Person.Add(this.person);

            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

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
        public void VerifyThatEventAreCaught()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);

            revision.SetValue(this.siteDir, 2);
            this.siteDir.Organization.Add(this.orga2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(2, vm.Organizations.Count);

            var row = vm.Organizations.Single(x => x.Thing == this.orga2);
            Assert.AreEqual(1, row.ContainedRows.Count);

            revision.SetValue(this.siteDir, 20);
            this.person.Organization = null;
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(0, row.ContainedRows.Count);

            revision.SetValue(this.siteDir, 30);
            this.siteDir.Organization.Remove(this.orga2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(1, vm.Organizations.Count);
        }

        [Test]
        public void VerifyStringProperties()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);

            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatUpdateOnOrganizationAreCaught()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);

            this.orga1.Name = "rhea";

            // workaround to modify a read-only field
            var type = this.orga1.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.orga1, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.orga1, EventKind.Updated);

            var org = vm.Organizations.Single();
            Assert.AreEqual("rhea", org.Name);
        }

        [Test]
        public void VerifyThatReactiveCommandCanExecuteProperly()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);
            Assert.IsTrue(vm.CreateCommand.CanExecute(null));
            Assert.IsFalse(vm.InspectCommand.CanExecute(null));
            Assert.IsFalse(vm.UpdateCommand.CanExecute(null));

            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            CDPMessageBus.Current.SendObjectChangeEvent(organization, EventKind.Added);

            vm.SelectedThing = vm.Organizations.Single();
            vm.ComputePermission();

            Assert.IsTrue(vm.InspectCommand.CanExecute(null));
            Assert.IsTrue(vm.UpdateCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatReactiveCommandsOpenDialogsOnExecute()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);
            vm.CreateCommand.Execute(null);
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Organization>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<Thing>(), null));

            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            CDPMessageBus.Current.SendObjectChangeEvent(organization, EventKind.Added);

            vm.InspectCommand.Execute(null);
            Assert.Throws<MockException>(() => this.dialogNavigation.Verify(x => x.Navigate(organization, It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.dialogNavigation.Object, It.IsAny<Thing>(), null)));

            vm.SelectedThing = vm.Organizations.Single();

            vm.InspectCommand.Execute(null);
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Organization>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.dialogNavigation.Object, It.IsAny<Thing>(), null));
            
            vm.UpdateCommand.Execute(null);
        }

        [Test]
        public void VerifyThatAnOrganizationCantBeAddedMoreThanOnce()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null);
            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            CDPMessageBus.Current.SendObjectChangeEvent(organization, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(organization, EventKind.Added);
            Assert.AreEqual(1, vm.Thing.Organization.Count);
        }
    }
}