// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipTraceabilityViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4DiagramEditor.Behaviors;
    using CDP4DiagramEditor.Helpers;
    using CDP4DiagramEditor.Settings;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    /// <summary>
    /// The view-model of the relationship traceability diagram panel: an ephemeral, read-only diagram of the
    /// <see cref="Relationship"/>s reachable from a configurable set of roots, built by the
    /// <see cref="RelationshipGraphBuilder"/>. Nothing on this panel is written to the model.
    /// </summary>
    public class RelationshipTraceabilityViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Relationship Traceability";

        /// <summary>
        /// A value indicating whether the view-model finished initializing, used to keep the construction-time
        /// property assignments from triggering a recompute per property
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// The cached <see cref="RelationshipGraphBuilder"/>, invalidated when a relationship changes so the
        /// relationship indexes are not rebuilt on every configuration change
        /// </summary>
        private RelationshipGraphBuilder builder;

        /// <summary>
        /// Backing field for <see cref="SelectedRootThing"/>
        /// </summary>
        private Thing selectedRootThing;

        /// <summary>
        /// Backing field for <see cref="SelectedRootClassKinds"/>
        /// </summary>
        private List<ClassKind> selectedRootClassKinds = new List<ClassKind>();

        /// <summary>
        /// Backing field for <see cref="SelectedRootCategories"/>
        /// </summary>
        private List<Category> selectedRootCategories = new List<Category>();

        /// <summary>
        /// Backing field for <see cref="DepthDown"/>
        /// </summary>
        private int depthDown = 2;

        /// <summary>
        /// Backing field for <see cref="DepthUp"/>
        /// </summary>
        private int depthUp;

        /// <summary>
        /// Backing field for <see cref="MaxNodes"/>
        /// </summary>
        private int maxNodes = 300;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultLevelCategories"/>
        /// </summary>
        private List<Category> selectedDefaultLevelCategories = new List<Category>();

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultDirection"/>
        /// </summary>
        private TraceabilityDirectionOption selectedDefaultDirection = TraceabilityDirectionOption.All[0];

        /// <summary>
        /// Backing field for <see cref="SelectedExcludedThing"/>
        /// </summary>
        private Thing selectedExcludedThing;

        /// <summary>
        /// Backing field for <see cref="SelectedNode"/>
        /// </summary>
        private TraceabilityNodeViewModel selectedNode;

        /// <summary>
        /// Backing field for <see cref="LayoutDirection"/>
        /// </summary>
        private Direction layoutDirection = Direction.Down;

        /// <summary>
        /// Backing field for <see cref="SelectedLevelFilterRow"/>
        /// </summary>
        private TraceabilityLevelFilterRowViewModel selectedLevelFilterRow;

        /// <summary>
        /// Backing field for <see cref="IsMaxNodeCountReached"/>
        /// </summary>
        private bool isMaxNodeCountReached;

        /// <summary>
        /// Backing field for <see cref="StatusMessage"/>
        /// </summary>
        private string statusMessage;

        /// <summary>
        /// Backing field for <see cref="ConfigurationName"/>
        /// </summary>
        private string configurationName;

        /// <summary>
        /// Backing field for <see cref="SelectedConfiguration"/>
        /// </summary>
        private TraceabilityConfiguration selectedConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipTraceabilityViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> whose relationships are visualized</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        public RelationshipTraceabilityViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.RootThings = new ReactiveList<Thing>();
            this.ExcludedThings = new ReactiveList<Thing>();
            this.Nodes = new ReactiveList<TraceabilityNodeViewModel>();
            this.Edges = new ReactiveList<TraceabilityEdgeViewModel>();
            this.LevelFilterRows = new ReactiveList<TraceabilityLevelFilterRowViewModel>();
            this.PossibleCategories = new ReactiveList<Category>();
            this.PossibleRelationshipCategories = new ReactiveList<Category>();
            this.PossibleClassKinds = new ReactiveList<ClassKind>();
            this.SavedConfigurations = new ReactiveList<TraceabilityConfiguration>();

            this.PopulatePossibleValues();
            this.LoadSavedConfigurations();

            this.InitializeTraceabilityCommands();
            this.AddTraceabilitySubscriptions();

            this.isInitialized = true;
            this.ComputeGraph();
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets the explicitly picked root <see cref="Thing"/>s, populated by dropping things onto the diagram
        /// </summary>
        public ReactiveList<Thing> RootThings { get; }

        /// <summary>
        /// Gets the nodes of the rendered graph
        /// </summary>
        public ReactiveList<TraceabilityNodeViewModel> Nodes { get; }

        /// <summary>
        /// Gets the connectors of the rendered graph
        /// </summary>
        public ReactiveList<TraceabilityEdgeViewModel> Edges { get; }

        /// <summary>
        /// Gets the per-level overrides of the default relationship filter
        /// </summary>
        public ReactiveList<TraceabilityLevelFilterRowViewModel> LevelFilterRows { get; }

        /// <summary>
        /// Gets the <see cref="Category"/>s defined in the open reference data libraries
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets the <see cref="Category"/>s that are permissible on a <see cref="BinaryRelationship"/> or
        /// <see cref="MultiRelationship"/>
        /// </summary>
        public ReactiveList<Category> PossibleRelationshipCategories { get; }

        /// <summary>
        /// Gets the categorizable <see cref="ClassKind"/>s the root set and the node filter may be scoped to
        /// </summary>
        public ReactiveList<ClassKind> PossibleClassKinds { get; }

        /// <summary>
        /// Gets the saved <see cref="TraceabilityConfiguration"/> presets
        /// </summary>
        public ReactiveList<TraceabilityConfiguration> SavedConfigurations { get; }

        /// <summary>
        /// Gets or sets the root <see cref="Thing"/> selected in the roots list, used by the remove command
        /// </summary>
        public Thing SelectedRootThing
        {
            get => this.selectedRootThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedRootThing, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind"/>s that define the root set, or empty when the roots are only the
        /// explicitly dropped <see cref="Thing"/>s
        /// </summary>
        public List<ClassKind> SelectedRootClassKinds
        {
            get => this.selectedRootClassKinds;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedRootClassKinds, value ?? new List<ClassKind>());
                this.PopulatePossibleCategories();
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Category"/>s the <see cref="SelectedRootClassKinds"/> root set is filtered by
        /// </summary>
        public List<Category> SelectedRootCategories
        {
            get => this.selectedRootCategories;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedRootCategories, value ?? new List<Category>());
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the number of levels that are traversed downward
        /// </summary>
        public int DepthDown
        {
            get => this.depthDown;
            set
            {
                this.RaiseAndSetIfChanged(ref this.depthDown, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the number of levels that are traversed upward
        /// </summary>
        public int DepthUp
        {
            get => this.depthUp;
            set
            {
                this.RaiseAndSetIfChanged(ref this.depthUp, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of nodes that the diagram may contain
        /// </summary>
        public int MaxNodes
        {
            get => this.maxNodes;
            set
            {
                this.RaiseAndSetIfChanged(ref this.maxNodes, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Category"/>s that a relationship must be a member of to be followed, at any
        /// level without an override. Empty means every relationship is followed.
        /// </summary>
        public List<Category> SelectedDefaultLevelCategories
        {
            get => this.selectedDefaultLevelCategories;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedDefaultLevelCategories, value ?? new List<Category>());
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the node currently selected on the diagram, used by the exclude command
        /// </summary>
        public TraceabilityNodeViewModel SelectedNode
        {
            get => this.selectedNode;
            set => this.RaiseAndSetIfChanged(ref this.selectedNode, value);
        }

        /// <summary>
        /// Gets the <see cref="Thing"/>s that were excluded from the diagram by the user
        /// </summary>
        public ReactiveList<Thing> ExcludedThings { get; }

        /// <summary>
        /// Gets or sets the <see cref="Thing"/> selected in the excluded list, used by the restore command
        /// </summary>
        public Thing SelectedExcludedThing
        {
            get => this.selectedExcludedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedExcludedThing, value);
        }

        /// <summary>
        /// Gets or sets how relationship arrows are followed at levels without an override; the first
        /// <see cref="TraceabilityDirectionOption"/> stands for the natural direction of the expansion. Overriding
        /// this allows a hierarchy whose relationship arrows change direction along the chain to be traversed in one
        /// pass.
        /// </summary>
        public TraceabilityDirectionOption SelectedDefaultDirection
        {
            get => this.selectedDefaultDirection;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedDefaultDirection, value ?? TraceabilityDirectionOption.All[0]);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets the traversal directions the user may pick from
        /// </summary>
        public IReadOnlyList<TraceabilityDirectionOption> PossibleDirections => TraceabilityDirectionOption.All;

        /// <summary>
        /// Gets or sets the orientation of the automatic layout
        /// </summary>
        public Direction LayoutDirection
        {
            get => this.layoutDirection;
            set
            {
                this.RaiseAndSetIfChanged(ref this.layoutDirection, value);
                this.Behavior?.ApplyLayout();
            }
        }

        /// <summary>
        /// Gets the orientations the automatic layout can be applied in
        /// </summary>
        public IReadOnlyList<TraceabilityLayoutOption> PossibleLayoutDirections { get; } = new List<TraceabilityLayoutOption>
        {
            new TraceabilityLayoutOption(Direction.Down, "Top-down"),
            new TraceabilityLayoutOption(Direction.Up, "Bottom-up"),
            new TraceabilityLayoutOption(Direction.Right, "Left to right"),
            new TraceabilityLayoutOption(Direction.Left, "Right to left")
        };

        /// <summary>
        /// Gets or sets the <see cref="ITraceabilityDiagramBehavior"/> that the attached diagram control injects, used
        /// to apply the layout and to export the diagram
        /// </summary>
        public ITraceabilityDiagramBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets the selected per-level override row, used by the remove command
        /// </summary>
        public TraceabilityLevelFilterRowViewModel SelectedLevelFilterRow
        {
            get => this.selectedLevelFilterRow;
            set => this.RaiseAndSetIfChanged(ref this.selectedLevelFilterRow, value);
        }

        /// <summary>
        /// Gets a value indicating whether the last traversal was cut short by <see cref="MaxNodes"/>, in which case
        /// the diagram is incomplete and a warning is shown
        /// </summary>
        public bool IsMaxNodeCountReached
        {
            get => this.isMaxNodeCountReached;
            private set => this.RaiseAndSetIfChanged(ref this.isMaxNodeCountReached, value);
        }

        /// <summary>
        /// Gets the status message describing the rendered graph
        /// </summary>
        public string StatusMessage
        {
            get => this.statusMessage;
            private set => this.RaiseAndSetIfChanged(ref this.statusMessage, value);
        }

        /// <summary>
        /// Gets or sets the name under which the current configuration is saved
        /// </summary>
        public string ConfigurationName
        {
            get => this.configurationName;
            set => this.RaiseAndSetIfChanged(ref this.configurationName, value);
        }

        /// <summary>
        /// Gets or sets the selected saved configuration. Setting it applies the preset to the panel.
        /// </summary>
        public TraceabilityConfiguration SelectedConfiguration
        {
            get => this.selectedConfiguration;
            set
            {
                // the editor re-commits an unchanged selection on focus changes; re-applying then would silently
                // revert any tweak the user made since selecting the preset
                var changed = !ReferenceEquals(this.selectedConfiguration, value);

                this.RaiseAndSetIfChanged(ref this.selectedConfiguration, value);

                if (changed)
                {
                    this.ApplyConfiguration(value);
                }
            }
        }

        /// <summary>
        /// Gets the command that recomputes the graph
        /// </summary>
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the command that removes <see cref="SelectedRootThing"/> from <see cref="RootThings"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveRootThingCommand { get; private set; }

        /// <summary>
        /// Gets the command that clears <see cref="RootThings"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearRootThingsCommand { get; private set; }

        /// <summary>
        /// Gets the command that adds a per-level override row
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddLevelFilterRowCommand { get; private set; }

        /// <summary>
        /// Gets the command that removes <see cref="SelectedLevelFilterRow"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveLevelFilterRowCommand { get; private set; }

        /// <summary>
        /// Gets the command that saves the current configuration under <see cref="ConfigurationName"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveConfigurationCommand { get; private set; }

        /// <summary>
        /// Gets the command that deletes <see cref="SelectedConfiguration"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteConfigurationCommand { get; private set; }

        /// <summary>
        /// Gets the command invoked when the diagram selection changes; it publishes the selected node's
        /// <see cref="Thing"/> so the property grid shows it
        /// </summary>
        public ReactiveCommand<object, Unit> DiagramSelectionChangedCommand { get; private set; }

        /// <summary>
        /// Gets the command that excludes <see cref="SelectedNode"/> from the diagram; everything that is only
        /// reachable through it disappears with it
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExcludeSelectedNodeCommand { get; private set; }

        /// <summary>
        /// Gets the command that restores <see cref="SelectedExcludedThing"/> to the diagram
        /// </summary>
        public ReactiveCommand<Unit, Unit> RestoreExcludedThingCommand { get; private set; }

        /// <summary>
        /// Gets the command that clears all exclusions
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetExclusionsCommand { get; private set; }

        /// <summary>
        /// Gets the command that exports the diagram; its parameter is the name of a
        /// <see cref="DiagramExportFormat"/> value (PNG, JPEG or SVG)
        /// </summary>
        public ReactiveCommand<string, Unit> ExportCommand { get; private set; }

        /// <summary>
        /// Updates the current drag state
        /// </summary>
        /// <param name="dropInfo">Information about the drag operation</param>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Thing thing && this.IsDroppableRoot(thing))
            {
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation: the dropped <see cref="Thing"/> becomes an additional root
        /// </summary>
        /// <param name="dropInfo">Information about the drop operation</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Thing thing && this.IsDroppableRoot(thing))
            {
                this.RootThings.Add(thing);
                this.ComputeGraph();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Asserts whether a dragged <see cref="Thing"/> may become a root: it must belong to the iteration this panel
        /// shows and not be a root already
        /// </summary>
        /// <param name="thing">The dragged <see cref="Thing"/></param>
        /// <returns>true when the <see cref="Thing"/> may become a root</returns>
        private bool IsDroppableRoot(Thing thing)
        {
            return thing.GetContainerOfType<Iteration>()?.Iid == this.Thing.Iid && this.RootThings.All(x => x.Iid != thing.Iid);
        }

        /// <summary>
        /// Recomputes the graph from the current configuration and repopulates <see cref="Nodes"/> and
        /// <see cref="Edges"/>
        /// </summary>
        public void ComputeGraph()
        {
            if (!this.isInitialized)
            {
                return;
            }

            // the node view-models are about to be recreated, so any diagram selection becomes stale
            this.SelectedNode = null;

            var roots = new List<Thing>(this.RootThings);

            if (this.SelectedRootClassKinds.Any())
            {
                roots.AddRange(this.QueryRootsByClassKindAndCategory());
            }

            this.builder = this.builder ?? new RelationshipGraphBuilder(this.Thing);
            var graph = this.builder.Build(roots, this.BuildConfiguration());

            this.Nodes.Clear();
            this.Nodes.AddRange(graph.Nodes.Select(x => new TraceabilityNodeViewModel(x)));

            var nodeLevels = graph.Nodes.ToDictionary(x => x.Thing.Iid, x => x.Level);

            this.Edges.Clear();
            this.Edges.AddRange(graph.Edges.Select(x => new TraceabilityEdgeViewModel(x, nodeLevels)));

            this.IsMaxNodeCountReached = graph.MaxNodeCountReached;

            var exclusionSuffix = this.ExcludedThings.Any() ? $", {this.ExcludedThings.Count} excluded" : string.Empty;
            this.StatusMessage = $"{graph.Nodes.Count} nodes, {graph.Edges.Count} relationships{exclusionSuffix}";
        }

        /// <summary>
        /// Queries the <see cref="Thing"/>s of the current iteration that match <see cref="SelectedRootClassKinds"/>
        /// and <see cref="SelectedRootCategories"/>
        /// </summary>
        /// <returns>The matching <see cref="Thing"/>s</returns>
        private IEnumerable<Thing> QueryRootsByClassKindAndCategory()
        {
            var candidates = this.Session.Assembler.Cache
                .Where(x => x.Key.Iteration.HasValue && x.Key.Iteration.Value == this.Thing.Iid)
                .Select(x => x.Value.Value)
                .Where(x => this.SelectedRootClassKinds.Contains(x.ClassKind));

            if (!this.SelectedRootCategories.Any())
            {
                return candidates;
            }

            var filter = new RelationshipGraphFilter();
            filter.Categories.AddRange(this.SelectedRootCategories);

            return candidates.Where(filter.IsMatch);
        }

        /// <summary>
        /// Builds the <see cref="RelationshipGraphConfiguration"/> from the current state of the panel
        /// </summary>
        /// <returns>The <see cref="RelationshipGraphConfiguration"/></returns>
        private RelationshipGraphConfiguration BuildConfiguration()
        {
            var configuration = new RelationshipGraphConfiguration
            {
                DepthDown = this.DepthDown,
                DepthUp = this.DepthUp,
                MaxNodes = this.MaxNodes,
                DefaultLevelFilter = BuildFilter(this.SelectedDefaultLevelCategories, this.SelectedDefaultDirection.Direction)
            };

            foreach (var excludedThing in this.ExcludedThings)
            {
                configuration.ExcludedThings.Add(excludedThing.Iid);
            }

            foreach (var row in this.LevelFilterRows.Where(x => x.IsActive))
            {
                configuration.LevelFilterOverrides[row.Level] = BuildFilter(row.SelectedCategories, row.Direction);
            }

            return configuration;
        }

        /// <summary>
        /// Builds a <see cref="RelationshipGraphFilter"/> from a set of <see cref="Category"/>s and a traversal
        /// direction
        /// </summary>
        /// <param name="categories">The <see cref="Category"/> facet</param>
        /// <param name="direction">The traversal direction, or null for the natural direction</param>
        /// <returns>The <see cref="RelationshipGraphFilter"/>, or null when both facets are empty</returns>
        private static RelationshipGraphFilter BuildFilter(IReadOnlyCollection<Category> categories, RelationshipTraversalDirection? direction)
        {
            if (categories.Count == 0 && direction == null)
            {
                return null;
            }

            var filter = new RelationshipGraphFilter { Direction = direction };
            filter.Categories.AddRange(categories);

            return filter;
        }

        /// <summary>
        /// Populates the possible <see cref="Category"/> and <see cref="ClassKind"/> pickers from the open reference
        /// data libraries
        /// </summary>
        private void PopulatePossibleValues()
        {
            this.PossibleRelationshipCategories.Clear();

            this.PossibleRelationshipCategories.AddRange(
                this.QueryOpenRdlCategories()
                    .Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship) || x.PermissibleClass.Contains(ClassKind.MultiRelationship)));

            this.PossibleClassKinds.Clear();

            this.PossibleClassKinds.AddRange(
                DiagramEditorPluginSettings.DefaultClassKinds
                    .Where(x => TypeInitializer.Initialize(x) is ICategorizableThing)
                    .OrderBy(x => x.ToString()));

            this.PopulatePossibleCategories();
        }

        /// <summary>
        /// Populates <see cref="PossibleCategories"/> with the <see cref="Category"/>s that are permissible on the
        /// selected root <see cref="ClassKind"/>s, or on any offered <see cref="ClassKind"/> when none is selected,
        /// mirroring the Relationship Matrix source configuration
        /// </summary>
        private void PopulatePossibleCategories()
        {
            var applicableKinds = this.SelectedRootClassKinds.Any()
                ? (IReadOnlyCollection<ClassKind>)this.SelectedRootClassKinds
                : this.PossibleClassKinds.ToList();

            this.PossibleCategories.Clear();

            this.PossibleCategories.AddRange(
                this.QueryOpenRdlCategories().Where(x => x.PermissibleClass.Intersect(applicableKinds).Any()));
        }

        /// <summary>
        /// Queries the <see cref="Category"/>s defined in the open reference data libraries, ordered by name
        /// </summary>
        /// <returns>The <see cref="Category"/>s</returns>
        private List<Category> QueryOpenRdlCategories()
        {
            return this.Session.OpenReferenceDataLibraries
                .SelectMany(x => x.DefinedCategory)
                .Distinct()
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand{TParam,TResult}"/>s of this panel
        /// </summary>
        private void InitializeTraceabilityCommands()
        {
            this.RefreshCommand = ReactiveCommandCreator.Create(this.ComputeGraph);

            this.RemoveRootThingCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    this.RootThings.Remove(this.SelectedRootThing);
                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedRootThing).Select(x => x != null));

            this.ClearRootThingsCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    this.RootThings.Clear();
                    this.ComputeGraph();
                });

            this.AddLevelFilterRowCommand = ReactiveCommandCreator.Create(
                () => this.LevelFilterRows.Add(new TraceabilityLevelFilterRowViewModel(this.PossibleRelationshipCategories, this.ComputeGraph)));

            this.RemoveLevelFilterRowCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    this.LevelFilterRows.Remove(this.SelectedLevelFilterRow);
                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedLevelFilterRow).Select(x => x != null));

            this.SaveConfigurationCommand = ReactiveCommandCreator.Create(
                this.SaveConfiguration,
                this.WhenAnyValue(x => x.ConfigurationName).Select(x => !string.IsNullOrWhiteSpace(x)));

            this.DeleteConfigurationCommand = ReactiveCommandCreator.Create(
                this.DeleteConfiguration,
                this.WhenAnyValue(x => x.SelectedConfiguration).Select(x => x != null));

            this.DiagramSelectionChangedCommand = ReactiveCommandCreator.Create<object>(this.OnDiagramSelectionChanged);

            this.ExcludeSelectedNodeCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    if (this.ExcludedThings.All(x => x.Iid != this.SelectedNode.Thing.Iid))
                    {
                        this.ExcludedThings.Add(this.SelectedNode.Thing);
                    }

                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedNode).Select(x => x != null));

            this.RestoreExcludedThingCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    this.ExcludedThings.Remove(this.SelectedExcludedThing);
                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedExcludedThing).Select(x => x != null));

            this.ResetExclusionsCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    this.ExcludedThings.Clear();
                    this.ComputeGraph();
                });

            this.ExportCommand = ReactiveCommandCreator.Create<string>(
                formatName =>
                {
                    if (Enum.TryParse(formatName, true, out DiagramExportFormat format))
                    {
                        this.Behavior?.Export(format);
                    }
                });
        }

        /// <summary>
        /// Adds the message bus subscriptions that keep the graph in sync with the model
        /// </summary>
        private void AddTraceabilitySubscriptions()
        {
            foreach (var relationshipType in new[] { typeof(BinaryRelationship), typeof(MultiRelationship) })
            {
                this.Disposables.Add(
                    this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(relationshipType)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ =>
                        {
                            this.builder = null;
                            this.ComputeGraph();
                        }));
            }

            this.Disposables.Add(
                this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Category))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                    {
                        this.PopulatePossibleValues();
                        this.ComputeGraph();
                    }));
        }

        /// <summary>
        /// Handles a selection change of the diagram control by publishing the selected node's <see cref="Thing"/> on
        /// the message bus, so the property grid shows it
        /// </summary>
        /// <param name="eventArgs">The event arguments of the selection change</param>
        private void OnDiagramSelectionChanged(object eventArgs)
        {
            if (!((eventArgs as DiagramSelectionChangedEventArgs)?.Source is DiagramControl diagramControl))
            {
                return;
            }

            var node = diagramControl.SelectedItems
                .OfType<DiagramContentItem>()
                .Select(x => x.Content)
                .OfType<TraceabilityNodeViewModel>()
                .FirstOrDefault();

            this.SelectedNode = node;

            if (node != null)
            {
                this.Session.CDPMessageBus.SendMessage(new SelectedThingChangedEvent(node.Thing, this.Session));
            }
        }

        /// <summary>
        /// Reads the <see cref="DiagramEditorPluginSettings"/>, creating the settings file with defaults on first use
        /// </summary>
        /// <returns>The <see cref="DiagramEditorPluginSettings"/></returns>
        private DiagramEditorPluginSettings ReadOrInitializeSettings()
        {
            try
            {
                return this.PluginSettingsService.Read<DiagramEditorPluginSettings>();
            }
            catch (PluginSettingsException)
            {
                // first use: no settings file exists yet, create it with defaults
                var settings = new DiagramEditorPluginSettings();
                this.PluginSettingsService.Write(settings);

                return settings;
            }
        }

        /// <summary>
        /// Loads the saved <see cref="TraceabilityConfiguration"/> presets from the plugin settings
        /// </summary>
        private void LoadSavedConfigurations()
        {
            var settings = this.ReadOrInitializeSettings();

            this.SavedConfigurations.Clear();
            this.SavedConfigurations.AddRange(settings.SavedConfigurations.OfType<TraceabilityConfiguration>());
        }

        /// <summary>
        /// Saves the current configuration under <see cref="ConfigurationName"/>, replacing an existing preset with
        /// the same name
        /// </summary>
        private void SaveConfiguration()
        {
            this.ConfigurationName = this.ConfigurationName.Trim();

            var configuration = new TraceabilityConfiguration
            {
                Name = this.ConfigurationName,
                RootClassKinds = this.SelectedRootClassKinds.ToList(),
                RootCategories = this.SelectedRootCategories.Select(x => x.Iid).ToList(),
                DepthDown = this.DepthDown,
                DepthUp = this.DepthUp,
                MaxNodes = this.MaxNodes,
                LayoutDirection = this.LayoutDirection,
                DefaultLevelCategories = this.SelectedDefaultLevelCategories.Select(x => x.Iid).ToList(),
                DefaultDirection = this.SelectedDefaultDirection.Direction,
                LevelOverrides = this.LevelFilterRows
                    .Where(x => x.IsActive)
                    .Select(x => new TraceabilityLevelOverride { Level = x.Level, Direction = x.Direction, Categories = x.SelectedCategories.Select(c => c.Iid).ToList() })
                    .ToList()
            };

            var settings = this.ReadOrInitializeSettings();

            var existing = settings.SavedConfigurations
                .OfType<TraceabilityConfiguration>()
                .FirstOrDefault(x => x.Name == configuration.Name);

            if (existing != null)
            {
                configuration.Id = existing.Id;
                settings.SavedConfigurations.Remove(existing);
            }

            settings.SavedConfigurations.Add(configuration);
            this.PluginSettingsService.Write(settings);

            this.LoadSavedConfigurations();
            this.selectedConfiguration = this.SavedConfigurations.FirstOrDefault(x => x.Id == configuration.Id);
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));
        }

        /// <summary>
        /// Deletes <see cref="SelectedConfiguration"/> from the plugin settings
        /// </summary>
        private void DeleteConfiguration()
        {
            var settings = this.ReadOrInitializeSettings();

            var existing = settings.SavedConfigurations
                .OfType<TraceabilityConfiguration>()
                .FirstOrDefault(x => x.Id == this.SelectedConfiguration.Id);

            if (existing != null)
            {
                settings.SavedConfigurations.Remove(existing);
                this.PluginSettingsService.Write(settings);
            }

            this.selectedConfiguration = null;
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));
            this.LoadSavedConfigurations();
        }

        /// <summary>
        /// Applies a saved <see cref="TraceabilityConfiguration"/> to the panel and recomputes the graph once
        /// </summary>
        /// <param name="configuration">The <see cref="TraceabilityConfiguration"/> to apply, or null</param>
        private void ApplyConfiguration(TraceabilityConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            this.isInitialized = false;

            try
            {
                this.ConfigurationName = configuration.Name;
                this.SelectedRootClassKinds = configuration.RootClassKinds.ToList();
                this.SelectedRootCategories = this.ResolveCategories(configuration.RootCategories);
                this.DepthDown = configuration.DepthDown;
                this.DepthUp = configuration.DepthUp;
                this.MaxNodes = configuration.MaxNodes;
                this.LayoutDirection = configuration.LayoutDirection;
                this.SelectedDefaultLevelCategories = this.ResolveCategories(configuration.DefaultLevelCategories);
                this.SelectedDefaultDirection = ResolveDirectionOption(configuration.DefaultDirection);

                // ad-hoc exclusions would silently distort the applied preset
                this.ExcludedThings.Clear();

                this.LevelFilterRows.Clear();

                foreach (var levelOverride in configuration.LevelOverrides)
                {
                    this.LevelFilterRows.Add(new TraceabilityLevelFilterRowViewModel(this.PossibleRelationshipCategories, this.ComputeGraph)
                    {
                        Level = levelOverride.Level,
                        SelectedDirection = ResolveDirectionOption(levelOverride.Direction),
                        SelectedCategories = this.ResolveCategories(levelOverride.Categories)
                    });
                }
            }
            finally
            {
                this.isInitialized = true;
            }

            this.ComputeGraph();
        }

        /// <summary>
        /// Resolves a persisted traversal direction to its <see cref="TraceabilityDirectionOption"/>
        /// </summary>
        /// <param name="direction">The persisted direction, or null for the natural direction</param>
        /// <returns>The matching <see cref="TraceabilityDirectionOption"/></returns>
        private static TraceabilityDirectionOption ResolveDirectionOption(RelationshipTraversalDirection? direction)
        {
            return TraceabilityDirectionOption.All.First(x => x.Direction == direction);
        }

        /// <summary>
        /// Resolves persisted <see cref="Category"/> identifiers against the open reference data libraries. Categories
        /// that are not present in them are silently dropped.
        /// </summary>
        /// <param name="iids">The persisted <see cref="Thing.Iid"/>s</param>
        /// <returns>The resolved <see cref="Category"/>s</returns>
        private List<Category> ResolveCategories(IEnumerable<Guid> iids)
        {
            var openRdlCategories = this.QueryOpenRdlCategories();

            return iids.Select(iid => openRdlCategories.FirstOrDefault(x => x.Iid == iid))
                .Where(x => x != null)
                .ToList();
        }
    }
}
