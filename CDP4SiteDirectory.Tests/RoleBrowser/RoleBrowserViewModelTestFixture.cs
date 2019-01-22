// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
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
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Person person;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
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

        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSelectedSessionAreDisplayedInPropertyGrid()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null);

            viewmodel.SelectedThing = viewmodel.Roles.First();
            this.navigation.Verify(x => x.Open(It.IsAny<Thing>(), this.session.Object));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null);
            
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);

            Assert.IsTrue(viewmodel.CreateParticipantRoleCommand.CanExecute(null));
            Assert.IsTrue(viewmodel.CreatePersonRoleCommand.CanExecute(null));

            var personRoleRow = viewmodel.Roles.First();
            var participantRoleRow = viewmodel.Roles.Last();

            Assert.IsNotEmpty(personRoleRow.ContainedRows);
            Assert.IsNotEmpty(participantRoleRow.ContainedRows);
        }

        [Test]
        public void VerifyThatNewRolesAreAdded()
        {
            var viewmodel = new RoleBrowserViewModel(this.session.Object, this.siteDir, null, this.navigation.Object, null);
            var newPersonRole = new PersonRole(Guid.NewGuid(), null, this.uri);
            var newParticipantRole = new ParticipantRole(Guid.NewGuid(), null, this.uri);

            this.siteDir.PersonRole.Add(newPersonRole);
            this.siteDir.ParticipantRole.Add(newParticipantRole);

            var rev = typeof (Thing).GetProperty("RevisionNumber");
            rev.SetValue(this.siteDir, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            var personRoleRow = viewmodel.Roles.First();
            var participantRoleRow = viewmodel.Roles.Last();

            Assert.AreEqual(2, personRoleRow.ContainedRows.Count);
            Assert.AreEqual(2, participantRoleRow.ContainedRows.Count);

            this.siteDir.PersonRole.Clear();
            this.siteDir.ParticipantRole.Clear();

            rev.SetValue(this.siteDir, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.AreEqual(0, personRoleRow.ContainedRows.Count);
            Assert.AreEqual(0, participantRoleRow.ContainedRows.Count);

            viewmodel.Dispose();
        }
    }
}