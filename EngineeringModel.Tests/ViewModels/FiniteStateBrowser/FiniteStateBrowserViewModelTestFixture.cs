// -------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.FiniteStateBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class FiniteStateBrowserViewModelTestFixture
    {
        private PropertyInfo rev;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;

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
            this.rev = typeof (Thing).GetProperty("RevisionNumber");

            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
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
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            Assert.IsNotNullOrEmpty(viewmodel.Caption);
            Assert.IsNotNullOrEmpty(viewmodel.ToolTip);
            Assert.IsNotNullOrEmpty(viewmodel.CurrentModel);
            Assert.IsNotNullOrEmpty(viewmodel.DomainOfExpertise);
            Assert.IsNotNull(viewmodel.CurrentIteration);
        }

        [Test]
        public void VerifyThatTreeIsBuiltCorrectly()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            Assert.IsNotEmpty(viewmodel.FiniteStateList);

            var possibleList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.PossibleFiniteStateList.Add(possibleList);

            this.rev.SetValue(this.iteration, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var possibleListRow = viewmodel.FiniteStateList.FirstOrDefault();
            Assert.IsNotNull(possibleListRow);

            Assert.IsTrue(possibleListRow.ContainedRows.Select(x => x.Thing).Contains(possibleList));

            this.iteration.PossibleFiniteStateList.Clear();
            this.rev.SetValue(this.iteration, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsNotEmpty(viewmodel.FiniteStateList);

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.rev.SetValue(this.iteration, 3);
            this.iteration.ActualFiniteStateList.Add(actualList);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var actualListRow = viewmodel.FiniteStateList.Last();
            Assert.IsNotNull(actualListRow);

            Assert.IsNotEmpty(actualListRow.ContainedRows);

            this.iteration.PossibleFiniteStateList.Add(possibleList);
            this.iteration.ActualFiniteStateList.Remove(actualList);
            this.rev.SetValue(this.iteration, 4);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(2, viewmodel.FiniteStateList.Count);
            Assert.AreSame(possibleListRow, viewmodel.FiniteStateList.First());
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
            });

            var vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, null}
            });

            vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatContextMenuPopulated()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);


            var possibleList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var ps = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            possibleList.PossibleState.Add(ps);

            this.iteration.PossibleFiniteStateList.Add(possibleList);

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualList.PossibleFiniteStateList.Add(possibleList);
            var astate = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            astate.PossibleState.Add(ps);

            actualList.ActualState.Add(astate);

            this.iteration.ActualFiniteStateList.Add(actualList);

            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            
            // no row selected
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            // posible state row selected
            var pslFolder = viewmodel.FiniteStateList.First();
            var psRow = pslFolder.ContainedRows.First().ContainedRows.First();
            viewmodel.SelectedThing = psRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(8, viewmodel.ContextMenu.Count);

            // execute set default
            Assert.IsTrue(viewmodel.CanSetAsDefault);
            viewmodel.SetDefaultStateCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatCanSetDefaultCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            var possibleList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var ps = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            possibleList.PossibleState.Add(ps);

            this.iteration.PossibleFiniteStateList.Add(possibleList);

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualList.PossibleFiniteStateList.Add(possibleList);
            var astate = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            astate.PossibleState.Add(ps);

            actualList.ActualState.Add(astate);

            this.iteration.ActualFiniteStateList.Add(actualList);

            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.IsFalse(viewmodel.CanSetAsDefault);
            viewmodel.SetDefaultStateCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            // posible state row selected
            var pslFolder = viewmodel.FiniteStateList.First();
            var psRow = pslFolder.ContainedRows.First().ContainedRows.First();
            viewmodel.SelectedThing = psRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.IsTrue(viewmodel.CanSetAsDefault);
            viewmodel.SetDefaultStateCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatDisposeWorks()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            viewmodel.Dispose();

            Assert.IsNull(viewmodel.Thing);
        }
    }
}