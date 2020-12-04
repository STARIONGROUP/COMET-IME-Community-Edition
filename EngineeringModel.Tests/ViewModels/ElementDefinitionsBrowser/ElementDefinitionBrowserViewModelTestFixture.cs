// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;    
    
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
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
    
    using ReactiveUI;

    using ElementDefinitionRowViewModel = CDP4EngineeringModel.ViewModels.ElementDefinitionRowViewModel;
    using ElementUsageRowViewModel = CDP4EngineeringModel.ViewModels.ElementUsageRowViewModel;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class ElementDefinitionBrowserViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IParameterSubscriptionBatchService> parameterSubscriptionBatchService;
        private Mock<IChangeOwnershipBatchService> changeOwnershipBatchService;

        private SiteDirectory sitedir;
        private EngineeringModel model;
        private Person person;
        private List<Thing> cache;

        private EngineeringModelSetup engineeringModelSetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Iteration iteration;
        private ElementDefinition elementDef;
        private Participant participant;
        private Uri uri;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private TextParameterType pt;
        private readonly PropertyInfo rev = typeof (Thing).GetProperty("RevisionNumber");

        [SetUp]
        public void Setup()
        {
            this.cache = new List<Thing>();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);
            this.panelNavigationService = new Mock<IPanelNavigationService>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.pt = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Person.Add(this.person);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri){Name = "TestDoE"};
            this.sitedir.Domain.Add(this.domain);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1", 
                Owner = this.domain, 
                Container = this.iteration
            };
            
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.participant.Domain.Add(this.domain);

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.elementDef,
                ParameterType = this.pt,
                Owner = this.elementDef.Owner
            };

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain, Parameter = parameter };
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri){  ElementDefinition = this.elementDef };
            elementUsage.ParameterOverride.Add(parameterOverride);
            this.elementDef.ContainedElement.Add(elementUsage);
            
            this.model.Iteration.Add(this.iteration);
            
            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration.IterationSetup = iterationSetup;

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ModelSetup" };
            this.engineeringModelSetup.IterationSetup.Add(iterationSetup);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.srdl.ParameterType.Add(this.pt);

            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                RequiredRdl = this.srdl
            };

            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);

            this.model.EngineeringModelSetup = this.engineeringModelSetup;
            this.model.EngineeringModelSetup.Participant.Add(this.participant);
            this.permissionService = new Mock<IPermissionService>();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            this.parameterSubscriptionBatchService = new Mock<IParameterSubscriptionBatchService>();
            this.changeOwnershipBatchService = new Mock<IChangeOwnershipBatchService>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatElementDefArePopulatedFromEvent()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            this.rev.SetValue(this.iteration, 50);
            this.iteration.Element.Add(this.elementDef);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(1, vm.ElementDefinitionRowViewModels.Count);

            this.rev.SetValue(this.iteration, 51);
            this.iteration.Element.Remove(this.elementDef);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(0, vm.ElementDefinitionRowViewModels.Count);
        }

        [Test]
        public void VerifyThatElementDefArePopulated()
        {
            this.iteration.Element.Add(this.elementDef);
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            Assert.AreEqual(1, vm.ElementDefinitionRowViewModels.Count);

            Assert.IsNotNull(vm.Caption);
            Assert.IsNotNull(vm.ToolTip);
            Assert.IsNotNull(vm.DataSource);
            Assert.IsNotNull(vm.CurrentModel);
            Assert.IsNotNull(vm.DomainOfExpertise);
            Assert.AreEqual(0, vm.CurrentIteration);

            var row = (ElementDefinitionRowViewModel) vm.ElementDefinitionRowViewModels.First();            
            Assert.That(row.Name, Is.Not.Null.Or.Empty);
            Assert.IsNotNull(row.Owner);

            this.elementDef.Name = "updated";
            this.elementDef.Owner = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };

            // workaround to modify a read-only field
            this.rev.SetValue(this.elementDef, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);

            Assert.AreEqual(this.elementDef.Name, row.Name);
            Assert.AreSame(this.elementDef.Owner, row.Owner);
        }

        [Test]
        public void VerifyHighlightElementUsagesEventIsSent()
        {
            var eu1 = new ElementDefinition();
            var eu2 = new ElementDefinition();

            CDPMessageBus.Current.Listen<ElementUsageHighlightEvent>().Subscribe(x => this.OnElementUsageHighlightEvent(x.ElementDefinition));

            CDPMessageBus.Current.SendMessage(new ElementUsageHighlightEvent(eu1));
            Assert.AreEqual(1, this.cache.Count);

            CDPMessageBus.Current.SendMessage(new ElementUsageHighlightEvent(eu2));
            Assert.AreEqual(2, this.cache.Count);
        }

        private void OnElementUsageHighlightEvent(Thing highlightedThings)
        {
            this.cache.Add(highlightedThings);
        }

        [Test]
        public void VerifyThatParticipantWithoutDomainSelectedCannotDropOnElementDefBrowser()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri);
            simpleQuantityKind.DefaultScale = ratioScale;
            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            vm.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatDragWorks()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, this.panelNavigationService.Object, null, null, null, null);
            var draginfo = new Mock<IDragInfo>();
            var dragSource = new Mock<IDragSource>();

            draginfo.Setup(x => x.Payload).Returns(dragSource.Object);

            vm.StartDrag(draginfo.Object);
            dragSource.Verify(x => x.StartDrag(draginfo.Object));
        }

        [Test]
        public async Task VerifyThatDropsWorkDomain()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();

            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);
            droptarget.Setup(x => x.Drop(It.IsAny<IDropInfo>())).Throws(new Exception("ex"));

            await vm.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));

            Assert.AreEqual("ex", vm.Feedback);
        }

        [Test]
        public async Task VerifyThatDropsWorkIfNoDomain()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();

            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            await vm.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object), Times.Once);
        }

        [Test]
        public void VerifyCreateParameterOverride()
        {
            var browser = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, this.panelNavigationService.Object, null, null, null, null);
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.elementDef.Owner, ElementDefinition = this.elementDef, Container = this.elementDef };
            var usageRow = new ElementUsageRowViewModel(elementUsage, this.elementDef.Owner, this.session.Object, null);
            var qk = new SimpleQuantityKind();
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.elementDef, 
                ParameterType = qk,
                Owner = this.elementDef.Owner
            };

            var parameterRow = new ParameterRowViewModel(parameter, this.session.Object, usageRow, false);
            Assert.IsFalse(browser.CreateOverrideCommand.CanExecute(null));
            browser.SelectedThing = parameterRow;            
            Assert.IsTrue(browser.CreateOverrideCommand.CanExecute(null));
            browser.CreateOverrideCommand.Execute(parameter);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            browser.SelectedThing = null;
            browser.ComputePermission();
            browser.CreateOverrideCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            var paramtType = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            paramtType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri), Scale = null });
            parameter.ParameterType = paramtType;

            var elementDefRow = new ElementDefinitionRowViewModel(this.elementDef, this.elementDef.Owner, this.session.Object, null);
            var parameterValueBaseRow = new ParameterComponentValueRowViewModel(parameter, 0, this.session.Object, null, null, elementDefRow, false);
            browser.SelectedThing = parameterValueBaseRow;
            browser.ComputePermission();
            browser.CreateOverrideCommand.Execute(null);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri) {Parameter = parameter, Owner = this.elementDef.Owner };
            parameterValueBaseRow = new ParameterComponentValueRowViewModel(parameterOverride, 0, this.session.Object, null, null, usageRow, false);
            browser.SelectedThing = parameterValueBaseRow;
            browser.CreateOverrideCommand.Execute(null);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(domainOfExpertise);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            domainOfExpertise = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(domainOfExpertise);

            vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifThatIfDomainIsRenamedBrowserIsUpdated()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "System", ShortName = "SYS" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(domainOfExpertise);
            
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            Assert.AreEqual("System [SYS]", vm.DomainOfExpertise);

            domainOfExpertise.Name = "Systems";
            this.rev.SetValue(domainOfExpertise, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(domainOfExpertise, EventKind.Updated);
            Assert.AreEqual("Systems [SYS]", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatIfEngineeringModelSetupIsChangedBrowserIsUpdated()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "System", ShortName = "SYS" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(domainOfExpertise);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            Assert.AreEqual("ModelSetup", vm.CurrentModel);

            this.engineeringModelSetup.Name = "testing";
            this.rev.SetValue(this.engineeringModelSetup, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);
            Assert.AreEqual("testing", vm.CurrentModel);
        }

        [Test]
        public void VerifyThatDragOverWorksWithDropTarget()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            
            var target = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(target.Object);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            target.Verify(x => x.DragOver(dropinfo.Object));
        }

        [Test]
        public void VerifyThatDragOverWorksWithoutDropTarget()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();

            dropinfo.Setup(x => x.Payload).Returns(this.elementDef);
            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            vm.DragOver(dropinfo.Object);
            Assert.AreNotEqual(DragDropEffects.All, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyDropElementDef()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Domain.Add(domainOfExpertise);
            this.engineeringModelSetup.ActiveDomain.Add(domainOfExpertise);

            var model2 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var modelsetup2 = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelIid = model2.Iid
            };
            
            model2.EngineeringModelSetup = modelsetup2;
            modelsetup2.ActiveDomain.Add(domainOfExpertise);

            this.sitedir.Model.Add(modelsetup2);

            var model2Iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var def = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = domainOfExpertise
            };

            model2.Iteration.Add(model2Iteration);
            model2Iteration.Element.Add(def);

            this.assembler.Cache.TryAdd(new CacheKey(model2.Iid, null), new Lazy<Thing>(() => model2));
            this.assembler.Cache.TryAdd(new CacheKey(model2Iteration.Iid, null), new Lazy<Thing>(() => model2Iteration));
            this.assembler.Cache.TryAdd(new CacheKey(def.Iid, model2Iteration.Iid), new Lazy<Thing>(() => def));

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();

            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, null)},
                    {model2Iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, null)}
                });

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            dropinfo.Setup(x => x.Payload).Returns(def);
            dropinfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            await vm.Drop(dropinfo.Object);           
            Assert.That(vm.Feedback, Is.Null.Or.Empty);
        }

        [Test]
        public async Task VerifyDropElementDefExceptionCaught()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Domain.Add(domainOfExpertise);
            this.engineeringModelSetup.ActiveDomain.Add(domainOfExpertise);

            var model2 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var modelsetup2 = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelIid = model2.Iid
            };

            model2.EngineeringModelSetup = modelsetup2;
            modelsetup2.ActiveDomain.Add(domainOfExpertise);

            this.sitedir.Model.Add(modelsetup2);

            var model2Iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var def = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = domainOfExpertise
            };

            model2.Iteration.Add(model2Iteration);
            model2Iteration.Element.Add(def);

            this.assembler.Cache.TryAdd(new CacheKey(model2.Iid, null), new Lazy<Thing>(() => model2));
            this.assembler.Cache.TryAdd(new CacheKey(model2Iteration.Iid, null), new Lazy<Thing>(() => model2Iteration));
            this.assembler.Cache.TryAdd(new CacheKey(def.Iid, model2Iteration.Iid), new Lazy<Thing>(() => def));

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, null)},
                    {model2Iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, null)}
                });

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            dropinfo.Setup(x => x.Payload).Returns(def);

            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;
            dropinfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            await vm.Drop(dropinfo.Object);
            Assert.AreEqual("test", vm.Feedback);
        }

        [Test]
        public void VerifyThatContextMenuIsPopulated()
        {
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.pt
            };

            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var usage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            var paramOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Parameter = parameter2
            };

            parameter2.ParameterType = this.pt;

            var usage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ElementDefinition = def2
            };

            def2.Parameter.Add(parameter2);
            usage.ParameterOverride.Add(paramOverride);
            usage.ElementDefinition = def2;

            this.elementDef.Parameter.Add(parameter);
            this.elementDef.ParameterGroup.Add(group);
            this.elementDef.ContainedElement.Add(usage);
            this.elementDef.ContainedElement.Add(usage2);

            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            vm.PopulateContextMenu();

            Assert.AreEqual(2, vm.ContextMenu.Count);

            var defRow = vm.ElementDefinitionRowViewModels.Last();

            vm.SelectedThing = defRow;
            vm.PopulateContextMenu();
            Assert.AreEqual(16, vm.ContextMenu.Count);

            vm.SelectedThing = defRow.ContainedRows[0];
            vm.PopulateContextMenu();
            Assert.AreEqual(11, vm.ContextMenu.Count);

            vm.SelectedThing = defRow.ContainedRows[1];
            vm.PopulateContextMenu();
            Assert.AreEqual(10, vm.ContextMenu.Count);

            var usageRow = defRow.ContainedRows[2];
            var usage2Row = defRow.ContainedRows[3];

            vm.SelectedThing = usageRow;
            vm.PopulateContextMenu();
            Assert.AreEqual(8, vm.ContextMenu.Count);

            vm.SelectedThing = usageRow.ContainedRows.Single();
            vm.PopulateContextMenu();
            Assert.AreEqual(11, vm.ContextMenu.Count);

            vm.SelectedThing = usage2Row.ContainedRows.Single();
            vm.PopulateContextMenu();
            Assert.AreEqual(11, vm.ContextMenu.Count);

            vm.Dispose();
        }

        [Test]
        public void VerifyThatSetTopElementWorks()
        {
            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            this.iteration.TopElement = this.elementDef;

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            var defRow = (ElementDefinitionRowViewModel)vm.ElementDefinitionRowViewModels.Single(x => x.Thing.Iid == this.elementDef.Iid);
            var def2Row = (ElementDefinitionRowViewModel)vm.ElementDefinitionRowViewModels.Single(x => x.Thing.Iid == def2.Iid);
            Assert.IsTrue(defRow.IsTopElement);
            Assert.IsFalse(def2Row.IsTopElement);

            this.iteration.TopElement = def2;
            this.rev.SetValue(this.iteration, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsFalse(defRow.IsTopElement);
            Assert.IsTrue(def2Row.IsTopElement);

            this.iteration.Element.Remove(def2);
            this.rev.SetValue(this.iteration, 51);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsTrue(def2Row.IsTopElement);
        }

        [Test]
        public void VerifyThatSubscriptionCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> 
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(new DomainOfExpertise(), null) }
                });

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.pt };

            this.elementDef.Parameter.Add(parameter);
            this.iteration.Element.Add(this.elementDef);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            Assert.IsFalse(vm.CreateSubscriptionCommand.CanExecute(null));

            var defRow = vm.ElementDefinitionRowViewModels.First();
            vm.SelectedThing = defRow.ContainedRows.First();
            vm.ComputePermission();
            vm.PopulateContextMenu();

            Assert.IsTrue(vm.CreateSubscriptionCommand.CanExecute(null));
            vm.CreateSubscriptionCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            vm.SelectedThing = null;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            vm.SelectedThing = defRow;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);

            // Assert that is possible to subscribe to a ParameterOverride that is not owned by the current domain
            var parameterOverrideRow = defRow.ContainedRows.Last().ContainedRows.Last();
            vm.SelectedThing = parameterOverrideRow;
            vm.ComputePermission();
            Assert.IsTrue(vm.CanCreateSubscription);

            // Assert that is NOT possible to subscribe to a ParameterOverride that is owned by the current domain
            ((ParameterOverride)parameterOverrideRow.Thing).Owner = this.session.Object.OpenIterations.Values.Single().Item1;
            vm.ComputePermission();
            Assert.IsFalse(vm.CanCreateSubscription);
        }

        [Test]
        public void VerifyThatExecuteCreateParameterGroupWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(new DomainOfExpertise(), null) } });

            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.elementDef.ParameterGroup.Add(group);
            this.iteration.Element.Add(this.elementDef);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null, null, null);

            var defRow = vm.ElementDefinitionRowViewModels.Single();
            vm.SelectedThing = defRow.ContainedRows.Single(x => x.Thing is ParameterGroup);

            vm.CreateParameterGroup.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.Is<ParameterGroup>(gr => gr.ContainingGroup == group), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatCreateChangeRequestWorks()
        {
            this.iteration.Element.Add(this.elementDef);
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, null, null, null, null, null);

            vm.SelectedThing = vm.ElementDefinitionRowViewModels.First();

            vm.CreateChangeRequestCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.Is<ChangeRequest>(cr => cr.Author == this.participant && cr.RelatedThing.Count == 1), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModel>(), null));
        }

        [Test]
        public void VerifyThatRefocusWorks()
        {
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.pt
            };

            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var usage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var paramOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Parameter = parameter2
            };
            
            parameter2.ParameterType = this.pt;

            var usage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ElementDefinition = def2
            };

            def2.Parameter.Add(parameter2);
            usage.ParameterOverride.Add(paramOverride);
            usage.ElementDefinition = def2;

            this.elementDef.Parameter.Add(parameter);
            this.elementDef.ParameterGroup.Add(group);
            this.elementDef.ContainedElement.Add(usage);
            this.elementDef.ContainedElement.Add(usage2);

            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null, null, null);
            vm.PopulateContextMenu();

            Assert.AreEqual(2, vm.ContextMenu.Count);

            var defRow = vm.ElementDefinitionRowViewModels.Last();
            var usageRow = defRow.ContainedRows[2];

            vm.SelectedThing = usageRow;
            vm.ChangeFocusCommand.Execute(null);

            var def2Row = vm.ElementDefinitionRowViewModels.Single(x => x.Thing == def2);

            Assert.IsTrue(vm.SelectedThing == def2Row);
            Assert.IsTrue(vm.FocusedRow == def2Row);
        }

        [Test]
        public async Task Verify_that_ExecuteBatchCreateSubscriptionCommand_works_as_expected()
        {
            var dialogResult = new CDP4EngineeringModel.ViewModels.Dialogs.CategoryDomainParameterTypeSelectorResult(true, false, Enumerable.Empty<ParameterType>(), Enumerable.Empty<Category>(), Enumerable.Empty<DomainOfExpertise>());
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(dialogResult);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null, this.parameterSubscriptionBatchService.Object, null);
            vm.PopulateContextMenu();

            Assert.AreEqual(2, vm.ContextMenu.Count);

            vm.BatchCreateSubscriptionCommand.Execute(null);

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Exactly(1));

            this.parameterSubscriptionBatchService.Verify(x => x.Create(this.session.Object, this.iteration, false, It.IsAny<IEnumerable<Category>>(), It.IsAny<IEnumerable<DomainOfExpertise>>(), It.IsAny<IEnumerable<ParameterType>>()), Times.Exactly(1));
        }

        [Test]
        public async Task Verify_that_ExecuteBatchChangeOwnershipElementDefinition_works_as_expected()
        {
            var dialogResult = new CDP4EngineeringModel.ViewModels.Dialogs.ChangeOwnershipSelectionResult(true, this.domain, true);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(dialogResult);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null, this.parameterSubscriptionBatchService.Object, this.changeOwnershipBatchService.Object);
            vm.SelectedThing = new ElementDefinitionRowViewModel(this.elementDef, this.domain, this.session.Object, null);

            vm.PopulateContextMenu();

            vm.ChangeOwnershipCommand.Execute(null);

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Exactly(1));

            this.changeOwnershipBatchService.Verify(x => x.Update(this.session.Object, It.IsAny<ElementDefinition>(), this.domain, true, It.IsAny<IEnumerable<ClassKind>>()), Times.Exactly(1));
        }
    }
}
