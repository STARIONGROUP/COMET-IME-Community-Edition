// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionCardViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.TeamComposition
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using System.Security.Policy;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class TeamCompositionCardViewModelTestFixture
    {
        private Uri url;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;

        private DomainOfExpertise domainOfExpertise1;
        private DomainOfExpertise domainOfExpertise2;

        private Participant participant;
        private PersonRole personRole;
        private ParticipantRole participantRole;
        private Organization organization;
        private Person person;
        private TeamCompositionCardViewModel teamCompositionCardViewModel;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.url = new Uri("http://www.rheagroup.com");
            
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.domainOfExpertise1 = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.url) {ShortName = "SYS"};
            this.domainOfExpertise2 = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.url) { ShortName = "THR" };

            this.organization = new Organization(Guid.NewGuid(), this.cache, this.url);
            this.organization.ShortName = "RHEA";
            this.organization.Name = "RHEA";

            this.personRole = new PersonRole(Guid.NewGuid(), this.cache, this.url);
            this.person = new Person(Guid.NewGuid(), this.cache, this.url)
                              {
                                  OrganizationalUnit = "SESS",
                                  Organization = this.organization,
                                  Role = this.personRole
                              };

            this.participantRole = new ParticipantRole(Guid.NewGuid(), this.cache, this.url);
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.url);
            this.participant.Person = this.person;
            this.participant.Role = this.participantRole;
            this.participant.IsActive = true;
            
            this.participant.Domain.Add(this.domainOfExpertise1);
            this.participant.Domain.Add(this.domainOfExpertise2);


            this.session.Setup(x => x.DataSourceUri).Returns(this.url.ToString);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual("RHEA", this.teamCompositionCardViewModel.Organization);
            Assert.AreEqual("SESS", this.teamCompositionCardViewModel.OrganizationalUnit);
            Assert.AreEqual("SYS THR", this.teamCompositionCardViewModel.DomainShortnames);
            Assert.IsTrue(this.teamCompositionCardViewModel.IsActive);
        }

        [Test]
        public void VerifyThatPersonUpdatesAreProcessed()
        {
            this.person.GivenName = "John";
            this.person.Surname = "Doe";

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(this.person.Name, this.teamCompositionCardViewModel.Person);

            this.person.GivenName = "Jane";
            var type = this.person.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.person, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);

            Assert.AreEqual(this.person.Name, this.teamCompositionCardViewModel.Person); 
        }

        [Test]
        public void VerifyThatPersonRoleUpdatesAreProcessed()
        {
            this.personRole.Name = "Person Role";

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(this.personRole.Name, this.teamCompositionCardViewModel.PersonRole);

            this.personRole.Name = "Updated Person Role";
            var type = this.personRole.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.personRole, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.personRole, EventKind.Updated);

            Assert.AreEqual(this.personRole.Name, this.teamCompositionCardViewModel.PersonRole);
        }

        [Test]
        public void VerifyThatParticipantRoleUpdatesAreProcessed()
        {
            this.participantRole.Name = "Participant Role";

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(this.participantRole.Name, this.teamCompositionCardViewModel.ParticipantRole);

            this.participantRole.Name = "Updated Participant Role";
            var type = this.participantRole.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.participantRole, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.participantRole, EventKind.Updated);

            Assert.AreEqual(this.participantRole.Name, this.teamCompositionCardViewModel.ParticipantRole);
        }

        [Test]
        public void VerifyThatEmailCanExecuteIsFalseWhenNoEmailExists()
        {
            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.IsNullOrEmpty(this.teamCompositionCardViewModel.EmailAddress); 

            Assert.IsFalse(this.teamCompositionCardViewModel.OpenEmail.CanExecute(null));
        }

        [Test]
        public void VerifyThatIfEmailIsPresentEmailCanExecuteIsTrue()
        {
            var email = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "johndoe@rheagroup.com" };
            this.person.EmailAddress.Add(email);

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(email.Value, this.teamCompositionCardViewModel.EmailAddress);

            Assert.IsTrue(this.teamCompositionCardViewModel.OpenEmail.CanExecute(null));
        }

        [Test]
        public void VerifThatDefaultEmailAddresIsUsed()
        {
            var email = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "johndoe@rheagroup.com" };
            this.person.EmailAddress.Add(email);

            var defaultEmail = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "default@rheagroup.com" };
            this.person.EmailAddress.Add(defaultEmail);
            this.person.DefaultEmailAddress = defaultEmail;

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(defaultEmail.Value, this.teamCompositionCardViewModel.EmailAddress);
        }

        [Test]
        public void VerifuThatIfNoDefaultEmailIsSetFirstEmailIsUsed()
        {
            var firstEmail = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "johndoe@rheagroup.com" };
            this.person.EmailAddress.Add(firstEmail);

            var secondEmail = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "default@rheagroup.com" };
            this.person.EmailAddress.Add(secondEmail);
            
            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(firstEmail.Value, this.teamCompositionCardViewModel.EmailAddress);
        }

        [Test]
        public void VerifyThatIfNoDefaultTelephoneNumberIsUsedFirsTelephoneNumberIsUsed()
        {
            var firstNumber = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            firstNumber.Value = "123456879";
            this.person.TelephoneNumber.Add(firstNumber);

            var secondNumner = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            secondNumner.Value = "987654321";
            this.person.TelephoneNumber.Add(secondNumner);

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(firstNumber.Value, this.teamCompositionCardViewModel.TelephoneNumber);
        }

        [Test]
        public void VerifyThatifDefaultNumberIsSetThatOneIsUSed()
        {
            var firstNumber = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            firstNumber.Value = "123456879";
            this.person.TelephoneNumber.Add(firstNumber);

            var secondNumner = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            secondNumner.Value = "987654321";
            this.person.TelephoneNumber.Add(secondNumner);

            this.person.DefaultTelephoneNumber = secondNumner;

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(secondNumner.Value, this.teamCompositionCardViewModel.TelephoneNumber);
        }

        [Test]
        public void VerifyThatUpdateToTelephoneNumberAreProcessed()
        {
            var firstNumber = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            firstNumber.Value = "123456879";
            this.person.TelephoneNumber.Add(firstNumber);

            var secondNumner = new TelephoneNumber(Guid.NewGuid(), this.cache, this.url);
            secondNumner.Value = "987654321";
            this.person.TelephoneNumber.Add(secondNumner);

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            Assert.AreEqual(firstNumber.Value, this.teamCompositionCardViewModel.TelephoneNumber);

            firstNumber.Value = "456789123";
            var type = firstNumber.GetType();
            type.GetProperty("RevisionNumber").SetValue(firstNumber, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(firstNumber, EventKind.Updated);

            Assert.AreEqual(firstNumber.Value, this.teamCompositionCardViewModel.TelephoneNumber);
        }

        [Test]
        public void VerifyThatUpdateToEmailAddressIsProcessed()
        {
            var firstEmail = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "johndoe@rheagroup.com" };
            this.person.EmailAddress.Add(firstEmail);

            var secondEmail = new EmailAddress(Guid.NewGuid(), this.cache, this.url) { Value = "default@rheagroup.com" };
            this.person.EmailAddress.Add(secondEmail);

            this.teamCompositionCardViewModel = new TeamCompositionCardViewModel(this.participant, this.session.Object, null);

            firstEmail.Value = "johnsemail@rheagroup.com";
            var type = firstEmail.GetType();
            type.GetProperty("RevisionNumber").SetValue(firstEmail, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(firstEmail, EventKind.Updated);

            Assert.AreEqual(firstEmail.Value, this.teamCompositionCardViewModel.EmailAddress);
        }
    }
}
