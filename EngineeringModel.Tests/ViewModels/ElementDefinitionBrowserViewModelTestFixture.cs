// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
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
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using ElementDefinitionRowViewModel = CDP4EngineeringModel.ViewModels.ElementDefinitionRowViewModel;
    using ElementUsageRowViewModel = CDP4EngineeringModel.ViewModels.ElementUsageRowViewModel;

    [TestFixture]
    public class ElementDefinitionBrowserViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
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
        private PropertyInfo rev = typeof (Thing).GetProperty("RevisionNumber");

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
            this.sitedir.Person.Add(person);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri){Name = "TestDoE"};
            this.sitedir.Domain.Add(this.domain);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "1" };
            this.elementDef.Owner = this.domain;
            this.elementDef.Container = this.iteration;
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

            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl.RequiredRdl = this.srdl;
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);

            this.model.EngineeringModelSetup = this.engineeringModelSetup;
            this.model.EngineeringModelSetup.Participant.Add(this.participant);
            this.permissionService = new Mock<IPermissionService>();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatElementDefArePopulatedFromEvent()
        {
            var type = this.iteration.GetType();
            var rev = type.GetProperty("RevisionNumber");

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            rev.SetValue(this.iteration, 50);
            this.iteration.Element.Add(this.elementDef);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(1, vm.ElementDefinitionRowViewModels.Count);

            rev.SetValue(this.iteration, 51);
            this.iteration.Element.Remove(this.elementDef);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(0, vm.ElementDefinitionRowViewModels.Count);
        }

        [Test]
        public void VerifyThatElementDefArePopulated()
        {
            this.iteration.Element.Add(this.elementDef);
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

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
            var type = this.elementDef.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.elementDef, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);

            Assert.AreEqual(this.elementDef.Name, row.Name);
            Assert.AreSame(this.elementDef.Owner, row.Owner);
        }

        [Test]
        public void VerifyHighlightElementUsagesEventIsSent()
        {
            var eu1 = new CDP4Common.EngineeringModelData.ElementDefinition();
            var eu2 = new CDP4Common.EngineeringModelData.ElementDefinition();

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
        public void VerifyThatNoneIsReturnedUponNullDomain()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> {{ this.iteration, new Tuple<DomainOfExpertise, Participant>(null,null) }} );
            this.iteration.Element.Add(this.elementDef);
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatParticipantWithoutDomainSelectedCannotDropOnElementDefBrowser()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

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
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, this.panelNavigationService.Object, null, null);
            var draginfo = new Mock<IDragInfo>();
            var dragSource = new Mock<IDragSource>();

            draginfo.Setup(x => x.Payload).Returns(dragSource.Object);

            vm.StartDrag(draginfo.Object);
            dragSource.Verify(x => x.StartDrag(draginfo.Object));
        }

        [Test]
        public void VerifyThatDropsWorkDomain()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();

            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);
            droptarget.Setup(x => x.Drop(It.IsAny<IDropInfo>())).Throws(new Exception("ex"));

            vm.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));

            Assert.AreEqual("ex", vm.Feedback);
        }

        [Test]
        public void VerifyThatDropsWorkIfNoDomain()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();

            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            vm.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object), Times.Once);
        }

        [Test]
        public void VerifyCreateParameterOverride()
        {
            var browser = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, this.panelNavigationService.Object, null, null);
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
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, this.participant) }
            });

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null) }
            });

            vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifThatIfDomainIsRenamedBrowserIsUpdated()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "System", ShortName = "SYS" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null) }
            });

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("System [SYS]", vm.DomainOfExpertise);

            var type = domain.GetType();
            var revisionNumber = type.GetProperty("RevisionNumber");
            domain.Name = "Systems";
            revisionNumber.SetValue(domain, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(domain, EventKind.Updated);
            Assert.AreEqual("Systems [SYS]", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatIfEngineeringModelSetupIsChangedBrowserIsUpdated()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "System", ShortName = "SYS" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null) }
            });

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("ModelSetup", vm.CurrentModel);

            var type = this.engineeringModelSetup.GetType();
            var revisionNumber = type.GetProperty("RevisionNumber");
            this.engineeringModelSetup.Name = "testing";
            revisionNumber.SetValue(this.engineeringModelSetup, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.engineeringModelSetup, EventKind.Updated);
            Assert.AreEqual("testing", vm.CurrentModel);
        }

        [Test]
        public void VerifyThatDragOverWorksWithDropTarget()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(new DomainOfExpertise(), null)}
                });

            var target = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(target.Object);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            target.Verify(x => x.DragOver(dropinfo.Object));
        }

        [Test]
        public void VerifyThatDragOverWorksWithoutDropTarget()
        {
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(new DomainOfExpertise(), null)}
                });


            dropinfo.Setup(x => x.Payload).Returns(this.elementDef);

            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            vm.DragOver(dropinfo.Object);
            Assert.AreNotEqual(DragDropEffects.All, dropinfo.Object.Effects);
        }


        [Test]
        public void VerifyDropElementDef()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Domain.Add(domain);
            this.engineeringModelSetup.ActiveDomain.Add(domain);

            var model2 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var modelsetup2 = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelsetup2.EngineeringModelIid = model2.Iid;
            model2.EngineeringModelSetup = modelsetup2;
            modelsetup2.ActiveDomain.Add(domain);

            this.sitedir.Model.Add(modelsetup2);

            var model2Iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var def = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            def.Owner = domain;

            model2.Iteration.Add(model2Iteration);
            model2Iteration.Element.Add(def);

            this.assembler.Cache.TryAdd(new CacheKey(model2.Iid, null), new Lazy<Thing>(() => model2));
            this.assembler.Cache.TryAdd(new CacheKey(model2Iteration.Iid, null), new Lazy<Thing>(() => model2Iteration));
            this.assembler.Cache.TryAdd(new CacheKey(def.Iid, model2Iteration.Iid), new Lazy<Thing>(() => def));

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)},
                    {model2Iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
                });

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            dropinfo.Setup(x => x.Payload).Returns(def);
            dropinfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            vm.Drop(dropinfo.Object);           
            Assert.That(vm.Feedback, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyDropElementDefExceptionCaught()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Domain.Add(domain);
            this.engineeringModelSetup.ActiveDomain.Add(domain);

            var model2 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var modelsetup2 = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelsetup2.EngineeringModelIid = model2.Iid;
            model2.EngineeringModelSetup = modelsetup2;
            modelsetup2.ActiveDomain.Add(domain);

            this.sitedir.Model.Add(modelsetup2);

            var model2Iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var def = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            def.Owner = domain;

            model2.Iteration.Add(model2Iteration);
            model2Iteration.Element.Add(def);

            this.assembler.Cache.TryAdd(new CacheKey(model2.Iid, null), new Lazy<Thing>(() => model2));
            this.assembler.Cache.TryAdd(new CacheKey(model2Iteration.Iid, null), new Lazy<Thing>(() => model2Iteration));
            this.assembler.Cache.TryAdd(new CacheKey(def.Iid, model2Iteration.Iid), new Lazy<Thing>(() => def));

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)},
                    {model2Iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
                });

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            dropinfo.Setup(x => x.Payload).Returns(def);

            dropinfo.SetupProperty(x => x.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;
            dropinfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            vm.Drop(dropinfo.Object);
            Assert.AreEqual("test", vm.Feedback);
        }

        [Test]
        public void VerifyThatContextMenuIsPopulated()
        {
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            parameter.ParameterType = this.pt;

            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var usage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var paramOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri);
            paramOverride.Parameter = parameter2;
            parameter2.ParameterType = this.pt;

            var usage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);
            usage2.ElementDefinition = def2;

            def2.Parameter.Add(parameter2);
            usage.ParameterOverride.Add(paramOverride);
            usage.ElementDefinition = def2;

            this.elementDef.Parameter.Add(parameter);
            this.elementDef.ParameterGroup.Add(group);
            this.elementDef.ContainedElement.Add(usage);
            this.elementDef.ContainedElement.Add(usage2);

            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            vm.PopulateContextMenu();

            Assert.AreEqual(2, vm.ContextMenu.Count);

            var defRow = vm.ElementDefinitionRowViewModels.Last();

            vm.SelectedThing = defRow;
            vm.PopulateContextMenu();
            Assert.AreEqual(13, vm.ContextMenu.Count);

            vm.SelectedThing = defRow.ContainedRows[0];
            vm.PopulateContextMenu();
            Assert.AreEqual(10, vm.ContextMenu.Count);

            vm.SelectedThing = defRow.ContainedRows[1];
            vm.PopulateContextMenu();
            Assert.AreEqual(10, vm.ContextMenu.Count);

            var usageRow = defRow.ContainedRows[2];
            var usage2Row = defRow.ContainedRows[3];

            vm.SelectedThing = usageRow;
            vm.PopulateContextMenu();
            Assert.AreEqual(6, vm.ContextMenu.Count);

            vm.SelectedThing = usageRow.ContainedRows.Single();
            vm.PopulateContextMenu();
            Assert.AreEqual(10, vm.ContextMenu.Count);

            vm.SelectedThing = usage2Row.ContainedRows.Single();
            vm.PopulateContextMenu();
            Assert.AreEqual(10, vm.ContextMenu.Count);

            vm.Dispose();
        }

        [Test]
        public void VerifyThatSetTopElementWorks()
        {
            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            this.iteration.TopElement = this.elementDef;

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            var defRow = (ElementDefinitionRowViewModel)vm.ElementDefinitionRowViewModels.Single(x => x.Thing.Iid == this.elementDef.Iid);
            var def2Row = (ElementDefinitionRowViewModel)vm.ElementDefinitionRowViewModels.Single(x => x.Thing.Iid == def2.Iid);
            Assert.IsTrue(defRow.IsTopElement);
            Assert.IsFalse(def2Row.IsTopElement);

            this.iteration.TopElement = def2;
            rev.SetValue(this.iteration, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsFalse(defRow.IsTopElement);
            Assert.IsTrue(def2Row.IsTopElement);

            //this.iteration.Element.Remove(def2);
            this.iteration.Element.Remove(def2);
            rev.SetValue(this.iteration, 51);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsTrue(def2Row.IsTopElement);
        }

        [Test]
        public void VerifyThatSubscriptionCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(new DomainOfExpertise(), null) } });

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.pt };

            this.elementDef.Parameter.Add(parameter);
            this.iteration.Element.Add(this.elementDef);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
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

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var defRow = vm.ElementDefinitionRowViewModels.Single();
            vm.SelectedThing = defRow.ContainedRows.Single(x => x.Thing is ParameterGroup);

            vm.CreateParameterGroup.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.Is<ParameterGroup>(gr => gr.ContainingGroup == group), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatCreateChangeRequestWorks()
        {
            this.iteration.Element.Add(this.elementDef);
            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, null, null, null);

            vm.SelectedThing = vm.ElementDefinitionRowViewModels.First();

            vm.CreateChangeRequestCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.Is<ChangeRequest>(cr => cr.Author == this.participant && cr.RelatedThing.Count == 1), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModel>(), null));
        }

        [Test]
        public void VerifyThatRefocusWorks()
        {
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            parameter.ParameterType = this.pt;

            var def2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var usage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var paramOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri);
            paramOverride.Parameter = parameter2;
            parameter2.ParameterType = this.pt;

            var usage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri);
            usage2.ElementDefinition = def2;

            def2.Parameter.Add(parameter2);
            usage.ParameterOverride.Add(paramOverride);
            usage.ElementDefinition = def2;

            this.elementDef.Parameter.Add(parameter);
            this.elementDef.ParameterGroup.Add(group);
            this.elementDef.ContainedElement.Add(usage);
            this.elementDef.ContainedElement.Add(usage2);

            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(def2);

            var vm = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
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
    }
}