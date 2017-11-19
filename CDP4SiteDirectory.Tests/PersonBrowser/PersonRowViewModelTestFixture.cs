// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="PersonRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class PersonRowViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
        [Test]
        public void VerifyThatPropertiesAreSetProperly()
        {
            var participantRole = new ParticipantRole(Guid.NewGuid(), null, this.uri) { Container = this.siteDir };
            var person = new Person(Guid.NewGuid(), null, this.uri) { Container = this.siteDir };
            var participant = new Participant(Guid.NewGuid(), null, this.uri)
            {
                Container = this.siteDir,
                Person = person,
                Role = participantRole
            };
            var engModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            engModelSetup.Participant.Add(participant);
            this.siteDir.Model.Add(engModelSetup);
            var personRow = new PersonRowViewModel(person, this.session.Object, null);

            Assert.AreEqual(personRow.GivenName, person.GivenName);
            Assert.AreEqual(personRow.Surname, person.Surname);
            Assert.AreEqual(1, personRow.ContainedRows.Count);
        }
    }
}