// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.FiniteStateBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels;
    
    using Moq;
    
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="FiniteStateBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    internal class FiniteStateBrowserViewModelTestFixture
    {
        private PropertyInfo rev;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IParameterActualFiniteStateListApplicationBatchService> parameterActualFiniteStateListApplicationBatchService;
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
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

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
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

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
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.parameterActualFiniteStateListApplicationBatchService = new Mock<IParameterActualFiniteStateListApplicationBatchService>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null);
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.CurrentModel, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.IsNotNull(viewmodel.CurrentIteration);
        }

        [Test]
        public void VerifyThatTreeIsBuiltCorrectly()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null);
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
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);
            
            var vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            testDomain = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);

            vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public async Task VerifyThatContextMenuPopulated()
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

            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null);

            //no row selected. SelectedThing is null
            Assert.AreEqual(0, viewmodel.ContextMenu.Count);

            //selected row Possible List
            viewmodel.SelectedThing = viewmodel.FiniteStateList[0];
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            var menuKindPossible = viewmodel.ContextMenu[1].thingKind;
            Assert.True(menuKindPossible.ToString() == "PossibleFiniteStateList");

            //selected row Actual List
            viewmodel.SelectedThing = viewmodel.FiniteStateList[1];
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            var menuKindActual = viewmodel.ContextMenu[1].thingKind;
            Assert.True (menuKindActual.ToString() == "ActualFiniteStateList");

            // posible state row selected
            var pslFolder = viewmodel.FiniteStateList.First();
            var psRow = pslFolder.ContainedRows.First().ContainedRows.First();
            viewmodel.SelectedThing = psRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(5, viewmodel.ContextMenu.Count);

            // execute set default
            Assert.IsTrue(viewmodel.CanSetAsDefault);
            await viewmodel.SetDefaultStateCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public async Task VerifyThatCanSetDefaultCommandWorks()
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

            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null);

            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.IsFalse(viewmodel.CanSetAsDefault);
            await viewmodel.SetDefaultStateCommand.Execute();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            // posible state row selected
            var pslFolder = viewmodel.FiniteStateList.First();
            var psRow = pslFolder.ContainedRows.First().ContainedRows.First();
            viewmodel.SelectedThing = psRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.IsTrue(viewmodel.CanSetAsDefault);
            await viewmodel.SetDefaultStateCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatDisposeWorks()
        {
            var viewmodel = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null);
            viewmodel.Dispose();

            Assert.IsNull(viewmodel.Thing);
        }

        [Test]
        public async Task Verify_that_ExecuteBatchUpdateParameterCommand_works_as_expected()
        {
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
            var dialogResult = new CDP4EngineeringModel.ViewModels.Dialogs.CategoryDomainParameterTypeSelectorResult(true, false, Enumerable.Empty<ParameterType>(), Enumerable.Empty<Category>(), Enumerable.Empty<DomainOfExpertise>());
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(dialogResult);

            var vm = new FiniteStateBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null, this.parameterActualFiniteStateListApplicationBatchService.Object);

            vm.SelectedThing = new ActualFiniteStateListRowViewModel(actualList, this.session.Object, null);

            await vm.BatchUpdateParameterCommand.Execute();

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Exactly(1));

            this.parameterActualFiniteStateListApplicationBatchService.Verify(x => x.Update(this.session.Object, this.iteration, It.IsAny<ActualFiniteStateList>(), false, It.IsAny<IEnumerable<Category>>(), It.IsAny<IEnumerable<DomainOfExpertise>>(), It.IsAny<IEnumerable<ParameterType>>()), Times.Exactly(1));
        }
    }
}
