// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests.OrganizationBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class OrganizationBrowserViewModelTestFixture
    {
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
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
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatEventAreCaught()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);

            this.revision.SetValue(this.siteDir, 2);
            this.siteDir.Organization.Add(this.orga2);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(2, vm.Organizations.Count);

            var row = vm.Organizations.Single(x => x.Thing == this.orga2);
            Assert.AreEqual(1, row.ContainedRows.Count);

            this.revision.SetValue(this.siteDir, 20);
            this.person.Organization = null;
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(0, row.ContainedRows.Count);

            this.revision.SetValue(this.siteDir, 30);
            this.siteDir.Organization.Remove(this.orga2);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(1, vm.Organizations.Count);
        }

        [Test]
        public void VerifyStringProperties()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);

            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatUpdateOnOrganizationAreCaught()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);

            this.orga1.Name = "rhea";

            // workaround to modify a read-only field
            var type = this.orga1.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.orga1, 50);

            this.messageBus.SendObjectChangeEvent(this.orga1, EventKind.Updated);

            var org = vm.Organizations.Single();
            Assert.AreEqual("rhea", org.Name);
        }

        [Test]
        public void VerifyThatReactiveCommandCanExecuteProperly()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.IsTrue(((ICommand)vm.CreateCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)vm.InspectCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)vm.UpdateCommand).CanExecute(null));

            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            this.messageBus.SendObjectChangeEvent(organization, EventKind.Added);

            vm.SelectedThing = vm.Organizations.Single();
            vm.ComputePermission();

            Assert.IsTrue(((ICommand)vm.InspectCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)vm.UpdateCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatReactiveCommandsOpenDialogsOnExecute()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);
            await vm.CreateCommand.Execute();
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Organization>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<Thing>(), null));

            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            this.messageBus.SendObjectChangeEvent(organization, EventKind.Added);

            await vm.InspectCommand.Execute();
            Assert.Throws<MockException>(() => this.dialogNavigation.Verify(x => x.Navigate(organization, It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.dialogNavigation.Object, It.IsAny<Thing>(), null)));

            vm.SelectedThing = vm.Organizations.Single();

            await vm.InspectCommand.Execute();
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Organization>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.dialogNavigation.Object, It.IsAny<Thing>(), null));

            await vm.UpdateCommand.Execute();
        }

        [Test]
        public void VerifyThatAnOrganizationCantBeAddedMoreThanOnce()
        {
            var vm = new OrganizationBrowserViewModel(this.session.Object, this.siteDir, this.dialogNavigation.Object, this.navigation.Object, null, null);
            var organization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "1", ShortName = "1" };
            this.messageBus.SendObjectChangeEvent(organization, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(organization, EventKind.Added);
            Assert.AreEqual(1, vm.Thing.Organization.Count);
        }
    }
}
