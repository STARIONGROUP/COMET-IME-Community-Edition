// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipTraceabilityViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4DiagramEditor.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.Behaviors;
    using CDP4DiagramEditor.Helpers;
    using CDP4DiagramEditor.Settings;
    using CDP4DiagramEditor.ViewModels;

    using DevExpress.Diagram.Core;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipTraceabilityViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RelationshipTraceabilityViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private readonly Uri uri = new Uri("http://test.com");
        private CDPMessageBus messageBus;
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;

        private Category specCategory;
        private Category traceCategory;
        private Category otherCategory;

        private RequirementsSpecification spec1;
        private RequirementsSpecification spec2;
        private RequirementsSpecification spec3;
        private ElementDefinition elementDefinition;

        private DiagramEditorPluginSettings settings;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.assembler.Cache;
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.specCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "spec", ShortName = "spec" };
            this.specCategory.PermissibleClass.Add(ClassKind.RequirementsSpecification);

            this.traceCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "trace", ShortName = "trace" };
            this.traceCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.otherCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "other", ShortName = "other" };
            this.otherCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);
            this.otherCategory.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.srdl.DefinedCategory.Add(this.specCategory);
            this.srdl.DefinedCategory.Add(this.traceCategory);
            this.srdl.DefinedCategory.Add(this.otherCategory);

            this.spec1 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec1" };
            this.spec2 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec2" };
            this.spec3 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec3" };
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { ShortName = "ed" };

            this.spec1.Category.Add(this.specCategory);
            this.spec2.Category.Add(this.specCategory);

            this.iteration.RequirementsSpecification.Add(this.spec1);
            this.iteration.RequirementsSpecification.Add(this.spec2);
            this.iteration.RequirementsSpecification.Add(this.spec3);
            this.iteration.Element.Add(this.elementDefinition);

            this.AddToCache(this.iteration);
            this.AddToCache(this.spec1);
            this.AddToCache(this.spec2);
            this.AddToCache(this.spec3);
            this.AddToCache(this.elementDefinition);

            this.settings = new DiagramEditorPluginSettings();
            this.pluginSettingsService.Setup(x => x.Read<DiagramEditorPluginSettings>(false)).Returns(this.settings);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new List<ReferenceDataLibrary> { this.srdl });
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// Adds a <see cref="Thing"/> to the assembler cache under the iteration under test
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to add</param>
        private void AddToCache(Thing thing)
        {
            this.cache.TryAdd(new CacheKey(thing.Iid, this.iteration.Iid), new Lazy<Thing>(() => thing));
        }

        /// <summary>
        /// Adds a <see cref="BinaryRelationship"/> to the <see cref="Iteration"/> under test
        /// </summary>
        /// <param name="source">The source <see cref="Thing"/></param>
        /// <param name="target">The target <see cref="Thing"/></param>
        /// <param name="categories">The <see cref="Category"/>s of the relationship</param>
        /// <returns>The created <see cref="BinaryRelationship"/></returns>
        private BinaryRelationship AddBinaryRelationship(Thing source, Thing target, params Category[] categories)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri)
            {
                Source = source,
                Target = target
            };

            relationship.Category.AddRange(categories);
            this.iteration.Relationship.Add(relationship);

            return relationship;
        }

        /// <summary>
        /// Creates the <see cref="RelationshipTraceabilityViewModel"/> under test
        /// </summary>
        /// <returns>The created <see cref="RelationshipTraceabilityViewModel"/></returns>
        private RelationshipTraceabilityViewModel CreateViewModel()
        {
            return new RelationshipTraceabilityViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                null,
                this.pluginSettingsService.Object);
        }

        [Test]
        public void VerifyThatAMissingSettingsFileIsCreatedOnFirstUse()
        {
            this.pluginSettingsService
                .Setup(x => x.Read<DiagramEditorPluginSettings>(false))
                .Throws(new PluginSettingsException("The PluginSettings could not be read", new System.IO.FileNotFoundException()));

            RelationshipTraceabilityViewModel viewModel = null;
            Assert.DoesNotThrow(() => viewModel = this.CreateViewModel());

            Assert.That(viewModel.SavedConfigurations, Is.Empty);
            this.pluginSettingsService.Verify(x => x.Write(It.IsAny<DiagramEditorPluginSettings>()), Times.Once);

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatPanelInitializesEmpty()
        {
            var viewModel = this.CreateViewModel();

            Assert.That(viewModel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewModel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewModel.Nodes, Is.Empty);
            Assert.That(viewModel.Edges, Is.Empty);

            // only categories permissible on the offered class kinds are shown
            Assert.That(viewModel.PossibleCategories, Is.EquivalentTo(new[] { this.specCategory, this.otherCategory }));
            Assert.That(viewModel.PossibleRelationshipCategories, Is.EquivalentTo(new[] { this.traceCategory, this.otherCategory }));

            // the curated class kind set, not every categorizable class kind of the model
            Assert.That(viewModel.PossibleClassKinds, Does.Contain(ClassKind.RequirementsSpecification));
            Assert.That(viewModel.PossibleClassKinds, Does.Not.Contain(ClassKind.Glossary));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatPossibleCategoriesFollowTheSelectedRootClassKinds()
        {
            var viewModel = this.CreateViewModel();

            viewModel.SelectedRootClassKinds = new List<ClassKind> { ClassKind.RequirementsSpecification };
            Assert.That(viewModel.PossibleCategories, Is.EquivalentTo(new[] { this.specCategory }));

            viewModel.SelectedRootClassKinds = new List<ClassKind> { ClassKind.ElementDefinition };
            Assert.That(viewModel.PossibleCategories, Is.EquivalentTo(new[] { this.otherCategory }));

            viewModel.SelectedRootClassKinds = new List<ClassKind>();
            Assert.That(viewModel.PossibleCategories, Is.EquivalentTo(new[] { this.specCategory, this.otherCategory }));

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatDroppingAThingAddsARootAndComputesTheGraph()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var viewModel = this.CreateViewModel();

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.spec1);
            dropInfo.SetupProperty(x => x.Effects);

            viewModel.DragOver(dropInfo.Object);
            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.Copy));

            await viewModel.Drop(dropInfo.Object);

            // default configuration: two levels down
            Assert.That(viewModel.RootThings, Is.EquivalentTo(new Thing[] { this.spec1 }));
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
            Assert.That(viewModel.Edges.Count, Is.EqualTo(2));
            Assert.That(viewModel.StatusMessage, Does.Contain("3 nodes"));

            // dropping the same thing again is refused
            viewModel.DragOver(dropInfo.Object);
            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.None));

            // a thing that does not belong to this iteration is refused
            var foreignThing = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            dropInfo.Setup(x => x.Payload).Returns(foreignThing);

            viewModel.DragOver(dropInfo.Object);
            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.None));

            await viewModel.Drop(dropInfo.Object);
            Assert.That(viewModel.RootThings, Does.Not.Contain(foreignThing));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatClassKindAndCategoryRootsAreComputed()
        {
            this.AddBinaryRelationship(this.spec1, this.elementDefinition, this.traceCategory);

            var viewModel = this.CreateViewModel();

            viewModel.SelectedRootClassKinds = new List<ClassKind> { ClassKind.RequirementsSpecification };

            // spec1..spec3 are roots, spec1 reaches the element definition
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3, this.elementDefinition }));

            viewModel.SelectedRootCategories = new List<Category> { this.specCategory };

            // only spec1 and spec2 carry the spec category
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.elementDefinition }));

            // multiple class kinds combine: the uncategorized element definition becomes a root through its own kind
            viewModel.SelectedRootCategories = new List<Category>();
            viewModel.SelectedRootClassKinds = new List<ClassKind> { ClassKind.RequirementsSpecification, ClassKind.ElementDefinition };

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3, this.elementDefinition }));
            Assert.That(viewModel.Nodes.Single(x => x.Thing == this.elementDefinition).IsRoot, Is.True);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatExcludingANodeRemovesItAndItsSubtree()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var viewModel = this.CreateViewModel();

            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            Assert.That(viewModel.Nodes.Count, Is.EqualTo(3));

            viewModel.SelectedNode = viewModel.Nodes.Single(x => x.Thing == this.spec2);
            await viewModel.ExcludeSelectedNodeCommand.Execute();

            // spec3 was only reachable through spec2, so it disappears with it
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1 }));
            Assert.That(viewModel.StatusMessage, Does.Contain("1 excluded"));
            Assert.That(viewModel.SelectedNode, Is.Null);

            // the exclusion is listed and can be restored individually
            Assert.That(viewModel.ExcludedThings, Is.EquivalentTo(new Thing[] { this.spec2 }));

            viewModel.SelectedExcludedThing = this.spec2;
            await viewModel.RestoreExcludedThingCommand.Execute();

            Assert.That(viewModel.Nodes.Count, Is.EqualTo(3));
            Assert.That(viewModel.ExcludedThings, Is.Empty);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatEdgesRunInLevelOrderAndMarkReversedArrows()
        {
            // spec1 -> spec2 along the arrows, spec3 -> spec2 against them (reached at level 2)
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);

            await viewModel.AddLevelFilterRowCommand.Execute();
            var row = viewModel.LevelFilterRows.Single();
            row.Level = 2;
            row.SelectedDirection = TraceabilityDirectionOption.All.Single(x => x.Direction == RelationshipTraversalDirection.AgainstArrows);

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));

            var alongEdge = viewModel.Edges.Single(x => x.FromId == this.spec1.Iid);
            Assert.That(alongEdge.ToId, Is.EqualTo(this.spec2.Iid));
            Assert.That(alongEdge.IsReversed, Is.False);

            // the reversed link is laid out level 1 -> level 2 but flagged so the arrow is drawn backwards
            var reversedEdge = viewModel.Edges.Single(x => x.ToId == this.spec3.Iid);
            Assert.That(reversedEdge.FromId, Is.EqualTo(this.spec2.Iid));
            Assert.That(reversedEdge.IsReversed, Is.True);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatDroppingAnExcludedThingLiftsTheExclusion()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            viewModel.SelectedNode = viewModel.Nodes.Single(x => x.Thing == this.spec2);
            await viewModel.ExcludeSelectedNodeCommand.Execute();

            Assert.That(viewModel.ExcludedThings, Is.EquivalentTo(new Thing[] { this.spec2 }));

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.spec2);
            dropInfo.SetupProperty(x => x.Effects);

            viewModel.DragOver(dropInfo.Object);
            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.Copy));

            await viewModel.Drop(dropInfo.Object);

            // the accepted drop actually shows the thing instead of being swallowed by the exclusion
            Assert.That(viewModel.ExcludedThings, Is.Empty);
            Assert.That(viewModel.RootThings, Does.Contain(this.spec2));
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Does.Contain(this.spec2));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatDeprecatingADisplayedThingRefreshesTheDiagram()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            Assert.That(viewModel.Nodes.Count, Is.EqualTo(2));

            this.spec2.IsDeprecated = true;
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1 }));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatRenamingADisplayedThingRefreshesTheDiagram()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            this.spec2.ShortName = "renamed";
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.That(viewModel.Nodes.Single(x => x.Thing == this.spec2).ShortName, Is.EqualTo("renamed"));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatTheDefaultDirectionIsApplied()
        {
            // both arrows point at spec1, so the natural downward expansion finds nothing
            this.AddBinaryRelationship(this.spec2, this.spec1, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            Assert.That(viewModel.Nodes.Count, Is.EqualTo(1));

            viewModel.SelectedDefaultDirection = viewModel.PossibleDirections.First(x => x.Direction == RelationshipTraversalDirection.AgainstArrows);

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatLayoutDirectionAndExportAreForwardedToTheBehavior()
        {
            var behavior = new Mock<ITraceabilityDiagramBehavior>();

            var viewModel = this.CreateViewModel();
            viewModel.Behavior = behavior.Object;

            viewModel.LayoutDirection = Direction.Up;
            behavior.Verify(x => x.ApplyLayout(), Times.Once);

            await viewModel.ExportCommand.Execute("SVG");
            behavior.Verify(x => x.Export(DiagramExportFormat.SVG), Times.Once);

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatTheDefaultLevelFilterIsApplied()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec1, this.spec3, this.otherCategory);

            var viewModel = this.CreateViewModel();

            viewModel.RootThings.Add(this.spec1);
            viewModel.SelectedDefaultLevelCategories = new List<Category> { this.traceCategory };

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatALevelFilterOverrideRowIsApplied()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);

            await viewModel.AddLevelFilterRowCommand.Execute();
            var row = viewModel.LevelFilterRows.Single();

            row.Level = 2;
            row.SelectedCategories = new List<Category> { this.otherCategory };

            // level two only follows the other category, so spec3 is no longer reached
            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));

            viewModel.SelectedLevelFilterRow = row;
            await viewModel.RemoveLevelFilterRowCommand.Execute();

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatTheMaxNodeWarningTrips()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var viewModel = this.CreateViewModel();

            viewModel.RootThings.Add(this.spec1);
            viewModel.MaxNodes = 2;

            Assert.That(viewModel.IsMaxNodeCountReached, Is.True);
            Assert.That(viewModel.Nodes.Count, Is.EqualTo(2));

            viewModel.MaxNodes = 300;

            Assert.That(viewModel.IsMaxNodeCountReached, Is.False);

            viewModel.Dispose();
        }

        [Test]
        public void VerifyThatARelationshipChangeRecomputesTheGraph()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var viewModel = this.CreateViewModel();
            viewModel.RootThings.Add(this.spec1);
            viewModel.ComputeGraph();

            Assert.That(viewModel.Nodes.Count, Is.EqualTo(2));

            var newRelationship = this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.messageBus.SendObjectChangeEvent(newRelationship, EventKind.Added);

            Assert.That(viewModel.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatAConfigurationCanBeSavedAppliedAndDeleted()
        {
            var viewModel = this.CreateViewModel();

            viewModel.DepthDown = 3;
            viewModel.DepthUp = 1;
            viewModel.SelectedDefaultLevelCategories = new List<Category> { this.traceCategory };
            viewModel.SelectedRootClassKinds = new List<ClassKind> { ClassKind.RequirementsSpecification, ClassKind.ElementDefinition };
            viewModel.SelectedRootCategories = new List<Category> { this.specCategory };
            viewModel.ConfigurationName = "my preset";

            await viewModel.SaveConfigurationCommand.Execute();

            this.pluginSettingsService.Verify(x => x.Write(this.settings), Times.Once);
            Assert.That(this.settings.SavedConfigurations.OfType<TraceabilityConfiguration>().Single().Name, Is.EqualTo("my preset"));
            Assert.That(viewModel.SavedConfigurations.Count, Is.EqualTo(1));
            Assert.That(viewModel.SelectedConfiguration, Is.Not.Null);

            // change the state away from the preset, then re-apply it
            viewModel.DepthDown = 1;
            viewModel.SelectedDefaultLevelCategories = new List<Category>();

            viewModel.SelectedConfiguration = null;
            viewModel.SelectedConfiguration = viewModel.SavedConfigurations.Single();

            Assert.That(viewModel.DepthDown, Is.EqualTo(3));
            Assert.That(viewModel.DepthUp, Is.EqualTo(1));
            Assert.That(viewModel.SelectedDefaultLevelCategories, Is.EquivalentTo(new[] { this.traceCategory }));
            Assert.That(viewModel.SelectedRootClassKinds, Is.EquivalentTo(new[] { ClassKind.RequirementsSpecification, ClassKind.ElementDefinition }));
            Assert.That(viewModel.SelectedRootCategories, Is.EquivalentTo(new[] { this.specCategory }));

            await viewModel.DeleteConfigurationCommand.Execute();

            Assert.That(this.settings.SavedConfigurations, Is.Empty);
            Assert.That(viewModel.SavedConfigurations, Is.Empty);
            Assert.That(viewModel.SelectedConfiguration, Is.Null);

            viewModel.Dispose();
        }

        [Test]
        public async Task VerifyThatSavingUnderAnExistingNameReplacesThePreset()
        {
            var viewModel = this.CreateViewModel();

            viewModel.ConfigurationName = "preset";
            viewModel.DepthDown = 2;
            await viewModel.SaveConfigurationCommand.Execute();

            viewModel.DepthDown = 5;
            await viewModel.SaveConfigurationCommand.Execute();

            var saved = this.settings.SavedConfigurations.OfType<TraceabilityConfiguration>().Single();
            Assert.That(saved.DepthDown, Is.EqualTo(5));

            viewModel.Dispose();
        }
    }
}
