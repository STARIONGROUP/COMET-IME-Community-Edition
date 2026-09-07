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

namespace CDP4Grapher.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
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
    using CDP4Composition.ViewModels;
    using CDP4Composition.ViewModels.DialogResult;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Helpers;
    using CDP4Grapher.Settings;

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
        /// The smallest value <see cref="MaxNodes"/> may take. Typed as the spin editor's own value type so that the
        /// editor bounds and the warning message cannot drift apart.
        /// </summary>
        public const decimal MinimumMaxNodes = 10;

        /// <summary>
        /// The largest value <see cref="MaxNodes"/> may take. Typed as the spin editor's own value type so that the
        /// editor bounds and the warning message cannot drift apart.
        /// </summary>
        public const decimal MaximumMaxNodes = 5000;

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
        /// The subscriptions on the <see cref="Thing"/>s that are currently rendered as nodes, so that renaming or
        /// deprecating a displayed <see cref="Thing"/> refreshes the diagram. They are replaced on every recompute.
        /// </summary>
        private readonly CompositeDisposable nodeSubscriptions = new CompositeDisposable();

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
        /// Backing field for <see cref="SelectedEdge"/>
        /// </summary>
        private TraceabilityEdgeViewModel selectedEdge;

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
        /// Backing field for <see cref="MaxNodeCountMessage"/>
        /// </summary>
        private string maxNodeCountMessage;

        /// <summary>
        /// Backing field for <see cref="SelectedConfiguration"/>
        /// </summary>
        private TraceabilityConfiguration selectedConfiguration;

        /// <summary>
        /// Backing field for <see cref="ShowClassKind"/>
        /// </summary>
        private bool showClassKind = true;

        /// <summary>
        /// Backing field for <see cref="ShowShortName"/>
        /// </summary>
        private bool showShortName = true;

        /// <summary>
        /// Backing field for <see cref="ShowName"/>
        /// </summary>
        private bool showName = true;

        /// <summary>
        /// Backing field for <see cref="ShowDefinition"/>
        /// </summary>
        private bool showDefinition;

        /// <summary>
        /// Backing field for <see cref="DefinitionMaxLength"/>
        /// </summary>
        private int definitionMaxLength = 100;

        /// <summary>
        /// Backing field for <see cref="LinkSourceThing"/>
        /// </summary>
        private Thing linkSourceThing;

        /// <summary>
        /// Backing field for <see cref="SelectedThingDetails"/>
        /// </summary>
        private string selectedThingDetails;

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
                if (value == this.depthDown)
                {
                    return;
                }

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
                if (value == this.depthUp)
                {
                    return;
                }

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
                if (value == this.maxNodes)
                {
                    return;
                }

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
        /// Gets or sets the connector currently selected on the diagram, used by the edit and inspect commands
        /// </summary>
        public TraceabilityEdgeViewModel SelectedEdge
        {
            get => this.selectedEdge;
            set => this.RaiseAndSetIfChanged(ref this.selectedEdge, value);
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> the diagram selection points at: the <see cref="Thing"/> of the
        /// <see cref="SelectedNode"/>, or the <see cref="Relationship"/> of the <see cref="SelectedEdge"/> when a
        /// connector is selected instead
        /// </summary>
        public Thing SelectedDiagramThing => this.SelectedNode?.Thing ?? this.SelectedEdge?.Relationship;

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
        /// Gets the warning shown when <see cref="IsMaxNodeCountReached"/>, naming the maximum that was hit and the
        /// range it may be raised within, so the user does not have to guess the numbers
        /// </summary>
        public string MaxNodeCountMessage
        {
            get => this.maxNodeCountMessage;
            private set => this.RaiseAndSetIfChanged(ref this.maxNodeCountMessage, value);
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
        /// Gets or sets a value indicating whether the node boxes show the class kind line
        /// </summary>
        public bool ShowClassKind
        {
            get => this.showClassKind;
            set
            {
                this.RaiseAndSetIfChanged(ref this.showClassKind, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node boxes show the short name line
        /// </summary>
        public bool ShowShortName
        {
            get => this.showShortName;
            set
            {
                this.RaiseAndSetIfChanged(ref this.showShortName, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node boxes show the name line
        /// </summary>
        public bool ShowName
        {
            get => this.showName;
            set
            {
                this.RaiseAndSetIfChanged(ref this.showName, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node boxes show the definition line
        /// </summary>
        public bool ShowDefinition
        {
            get => this.showDefinition;
            set
            {
                this.RaiseAndSetIfChanged(ref this.showDefinition, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of characters of the definition shown on the node boxes
        /// </summary>
        public int DefinitionMaxLength
        {
            get => this.definitionMaxLength;
            set
            {
                if (value == this.definitionMaxLength)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref this.definitionMaxLength, value);
                this.ComputeGraph();
            }
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> a link is being drawn from, set by "start link" and cleared once the link is
        /// created or cancelled, or null when no link is in progress
        /// </summary>
        public Thing LinkSourceThing
        {
            get => this.linkSourceThing;
            private set
            {
                this.RaiseAndSetIfChanged(ref this.linkSourceThing, value);
                this.RaisePropertyChanged(nameof(this.IsLinking));
                this.RaisePropertyChanged(nameof(this.LinkStatusMessage));
                this.UpdateLinkSourceHighlight();
            }
        }

        /// <summary>
        /// Gets a value indicating whether a link is currently being drawn
        /// </summary>
        public bool IsLinking => this.LinkSourceThing != null;

        /// <summary>
        /// Gets the banner shown while a link is being drawn, naming the source and how to proceed
        /// </summary>
        public string LinkStatusMessage => this.IsLinking
            ? $"Linking from '{this.LinkSourceThing.UserFriendlyName}'. Right-click a target node to create the link, or press Escape to cancel."
            : string.Empty;

        /// <summary>
        /// Gets the copy-pasteable details of the <see cref="SelectedDiagramThing"/>, shown on the Details tab
        /// </summary>
        public string SelectedThingDetails
        {
            get => this.selectedThingDetails;
            private set => this.RaiseAndSetIfChanged(ref this.selectedThingDetails, value);
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
        /// Gets the command that saves the current configuration as a named preset
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveConfigurationCommand { get; private set; }

        /// <summary>
        /// Gets the command that opens the manager of the saved presets
        /// </summary>
        public ReactiveCommand<Unit, Unit> ManageConfigurationsCommand { get; private set; }

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
        /// Gets the command that restarts the traversal from <see cref="SelectedNode"/>, which is how a node deeper
        /// down the tree is inspected for the other paths that lead to it
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetSelectedNodeAsRootCommand { get; private set; }

        /// <summary>
        /// Gets the command that adds <see cref="SelectedNode"/> to the roots, keeping the roots that are already
        /// there, so that a second branch is expanded next to the current one
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddSelectedNodeToRootsCommand { get; private set; }

        /// <summary>
        /// Gets the command that opens the update dialog of the <see cref="SelectedDiagramThing"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditSelectedThingCommand { get; private set; }

        /// <summary>
        /// Gets the command that opens the inspect dialog of the <see cref="SelectedDiagramThing"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedThingCommand { get; private set; }

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
        /// Gets the command that starts drawing a link from <see cref="SelectedNode"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> StartLinkCommand { get; private set; }

        /// <summary>
        /// Gets the command that abandons the link currently being drawn
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelLinkCommand { get; private set; }

        /// <summary>
        /// Gets the command that creates the <see cref="BinaryRelationship"/> described by its
        /// <see cref="LinkCreationOption"/> parameter
        /// </summary>
        public ReactiveCommand<LinkCreationOption, Unit> CreateLinkCommand { get; private set; }

        /// <summary>
        /// Gets the command that deletes the <see cref="Relationship"/> of the <see cref="SelectedEdge"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteRelationshipCommand { get; private set; }

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
                // dropping a thing that was excluded earlier is an explicit request to see it, so the exclusion is
                // lifted rather than silently swallowing the drop
                var excluded = this.ExcludedThings.FirstOrDefault(x => x.Iid == thing.Iid);

                if (excluded != null)
                {
                    this.ExcludedThings.Remove(excluded);
                }

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

            // the node and edge view-models are about to be recreated, so any diagram selection becomes stale
            this.SelectedNode = null;
            this.SelectedEdge = null;

            var roots = new List<Thing>(this.RootThings);

            if (this.SelectedRootClassKinds.Any())
            {
                roots.AddRange(this.QueryRootsByClassKindAndCategory());
            }

            this.builder = this.builder ?? new RelationshipGraphBuilder(this.Thing);
            var graph = this.builder.Build(roots, this.BuildConfiguration());

            var displayOptions = this.BuildDisplayOptions();

            this.Nodes.Clear();
            this.Nodes.AddRange(graph.Nodes.Select(x => new TraceabilityNodeViewModel(x, displayOptions)));

            var nodeLevels = graph.Nodes.ToDictionary(x => x.Thing.Iid, x => x.Level);

            this.Edges.Clear();
            this.Edges.AddRange(graph.Edges.Select(x => new TraceabilityEdgeViewModel(x, nodeLevels)));

            this.SubscribeToNodes(graph.Nodes.Select(x => x.Thing));

            this.IsMaxNodeCountReached = graph.MaxNodeCountReached;

            this.MaxNodeCountMessage = $"The maximum of {this.MaxNodes} nodes was reached, so the diagram is incomplete. Raise the maximum (allowed {MinimumMaxNodes} to {MaximumMaxNodes}), lower the depth or add filters.";

            var exclusionSuffix = this.ExcludedThings.Any() ? $", {this.ExcludedThings.Count} excluded" : string.Empty;
            this.StatusMessage = $"{graph.Nodes.Count} nodes, {graph.Edges.Count} relationships{exclusionSuffix}";

            // the nodes were just recreated, so a link in progress must be re-highlighted on the new node
            this.UpdateLinkSourceHighlight();
        }

        /// <summary>
        /// Flags the node that a link is being drawn from, so the view outlines it, and clears the flag on the others
        /// </summary>
        private void UpdateLinkSourceHighlight()
        {
            foreach (var node in this.Nodes)
            {
                node.IsLinkSource = this.linkSourceThing != null && node.Thing.Iid == this.linkSourceThing.Iid;
            }
        }

        /// <summary>
        /// Queries the <see cref="Thing"/>s of the current iteration that match <see cref="SelectedRootClassKinds"/>
        /// and <see cref="SelectedRootCategories"/>
        /// </summary>
        /// <returns>The matching <see cref="Thing"/>s</returns>
        private IEnumerable<Thing> QueryRootsByClassKindAndCategory()
        {
            // walking the containment tree of this iteration only, rather than the session cache which holds the
            // things of every open iteration and model
            var candidates = this.Thing.QueryContainedThingsDeep()
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
                RelationshipClassKinds.Default
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

            this.SaveConfigurationCommand = ReactiveCommandCreator.Create(this.ExecuteSaveConfiguration);

            this.ManageConfigurationsCommand = ReactiveCommandCreator.Create(this.ExecuteManageConfigurations);

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

            this.SetSelectedNodeAsRootCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    var thing = this.SelectedNode.Thing;

                    // the picked node becomes the only root, so the diagram is re-centred on it and the upward cone
                    // shows every other path that leads to it; suppressed so that this is a single recompute
                    this.isInitialized = false;

                    try
                    {
                        this.RootThings.Clear();
                        this.RootThings.Add(thing);
                        this.SelectedRootClassKinds = new List<ClassKind>();
                    }
                    finally
                    {
                        this.isInitialized = true;
                    }

                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedNode).Select(x => x != null));

            this.AddSelectedNodeToRootsCommand = ReactiveCommandCreator.Create(
                () =>
                {
                    var thing = this.SelectedNode.Thing;

                    if (this.RootThings.All(x => x.Iid != thing.Iid))
                    {
                        this.RootThings.Add(thing);
                    }

                    this.ComputeGraph();
                },
                this.WhenAnyValue(x => x.SelectedNode).Select(x => x != null));

            var hasSelectedDiagramThing = this.WhenAnyValue(x => x.SelectedNode, x => x.SelectedEdge)
                .Select(_ => this.SelectedDiagramThing != null);

            this.EditSelectedThingCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteUpdateCommand(this.SelectedDiagramThing),
                hasSelectedDiagramThing);

            this.InspectSelectedThingCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteInspectCommand(this.SelectedDiagramThing),
                hasSelectedDiagramThing);

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

            this.StartLinkCommand = ReactiveCommandCreator.Create(
                () => this.LinkSourceThing = this.SelectedNode.Thing,
                this.WhenAnyValue(x => x.SelectedNode).Select(x => x != null));

            this.CancelLinkCommand = ReactiveCommandCreator.Create(
                () => this.LinkSourceThing = null,
                this.WhenAnyValue(x => x.LinkSourceThing).Select(x => x != null));

            this.CreateLinkCommand = ReactiveCommandCreator.Create<LinkCreationOption>(this.ExecuteCreateLink);

            this.DeleteRelationshipCommand = ReactiveCommandCreator.Create(
                this.ExecuteDeleteRelationship,
                this.WhenAnyValue(x => x.SelectedEdge).Select(x => x != null));
        }

        /// <summary>
        /// Deletes the <see cref="Relationship"/> of the <see cref="SelectedEdge"/> after the user confirms it. The
        /// graph is recomputed by the write echo through the relationship subscription.
        /// </summary>
        private async void ExecuteDeleteRelationship()
        {
            var relationship = this.SelectedEdge?.Relationship;

            if (relationship == null)
            {
                return;
            }

            var confirmation = new ConfirmationDialogViewModel(relationship);

            if (!((this.DialogNavigationService.NavigateModal(confirmation)?.Result) ?? false))
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var iterationClone = this.Thing.Clone(false);
            var transaction = new ThingTransaction(transactionContext, iterationClone);
            transaction.Delete(relationship.Clone(false), iterationClone);

            try
            {
                await this.Session.Write(transaction.FinalizeTransaction());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Deletion of the relationship failed: {ex.Message}", "Deleting relationship failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Builds the applicable link kinds when drawing a relationship from <paramref name="source"/> to
        /// <paramref name="target"/>: the <see cref="BinaryRelationshipRule"/>s of the open reference data libraries
        /// whose source and target categories both fit the two <see cref="Thing"/>s. Only rules are offered, so a link
        /// can only be created where a rule allows it.
        /// </summary>
        /// <param name="source">The <see cref="Thing"/> the relationship would run from</param>
        /// <param name="target">The <see cref="Thing"/> the relationship would run to</param>
        /// <returns>The applicable <see cref="LinkCreationOption"/>s</returns>
        public IReadOnlyList<LinkCreationOption> GetLinkOptions(Thing source, Thing target)
        {
            if (source == null || target == null || source.Iid == target.Iid
                || !(source is ICategorizableThing categorizableSource) || !(target is ICategorizableThing categorizableTarget))
            {
                return new List<LinkCreationOption>();
            }

            return this.Session.OpenReferenceDataLibraries
                .SelectMany(x => x.Rule)
                .OfType<BinaryRelationshipRule>()
                .Where(rule => categorizableSource.IsMemberOfCategory(rule.SourceCategory) && categorizableTarget.IsMemberOfCategory(rule.TargetCategory))
                .OrderBy(rule => rule.Name)
                .Select(rule => new LinkCreationOption(rule.Name, source, target, rule.RelationshipCategory))
                .ToList();
        }

        /// <summary>
        /// Creates the <see cref="BinaryRelationship"/> described by a <see cref="LinkCreationOption"/> and writes it to
        /// the model, mirroring the way the Relationship Editor creates a relationship
        /// </summary>
        /// <param name="option">The chosen <see cref="LinkCreationOption"/></param>
        private async void ExecuteCreateLink(LinkCreationOption option)
        {
            if (option?.Source == null || option.Target == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing, out var tuple);

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = tuple?.Item1 };

            if (option.RelationshipCategory != null)
            {
                relationship.Category.Add(option.RelationshipCategory);
            }

            var iterationClone = this.Thing.Clone(false);
            iterationClone.Relationship.Add(relationship);
            relationship.Container = iterationClone;
            relationship.Source = option.Source;
            relationship.Target = option.Target;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext, iterationClone);
            transaction.CreateOrUpdate(relationship);

            try
            {
                await this.Session.Write(transaction.FinalizeTransaction());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Creation of the relationship failed: {ex.Message}", "Creating relationship failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // the write echo recomputes the graph through the relationship subscription; the link is finished
                this.LinkSourceThing = null;
            }
        }

        /// <summary>
        /// Builds the <see cref="NodeDisplayOptions"/> from the current state of the block-content configuration
        /// </summary>
        /// <returns>The <see cref="NodeDisplayOptions"/></returns>
        private NodeDisplayOptions BuildDisplayOptions()
        {
            return new NodeDisplayOptions
            {
                ShowClassKind = this.ShowClassKind,
                ShowShortName = this.ShowShortName,
                ShowName = this.ShowName,
                ShowDefinition = this.ShowDefinition,
                DefinitionMaxLength = this.DefinitionMaxLength
            };
        }

        /// <summary>
        /// Builds the copy-pasteable details of a <see cref="Thing"/> shown on the Details tab
        /// </summary>
        /// <param name="thing">The selected <see cref="Thing"/>, or null</param>
        /// <returns>The details text, or an empty string when nothing is selected</returns>
        private static string BuildDetails(Thing thing)
        {
            if (thing == null)
            {
                return string.Empty;
            }

            var lines = new List<string> { $"Kind: {thing.ClassKind}" };

            if (thing is INamedThing namedThing)
            {
                lines.Add($"Name: {namedThing.Name}");
            }

            if (thing is IShortNamedThing shortNamedThing)
            {
                lines.Add($"Short name: {shortNamedThing.ShortName}");
            }

            if (thing is IOwnedThing ownedThing && ownedThing.Owner != null)
            {
                lines.Add($"Owner: {ownedThing.Owner.ShortName}");
            }

            if (thing is ICategorizableThing categorizableThing && categorizableThing.Category.Any())
            {
                lines.Add($"Categories: {string.Join(", ", categorizableThing.Category.Select(x => x.Name))}");
            }

            if (thing is BinaryRelationship binaryRelationship)
            {
                lines.Add($"Source: {binaryRelationship.Source?.UserFriendlyName}");
                lines.Add($"Target: {binaryRelationship.Target?.UserFriendlyName}");
            }

            if (thing is MultiRelationship multiRelationship)
            {
                lines.Add($"Related: {string.Join(", ", multiRelationship.RelatedThing.Select(x => x.UserFriendlyName))}");
            }

            var definition = (thing as DefinedThing)?.Definition.FirstOrDefault()?.Content;

            if (!string.IsNullOrWhiteSpace(definition))
            {
                lines.Add($"Definition: {definition}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Replaces the subscriptions on the <see cref="Thing"/>s that are rendered as nodes, so that renaming or
        /// deprecating one of them refreshes the diagram
        /// </summary>
        /// <param name="things">The <see cref="Thing"/>s that are currently rendered</param>
        private void SubscribeToNodes(IEnumerable<Thing> things)
        {
            this.nodeSubscriptions.Clear();

            foreach (var thing in things)
            {
                this.nodeSubscriptions.Add(
                    this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(thing)
                        .Where(objectChange => objectChange.EventKind != EventKind.Added)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => this.ComputeGraph()));
            }
        }

        /// <summary>
        /// Adds the message bus subscriptions that keep the graph in sync with the model
        /// </summary>
        private void AddTraceabilitySubscriptions()
        {
            this.Disposables.Add(this.nodeSubscriptions);

            foreach (var relationshipType in new[] { typeof(BinaryRelationship), typeof(MultiRelationship) })
            {
                this.Disposables.Add(
                    this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(relationshipType)
                        // the subscription is registered by type, so a relationship of another open iteration would
                        // otherwise rebuild this panel's graph for nothing
                        .Where(objectChange => objectChange.ChangedThing.GetContainerOfType<Iteration>()?.Iid == this.Thing.Iid)
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

            // the Details tab follows the diagram selection, whether it changed from the diagram or programmatically
            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedNode, x => x.SelectedEdge)
                    .Subscribe(_ => this.SelectedThingDetails = BuildDetails(this.SelectedDiagramThing)));
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

            this.SelectedEdge = node != null
                ? null
                : diagramControl.SelectedItems
                    .OfType<DiagramConnector>()
                    .Select(x => x.Content ?? x.DataContext)
                    .OfType<TraceabilityEdgeViewModel>()
                    .FirstOrDefault();

            if (this.SelectedDiagramThing != null)
            {
                this.Session.CDPMessageBus.SendMessage(new SelectedThingChangedEvent(this.SelectedDiagramThing, this.Session));
            }
        }

        /// <summary>
        /// Reads the <see cref="GrapherPluginSettings"/>, creating the settings file with defaults on first use
        /// </summary>
        /// <returns>The <see cref="GrapherPluginSettings"/></returns>
        private GrapherPluginSettings ReadOrInitializeSettings()
        {
            try
            {
                return this.PluginSettingsService.Read<GrapherPluginSettings>();
            }
            catch (PluginSettingsException)
            {
                // first use: no settings file exists yet, create it with defaults
                var settings = new GrapherPluginSettings();
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
        /// Saves the current configuration as a preset, through the shared name and description dialog
        /// </summary>
        private void ExecuteSaveConfiguration()
        {
            var configuration = this.BuildSavedConfiguration();

            var dialogViewModel = new SavedConfigurationDialogViewModel<GrapherPluginSettings>(this.PluginSettingsService, configuration);

            if (!((this.DialogNavigationService.NavigateModal(dialogViewModel) as SavedConfigurationResult)?.Result ?? false))
            {
                return;
            }

            this.LoadSavedConfigurations();

            this.selectedConfiguration = this.SavedConfigurations.FirstOrDefault(x => x.Id == configuration.Id);
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));
        }

        /// <summary>
        /// Opens the shared manager of the saved presets, which is where a preset is deleted
        /// </summary>
        private void ExecuteManageConfigurations()
        {
            var dialogViewModel = new ManageConfigurationsDialogViewModel<GrapherPluginSettings>(this.PluginSettingsService);

            if (!((this.DialogNavigationService.NavigateModal(dialogViewModel) as ManageConfigurationsResult)?.Result ?? false))
            {
                return;
            }

            this.LoadSavedConfigurations();

            // the applied preset may have just been deleted
            this.selectedConfiguration = this.SavedConfigurations.FirstOrDefault(x => x.Id == this.selectedConfiguration?.Id);
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));
        }

        /// <summary>
        /// Builds a <see cref="TraceabilityConfiguration"/> from the current state of the panel; its name and
        /// description are filled in by the save dialog
        /// </summary>
        /// <returns>The <see cref="TraceabilityConfiguration"/></returns>
        private TraceabilityConfiguration BuildSavedConfiguration()
        {
            return new TraceabilityConfiguration
            {
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
                    .ToList(),
                ShowClassKind = this.ShowClassKind,
                ShowShortName = this.ShowShortName,
                ShowName = this.ShowName,
                ShowDefinition = this.ShowDefinition,
                DefinitionMaxLength = this.DefinitionMaxLength
            };
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
                this.SelectedRootClassKinds = configuration.RootClassKinds.ToList();
                this.SelectedRootCategories = this.ResolveCategories(configuration.RootCategories);
                this.DepthDown = configuration.DepthDown;
                this.DepthUp = configuration.DepthUp;
                this.MaxNodes = configuration.MaxNodes;
                this.LayoutDirection = configuration.LayoutDirection;
                this.SelectedDefaultLevelCategories = this.ResolveCategories(configuration.DefaultLevelCategories);
                this.SelectedDefaultDirection = ResolveDirectionOption(configuration.DefaultDirection);
                this.ShowClassKind = configuration.ShowClassKind;
                this.ShowShortName = configuration.ShowShortName;
                this.ShowName = configuration.ShowName;
                this.ShowDefinition = configuration.ShowDefinition;
                this.DefinitionMaxLength = configuration.DefinitionMaxLength;

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
