// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.FiniteStateBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class PossibleFiniteStateListRowViewModelTestFixture
    {
        private PropertyInfo rev;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            this.rev = typeof(Thing).GetProperty("RevisionNumber");

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTreeIsCorrectlyBuilt()
        {
            var list = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.PossibleFiniteStateList.Add(list);
            var row = new PossibleFiniteStateListRowViewModel(list, this.session.Object, null);

            Assert.IsEmpty(row.ContainedRows);
            var state = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            list.PossibleState.Add(state);

            rev.SetValue(list, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(list, EventKind.Updated);
            var staterow = (PossibleFiniteStateRowViewModel)row.ContainedRows.SingleOrDefault();
            Assert.IsNotNull(staterow);
        }

        [Test]
        public void VerifyThatOrderOfPossibleStatesIsUpdated()
        {
            var list = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.PossibleFiniteStateList.Add(list);
            var row = new PossibleFiniteStateListRowViewModel(list, this.session.Object, null);

            var state1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "1" };
            var state2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "2" };
            list.PossibleState.Add(state1);
            list.PossibleState.Add(state2);

            this.rev.SetValue(list, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(list, EventKind.Updated);
            Assert.AreEqual(2, row.ContainedRows.Count);
            Assert.AreEqual("1", ((PossibleFiniteStateRowViewModel)row.ContainedRows.First()).Name);
            Assert.AreEqual("2", ((PossibleFiniteStateRowViewModel)row.ContainedRows.Last()).Name);

            list.PossibleState.Move(1, 0);
            this.rev.SetValue(list, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(list, EventKind.Updated);
            Assert.AreEqual("2", ((PossibleFiniteStateRowViewModel)row.ContainedRows.First()).Name);
            Assert.AreEqual("1", ((PossibleFiniteStateRowViewModel)row.ContainedRows.Last()).Name);
        }
    }
}