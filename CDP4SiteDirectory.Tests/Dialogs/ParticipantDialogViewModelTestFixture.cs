// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
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
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private EngineeringModelSetup clone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

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
            this.cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.clone = this.model.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.sitedir);
            this.thingTransaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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

            dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(2, dialog.PossiblePerson.Count);
            dialog.SelectedPerson = this.person;
            Assert.IsTrue(dialog.Domain.Contains(this.domain));

            participant.Person = this.person;
            dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, dialog.PossiblePerson.Count);
        }

        [Test]
        public void VerifyOkCanExecute()
        {
            var participant = new Participant(Guid.NewGuid(), this.cache, this.uri);

            var dialog = new ParticipantDialogViewModel(participant, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.IsFalse(dialog.OkCanExecute);

            dialog.SelectedPerson = this.person;
            Assert.IsFalse(dialog.OkCanExecute);

            dialog.SelectedRole = this.role;
            Assert.IsTrue(dialog.OkCanExecute);

            dialog.Domain = new ReactiveList<DomainOfExpertise> { this.domain };
            Assert.IsTrue(dialog.OkCanExecute);

            dialog.Domain.Clear();
            Assert.IsFalse(dialog.OkCanExecute);

            dialog.Domain = new ReactiveList<DomainOfExpertise> { this.domain };
            Assert.IsTrue(dialog.OkCanExecute);

            dialog.SelectedSelectedDomain = null;
            Assert.IsFalse(dialog.OkCanExecute);

            dialog.SelectedSelectedDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            Assert.IsFalse(dialog.OkCanExecute);

            dialog.Domain.Add(dialog.SelectedSelectedDomain);
            Assert.IsTrue(dialog.OkCanExecute);
        }
    }
}
