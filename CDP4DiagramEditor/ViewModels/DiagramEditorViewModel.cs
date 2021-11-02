// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEditorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;
    using CDP4CommonView.Diagram.Views;
    using CDP4CommonView.EventAggregator;

    using CDP4Composition;
    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4DiagramEditor.ViewModels.Palette;
    using CDP4DiagramEditor.ViewModels.Relation;
    using CDP4DiagramEditor.ViewModels.Tools;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    using DiagramShape = CDP4Common.DiagramData.DiagramShape;
    using Point = System.Windows.Point;

    /// <summary>
    /// The view-model for the <see cref="CDP4DiagramEditor" /> view
    /// </summary>
    public class DiagramEditorViewModel : BrowserViewModelBase<DiagramCanvas>, IPanelViewModel, IDropTarget, IDiagramEditorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanCreateDiagram" />
        /// </summary>
        private bool canCreateDiagram;

        /// <summary>
        /// Backing field for <see cref="ConnectorViewModels" />
        /// </summary>
        private DisposableReactiveList<IDiagramConnectorViewModel> connectorViewModels;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="CurrentModel" />
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="DropContextMenuIsOpen" />
        /// </summary>
        private bool dropContextMenuIsOpen;

        /// <summary>
        /// Backing field for <see cref="IsDirty" />
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Backing field for the <see cref="IsTopDiagramElementSet" />
        /// </summary>
        private bool isTopDiagramElementSet;

        /// <summary>
        /// Backing field for <see cref="PaletteViewModel" />
        /// </summary>
        private DiagramPaletteViewModel paletteViewModel;

        /// <summary>
        /// Backing field for <see cref="RelationshipRules" />
        /// </summary>
        private DisposableReactiveList<RuleNavBarRelationViewModel> relationshipRules;

        /// <summary>
        /// Backing field for <see cref="SelectedItem" />
        /// </summary>
        private DiagramItem selectedItem;

        /// <summary>
        /// Backing field for <see cref="SelectedItems" />
        /// </summary>
        private ReactiveList<DiagramItem> selectedItems;

        /// <summary>
        /// Backing field for <see cref="ThingDiagramItemViewModels" />
        /// </summary>
        private DisposableReactiveList<IThingDiagramItemViewModel> thingDiagramItemViewModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEditorViewModel" /> class
        /// </summary>
        /// <param name="diagram">The diagram of the <see cref="Thing" />s to display</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService" /> instance</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService" /> instance</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService" /> instance</param>
        public DiagramEditorViewModel(DiagramCanvas diagram, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(diagram, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = this.GetCaption();
            this.ToolTip = $"The {this.Thing.Name} Diagram Editor";

            // initialize palette
            this.PaletteViewModel = new DiagramPaletteViewModel(diagram, this);

            this.DropContextMenuItems = new ReactiveList<ContextMenuItemViewModel>();

            // initialize listeners
            this.InitializeListeners();
        }

        /// <summary>
        /// Gets the drop context Menu for the diagram
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> DropContextMenuItems { get; }

        /// <summary>
        /// Gets the <see cref="DiagramPaletteViewModel" />
        /// </summary>
        public DiagramPaletteViewModel PaletteViewModel
        {
            get { return this.paletteViewModel; }
            private set { this.RaiseAndSetIfChanged(ref this.paletteViewModel, value); }
        }

        /// <summary>
        /// Gets or sets the current Model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// The <see cref="IEventPublisher" /> that allows view/view-model communication
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="EngineeringModelSetup" />
        /// </summary>
        public bool CanCreateDiagram
        {
            get { return this.canCreateDiagram; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateDiagram, value); }
        }

        /// <summary>
        /// Gets the save command
        /// </summary>
        public ReactiveCommand<Unit> SaveDiagramCommand { get; private set; }

        /// <summary>
        /// Gets or sets the delete from model Command
        /// </summary>
        public ReactiveCommand<object> DeleteFromModelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the delete from diagram Command
        /// </summary>
        public ReactiveCommand<object> DeleteFromDiagramCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the add usages to diagram command
        /// </summary>
        public ReactiveCommand<object> AddUsagesToDiagramCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the RelationshipRules
        /// </summary>
        public DisposableReactiveList<RuleNavBarRelationViewModel> RelationshipRules
        {
            get { return this.relationshipRules; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipRules, value); }
        }

        /// <summary>
        /// Gets or sets the set as top element Command
        /// </summary>
        public ReactiveCommand<Unit> SetAsTopElementCommand { get; private set; }

        /// <summary>
        /// Gets or sets the unset top element Command
        /// </summary>
        public ReactiveCommand<Unit> UnsetTopElementCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the diagram has its top element set
        /// </summary>
        public bool IsTopDiagramElementSet
        {
            get { return this.isTopDiagramElementSet; }
            set { this.RaiseAndSetIfChanged(ref this.isTopDiagramElementSet, value); }
        }

        /// <summary>
        /// Gets or sets the context menu for optional selections when an item is dropped on to the diagram
        /// </summary>
        public bool DropContextMenuIsOpen
        {
            get { return this.dropContextMenuIsOpen; }
            set { this.RaiseAndSetIfChanged(ref this.dropContextMenuIsOpen, value); }
        }

        /// <summary>
        /// Gets or sets the collection of diagram items.
        /// </summary>
        public DisposableReactiveList<IThingDiagramItemViewModel> ThingDiagramItemViewModels
        {
            get { return this.thingDiagramItemViewModels; }
            set { this.RaiseAndSetIfChanged(ref this.thingDiagramItemViewModels, value); }
        }

        /// <summary>
        /// Gets or sets the collection of diagram connectors.
        /// </summary>
        public DisposableReactiveList<IDiagramConnectorViewModel> ConnectorViewModels
        {
            get { return this.connectorViewModels; }
            set { this.RaiseAndSetIfChanged(ref this.connectorViewModels, value); }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="DiagramItem" /> items that are selected.
        /// </summary>
        public ReactiveList<DiagramItem> SelectedItems
        {
            get { return this.selectedItems; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItems, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="DiagramItem" /> item that is selected.
        /// </summary>
        public DiagramItem SelectedItem
        {
            get { return this.selectedItem; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItem, value); }
        }

        /// <summary>
        /// gets or sets the behavior.
        /// </summary>
        public ICdp4DiagramBehavior Behavior { get; set; }

        /// <summary>
        /// Update this view-model
        /// </summary>
        public void UpdateProperties()
        {
            this.ComputeDiagramObject();
            this.ComputeGeneratedConnectors();
            this.IsDirty = false;
        }

        /// <summary>
        /// Compute the <see cref="DiagramEdge" /> to show
        /// </summary>
        public void ComputeDiagramConnector()
        {
            var updatedItems = this.Thing.DiagramElement.OfType<DiagramEdge>().ToList();
            var currentItems = this.ConnectorViewModels.OfType<IPersistedConnector>().Select(x => x.DiagramThing).ToList();

            var newItems = updatedItems.Except(currentItems);
            var oldItems = currentItems.Except(updatedItems);

            foreach (var diagramThing in oldItems)
            {
                var item = this.ConnectorViewModels.OfType<IPersistedConnector>().SingleOrDefault(x => x.DiagramThing == diagramThing);

                if (item != null)
                {
                    this.ConnectorViewModels.RemoveAndDispose(item);
                }
            }

            foreach (var diagramThing in newItems)
            {
                DrawnDiagramEdgeViewModel newDrawnDiagramElement = null;

                switch (diagramThing.DepictedThing)
                {
                    case ElementUsage:
                        newDrawnDiagramElement = new ElementUsageEdgeViewModel((DiagramEdge) diagramThing, this.Session, this);
                        break;
                    case BinaryRelationship:
                        newDrawnDiagramElement = new BinaryRelationshipEdgeViewModel((DiagramEdge)diagramThing, this.Session, this);
                        break;
                    case null:
                        if (diagramThing is DiagramEdge edge)
                        {
                            newDrawnDiagramElement = new SimpleEdgeViewModel(edge, this.Session, this);
                        }

                        break;
                }

                this.ConnectorViewModels.Add(newDrawnDiagramElement);
            }

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Occurs when a <see cref="ThingDiagramContentItemViewModel" /> gets removed
        /// </summary>
        /// <param name="contentItemContent">The removed object</param>
        public void RemoveDiagramThingItem(object contentItemContent)
        {
            switch (contentItemContent)
            {
                case DiagramConnector connector:
                    this.Behavior.RemoveConnector(connector);
                    break;
                case DrawnDiagramEdgeViewModel connectorViewModel:
                    this.ConnectorViewModels.RemoveAndDispose(connectorViewModel);
                    break;
                case ThingDiagramContentItemViewModel item:
                {
                    // cleanup ports
                    if (item is PortContainerDiagramContentItemViewModel portContainer)
                    {
                        foreach (var diagramPortViewModel in portContainer.PortCollection)
                        {
                            this.ThingDiagramItemViewModels.RemoveAndDispose(diagramPortViewModel);
                        }
                    }

                    this.ThingDiagramItemViewModels.RemoveAndDispose(item);
                    this.Behavior.ItemPositions.Remove(item);

                    var connectors = this.ConnectorViewModels.Where(x => ((DiagramEdge) x.DiagramThing).Source.DepictedThing == item.DiagramThing.DepictedThing || ((DiagramEdge) x.DiagramThing).Target.DepictedThing == item.DiagramThing.DepictedThing).ToArray();

                    foreach (var diagramEdgeViewModel in connectors)
                    {
                        this.ConnectorViewModels.RemoveAndDispose(diagramEdgeViewModel);
                    }

                    break;
                }
            }

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Removes a diagram item and its connectors by <see cref="Thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> by which to find and remove diagram things.</param>
        public void RemoveDiagramThingItemByThing(Thing thing)
        {
            var diagramItems = this.ThingDiagramItemViewModels.Where(di => di.Thing != null && di.Thing.Equals(thing)).ToList();

            foreach (var thingDiagramContentItem in diagramItems)
            {
                this.RemoveDiagramThingItem(thingDiagramContentItem);
            }

            var connectors = this.ConnectorViewModels.Where(c => c.Thing.Equals(thing)).ToList();

            foreach (var connector in connectors)
            {
                this.RemoveDiagramThingItem(connector);
            }
        }

        /// <summary>
        /// Initiate the create command of a certain Thing represented by TThing
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="container">The contaier of the object to be created</param>
        /// <typeparam name="TThing">The type of Thing to be creates</typeparam>
        public TThing Create<TThing>(object sender, Thing container = null) where TThing : Thing, new()
        {
            var thing = new TThing();

            this.ExecuteCreateCommand(thing, container ?? this.Thing.Container);

            if (this.Thing.Cache.TryGetValue(thing.CacheKey, out var returnedThing))
            {
                return (TThing) returnedThing.Value;
            }

            return null;
        }

        /// <summary>
        /// Update this <see cref="IsDirty" /> property
        /// </summary>
        public void UpdateIsDirty()
        {
            var currentObjects = this.Thing.DiagramElement.OfType<DiagramShape>().ToArray();
            var namedThingDiagramContentItemViewModels = this.ThingDiagramItemViewModels.OfType<NamedThingDiagramContentItemViewModel>().ToList();
            var displayedObjects = namedThingDiagramContentItemViewModels.Select(x => x.DiagramThing).ToArray();

            var removedItem = currentObjects.Except(displayedObjects).Count();
            var addedItem = displayedObjects.Except(currentObjects).Count();

            var currentEdges = this.Thing.DiagramElement.OfType<DiagramEdge>().ToArray();
            var persistedConnectors = this.ConnectorViewModels.OfType<IPersistedConnector>().ToList();

            var displayedEdges = persistedConnectors.Select(x => x.DiagramThing).ToArray();

            var removedEdges = currentEdges.Except(displayedEdges).Count();
            var addedEdges = displayedEdges.Except(currentEdges).Count();

            this.IsDirty = namedThingDiagramContentItemViewModels.Any(x => x.IsDirty) || removedItem > 0;
            this.IsDirty |= addedItem > 0;
            this.IsDirty |= addedEdges > 0;
            this.IsDirty |= persistedConnectors.Any(x => x.IsDirty) || removedEdges > 0;
        }

        /// <summary>
        /// Shows a context menu in the diagram at the current mouse position with the specified options
        /// </summary>
        /// <param name="contextMenuItems">The menu options to display</param>
        public void ShowDropContextMenuOptions(IEnumerable<ContextMenuItemViewModel> contextMenuItems)
        {
            this.DropContextMenuItems.Clear();
            this.DropContextMenuItems.AddRange(contextMenuItems);
            this.DropContextMenuIsOpen = true;
        }

        /// <summary>
        /// Adds a port to the items collection
        /// </summary>
        /// <param name="port">The port view model</param>
        public void AddPortToItems(IDiagramPortViewModel port)
        {
            this.ThingDiagramItemViewModels.Add(port);
        }

        /// <summary>
        /// Activate a connector tool.
        /// </summary>
        /// <typeparam name="TTool">The type of tool</typeparam>
        /// <param name="sender">The sender object.</param>
        /// <returns>An empty task</returns>
        public async Task ActivateConnectorTool<TTool>(object sender) where TTool : DiagramTool, IConnectorTool, new()
        {
            this.Behavior.ActivateConnectorTool<TTool>();
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects" /> property on
        /// <paramref name="dropInfo" /> should be set to a value other than <see cref="DragDropEffects.None" />
        /// and <see cref="DropInfo.Payload" /> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            switch (dropInfo.Payload)
            {
                case Thing rowPayload when !this.ThingDiagramItemViewModels.OfType<NamedThingDiagramContentItemViewModel>().Select(x => x.Thing).Contains(rowPayload):
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                case Tuple<ParameterType, MeasurementScale> tuplePayload:
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                default:
                    dropInfo.Effects = DragDropEffects.None;
                    break;
            }
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var convertedDropPosition = this.Behavior.GetDiagramPositionFromMousePosition(dropInfo.DropPosition);

            // handle existing thing creation
            if (dropInfo.Payload is Thing rowPayload)
            {
                this.CreateThingShape(dropInfo, rowPayload, convertedDropPosition);
                return;
            }

            // handle things being dropped from palette
            if (dropInfo.Payload is DataObject dataObject)
            {
                var formats = dataObject.GetFormats(true);

                if (formats != null && formats.Any())
                {
                    if (dataObject.GetData(formats.First()) is IPaletteDroppableItemViewModel palettePayload)
                    {
                        await palettePayload.HandleMouseDrop(dropInfo, t => this.CreateThingShape(dropInfo, t, convertedDropPosition));
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        /// <remarks>
        /// This is set by the children view-models
        /// </remarks>
        public override bool IsDirty
        {
            get { return this.isDirty; }
            set { this.RaiseAndSetIfChanged(ref this.isDirty, value); }
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Initializes listeners for object change events related to creation/removal of generated elements
        /// </summary>
        private void InitializeListeners()
        {
            this.Disposables.Add(CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(sessionEvent => sessionEvent.Status == SessionStatus.Closed && sessionEvent.Session.Equals(this.Session))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ClosePanel()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Container)
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ClosePanel()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                .Where(objectChange => objectChange.EventKind != EventKind.Removed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ComputeGeneratedConnectors()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveGeneratedConnector));
        }

        /// <summary>
        /// Close this panel on session close
        /// </summary>
        private void ClosePanel()
        {
            this.PanelNavigationService.CloseInDock(this);
        }

        /// <summary>
        /// Execute the <see cref="UpdateCommand" /> on the <see cref="SelectedThing" />
        /// </summary>
        protected override void ExecuteUpdateCommand()
        {
            if (this.SelectedItem == null)
            {
                return;
            }

            var thing = this.GetThingFromSelectedItem();

            if (thing == null)
            {
                return;
            }

            this.ExecuteUpdateCommand(thing);
        }

        /// <summary>
        /// Gets the <see cref="Thing" /> from <see cref="SelectedItem" />
        /// </summary>
        /// <returns>The <see cref="Thing" /> or null</returns>
        private Thing GetThingFromSelectedItem()
        {
            Thing thing = null;

            switch (this.SelectedItem)
            {
                case DiagramPortShape ps:
                    thing = (ps.DataContext as IDiagramPortViewModel)?.Thing;
                    break;
                case DiagramConnector connector:
                    thing = connector.GetViewModel()?.Thing;
                    break;
                case DiagramContentItem ci:
                    thing = (ci.Content as ThingDiagramContentItemViewModel)?.Thing;
                    break;
            }

            return thing;
        }

        /// <summary>
        /// Execute the <see cref="InspectCommand" />
        /// </summary>
        protected override void ExecuteInspectCommand()
        {
            if (this.SelectedItem == null)
            {
                return;
            }

            var thing = this.GetThingFromSelectedItem();

            if (thing == null)
            {
                return;
            }

            this.ExecuteInspectCommand(thing);
        }

        /// <summary>
        /// Creates the shape for the shape
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo" /> containing drag drop object</param>
        /// <param name="rowPayload">The <see cref="Thing" /> ti draw the shape for.</param>
        /// <param name="convertedDropPosition">The drop position.</param>
        private void CreateThingShape(IDropInfo dropInfo, Thing rowPayload, Point convertedDropPosition)
        {
            if (this.ThingDiagramItemViewModels.OfType<NamedThingDiagramContentItemViewModel>().Select(x => x.Thing).Contains(rowPayload))
            {
                return;
            }

            var position = new Point(convertedDropPosition.X, convertedDropPosition.Y);

            var bounds = new Bounds(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
            {
                X = (float) position.X,
                Y = (float) position.Y,
                Name = rowPayload.UserFriendlyName
            };

            var block = new DiagramObject(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
            {
                DepictedThing = rowPayload,
                Name = rowPayload.UserFriendlyName,
                Documentation = rowPayload.UserFriendlyName,
                Resolution = Cdp4DiagramHelper.DefaultResolution
            };

            block.Bounds.Add(bounds);

            NamedThingDiagramContentItemViewModel diagramItemViewModel = null;

            switch (rowPayload)
            {
                case ElementDefinition elementDefinition:
                {
                    var architectureBlock = new ArchitectureElement(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
                    {
                        DepictedThing = rowPayload,
                        Name = rowPayload.UserFriendlyName,
                        Documentation = rowPayload.UserFriendlyName,
                        Resolution = Cdp4DiagramHelper.DefaultResolution
                    };

                    architectureBlock.Bounds.Add(bounds);

                    PortContainerDiagramContentItemViewModel portContainer = new ElementDefinitionDiagramContentItemViewModel(architectureBlock, this.Session, this);

                    diagramItemViewModel = portContainer;
                    break;
                }

                case Requirement requirement:
                {
                    diagramItemViewModel = new RequirementDiagramContentItemViewModel(block, this.Session, this);
                    break;
                }

                case DiagramFrame frame:
                {
                    bounds.Width = 200;
                    bounds.Height = 150;

                    frame.Name = "Untitled";

                    frame.Bounds.Add(bounds);
                    diagramItemViewModel = new DiagramFrameViewModel(frame, this.Session, this);
                    break;
                }
                default:
                    if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuplePayload)
                    {
                        block.DepictedThing = tuplePayload.Item1;
                        diagramItemViewModel = new NamedThingDiagramContentItemViewModel(block, this.Session, this);
                    }
                    else
                    {
                        diagramItemViewModel = new NamedThingDiagramContentItemViewModel(block, this.Session, this);
                    }

                    break;
            }

            this.Behavior.ItemPositions.Add(diagramItemViewModel, convertedDropPosition);
            this.ThingDiagramItemViewModels.Add(diagramItemViewModel);

            (diagramItemViewModel as PortContainerDiagramContentItemViewModel)?.UpdatePorts();
            this.ComputeGeneratedConnectors();
            this.UpdateIsDirty();
        }

        /// <summary>
        /// Initialize the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.WhenAnyValue(x => x.IsDirty)
                .Subscribe(x => this.Caption = this.GetCaption());

            this.EventPublisher = new EventPublisher();

            var deleteObservable = this.EventPublisher.GetEvent<DiagramDeleteEvent>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(this.OnDiagramDeleteEvent);

            this.Disposables.Add(deleteObservable);
            this.RelationshipRules = new DisposableReactiveList<RuleNavBarRelationViewModel> { ChangeTrackingEnabled = true };
            this.ThingDiagramItemViewModels = new DisposableReactiveList<IThingDiagramItemViewModel> { ChangeTrackingEnabled = true };
            this.ConnectorViewModels = new DisposableReactiveList<IDiagramConnectorViewModel> { ChangeTrackingEnabled = true };
            this.SelectedItems = new ReactiveList<DiagramItem> { ChangeTrackingEnabled = true };
        }

        /// <summary>
        /// Initializes the <see cref="ICommand" />
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecute = this.WhenAnyValue(x => x.CanCreateDiagram, x => x.IsDirty, (x, y) => x && y);

            this.SaveDiagramCommand = ReactiveCommand.CreateAsyncTask(canExecute, x => this.ExecuteSaveDiagramCommand(), RxApp.MainThreadScheduler);
            this.SaveDiagramCommand.ThrownExceptions.Subscribe(x => logger.Error(x.Message));

            this.UpdateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(i => i != null && this.PermissionService.CanWrite(this.GetThingFromSelectedItem())));
            this.UpdateCommand.Subscribe(_ => this.ExecuteUpdateCommand());

            this.InspectCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(x => x != null && this.GetThingFromSelectedItem() != null));
            this.InspectCommand.Subscribe(_ => this.ExecuteInspectCommand());

            this.DeleteFromDiagramCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(this.CanDeleteFromDiagram));
            this.DeleteFromDiagramCommand.Subscribe(x => this.ExecuteDeleteFromDiagramCommand());

            this.DeleteFromModelCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(this.CanDeleteFromModel));
            this.DeleteFromModelCommand.Subscribe(x => this.ExecuteDeleteFromModelCommand());

            this.AddUsagesToDiagramCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(s => s != null && this.SelectedItems.OfType<DiagramContentItem>().Any(i => i.Content is ElementDefinitionDiagramContentItemViewModel)));
            this.AddUsagesToDiagramCommand.Subscribe(x => this.ExecuteAddUsagesToDiagramCommand());

            this.SetAsTopElementCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedItem)
                    .Select(s => s is DiagramContentItem { Content: ElementDefinitionDiagramContentItemViewModel }),
                _ => this.ExecuteSetTopElementCommand(), RxApp.MainThreadScheduler);

            this.SetAsTopElementCommand.ThrownExceptions.Subscribe(x => logger.Error(x.Message));

            this.UnsetTopElementCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.IsTopDiagramElementSet, x => x.CanCreateDiagram, (x, y) => x && y),
                _ => this.ExecuteUnsetTopElementCommand(), RxApp.MainThreadScheduler);

            this.UnsetTopElementCommand.ThrownExceptions.Subscribe(x => logger.Error(x.Message));
        }

        /// <summary>
        /// Populates the context-menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Save the Diagram", "", this.SaveDiagramCommand, MenuItemKind.Save, ClassKind.DiagramCanvas));

            this.ContextMenu.Add(new ContextMenuItemViewModel("Edit", "", this.UpdateCommand, MenuItemKind.Edit));

            this.ContextMenu.Add(new ContextMenuItemViewModel("Inspect", "", this.InspectCommand, MenuItemKind.Inspect));

            this.ContextMenu.Add(new ContextMenuItemViewModel("Expand Element Usages", "", this.AddUsagesToDiagramCommand, MenuItemKind.Navigate));

            if (this.Thing is ArchitectureDiagram)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Set as Top Element for This Diagram", "", this.SetAsTopElementCommand, MenuItemKind.Edit));

                this.ContextMenu.Add(new ContextMenuItemViewModel("Unset Top Element for This Diagram", "", this.UnsetTopElementCommand, MenuItemKind.Delete));
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Delete From Diagram", "Del", this.DeleteFromDiagramCommand, MenuItemKind.Deprecate));

            this.ContextMenu.Add(new ContextMenuItemViewModel("Delete From Model", "", this.DeleteFromModelCommand, MenuItemKind.Delete));
        }

        /// <summary>
        /// Compute and draw the auto-generated connectors
        /// </summary>
        private void ComputeGeneratedConnectors()
        {
            var existingConnectors = this.ConnectorViewModels.OfType<IGeneratedConnector>().ToList();

            // interfaces
            var binaryRelationships = (this.Thing.Container as Iteration)?.Relationship?.OfType<BinaryRelationship>().ToList();

            if (binaryRelationships is not null)
            {
                // interfaces are bunary relationships between two EUs with interface end kind set to not NONE
                var interfaces = binaryRelationships.Where(br => InterfaceConnectorTool.IsThingAnInterfaceEnd(br.Source) && InterfaceConnectorTool.IsThingAnInterfaceEnd(br.Target));

                foreach (var iface in interfaces)
                {
                    if (existingConnectors.Any(c => c.Thing.Equals(iface)))
                    {
                        continue;
                    }

                    // only draw if both source and target are on the canvas
                    var existingPorts = this.ThingDiagramItemViewModels.OfType<IDiagramPortViewModel>().ToList();
                    var source = existingPorts.FirstOrDefault(p => p.Thing.Equals(iface.Source));
                    var target = existingPorts.FirstOrDefault(p => p.Thing.Equals(iface.Target));

                    if (source == null || target == null)
                    {
                        continue;
                    }

                    InterfaceConnectorTool.CreateConnector(iface, (DiagramPort) source.DiagramThing, (DiagramPort) target.DiagramThing, this.Behavior);
                }
            }
        }

        /// <summary>
        /// Removes a generated connector based on a <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The removing <see cref="ObjectChangedEvent" /></param>
        private void RemoveGeneratedConnector(ObjectChangedEvent objectChangedEvent)
        {
            var existingConnector = this.ConnectorViewModels.OfType<IGeneratedConnector>().FirstOrDefault(c => c.Thing.Equals(objectChangedEvent.ChangedThing));

            if (existingConnector != null)
            {
                this.ConnectorViewModels.RemoveAndDispose(existingConnector);
            }
        }

        /// <summary>
        /// Adds ElementUsages to the selected element definitions
        /// </summary>
        private void ExecuteAddUsagesToDiagramCommand()
        {
            var edViewModels = this.SelectedItems.OfType<DiagramContentItem>().Select(i => i.Content as ElementDefinitionDiagramContentItemViewModel);

            foreach (var diagramContentItem in edViewModels.Where(vm => vm != null))
            {
                var elementDefinition = diagramContentItem.Thing as ElementDefinition;

                if (elementDefinition == null)
                {
                    continue;
                }

                var elementUsages = elementDefinition.ContainedElement.Where(e => e.InterfaceEnd == InterfaceEndKind.NONE).ToList();
                var underlyingDefinitions = elementUsages.Select(eu => eu.ElementDefinition);

                // create missing ED boxes. important to be distinct
                var definitionsToCreate = underlyingDefinitions.Except(this.ThingDiagramItemViewModels.Where(v => v.Thing is ElementDefinition).Select(d => d.Thing as ElementDefinition)).Distinct().ToList();

                // compute positional data
                var count = definitionsToCreate.Count;
                var width = diagramContentItem.GetDiagramContentItemWidth();
                var height = diagramContentItem.GetDiagramContentItemHeight();
                var horizontalGap = 0.5 * width;
                var verticalGap = 1 * height;

                var startPosition = diagramContentItem.DiagramRepresentation.Position;
                var totalWidth = count * width + (count - 1) * horizontalGap;

                var position = new Point(startPosition.X + 0.5 * width - 0.5 * totalWidth, startPosition.Y + height + verticalGap);

                foreach (var elementDefinitionToCreate in definitionsToCreate)
                {
                    var newDiagramElement = ElementDefinitionDiagramContentItemViewModel.CreatElementDefinitionDiagramContentItemViewModel(this.Session, elementDefinitionToCreate, this, position);

                    this.Behavior.ItemPositions.Add(newDiagramElement, position);
                    this.ThingDiagramItemViewModels.Add(newDiagramElement);

                    newDiagramElement.UpdatePorts();

                    position = new Point(position.X + width + horizontalGap, position.Y);
                }

                // create EU connectors
                foreach (var elementUsage in elementUsages)
                {
                    if (this.ConnectorViewModels.Any(c => c.Thing.Equals(elementUsage)))
                    {
                        continue;
                    }

                    var source = this.ThingDiagramItemViewModels.Where(v => v.Thing is ElementDefinition).First(vm => vm.Thing.Equals(elementUsage.ElementDefinition)).DiagramThing as ArchitectureElement;
                    var target = diagramContentItem.DiagramThing as ArchitectureElement;

                    ElementUsageConnectorTool.CreateConnector(elementUsage, source, target, this.Behavior);
                }
            }

            this.ComputeGeneratedConnectors();
        }

        /// <summary>
        /// Check whether the selection can be deleted from diagram
        /// </summary>
        /// <param name="selectedDiagramItem">The selected item</param>
        /// <returns>True if delete is possible</returns>
        private bool CanDeleteFromDiagram(DiagramItem selectedDiagramItem)
        {
            // ports cannot be deleted from diagram
            if (this.SelectedItems.All(s => s is DiagramPortShape))
            {
                return false;
            }

            if (this.SelectedItem is DiagramConnector connector)
            {
                // generated connectors cannot be removed from diagram
                if ((connector.DataContext as Connection)?.DataItem is IGeneratedConnector)
                {
                    return false;
                }
            }

            return selectedDiagramItem != null && this.SelectedItems.Any();
        }

        /// <summary>
        /// Check whether the selection can be deleted from model
        /// </summary>
        /// <param name="selectedDiagramItem">The selected item</param>
        /// <returns>True if delete is possible</returns>
        private bool CanDeleteFromModel(DiagramItem selectedDiagramItem)
        {
            if (this.SelectedItem is DiagramConnector connector)
            {
                // conenctors without a Thing cannot be deleted
                if ((connector.DataContext as Connection)?.DataItem is IDiagramConnectorViewModel connectorViewModel)
                {
                    return connectorViewModel.Thing != null ;
                }
            }

            if (this.SelectedItem is DiagramFrameShape)
            {
                return false;
            }

            return selectedDiagramItem != null && this.SelectedItems.Any();
        }

        /// <summary>
        /// Executes the unset top element command
        /// </summary>
        private async Task ExecuteUnsetTopElementCommand()
        {
            if (this.Thing is ArchitectureDiagram architectureDiagram && architectureDiagram.TopArchitectureElement != null)
            {
                await this.ExecuteSaveDiagramCommand(null, true);
            }
        }

        /// <summary>
        /// Executes the set top element command.
        /// </summary>
        private async Task ExecuteSetTopElementCommand()
        {
            // need to save the diagram twice as you cannot currently guarntee that the object being set as top is persisted or not
            if (this.IsDirty)
            {
                // capture the Iid of the selectedItem so we can find it again
                var diagramContentItem = this.SelectedItem as DiagramContentItem;

                Guid? selectedIid = null;

                if (diagramContentItem?.Content is ElementDefinitionDiagramContentItemViewModel elementDefinitionContentItem)
                {
                    selectedIid = (elementDefinitionContentItem.Thing as ElementDefinition)?.Iid;
                }

                await this.ExecuteSaveDiagramCommand();
                this.UpdateProperties();

                // find and select the right ED
                if (selectedIid != null)
                {
                    this.Behavior.SelectItemByThingIid(selectedIid);
                }
            }

            if (this.Thing is ArchitectureDiagram architectureDiagram)
            {
                var diagramContentItem = this.SelectedItem as DiagramContentItem;

                var elementDefinitionDiagramContentItem = diagramContentItem?.Content as ElementDefinitionDiagramContentItemViewModel;

                if (!(elementDefinitionDiagramContentItem?.DiagramThing is ArchitectureElement architectureElement))
                {
                    return;
                }

                await this.ExecuteSaveDiagramCommand(architectureElement);
            }
        }

        /// <summary>
        /// Executes the remove from diagram command
        /// </summary>
        private void ExecuteDeleteFromDiagramCommand()
        {
            var selectedDiagramObjects = this.SelectedItems.OfType<DiagramContentItem>().ToList();
            var selectedConnectors = this.SelectedItems.OfType<DiagramConnector>().ToList();

            foreach (var selectedDiagramObject in selectedDiagramObjects)
            {
                if (selectedDiagramObject is DiagramPortShape)
                {
                    // ignore port shapes
                    continue;
                }

                if (selectedDiagramObject is DiagramFrameShape)
                {
                    this.RemoveDiagramThingItem(selectedDiagramObject.DataContext);
                    continue;
                }

                this.RemoveDiagramThingItem(selectedDiagramObject.Content);
            }

            foreach (var selectedConnector in selectedConnectors)
            {
                if (selectedConnector.DataContext is IGeneratedConnector)
                {
                    continue;
                }

                this.RemoveDiagramThingItem(selectedConnector.GetViewModel());
            }
        }

        /// <summary>
        /// Executes the remove from model command
        /// </summary>
        private async void ExecuteDeleteFromModelCommand()
        {
            var selectedPorts = this.SelectedItems.OfType<DiagramPortShape>().ToList();

            foreach (var portViewModel in selectedPorts.Select(x => x.DataContext as PortDiagramContentItemViewModel))
            {
                if (portViewModel?.Thing != null)
                {
                    this.ExecuteDeleteCommand(portViewModel.Thing);
                }
            }

            var selectedDiagramObjects = this.SelectedItems.OfType<DiagramContentItem>().ToList();

            foreach (var selectedThing in selectedDiagramObjects.Select(s => s.Content))
            {
                if (selectedThing is ThingDiagramContentItemViewModel thingContentItem)
                {
                    if (thingContentItem.Thing != null)
                    {
                        // if the thing is deprecatable, deprecate it instead and remove the object from diagram
                        if (thingContentItem.Thing is IDeprecatableThing deprecatableThing)
                        {
                            this.DeprecateThing(deprecatableThing);
                            this.RemoveDiagramThingItem(selectedThing);
                        }
                        else
                        {
                            // if not execute delete and response will remove the diagram object by itself
                            this.ExecuteDeleteCommand(thingContentItem.Thing);
                        }
                    }
                    else
                    {
                        // no Thing connected, just remove the item
                        this.RemoveDiagramThingItem(selectedThing);
                    }
                }
            }

            var selectedConnectors = this.SelectedItems.OfType<DiagramConnector>().ToList();

            foreach (var selectedConnector in selectedConnectors.Select(s => s.GetViewModel()))
            {
                if (selectedConnector is ThingDiagramConnectorViewModel connectorViewModel)
                {
                    if (connectorViewModel.Thing != null)
                    {
                        // if the thing is deprecatable, deprecate it instead and remove the object from diagram
                        if (connectorViewModel.Thing is IDeprecatableThing deprecatableThing)
                        {
                            this.DeprecateThing(deprecatableThing);
                            this.RemoveDiagramThingItem(selectedConnector);
                        }
                        else
                        {
                            // if not execute delete and response will remove the diagram object by itself
                            this.ExecuteDeleteCommand(connectorViewModel.Thing);
                        }
                    }
                    else
                    {
                        // no Thing connected, just remove the item
                        this.RemoveDiagramThingItem(selectedConnector);
                    }
                }
            }
        }

        /// <summary>
        /// Deprecates the provided <see cref="Thing" />
        /// </summary>
        private async void DeprecateThing(IDeprecatableThing thing)
        {
            var clone = ((Thing) thing).Clone(false);

            var isDeprecatedPropertyInfo = clone.GetType().GetProperty("IsDeprecated");

            if (isDeprecatedPropertyInfo == null)
            {
                return;
            }

            var oldValue = thing.IsDeprecated;
            isDeprecatedPropertyInfo.SetValue(clone, !oldValue);

            var context = TransactionContextResolver.ResolveContext(this.Thing);

            var transaction = new ThingTransaction(context);
            transaction.CreateOrUpdate(clone);

            try
            {
                await this.Session.Write(transaction.FinalizeTransaction());
            }
            catch (Exception ex)
            {
                logger.Error("An error was produced when (un)deprecating {0}: {1}", ((Thing) thing).ClassKind, ex.Message);
                this.Feedback = ex.Message;
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateDiagram = this.PermissionService.CanWrite(ClassKind.DiagramCanvas, this.Thing);
        }

        /// <summary>
        /// Gets the caption for this editor.
        /// </summary>
        /// <returns>The string caption.</returns>
        private string GetCaption()
        {
            return $"{(this.IsDirty ? "*" : string.Empty)}{this.Thing.Name} <<{this.Thing.GetType().Name}>>";
        }

        /// <summary>
        /// Compute the <see cref="DiagramObject" /> to display
        /// </summary>
        private void ComputeDiagramObject()
        {
            var updatedItems = this.Thing.DiagramElement.OfType<DiagramShape>().ToList();
            var currentItems = this.ThingDiagramItemViewModels.Select(x => x.DiagramThing).OfType<DiagramObject>().ToList();

            var newItems = updatedItems.Except(currentItems);
            var oldItems = currentItems.Except(updatedItems);

            foreach (var diagramThing in oldItems)
            {
                var item = this.ThingDiagramItemViewModels.SingleOrDefault(x => x.DiagramThing == diagramThing);

                if (item != null)
                {
                    this.ThingDiagramItemViewModels.RemoveAndDispose(item);
                    this.Behavior.ItemPositions.Remove(item);
                }
            }

            foreach (var diagramThing in newItems)
            {
                NamedThingDiagramContentItemViewModel newDiagramElement = null;

                switch (diagramThing.DepictedThing)
                {
                    case ElementDefinition:
                        newDiagramElement = new ElementDefinitionDiagramContentItemViewModel((ArchitectureElement) diagramThing, this.Session, this);
                        break;
                    case Requirement:
                        newDiagramElement = new RequirementDiagramContentItemViewModel((DiagramObject)diagramThing, this.Session, this);
                        break;
                    case null:
                        if (diagramThing is DiagramFrame diagramFrame)
                        {
                            newDiagramElement = new DiagramFrameViewModel(diagramFrame, this.Session, this);
                        }

                        break;
                    default:
                        newDiagramElement = new NamedThingDiagramContentItemViewModel(diagramThing, this.Session, this);
                        break;
                }

                var bound = diagramThing.Bounds.Single();

                var position = new Point { X = bound.X, Y = bound.Y };

                this.Behavior.ItemPositions.Add(newDiagramElement, position);
                this.ThingDiagramItemViewModels.Add(newDiagramElement);

                (newDiagramElement as PortContainerDiagramContentItemViewModel)?.UpdatePorts();
            }

            this.ComputeDiagramTopElement();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="ViewModelBase{T}.Thing" /> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);

            this.ComputeDiagramTopElement();
        }

        /// <summary>
        /// Computes the diagram top element if <see cref="ArchitectureDiagram" />
        /// </summary>
        private void ComputeDiagramTopElement()
        {
            if (this.Thing is ArchitectureDiagram architectureDiagram)
            {
                // update top element selection
                var elementDefinitionDiagramContentItems = this.ThingDiagramItemViewModels.OfType<ElementDefinitionDiagramContentItemViewModel>().ToList();

                // reset all to false
                foreach (var elementDefinitionDiagramContentItem in elementDefinitionDiagramContentItems)
                {
                    elementDefinitionDiagramContentItem.IsTopDiagramElement = false;
                }

                this.IsTopDiagramElementSet = false;

                if (architectureDiagram.TopArchitectureElement != null)
                {
                    // if top element set, find and mark it
                    var topElementDiagramItem = elementDefinitionDiagramContentItems.FirstOrDefault(e => e.DiagramThing.Equals(architectureDiagram.TopArchitectureElement));

                    if (topElementDiagramItem != null)
                    {
                        topElementDiagramItem.IsTopDiagramElement = true;
                        this.IsTopDiagramElementSet = true;
                    }
                }
            }
        }

        /// <summary>
        /// Compute the <see cref="DiagramEdge" /> to show
        /// </summary>
        /// <param name="diagramItemViewModel">The diagram item.</param>
        private void ComputeDiagramConnector(ThingDiagramContentItemViewModel diagramItemViewModel)
        {
            this.GenerateRelationshipDiagramElements(diagramItemViewModel, false, false);
        }

        /// <summary>
        /// create a <see cref="DiagramObject" /> from a dropped thing
        /// </summary>
        /// <param name="depictedThing">The dropped <see cref="Thing" /></param>
        /// <param name="diagramPosition">The position of the <see cref="DiagramObject" /></param>
        /// <param name="shouldAddMissingThings">Indicates if missing things should be added</param>
        /// <returns>The <see cref="DiagramObjectViewModel" /> instantiated</returns>
        private ThingDiagramContentItemViewModel CreateDiagramObject(Thing depictedThing, Point diagramPosition, bool shouldAddMissingThings = true)
        {
            var row = this.ThingDiagramItemViewModels.OfType<ThingDiagramContentItemViewModel>().SingleOrDefault(x => x.DiagramThing.DepictedThing == depictedThing);

            if (row != null)
            {
                return row;
            }

            if (!shouldAddMissingThings)
            {
                return null;
            }

            var block = new DiagramObject(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
            {
                DepictedThing = depictedThing,
                Name = depictedThing.UserFriendlyName,
                Documentation = depictedThing.UserFriendlyName,
                Resolution = Cdp4DiagramHelper.DefaultResolution
            };

            var bound = new Bounds(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
            {
                X = (float) diagramPosition.X,
                Y = (float) diagramPosition.Y,
                Height = Cdp4DiagramHelper.DefaultHeight,
                Width = Cdp4DiagramHelper.DefaultWidth
            };

            block.Bounds.Add(bound);

            NamedThingDiagramContentItemViewModel newDiagramElement = null;

            switch (depictedThing)
            {
                case ElementDefinition:
                {
                    var archElement = new ArchitectureElement(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
                    {
                        DepictedThing = depictedThing,
                        Name = depictedThing.UserFriendlyName,
                        Documentation = depictedThing.UserFriendlyName,
                        Resolution = Cdp4DiagramHelper.DefaultResolution
                    };

                    archElement.Bounds.Add(bound);

                    newDiagramElement = new ElementDefinitionDiagramContentItemViewModel(archElement, this.Session, this);
                    break;
                }

                case Requirement:
                    newDiagramElement = new RequirementDiagramContentItemViewModel(block, this.Session, this);
                    break;
                default:
                    newDiagramElement = new NamedThingDiagramContentItemViewModel(block, this.Session, this);
                    break;
            }

            var position = new Point { X = bound.X, Y = bound.Y };

            this.Behavior.ItemPositions.Add(newDiagramElement, position);
            this.ThingDiagramItemViewModels.Add(newDiagramElement);

            return newDiagramElement;
        }

        /// <summary>
        /// Create a <see cref="DiagramEdge" /> from a <see cref="BinaryRelationship" />
        /// </summary>
        /// <param name="thing">The <see cref="BinaryRelationship" /></param>
        /// <param name="source">The <see cref="DiagramObject" /> source</param>
        /// <param name="target">The <see cref="DiagramObject" /> target</param>
        private void CreateDiagramConnector(Thing thing, DiagramObject source, DiagramObject target)
        {
            var connectorItem = this.ThingDiagramItemViewModels.OfType<ThingDiagramConnectorViewModel>().SingleOrDefault(x => ((DiagramEdge) x.DiagramThing)?.DepictedThing == thing);

            if (connectorItem != null)
            {
                return;
            }

            var connector = new DiagramEdge(Guid.NewGuid(), this.Thing.Cache, new Uri(this.Session.DataSourceUri))
            {
                Source = source,
                Target = target,
                DepictedThing = thing
            };

            connectorItem = new DrawnDiagramEdgeViewModel(connector, this.Session, this);

            this.ConnectorViewModels.Add(connectorItem);
        }

        /// <summary>
        /// Execute the <see cref="GenerateDiagramCommandShallow" />
        /// </summary>
        public void ExecuteGenerateDiagramCommand(bool extendDeep)
        {
            foreach (var item in this.SelectedItems)
            {
                if ((item as DiagramContentItem)?.Content is not ThingDiagramContentItemViewModel content)
                {
                    continue;
                }

                this.GenerateRelationshipDiagramElements(content, extendDeep);
                this.Behavior.ApplyChildLayout(item);
            }
        }

        /// <summary>
        /// Basicaly diagram Activate connector tool
        /// </summary>
        public void CreateInterfaceCommandExecute()
        {
            this.Behavior.ActivateConnectorTool();
        }

        /// <summary>
        /// Generate the diagram connectors from the <see cref="BinaryRelationship" /> associated to the depicted
        /// <see cref="Thing" />
        /// </summary>
        /// <param name="itemViewModel">The <see cref="ThingDiagramContentItemViewModel" /> to start from</param>
        /// <param name="extendDeep">
        /// Indicates whether the process shall keep going for the related
        /// <see cref="DiagramObjectViewModel" />
        /// </param>
        /// <param name="shouldAddMissingThings">True if missing things should be added to diagram.</param>
        public void GenerateRelationshipDiagramElements(ThingDiagramContentItemViewModel itemViewModel, bool extendDeep, bool shouldAddMissingThings = true)
        {
            var iteration = (Iteration) this.Thing.Container;

            var depictedThing = itemViewModel.DiagramThing.DepictedThing;

            var relationships =
                iteration.Relationship
                    .OfType<BinaryRelationship>()
                    .Where(r => r.Source == depictedThing || r.Target == depictedThing);

            foreach (var binaryRelationship in relationships)
            {
                if (this.ConnectorViewModels.OfType<ThingDiagramConnectorViewModel>().Any(x => ((DiagramEdge) x.DiagramThing)?.DepictedThing == binaryRelationship))
                {
                    continue;
                }

                var associatedViewModel = this.GenerateRelationshipDiagramObjectAndConnector(itemViewModel, binaryRelationship, shouldAddMissingThings);

                if (extendDeep && associatedViewModel != null)
                {
                    this.GenerateRelationshipDiagramElements(associatedViewModel, true);
                }
            }
        }

        /// <summary>
        /// Generate DiagramObject and DiagramConnectors for a <see cref="BinaryRelationship" />
        /// </summary>
        /// <param name="itemViewModel">The <see cref="ThingDiagramContentItemViewModel" /> to start from</param>
        /// <param name="binaryRelationship">The <see cref="BinaryRelationship" /></param>
        /// <param name="shouldAddMissingThings">True if missing things should be added to diagram.</param>
        /// <returns>
        /// The newly created <see cref="ThingDiagramContentItemViewModel" /> that is connected to  the
        /// <paramref name="itemViewModel" />.
        /// </returns>
        private ThingDiagramContentItemViewModel GenerateRelationshipDiagramObjectAndConnector(ThingDiagramContentItemViewModel itemViewModel, BinaryRelationship binaryRelationship, bool shouldAddMissingThings)
        {
            var depictedThing = itemViewModel.DiagramThing.DepictedThing;

            if (binaryRelationship.Source == depictedThing)
            {
                var associatedViewModel = this.CreateDiagramObject(binaryRelationship.Target, new Point(itemViewModel.DiagramRepresentation.Position.X + Cdp4DiagramHelper.DefaultSeparation, itemViewModel.DiagramRepresentation.Position.Y), shouldAddMissingThings);

                if (associatedViewModel == null)
                {
                    return null;
                }

                this.CreateDiagramConnector(binaryRelationship, (DiagramObject) itemViewModel.DiagramThing, (DiagramObject) associatedViewModel.DiagramThing);
                return associatedViewModel;
            }
            else
            {
                var associatedViewModel = this.CreateDiagramObject(binaryRelationship.Source, new Point(itemViewModel.DiagramRepresentation.Position.X - Cdp4DiagramHelper.DefaultSeparation, itemViewModel.DiagramRepresentation.Position.Y), shouldAddMissingThings);

                if (associatedViewModel == null)
                {
                    return null;
                }

                this.CreateDiagramConnector(binaryRelationship, (DiagramObject) associatedViewModel.DiagramThing, (DiagramObject) itemViewModel.DiagramThing);
                return associatedViewModel;
            }
        }

        /// <summary>
        /// Execute the save command asynchronously
        /// </summary>
        /// <returns>The task</returns>
        private async Task ExecuteSaveDiagramCommand(ArchitectureElement newTopElement = null, bool unsetTopeElement = false)
        {
            var clone = this.Thing.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.Thing));

            if (clone is ArchitectureDiagram architectureDiagram)
            {
                if (newTopElement != null)
                {
                    architectureDiagram.TopArchitectureElement = newTopElement;
                }
                else if (unsetTopeElement)
                {
                    architectureDiagram.TopArchitectureElement = null;
                }

                clone = architectureDiagram;
            }

            transaction.CreateOrUpdate(clone);

            var deletedDiagramObj = this.Thing.DiagramElement.OfType<DiagramObject>().Except(this.ThingDiagramItemViewModels.OfType<NamedThingDiagramContentItemViewModel>().Select(x => x.DiagramThing)).ToList();

            var deletedEdges = this.Thing.DiagramElement.OfType<DiagramEdge>().Except(this.ConnectorViewModels.OfType<IPersistedConnector>().Select(x => x.DiagramThing));

            foreach (var diagramObject in deletedDiagramObj)
            {
                transaction.Delete(diagramObject.Clone(false));
            }

            foreach (var diagramEdge in deletedEdges)
            {
                transaction.Delete(diagramEdge.Clone(false));
            }

            foreach (var diagramObjectViewModel in this.ThingDiagramItemViewModels.OfType<NamedThingDiagramContentItemViewModel>())
            {
                diagramObjectViewModel.UpdateTransaction(transaction, clone);
            }

            foreach (var connectorViewModel in this.ConnectorViewModels.OfType<IPersistedConnector>())
            {
                connectorViewModel.UpdateTransaction(transaction, clone);
            }

            try
            {
                await this.DalWrite(transaction);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return;
            }

            this.IsDirty = false;
        }

        /// <summary>
        /// Handles the <see cref="DiagramDeleteEvent" /> event
        /// </summary>
        /// <param name="deleteEvent">The <see cref="DiagramDeleteEvent" /></param>
        private void OnDiagramDeleteEvent(DiagramDeleteEvent deleteEvent)
        {
            if (deleteEvent.ViewModel is ThingDiagramConnectorViewModel connectorViewModel)
            {
                this.ConnectorViewModels.RemoveAndDispose(connectorViewModel);
            }

            if (deleteEvent.ViewModel is ThingDiagramContentItemViewModel diagramObjViewModel)
            {
                this.ThingDiagramItemViewModels.RemoveAndDispose(diagramObjViewModel);
            }

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.ThingDiagramItemViewModels.ClearAndDispose();
            this.ConnectorViewModels.ClearAndDispose();

            base.Dispose(disposing);
        }
    }
}
