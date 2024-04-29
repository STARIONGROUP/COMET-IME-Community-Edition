// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RoleBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class RoleBrowserViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private PersonRole personRole;
        private ParticipantRole participantRole;
        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private Person person;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };
            this.personRole = new PersonRole(Guid.NewGuid(), null, this.uri);
            this.participantRole = new ParticipantRole(Guid.NewGuid(), null, this.uri);
            this.siteDir.ParticipantRole.Add(this.participantRole);
            this.siteDir.PersonRole.Add(this.personRole);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSelectedSessionAreDisplayedInPropertyGrid()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null, null);

            var selectedThingChangedRaised = false;
            this.messageBus.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            viewmodel.SelectedThing = viewmodel.Roles.First();
            Assert.IsTrue(selectedThingChangedRaised);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null, null);

            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);

            Assert.IsTrue(((ICommand)viewmodel.CreateParticipantRoleCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)viewmodel.CreatePersonRoleCommand).CanExecute(null));

            var personRoleRow = viewmodel.Roles.First();
            var participantRoleRow = viewmodel.Roles.Last();

            Assert.IsNotEmpty(personRoleRow.ContainedRows);
            Assert.IsNotEmpty(participantRoleRow.ContainedRows);
        }

        [Test]
        public void VerifyThatNewRolesAreAdded()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null, null);
            var newPersonRole = new PersonRole(Guid.NewGuid(), null, this.uri);
            var newParticipantRole = new ParticipantRole(Guid.NewGuid(), null, this.uri);

            this.siteDir.PersonRole.Add(newPersonRole);
            this.siteDir.ParticipantRole.Add(newParticipantRole);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(this.siteDir, 3);

            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            var personRoleRow = viewmodel.Roles.First();
            var participantRoleRow = viewmodel.Roles.Last();

            Assert.AreEqual(2, personRoleRow.ContainedRows.Count);
            Assert.AreEqual(2, participantRoleRow.ContainedRows.Count);

            this.siteDir.PersonRole.Clear();
            this.siteDir.ParticipantRole.Clear();

            rev.SetValue(this.siteDir, 5);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(0, personRoleRow.ContainedRows.Count);
            Assert.AreEqual(0, participantRoleRow.ContainedRows.Count);

            viewmodel.Dispose();
        }
    }
}
