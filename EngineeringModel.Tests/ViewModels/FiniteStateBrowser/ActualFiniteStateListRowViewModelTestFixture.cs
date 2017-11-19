// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.FiniteStateBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ActualFiniteStateListRowViewModelTestFixture
    {
        private PropertyInfo rev;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://test.com");
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            this.rev = typeof(Thing).GetProperty("RevisionNumber");

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

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

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
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
            var possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.PossibleFiniteStateList.Add(possibleFiniteStateList);
            var possibleFiniteState = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState);

            var actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);

            this.iteration.ActualFiniteStateList.Add(actualFiniteStateList);
            var row = new ActualFiniteStateListRowViewModel(actualFiniteStateList, this.session.Object, null);

            Assert.IsEmpty(row.ContainedRows);
            var state = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            state.PossibleState.Add(possibleFiniteState);
            actualFiniteStateList.ActualState.Add(state);

            this.rev.SetValue(actualFiniteStateList, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(actualFiniteStateList, EventKind.Updated);
            var staterow = (ActualFiniteStateRowViewModel)row.ContainedRows.SingleOrDefault();
            Assert.IsNotNull(staterow);
        }

        [Test]
        public void VerifyThatTreeIsCorrectlyUpdated()
        {
            var possibleStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var possibleState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "1" };
            var possibleState2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "2" };
            var possibleState3 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "3" };
            possibleStateList.PossibleState.Add(possibleState1);
            possibleStateList.PossibleState.Add(possibleState2);
            possibleStateList.PossibleState.Add(possibleState3);

            this.iteration.PossibleFiniteStateList.Add(possibleStateList);
            var actualStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var actualState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri) { PossibleState = new List<PossibleFiniteState> { possibleState1 } };
            var actualState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri) { PossibleState = new List<PossibleFiniteState> { possibleState2 } };
            var actualState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri) { PossibleState = new List<PossibleFiniteState> { possibleState3 } };
            actualStateList.ActualState.Add(actualState1);
            actualStateList.ActualState.Add(actualState2);
            actualStateList.ActualState.Add(actualState3);

            actualStateList.PossibleFiniteStateList.Add(possibleStateList);
            this.iteration.ActualFiniteStateList.Add(actualStateList);

            var actualFiniteStateListRow = new ActualFiniteStateListRowViewModel(actualStateList, this.session.Object, null);
            Assert.AreEqual(3, actualFiniteStateListRow.ContainedRows.Count);
            Assert.AreEqual(possibleState1.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[0]).Name);
            Assert.AreEqual(possibleState2.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[1]).Name);
            Assert.AreEqual(possibleState3.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[2]).Name);

            var possibleState4 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "4" };
            var actualState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri) { PossibleState = new List<PossibleFiniteState> { possibleState4 } };
            possibleStateList.PossibleState.Add(possibleState4);
            possibleStateList.PossibleState.Remove(possibleStateList.PossibleState[1]);
            actualStateList.ActualState.Add(actualState4);
            actualStateList.ActualState.Remove(actualStateList.ActualState[1]);

            possibleStateList.RevisionNumber = 1;
            CDPMessageBus.Current.SendObjectChangeEvent(possibleStateList, EventKind.Updated);

            Assert.AreEqual(3, actualFiniteStateListRow.ContainedRows.Count);
            Assert.AreEqual(possibleState1.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[0]).Name);
            Assert.AreEqual(possibleState3.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[1]).Name);
            Assert.AreEqual(possibleState4.Name, ((ActualFiniteStateRowViewModel)actualFiniteStateListRow.ContainedRows[2]).Name);
        }
    }
}