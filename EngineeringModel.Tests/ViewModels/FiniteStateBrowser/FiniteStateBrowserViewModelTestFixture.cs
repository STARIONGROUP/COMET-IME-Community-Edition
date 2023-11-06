// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Windows;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels;

    using CommonServiceLocator;

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
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IMessageBoxService> messageBoxService;
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
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private ElementDefinition elementDefinition3;
        private Parameter parameterA;
        private Parameter parameterB;
        private Parameter parameterC;
        private Parameter parameterD;
        private PossibleFiniteState possibleStateA;
        private PossibleFiniteState possibleStateB;
        private PossibleFiniteState possibleStateC;
        private PossibleFiniteState possibleStateD;
        private PossibleFiniteStateList possibleFiniteStateList1;
        private PossibleFiniteStateList possibleFiniteStateList2;
        private ActualFiniteStateList actualFiniteStateList;


        [SetUp]
        public void Setup()
        {
            this.rev = typeof (Thing).GetProperty("RevisionNumber");

            this.session = new Mock<ISession>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>())
                .Returns(this.messageBoxService.Object);

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

        [Test]
        public void AssertThatDeleteCommandWorks()
        {            
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new Mock<IRowViewModelBase<Thing>>();
            rowViewModelBase.SetupGet(x => x.ContainedRows).Returns(new CDP4Composition.Mvvm.Types.DisposableReactiveList<IRowViewModelBase<Thing>>());
            rowViewModelBase.SetupGet(x => x.Thing).Returns(this.sitedir);
            browser.IsDeleteCommandOverrideAllowed = true;
            browser.SelectedThing = rowViewModelBase.Object;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
        }

        private void InitializeStateListsImportandData()
        {
            this.possibleStateA = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.possibleStateB = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.possibleStateC = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.possibleStateD = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);

            this.possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.possibleFiniteStateList1.PossibleState.Add(this.possibleStateA);
            this.possibleFiniteStateList1.PossibleState.Add(this.possibleStateB);
            this.possibleFiniteStateList1.DefaultState = this.possibleStateA;

            this.possibleFiniteStateList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.possibleFiniteStateList2.PossibleState.Add(this.possibleStateC);
            this.possibleFiniteStateList2.PossibleState.Add(this.possibleStateD);
            this.possibleFiniteStateList2.DefaultState = this.possibleStateC;

            this.actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.PossibleFiniteStateList.Add(this.possibleFiniteStateList1);
            this.actualFiniteStateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.possibleStateA },

                Kind = ActualFiniteStateKind.MANDATORY
            });

            this.actualFiniteStateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.possibleStateB },
                Kind = ActualFiniteStateKind.MANDATORY
            });

            this.parameterA = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, StateDependence = this.actualFiniteStateList };
            this.parameterB = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, StateDependence = this.actualFiniteStateList };
            this.parameterC = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.parameterD = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };

            this.elementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementDefinition3 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };

            this.elementDefinition1.Parameter.Add(this.parameterA);
            this.elementDefinition1.Parameter.Add(this.parameterB);

            this.elementDefinition2.Parameter.Add(this.parameterA);
            this.elementDefinition2.Parameter.Add(this.parameterC);

            this.elementDefinition3.Parameter.Add(this.parameterC);
            this.elementDefinition3.Parameter.Add(this.parameterD);

            this.iteration.Element.Add(this.elementDefinition1);
            this.iteration.Element.Add(this.elementDefinition2);
            this.iteration.Element.Add(this.elementDefinition3);

            this.iteration.ActualFiniteStateList.Add(this.actualFiniteStateList);
            this.iteration.PossibleFiniteStateList.Add(this.possibleFiniteStateList1);
            this.iteration.PossibleFiniteStateList.Add(this.possibleFiniteStateList2);
        }

        [Test]
        public void AssertThatDeleteCommandChecksForActualFiniteStateListDependencies()
        {
            InitializeStateListsImportandData();
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new ActualFiniteStateListRowViewModel(this.actualFiniteStateList, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.messageBoxService.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandChecksForPossibleFiniteStateList1Dependencies()
        {
            InitializeStateListsImportandData();
            //Try to delete a list that have dependencies
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new PossibleFiniteStateListRowViewModel(this.possibleFiniteStateList1,this.session.Object,browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.messageBoxService.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandChecksForPossibleFiniteStateList2Dependencies()
        {
            InitializeStateListsImportandData();
            //Try to delete a list that don't have dependencies
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new PossibleFiniteStateListRowViewModel(this.possibleFiniteStateList2, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.messageBoxService.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Never);
        }

        [Test]
        public void AssertThatDeleteCommandChecksForPossibleFiniteStateDependencies()
        {
            InitializeStateListsImportandData();
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new PossibleFiniteStateRowViewModel(this.possibleStateA, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.messageBoxService.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandShowDialogTextIsCorrect1()
        {
            InitializeStateListsImportandData();
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new ActualFiniteStateListRowViewModel(this.actualFiniteStateList, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());

            var textToContain = "3 parameter(s) will be affected by this deletion";
            this.messageBoxService.Verify(x => x.Show(It.Is<string>(s => s.Contains(textToContain)), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandShowDialogTextIsCorrect2()
        {
            InitializeStateListsImportandData();
            //Try to delete a list that have dependencies
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new PossibleFiniteStateListRowViewModel(this.possibleFiniteStateList1, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());

            var textToContain = "3 parameter(s) will be affected by this deletion";
            this.messageBoxService.Verify(x => x.Show(It.Is<string>(s=>s.Contains(textToContain)), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandShowDialogTextIsCorrect3()
        {
            InitializeStateListsImportandData();
            var browser = new FiniteStateBrowserDeleteCommandTestClass(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null, null);
            var rowViewModelBase = new PossibleFiniteStateRowViewModel(this.possibleStateA, this.session.Object, browser);

            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());

            var textToContain = "3 parameter(s) will be affected by this deletion";
            this.messageBoxService.Verify(x => x.Show(It.Is<string>(s => s.Contains(textToContain)), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), Times.Once);
        }

        internal class FiniteStateBrowserDeleteCommandTestClass : FiniteStateBrowserViewModel
        {
            internal FiniteStateBrowserDeleteCommandTestClass(Iteration iteration, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService)
            : base(iteration, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService, parameterActualFiniteStateListApplicationBatchService)
            {

            }

            public bool? IsDeleteCommandOverrideAllowed { get; set; }

            protected override bool IsDeleteCommandAllowed()
            {
                if (this.IsDeleteCommandOverrideAllowed.HasValue && !this.IsDeleteCommandOverrideAllowed.Value)
                {
                    return base.IsDeleteCommandAllowed();
                }

                return this.IsDeleteCommandOverrideAllowed.Value;
            }
        }
    }
}
