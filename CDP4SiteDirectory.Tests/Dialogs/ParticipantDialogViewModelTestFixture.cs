// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ParticipantDialogViewModelTestFixture
    {
        private Uri uri;
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService; 
        private SiteDirectory sitedir;
        private EngineeringModelSetup model;
        private Person person;
        private DomainOfExpertise domain;
        private ParticipantRole role;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private EngineeringModelSetup clone;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.person.DefaultDomain = this.domain;
            this.role = new ParticipantRole(Guid.NewGuid(), this.cache, this.uri);

            this.sitedir.ParticipantRole.Add(this.role);
            this.sitedir.Model.Add(this.model);
            this.sitedir.Domain.Add(this.domain);
            this.sitedir.Person.Add(this.person);

            this.model.ActiveDomain.Add(this.domain);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.clone = this.model.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.sitedir);
            this.thingTransaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            var dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, dialog.PossibleDomain.Count);
            Assert.AreEqual(1, dialog.PossiblePerson.Count);
            Assert.AreEqual(1, dialog.PossibleRole.Count);

            dialog.Domain = new ReactiveList<DomainOfExpertise> { this.domain };

            Assert.IsNotNull(dialog.SelectedSelectedDomain);

            dialog.SelectedPerson = this.person;
            dialog.SelectedRole = this.role;

            Assert.IsTrue(dialog.OkCanExecute);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var participantDialogViewModel = new ParticipantDialogViewModel();
            Assert.IsNotNull(participantDialogViewModel);
        }

        [Test]
        public void VerifyPossiblePerson()
        {
            var participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person };
            this.clone.Participant.Add(participant);
            var dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(0, dialog.PossiblePerson.Count);

            this.sitedir.Person.Add(new Person(Guid.NewGuid(), this.cache, this.uri));
            participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            this.clone.Participant.Clear();
            dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object,true, ThingDialogKind.Create,
                this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(2, dialog.PossiblePerson.Count);
            dialog.SelectedPerson = this.person;
            Assert.IsTrue(dialog.Domain.Contains(this.domain));

            participant.Person = this.person;
            dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, dialog.PossiblePerson.Count);
        }
    }
}