// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEditorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4DiagramEditor.ViewModels
{
    using System;

    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;
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

    using CDP4DiagramEditor.ViewModels.Relation;

    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    using Point = System.Windows.Point;
    using IDropTarget = CDP4Composition.DragDrop.IDropTarget;

    /// <summary>
    /// The view-model for the <see cref="CDP4DiagramEditor"/> view
    /// </summary>
    public class DiagramEditorViewModel : BrowserViewModelBase<DiagramCanvas>, IPanelViewModel, IDropTarget, ICdp4DiagramContainer, IDiagramEditorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ThingDiagramItems"/>
        /// </summary>
        private ReactiveList<ThingDiagramContentItem> thingDiagramItems;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="RelationshipRules"/>
        /// </summary>
        private DisposableReactiveList<RuleNavBarRelationViewModel> relationshipRules;

        /// <summary>
        /// Backing field for <see cref="CanCreateDiagram"/>
        /// </summary>
        private bool canCreateDiagram;

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>
        /// </summary>
        private DiagramItem selectedItem;

        /// <summary>
        /// Backing field for <see cref="SelectedItems"/>
        /// </summary>
        private ReactiveList<DiagramItem> selectedItems;

        /// <summary>
        /// Backing field for <see cref="IsDirty"/>
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEditorViewModel"/> class
        /// </summary>
        /// <param name="diagram">The diagram of the <see cref="Thing"/>s to display</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> instance</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> instance</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> instance</param>
        public DiagramEditorViewModel(DiagramCanvas diagram, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(diagram, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = this.Thing.Name;
            this.ToolTip = $"The {this.Thing.Name} diagram editor";
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
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets the collection diagramming-port to display.
        /// </summary>
        public ReactiveList<DiagramPortViewModel> DiagramPortCollection { get; private set; }

        /// <summary>
        /// Gets the collection diagramming-item to display.
        /// </summary>
        public ReactiveList<DiagramEdgeViewModel> DiagramConnectorCollection { get; private set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        public ReactiveList<DiagramItem> SelectedItems
        {
            get { return this.selectedItems; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItems, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        public DiagramItem SelectedItem
        {
            get { return this.selectedItem; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItem, value); }
        }

        /// <summary>
        /// gets or sets the behavior.
        /// </summary>
        public ICdp4DiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// The <see cref="IEventPublisher"/> that allows view/view-model communication
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="EngineeringModelSetup"/>
        /// </summary>
        public bool CanCreateDiagram
        {
            get => this.canCreateDiagram;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateDiagram, value);
        }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        /// <remarks>
        /// This is set by the children view-models
        /// </remarks>
        public override bool IsDirty
        {
            get => this.isDirty;
            set => this.RaiseAndSetIfChanged(ref this.isDirty, value);
        }

        /// <summary>
        /// Occurs when a <see cref="ThingDiagramContentItem"/> gets removed
        /// </summary>
        /// <param name="contentItemContent">The removed object</param>
        public void RemoveDiagramThingItem(object contentItemContent)
        {
            if (contentItemContent is ThingDiagramContentItem item)
            {
                this.ThingDiagramItems.Remove(item);
                this.Behavior.ItemPositions.Remove(item);

                var connectors = this.DiagramConnectorCollection.Where(x => x.Source.DepictedThing == item.DiagramThing.DepictedThing || x.Target.DepictedThing == item.DiagramThing.DepictedThing).ToArray();

                foreach (var diagramEdgeViewModel in connectors)
                {
                    this.DiagramConnectorCollection.Remove(diagramEdgeViewModel);
                }
            }
            else if (contentItemContent is DiagramEdgeViewModel connector)
            {
                this.DiagramConnectorCollection.Remove(connector);
            }

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Gets the save command
        /// </summary>
        public ReactiveCommand<Unit> SaveDiagramCommand { get; private set; }

        /// <summary>
        /// Gets the diagram generator command
        /// </summary>
        public ReactiveCommand<object> GenerateDiagramCommandShallow { get; private set; }

        /// <summary>
        /// Gets the diagram generator command
        /// </summary>
        public ReactiveCommand<object> GenerateDiagramCommandDeep { get; private set; }

        /// <summary>
        /// Gets or sets the RelationshipRules
        /// </summary>
        public DisposableReactiveList<RuleNavBarRelationViewModel> RelationshipRules
        {
            get { return this.relationshipRules; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipRules, value); }
        }

        /// <summary>
        /// Gets or sets the collection of diagram items.
        /// </summary>
        public ReactiveList<ThingDiagramContentItem> ThingDiagramItems
        {
            get { return this.thingDiagramItems; }
            set { this.RaiseAndSetIfChanged(ref this.thingDiagramItems, value); }
        }

        /// <summary>
        /// Gets or sets the Create Port Command
        /// </summary>
        public ReactiveCommand<object> CreatePortCommand { get; private set; }

        /// <summary>
        /// Gets or sets the Create RelationShip Command
        /// </summary>
        public ReactiveCommand<object> CreateInterfaceCommand { get; private set; }

        /// <summary>
        /// Gets or sets the Create BinaryRelationShip Command
        /// </summary>
        public ReactiveCommand<object> CreateBinaryRelationshipCommand { get; protected set; }

        /// <summary>
        /// Initialize the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.WhenAnyValue(x => x.IsDirty).Subscribe(x => this.Caption = $"{this.Thing.Name}{(x ? " (Dirty)" : string.Empty)}");

            this.EventPublisher = new EventPublisher();

            var deleteObservable = this.EventPublisher.GetEvent<DiagramDeleteEvent>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(this.OnDiagramDeleteEvent);

            this.Disposables.Add(deleteObservable);
            this.RelationshipRules = new DisposableReactiveList<RuleNavBarRelationViewModel> { ChangeTrackingEnabled = true };
            this.ThingDiagramItems = new ReactiveList<ThingDiagramContentItem> { ChangeTrackingEnabled = true };
            this.SelectedItems = new ReactiveList<DiagramItem> { ChangeTrackingEnabled = true };

            this.DiagramPortCollection = new ReactiveList<DiagramPortViewModel> { ChangeTrackingEnabled = true };
            this.DiagramConnectorCollection = new ReactiveList<DiagramEdgeViewModel> { ChangeTrackingEnabled = true };
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecute = this.WhenAnyValue(x => x.CanCreateDiagram, x => x.IsDirty, (x, y) => x && y);
            this.SaveDiagramCommand = ReactiveCommand.CreateAsyncTask(canExecute, x => this.ExecuteSaveDiagramCommand(), RxApp.MainThreadScheduler);
            this.SaveDiagramCommand.ThrownExceptions.Subscribe(x => logger.Error(x.Message));

            this.GenerateDiagramCommandShallow = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItems).Select(s => s != null && s.OfType<DiagramContentItem>().Any()));
            this.GenerateDiagramCommandShallow.Subscribe(x => this.ExecuteGenerateDiagramCommand(false));

            this.GenerateDiagramCommandDeep = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItems).Select(s => s != null && s.OfType<DiagramContentItem>().Any()));
            this.GenerateDiagramCommandDeep.Subscribe(x => this.ExecuteGenerateDiagramCommand(true));

            this.CreatePortCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItem).Select(s => (s as DiagramContentItem)?.Content is PortContainerDiagramContentItem));
            this.CreatePortCommand.Subscribe(_ => this.CreatePortCommandExecute());

            this.CreateInterfaceCommand = ReactiveCommand.Create();
            this.CreateInterfaceCommand.Subscribe(_ => this.CreateInterfaceCommandExecute());
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
        /// Populates the context-menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            var savemenu = new ContextMenuItemViewModel("Save the Diagram", "", this.SaveDiagramCommand, MenuItemKind.Save, ClassKind.DiagramCanvas);
            this.ContextMenu.Add(savemenu);

            var generatemenushallow = new ContextMenuItemViewModel("Generate Traces (Shallow)", "", this.GenerateDiagramCommandShallow, MenuItemKind.Create, ClassKind.BinaryRelationship);
            this.ContextMenu.Add(generatemenushallow);

            var generatemenudeep = new ContextMenuItemViewModel("Generate Traces (Deep)", "", this.GenerateDiagramCommandDeep, MenuItemKind.Create, ClassKind.BinaryRelationship);
            this.ContextMenu.Add(generatemenudeep);
        }

        /// <summary>
        /// Update this view-model
        /// </summary>
        public void UpdateProperties()
        {
            this.ComputeDiagramObject();
            this.IsDirty = false;
        }

        /// <summary>
        /// Compute the <see cref="DiagramObject"/> to display
        /// </summary>
        private void ComputeDiagramObject()
        {
            var updatedItems = this.Thing.DiagramElement.OfType<DiagramObject>().ToList();
            var currentItems = this.ThingDiagramItems.Select(x => x.DiagramThing).ToList();

            var newItems = updatedItems.Except<DiagramObject>(currentItems);
            var oldItems = currentItems.Except(updatedItems);

            foreach (var diagramThing in oldItems)
            {
                var item = this.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing == diagramThing);
                if (item != null)
                {
                    this.ThingDiagramItems.Remove(item);
                    this.Behavior.ItemPositions.Remove(item);
                }
            }

            foreach (var diagramThing in newItems)
            {
                NamedThingDiagramContentItem newDiagramElement = null;

                if (diagramThing.DepictedThing is ElementDefinition)
                {
                    newDiagramElement = new PortContainerDiagramContentItem(diagramThing, this);
                }
                else
                {
                    newDiagramElement = new NamedThingDiagramContentItem(diagramThing, this);
                }

                var bound = diagramThing.Bounds.Single();

                var position = new Point { X = bound.X, Y = bound.Y };

                newDiagramElement.Position = position;

                this.Behavior.ItemPositions.Add(newDiagramElement, position);
                this.ThingDiagramItems.Add(newDiagramElement);
            }
        }

        /// <summary>
        /// Compute the <see cref="DiagramEdge"/> to show
        /// </summary>
        public void ComputeDiagramConnector()
        {
            foreach (var item in this.ThingDiagramItems.ToList())
            {
                this.GenerateDiagramRelation(item, false, false);
            }
        }

        /// <summary>
        /// Compute the <see cref="DiagramEdge"/> to show
        /// </summary>
        /// <param name="diagramItem">The diagram item.</param>
        private void ComputeDiagramConnector(ThingDiagramContentItem diagramItem)
        {
            this.GenerateDiagramRelation(diagramItem, false, false);
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var rowPayload = dropInfo.Payload as Thing;

            if (rowPayload != null)
            {
                if (!this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().Select(x => x.Thing).Contains(rowPayload))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }
            }

            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuplePayload)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var rowPayload = dropInfo.Payload as Thing;

            var convertedDropPosition = this.Behavior.GetDiagramPositionFromMousePosition(dropInfo.DropPosition);

            if (rowPayload != null)
            {
                if (this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().Select(x => x.Thing).Contains(rowPayload))
                {
                    return;
                }

                var block = new DiagramObject()
                {
                    DepictedThing = rowPayload,
                    Name = rowPayload.UserFriendlyName,
                    Documentation = rowPayload.UserFriendlyName,
                    Resolution = Cdp4DiagramHelper.DefaultResolution
                };

                var position = new Point(convertedDropPosition.X, convertedDropPosition.Y);

                block.Bounds.Add(new Bounds { X = (float)position.X, Y = (float)position.Y});

                NamedThingDiagramContentItem diagramItem = null;

                if (rowPayload is ElementDefinition elementDefinition)
                {
                    diagramItem = new PortContainerDiagramContentItem(block, this);
                }
                else if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuplePayload)
                {
                    block.DepictedThing = tuplePayload.Item1;
                    diagramItem = new NamedThingDiagramContentItem(block, this);
                }
                else
                {
                    diagramItem = new NamedThingDiagramContentItem(block, this);
                }

                diagramItem.Position = position;

                this.Behavior.ItemPositions.Add(diagramItem, convertedDropPosition);
                this.ThingDiagramItems.Add(diagramItem);

                this.ComputeDiagramConnector(diagramItem);

                this.IsDirty = true;
                this.UpdateIsDirty();
            }
        }

        /// <summary>
        /// create a <see cref="DiagramObject"/> from a dropped thing
        /// </summary>
        /// <param name="depictedThing">The dropped <see cref="Thing"/></param>
        /// <param name="diagramPosition">The position of the <see cref="DiagramObject"/></param>
        /// <returns>The <see cref="DiagramObjectViewModel"/> instantiated</returns>
        private ThingDiagramContentItem CreateDiagramObject(Thing depictedThing, System.Windows.Point diagramPosition, bool shouldAddMissingThings = true)
        {
            var row = this.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing.DepictedThing == depictedThing);
            if (row != null)
            {
                return row;// row;
            }

            if (!shouldAddMissingThings)
            {
                return null;
            }

            var block = new DiagramObject()
            {
                DepictedThing = depictedThing,
                Name = depictedThing.UserFriendlyName,
                Documentation = depictedThing.UserFriendlyName,
                Resolution = Cdp4DiagramHelper.DefaultResolution
            };

            var bound = new Bounds()
            {
                X = (float)diagramPosition.X,
                Y = (float)diagramPosition.Y,
                Height = Cdp4DiagramHelper.DefaultHeight,
                Width = Cdp4DiagramHelper.DefaultWidth
            };

            block.Bounds.Add(bound);

            NamedThingDiagramContentItem newDiagramElement = null;

            if (depictedThing is ElementDefinition)
            {
                newDiagramElement = new PortContainerDiagramContentItem(block, this);
            }
            else
            {
                newDiagramElement = new NamedThingDiagramContentItem(block, this);
            }

            var position = new Point { X = bound.X, Y = bound.Y };

            newDiagramElement.Position = position;

            this.Behavior.ItemPositions.Add(newDiagramElement, position);
            this.ThingDiagramItems.Add(newDiagramElement);

            return newDiagramElement;
        }

        /// <summary>
        /// create a <see cref="PortContainerDiagramContentItem"/>
        /// </summary>
        /// <param name="depictedThing">The dropped <see cref="Thing"/></param>
        /// <param name="diagramPosition">The position of the <see cref="DiagramObject"/></param>
        /// <returns>The <see cref="DiagramObjectViewModel"/> instantiated</returns>
        private void CreateDiagramPort(Thing depictedThing, System.Windows.Point diagramPosition)
        {
            if (this.SelectedItem is DiagramContentItem target && target?.Content is PortContainerDiagramContentItem container)
            {
                var row = this.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing.DepictedThing == depictedThing);

                if (row != null)
                {
                    return; //return row;
                }

                var block = new DiagramObject()
                {
                    DepictedThing = depictedThing,
                    Name = depictedThing.UserFriendlyName,
                    Documentation = depictedThing.UserFriendlyName,
                    Resolution = Cdp4DiagramHelper.DefaultResolution
                };

                var bound = new Bounds()
                {
                    X = (float) target.Position.X,
                    Y = (float) target.Position.Y,
                    Height = (float) target.ActualHeight,
                    Width = (float) target.ActualWidth
                };

                block.Bounds.Add(bound);
                var diagramItem = new DiagramPortViewModel(block, this.Session, this);
                container.PortCollection.Add(diagramItem);

                this.DiagramPortCollection.Add(diagramItem);
                this.UpdateIsDirty();
            }
        }

        /// <summary>
        /// Handle the drop when the payload is a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="binaryRelationship">The dropped <see cref="BinaryRelationship"/></param>
        /// <param name="dropPosition">The dropped position</param>
        private void OnBinaryRelationshipDrop(BinaryRelationship binaryRelationship, System.Windows.Point dropPosition)
        {
            // draw the source if it does not exist
            var diagramObjectSource = this.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing.DepictedThing == binaryRelationship.Source);
            if (diagramObjectSource == null)
            {
                var sourceObjectPosition = new System.Windows.Point(dropPosition.X - Cdp4DiagramHelper.DefaultWidth, dropPosition.Y);
                diagramObjectSource = this.CreateDiagramObject(binaryRelationship.Source, sourceObjectPosition);
            }

            // draw the source if it does not exist
            var diagramObjectTarget = this.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing.DepictedThing == binaryRelationship.Target);
            if (diagramObjectTarget == null)
            {
                var targetObjectPosition = new System.Windows.Point(dropPosition.X + Cdp4DiagramHelper.DefaultWidth, dropPosition.Y);
                diagramObjectTarget = this.CreateDiagramObject(binaryRelationship.Target, targetObjectPosition);
            }

            this.CreateDiagramConnector(binaryRelationship, diagramObjectSource.DiagramThing, diagramObjectTarget.DiagramThing);
        }

        /// <summary>
        /// Create a <see cref="DiagramEdge"/> from a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="binaryRelationship">The <see cref="BinaryRelationship"/></param>
        /// <param name="source">The <see cref="DiagramObject"/> source</param>
        /// <param name="target">The <see cref="DiagramObject"/> target</param>
        private DiagramEdgeViewModel CreateDiagramConnector(BinaryRelationship binaryRelationship, DiagramObject source, DiagramObject target)
        {
            var connectorItem = this.DiagramConnectorCollection.SingleOrDefault(x => x.Thing.DepictedThing == binaryRelationship);
            if (connectorItem != null)
            {
                return connectorItem;
            }

            var connector = new DiagramEdge
            {
                Source = source,
                Target = target,
                DepictedThing = binaryRelationship
            };

            connectorItem = new DiagramEdgeViewModel(connector, this.Session, this);
            this.DiagramConnectorCollection.Add(connectorItem);

            return connectorItem;
        }

        /// <summary>
        /// Execute the <see cref="GenerateDiagramCommandShallow"/>
        /// </summary>
        public void ExecuteGenerateDiagramCommand(bool extendDeep)
        {
            foreach (var item in this.SelectedItems)
            {
                if (!((item as DiagramContentItem)?.Content is ThingDiagramContentItem content))
                {
                    continue;
                }

                this.GenerateDiagramRelation(content, extendDeep);
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
        /// Create a port with a dummy with a <see cref="ElementUsage"/>
        /// </summary>
        public void CreatePortCommandExecute()
        {
            this.CreateDiagramPort(new ElementUsage() { Name = "WhyNot", ShortName = "WhyNot"}, new Point(0,0));
        }

        /// <summary>
        /// Redraws the connectors of a specified item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        public void RedrawConnectors(ThingDiagramContentItem contentItem)
        {
            var iteration = (Iteration)this.Thing.Container;

            var depictedThing = contentItem.DiagramThing;
            var relationships = iteration.Relationship.OfType<BinaryRelationship>().Where(r => r.Source == depictedThing || r.Target == depictedThing);

            // cleanup existing and redraw them.
            var existingConnectors = this.DiagramConnectorCollection.Where(x => x.Thing.Source.DepictedThing.Iid.Equals(depictedThing.DepictedThing.Iid) || x.Thing.Target.DepictedThing.Iid.Equals(depictedThing.DepictedThing.Iid)).ToList();

            foreach (var diagramEdgeViewModel in existingConnectors)
            {
                this.DiagramConnectorCollection.Remove(diagramEdgeViewModel);
            }

            // simply readd them
            this.DiagramConnectorCollection.AddRange(existingConnectors);
        }

        /// <summary>
        /// Generate the diagram connectors from the <see cref="BinaryRelationship"/> associated to the depicted <see cref="Thing"/>
        /// </summary>
        /// <param name="item">The <see cref="DiagramObjectViewModel"/> to start from</param>
        /// <param name="extendDeep">Indicates whether the process shall keep going for the related <see cref="DiagramObjectViewModel"/></param>
        /// <param name="shouldAddMissingThings">True if missing things should be added to diagram.</param>
        public void GenerateDiagramRelation(ThingDiagramContentItem item, bool extendDeep, bool shouldAddMissingThings = true)
        {
            var iteration = (Iteration)this.Thing.Container;

            var depictedThing = item.DiagramThing.DepictedThing;
            var relationships = iteration.Relationship.OfType<BinaryRelationship>().Where(r => r.Source == depictedThing || r.Target == depictedThing);

            var sourceIsSet = false;

            foreach (var binaryRelationship in relationships)
            {
                if (this.DiagramConnectorCollection.Any(x => x.Thing.DepictedThing == binaryRelationship))
                {
                    continue;
                }

                ThingDiagramContentItem associatedViewModel;

                if (binaryRelationship.Source == depictedThing)
                {
                    associatedViewModel = this.CreateDiagramObject(binaryRelationship.Target, new Point(item.Position.X + Cdp4DiagramHelper.DefaultSeparation, item.Position.Y), shouldAddMissingThings);

                    if (associatedViewModel != null)
                    {
                        sourceIsSet = this.CreateDiagramConnector(binaryRelationship, item.DiagramThing, associatedViewModel.DiagramThing) != null;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    associatedViewModel = this.CreateDiagramObject(binaryRelationship.Source, new Point(item.Position.X - Cdp4DiagramHelper.DefaultSeparation, item.Position.Y), shouldAddMissingThings);

                    if (associatedViewModel != null)
                    {
                        this.CreateDiagramConnector(binaryRelationship, associatedViewModel.DiagramThing, item.DiagramThing);
                    }

                    sourceIsSet = false;
                }

                if (extendDeep && associatedViewModel != null)
                {
                    this.GenerateDiagramRelation(associatedViewModel, true);
                }
            }
        }

        /// <summary>
        /// Execute the save command asynchronously
        /// </summary>
        /// <returns>The task</returns>
        private async Task ExecuteSaveDiagramCommand()
        {
            var clone = this.Thing.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.Thing));

            transaction.CreateOrUpdate(clone);
            clone.DiagramElement.Clear();

            var deletedDiagramObj = this.Thing.DiagramElement.OfType<DiagramObject>().Except(this.ThingDiagramItems.Select(x => x.DiagramThing));
            foreach (var diagramObject in deletedDiagramObj)
            {
                transaction.Delete(diagramObject.Clone(false));
            }

            foreach (var diagramObjectViewModel in this.ThingDiagramItems)
            {
                diagramObjectViewModel.UpdateTransaction(transaction, clone);
            }

            await this.DalWrite(transaction);
            this.IsDirty = false;
        }

        /// <summary>
        /// Handles the <see cref="DiagramDeleteEvent"/> event
        /// </summary>
        /// <param name="deleteEvent">The <see cref="DiagramDeleteEvent"/></param>
        private void OnDiagramDeleteEvent(DiagramDeleteEvent deleteEvent)
        {
            if (deleteEvent.ViewModel is ThingDiagramContentItem diagramObjViewModel)
            {
                this.ThingDiagramItems.Remove(diagramObjViewModel);
                var connectors = this.DiagramConnectorCollection.Where(x => x.Source == diagramObjViewModel.Thing || x.Target == diagramObjViewModel.Thing).ToArray();
                foreach (var diagramEdgeViewModel in connectors)
                {
                    //this.DiagramConnectorCollection.Remove(diagramEdgeViewModel);
                }
            }

            if (deleteEvent.ViewModel is DiagramEdgeViewModel connectorViewModel)
            {
                this.DiagramConnectorCollection.Remove(connectorViewModel);
            }

            this.UpdateIsDirty();
        }

        /// <summary>
        /// Update this <see cref="IsDirty"/> property
        /// </summary>
        public void UpdateIsDirty()
        {
            var currentObject = this.Thing.DiagramElement.OfType<DiagramObject>().ToArray();
            var displayedObjects = this.ThingDiagramItems.Select(x => (x as NamedThingDiagramContentItem)?.DiagramThing).ToArray();

            var removedItem = currentObject.Except(displayedObjects).Count();
            var addedItem = displayedObjects.Except(currentObject).Count();

            this.IsDirty = this.ThingDiagramItems.Any(x => x.IsDirty) || removedItem > 0;

            this.IsDirty |= addedItem > 0;

            this.IsDirty |= this.DiagramPortCollection.Any(x => x.IsDirty) || removedItem > 0;
        }

        /// <summary>
        /// Show the <see cref="SelectedItem"/> in the Property Grid
        /// </summary>
        protected override void ShowInPropertyGrid()
        {
            if (this.SelectedItem != null)
            {
                this.PanelNavigationService.Open(((IThingDiagramItem)this.SelectedItem).Thing, this.Session);
            }
        }
    }
}