// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

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
    /// Suite of tests for the <see cref="TeamCompositionBrowserViewModel"/>
    /// </summary>
    public class TeamCompositionBrowserViewModelTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");

        private SiteDirectory siteDir;
        private EngineeringModelSetup engineeringModelSetup;
        private DomainOfExpertise systemEngineering;
        private DomainOfExpertise powerEngineering;
        private Person person;
        private PersonRole personRole;
        private ParticipantRole participantRole;
        private Participant participant;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.personRole = new PersonRole(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Admimistrator",
                ShortName = "Admin"
            };

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri) { Name = "site directory" };

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "test",
                Role = this.personRole
            };

            this.siteDir.Person.Add(this.person);

            this.systemEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SYS",
                Name = "System"
            };

            this.powerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "PWR",
                Name = "Power"
            };

            this.participantRole = new ParticipantRole(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "DomainExpert",
                Name = "Domain Expert"
            };

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModelSetup.ShortName = "testmodel";
            this.engineeringModelSetup.Name = "test model";

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person,
                Role = this.participantRole
            };

            this.participant.Domain.Add(this.systemEngineering);
            this.participant.Domain.Add(this.powerEngineering);

            this.engineeringModelSetup.Participant.Add(this.participant);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new TeamCompositionBrowserViewModel(this.engineeringModelSetup, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            Assert.AreEqual("Team Composition, test model", vm.Caption);
            Assert.AreEqual(1, vm.Participants.Count);

            var row = vm.Participants.Single();
            Assert.AreEqual(this.participant, row.Thing);
        }

        [Test]
        public void VerifyThatIfNewParticipantIsAddedItIsAddedToTheBrowser()
        {
            var vm = new TeamCompositionBrowserViewModel(this.engineeringModelSetup, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            Assert.AreEqual(1, vm.Participants.Count);

            var anotherPerson = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "Jane", Surname = "Doe" };
            anotherPerson.Role = this.personRole;

            var anotherParticipant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = anotherPerson,
                Role = this.participantRole
            };

            anotherParticipant.Domain.Add(this.systemEngineering);
            this.engineeringModelSetup.Participant.Add(anotherParticipant);

            var revisionNumber = typeof(Thing).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.engineeringModelSetup, this.engineeringModelSetup.RevisionNumber + 1);

            this.messageBus.SendObjectChangeEvent(anotherPerson, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);

            Assert.AreEqual(2, vm.Participants.Count);
        }

        [Test]
        public void VerifyThatIfParticipantIsRemovedItIsRemovedFromTheBrowser()
        {
            var vm = new TeamCompositionBrowserViewModel(this.engineeringModelSetup, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            Assert.AreEqual(1, vm.Participants.Count);
            this.engineeringModelSetup.Participant.Clear();

            var revisionNumber = typeof(Thing).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.engineeringModelSetup, this.engineeringModelSetup.RevisionNumber + 1);

            this.messageBus.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);

            Assert.AreEqual(0, vm.Participants.Count);
        }
    }
}
