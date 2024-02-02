// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEditorViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4DiagramEditor.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Mvvm.Behaviours;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.ViewModels;

    using CommonServiceLocator;

    using DevExpress.Xpf.Diagram;

    using Moq;

    using NUnit.Framework;

    using Point = System.Windows.Point;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DiagramEditorViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IDiagramDropInfo> dropinfo;
        private Mock<IExtendedDiagramOrgChartBehavior> mockExtendedDiagramBehavior;
        private Mock<ICdp4DiagramBehavior> mockDiagramBehavior;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private DiagramCanvas diagram;
        private DiagramObject diagramObject1;
        private DiagramObject diagramObject2;
        private DiagramObject diagramObject3;
        private DiagramEdge connector;
        private ElementDefinition elementDefinition;
        private Bounds bound1;
        private Bounds bound2;
        private Parameter parameter;

        private Category specCat;
        private Category relationshipCat;

        private RequirementsSpecification spec1;
        private RequirementsSpecification spec2;
        private RequirementsSpecification spec3;
        private BinaryRelationship link1;

        private Mock<IServiceLocator> serviceLocator;
        private CDPMessageBus messageBus;
        private Mock<IThingCreator> thingCreator;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>()).Returns(Mock.Of<IThingCreator>());

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.permissionService = new Mock<IPermissionService>();
            this.mockExtendedDiagramBehavior = new Mock<IExtendedDiagramOrgChartBehavior>();
            this.mockDiagramBehavior = new Mock<ICdp4DiagramBehavior>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.thingCreator = new Mock<IThingCreator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>()).Returns(this.thingCreator.Object);
            this.dropinfo = new Mock<IDiagramDropInfo>();
            this.cache = this.assembler.Cache;

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);

            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "pt", ShortName = "pt" },
                Scale = new RatioScale(Guid.NewGuid(), this.cache, this.uri)
            };

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person,
                SelectedDomain = this.domain
            };

            this.diagram = new DiagramCanvas(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
            {
                EngineeringModelSetup = this.modelsetup
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.iteration.DiagramCanvas.Add(this.diagram);
            this.model.Iteration.Add(this.iteration);

            this.specCat = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.relationshipCat = new Category(Guid.NewGuid(), this.cache, this.uri);

            this.spec1 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.spec2 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.spec3 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);

            this.link1 = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri)
            {
                Source = this.spec1,
                Target = this.spec2
            };

            this.link1.Category.Add(this.relationshipCat);
            this.spec1.Category.Add(this.specCat);
            this.spec2.Category.Add(this.specCat);
            this.spec3.Category.Add(this.specCat);

            this.srdl.DefinedCategory.Add(this.specCat);
            this.srdl.DefinedCategory.Add(this.relationshipCat);

            this.iteration.RequirementsSpecification.Add(this.spec1);
            this.iteration.RequirementsSpecification.Add(this.spec2);
            this.iteration.RequirementsSpecification.Add(this.spec3);
            this.iteration.Relationship.Add(this.link1);

            var tuple = new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant);

            var openedIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {
                    this.iteration, tuple
                }
            };

            this.diagramObject1 = new DiagramObject(Guid.NewGuid(), this.cache, this.uri) { DepictedThing = this.spec1 };

            this.diagramObject2 = new DiagramObject(Guid.NewGuid(), this.cache, this.uri) { DepictedThing = this.spec2 };
            this.diagramObject3 = new DiagramObject(Guid.NewGuid(), this.cache, this.uri) { DepictedThing = this.spec3 };

            this.connector = new DiagramEdge(Guid.NewGuid(), this.cache, this.uri)
            {
                Source = this.diagramObject1,
                Target = this.diagramObject2,
                DepictedThing = this.link1
            };

            this.elementDefinition = new ElementDefinition() { Name = "WhyNot", ShortName = "WhyNot" };
            this.iteration.Element.Add(this.elementDefinition);
            this.elementDefinition.Parameter.Add(this.parameter);

            this.bound1 = new Bounds(Guid.NewGuid(), this.cache, this.uri)
            {
                X = 1,
                Y = 1,
                Height = 12,
                Width = 10
            };

            this.bound2 = new Bounds(Guid.NewGuid(), this.cache, this.uri)
            {
                X = 1,
                Y = 1,
                Height = 12,
                Width = 10
            };

            this.diagramObject1.Bounds.Add(this.bound1);
            this.diagramObject2.Bounds.Add(this.bound2);

            this.diagram.DiagramElement.Add(this.diagramObject1);
            this.diagram.DiagramElement.Add(this.diagramObject2);
            this.diagram.DiagramElement.Add(this.connector);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(openedIterations);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns("http://example.org");
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
 
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.mockExtendedDiagramBehavior.Setup(x => x.GetDiagramPositionFromMousePosition(It.IsAny<Point>())).Returns(new Point());
            this.mockDiagramBehavior.Setup(x => x.GetDiagramPositionFromMousePosition(It.IsAny<Point>())).Returns(new Point());
            this.mockDiagramBehavior.Setup(x => x.ItemPositions).Returns(new Dictionary<object, Point>());
            this.mockDiagramBehavior.Setup(x => x.ApplyChildLayout(It.IsAny<DiagramItem>()));

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.UpdateProperties();
            viewModel.ComputeDiagramConnector();

            Assert.That(viewModel.Caption, Is.Not.Null.Or.Empty);
            Assert.AreEqual($"{this.diagram.Name} <<{this.diagram.GetType().Name}>>", viewModel.Caption);
            Assert.That(viewModel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.IsNotEmpty(viewModel.ThingDiagramItemViewModels);
            Assert.IsFalse(viewModel.CanCreateDiagram);
            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyDragOver()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            this.dropinfo.Setup(x => x.Payload).Returns(this.domain);
            viewModel.DragOver(this.dropinfo.Object);
            this.dropinfo.VerifySet(x => x.Effects = It.IsAny<DragDropEffects>(), Times.Once);
            await viewModel.Drop(this.dropinfo.Object);
            Assert.IsNotEmpty(viewModel.ThingDiagramItemViewModels);
            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.UpdateProperties();
            Assert.IsEmpty(viewModel.ThingDiagramItemViewModels);
            var drop = new Mock<IDiagramDropInfo>();
            drop.Setup(x => x.Payload).Returns(this.elementDefinition);
            drop.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));
            await viewModel.Drop(drop.Object);
            Assert.IsNotEmpty(viewModel.ThingDiagramItemViewModels);
            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatItemAreRemoved()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.UpdateProperties();
            var itemNumber = viewModel.ThingDiagramItemViewModels.Count;
            viewModel.RemoveDiagramThingItem(viewModel.ThingDiagramItemViewModels.FirstOrDefault());
            Assert.IsTrue(itemNumber == viewModel.ThingDiagramItemViewModels.Count + 1);
            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatSaveCommandWorks()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            var drop1 = new Mock<IDiagramDropInfo>();
            drop1.Setup(x => x.Payload).Returns(this.diagramObject1.DepictedThing);
            drop1.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));

            var drop2 = new Mock<IDiagramDropInfo>();
            drop2.Setup(x => x.Payload).Returns(this.diagramObject2.DepictedThing);
            drop2.Setup(x => x.DiagramDropPoint).Returns(new Point(10, 10));

            await viewModel.Drop(drop1.Object);
            await viewModel.Drop(drop2.Object);

            await viewModel.SaveDiagramCommand.Execute();
            this.cache.TryAdd(new CacheKey(this.diagram.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.diagram));
            this.session.Verify(x => x.Write(It.Is<OperationContainer>(s => true)));
            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyCanDeleteFromDiagramOnly()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            var drop1 = new Mock<IDiagramDropInfo>();
            drop1.Setup(x => x.Payload).Returns(this.diagramObject1.DepictedThing);
            drop1.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));

            var drop2 = new Mock<IDiagramDropInfo>();
            drop2.Setup(x => x.Payload).Returns(this.diagramObject2.DepictedThing);
            drop2.Setup(x => x.DiagramDropPoint).Returns(new Point(10, 10));

            await viewModel.Drop(drop1.Object);
            await viewModel.Drop(drop2.Object);

            Assert.AreEqual(2, viewModel.ThingDiagramItemViewModels.Count);

            viewModel.ThingDiagramItemViewModels[0].DiagramRepresentation = new DiagramContentItem() { Content = viewModel.ThingDiagramItemViewModels[0] };
            viewModel.ThingDiagramItemViewModels[1].DiagramRepresentation = new DiagramContentItem() { Content = viewModel.ThingDiagramItemViewModels[1] };
            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.ThingDiagramItemViewModels.First().DiagramRepresentation);
            viewModel.SelectedItem = null;
            viewModel.SelectedItem = viewModel.ThingDiagramItemViewModels.First().DiagramRepresentation;

            await viewModel.DeleteFromDiagramCommand.Execute();

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyCanDeleteFromDiagramAndModel()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            var drop1 = new Mock<IDiagramDropInfo>();
            drop1.Setup(x => x.Payload).Returns(this.diagramObject1.DepictedThing);
            drop1.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));

            var drop2 = new Mock<IDiagramDropInfo>();
            drop2.Setup(x => x.Payload).Returns(this.diagramObject2.DepictedThing);
            drop2.Setup(x => x.DiagramDropPoint).Returns(new Point(10, 10));

            await viewModel.Drop(drop1.Object);
            await viewModel.Drop(drop2.Object);

            Assert.AreEqual(2, viewModel.ThingDiagramItemViewModels.Count);

            viewModel.ThingDiagramItemViewModels[0].DiagramRepresentation = new DiagramContentItem() { Content = viewModel.ThingDiagramItemViewModels[0] };
            viewModel.ThingDiagramItemViewModels[1].DiagramRepresentation = new DiagramContentItem() { Content = viewModel.ThingDiagramItemViewModels[1] };
            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.ThingDiagramItemViewModels.First().DiagramRepresentation);
            viewModel.SelectedItem = null;
            viewModel.SelectedItem = viewModel.ThingDiagramItemViewModels.First().DiagramRepresentation;

            await viewModel.DeleteFromModelCommand.Execute();

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyDiagramFilteringWorks()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            var drop1 = new Mock<IDiagramDropInfo>();
            drop1.Setup(x => x.Payload).Returns(this.diagramObject1.DepictedThing);
            drop1.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));

            var drop2 = new Mock<IDiagramDropInfo>();
            drop2.Setup(x => x.Payload).Returns(this.diagramObject2.DepictedThing);
            drop2.Setup(x => x.DiagramDropPoint).Returns(new Point(10, 10));

            await viewModel.Drop(drop1.Object);
            await viewModel.Drop(drop2.Object);

            Assert.AreEqual(2, viewModel.ThingDiagramItemViewModels.Count);
            Assert.AreEqual(2, viewModel.DiagramElementTreeRowViewModels.Count);

            viewModel.VisibleDiagramElementTreeRowViewModels.Add(viewModel.DiagramElementTreeRowViewModels[0]);

            Assert.AreEqual(1, viewModel.ThingDiagramItemViewModels.Count(i => i.IsFiltered));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyInspectParameter()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.ExecuteInspectParameter(this.parameter);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyCanSubscribe()
        {
            this.diagram.DiagramElement.Clear();

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.ExecuteSubscribeParameter(this.parameter);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatIsDirtyIsTrueOnThingDropped()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            Assert.IsFalse(viewModel.IsDirty);
            var drop = new Mock<IDiagramDropInfo>();
            drop.Setup(x => x.Payload).Returns(this.elementDefinition);
            drop.Setup(x => x.DiagramDropPoint).Returns(new Point(1, 1));
            await viewModel.Drop(drop.Object);
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatIsDirtyIsTrueOnThingDeleted()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.UpdateProperties();
            await viewModel.SaveDiagramCommand.Execute();
            Assert.IsFalse(viewModel.IsDirty);

            var thingNumber = viewModel.ThingDiagramItemViewModels.Count;
            Assert.IsTrue(viewModel.ThingDiagramItemViewModels.Any());

            viewModel.RemoveDiagramThingItem(viewModel.ThingDiagramItemViewModels.FirstOrDefault());
            Assert.Greater(thingNumber, viewModel.ThingDiagramItemViewModels.Count);
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatIsDirtyIsFalseOnThingLoaded()
        {
            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            Assert.IsTrue(this.diagram.DiagramElement.Any());
            viewModel.UpdateProperties();
            Assert.IsTrue(viewModel.ThingDiagramItemViewModels.Any());
            Assert.IsFalse(viewModel.IsDirty);
        }

        [Test]
        public void VerifyThatRelationShipsGetDrawnOnLoad()
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri);
            relationship.Category.Add(this.relationshipCat);
            relationship.Source = this.diagramObject1.DepictedThing;
            relationship.Target = this.diagramObject2.DepictedThing;

            this.iteration.Relationship.Add(relationship);

            var viewModel = new DiagramEditorViewModel(this.diagram, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, this.pluginSettingsService.Object)
            {
                Behavior = this.mockDiagramBehavior.Object
            };

            viewModel.UpdateProperties();
            viewModel.ComputeDiagramConnector();
            Assert.IsNotEmpty(viewModel.ThingDiagramItemViewModels);
            viewModel.Dispose();
        }
    }
}
