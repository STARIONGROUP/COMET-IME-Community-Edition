// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEditorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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
    using System.Windows.Controls;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView.Diagram;
    using CDP4CommonView.EventAggregator;
    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;

    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="CDP4DiagramEditor"/> view
    /// </summary>
    public class DiagramEditorViewModel : BrowserViewModelBase<DiagramCanvas>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// Backing field for <see cref="CanCreateDiagram"/>
        /// </summary>
        private bool canCreateDiagram;

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>
        /// </summary>
        private DiagramElementThing selectedItem;

        /// <summary>
        /// Backing field for <see cref="SelectedItems"/>
        /// </summary>
        private ReactiveList<DiagramElementThing> selectedItems;

        /// <summary>
        /// Gets or sets the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        public DiagramElementThing SelectedItem
        {
            get { return this.selectedItem; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItem, value); }
        }

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
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Gets the collection diagramming-object to display.
        /// </summary>
        public ReactiveList<DiagramObjectViewModel> DiagramObjectCollection { get; private set; }

        /// <summary>
        /// Gets the collection diagramming-item to display.
        /// </summary>
        public ReactiveList<DiagramEdgeViewModel> DiagramConnectorCollection { get; private set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        public ReactiveList<DiagramElementThing> SelectedItems
        {
            get { return this.selectedItems; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItems, value); }
        }

        /// <summary>
        /// The <see cref="IEventPublisher"/> that allows view/view-model communication
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="EngineeringModelSetup"/>
        /// </summary>
        public bool CanCreateDiagram
        {
            get { return this.canCreateDiagram; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateDiagram, value); }
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
        /// Initialize the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.WhenAnyValue(x => x.IsDirty).Subscribe(x => this.Caption = string.Format("{0}{1}", this.Thing.Name, x ? " (Dirty)" : string.Empty));

            this.EventPublisher = new EventPublisher();
            var deleteObservable = this.EventPublisher.GetEvent<DiagramDeleteEvent>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(this.OnDiagramDeleteEvent);
            var selectionObservable = this.EventPublisher.GetEvent<DiagramSelectEvent>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(e => this.SelectedItems = e.SelectedViewModels);
            this.Disposables.Add(deleteObservable);
            this.Disposables.Add(selectionObservable);

            this.DiagramObjectCollection = new ReactiveList<DiagramObjectViewModel> { ChangeTrackingEnabled = true };
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

            this.GenerateDiagramCommandShallow = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItems).Select(s => s != null && s.OfType<DiagramObjectViewModel>().Any()));
            this.GenerateDiagramCommandShallow.Subscribe(x => this.ExecuteGenerateDiagramCommand(false));

            this.GenerateDiagramCommandDeep = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedItems).Select(s => s != null && s.OfType<DiagramObjectViewModel>().Any()));
            this.GenerateDiagramCommandDeep.Subscribe(x => this.ExecuteGenerateDiagramCommand(true));
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
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.ComputeDiagramObject();
            this.ComputeDiagramConnector();
            this.IsDirty = false;
        }

        /// <summary>
        /// Compute the <see cref="DiagramObject"/> to display
        /// </summary>
        private void ComputeDiagramObject()
        {
            var updatedItems = this.Thing.DiagramElement.OfType<DiagramObject>().ToArray();
            var currentItems = this.DiagramObjectCollection.Select(x => x.Thing).ToArray();

            var newItems = updatedItems.Except(currentItems).ToArray();
            var oldItems = currentItems.Except(updatedItems).ToArray();

            foreach (var diagramThingBase in oldItems)
            {
                var item = this.DiagramObjectCollection.SingleOrDefault(x => x.Thing == diagramThingBase);
                if (item != null)
                {
                    this.DiagramObjectCollection.Remove(item);
                    item.Dispose();
                }
            }

            foreach (var diagramThingBase in newItems)
            {
                var newDiagramElement = new DiagramObjectViewModel(diagramThingBase, this.Session, this);
                this.DiagramObjectCollection.Add(newDiagramElement);
            }
        }

        /// <summary>
        /// Compute the <see cref="DiagramEdge"/> to show
        /// </summary>
        private void ComputeDiagramConnector()
        {
            var updatedItems = this.Thing.DiagramElement.OfType<DiagramEdge>().ToArray();
            var currentItems = this.DiagramConnectorCollection.Select(x => x.Thing).OfType<DiagramEdge>().ToArray();

            var newItems = updatedItems.Except(currentItems).ToArray();
            var oldItems = currentItems.Except(updatedItems).ToArray();

            foreach (var diagramThingBase in oldItems)
            {
                var item = this.DiagramConnectorCollection.SingleOrDefault(x => x.Thing == diagramThingBase);
                if (item != null)
                {
                    this.DiagramConnectorCollection.Remove(item);
                    item.Dispose();
                }
            }

            foreach (var diagramThingBase in newItems)
            {
                var newDiagramElement = new DiagramEdgeViewModel(diagramThingBase, this.Session, this);
                this.DiagramConnectorCollection.Add(newDiagramElement);
            }
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
            if (rowPayload is DiagramThingBase)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (!this.DiagramObjectCollection.Select(x => x.Thing.DepictedThing).Contains(rowPayload))
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
            var droppedThing = dropInfo.Payload as Thing;
            if (droppedThing == null)
            {
                return;
            }

            if (this.DiagramObjectCollection.Select(x => x.Thing.DepictedThing).Contains(droppedThing))
            {
                return;
            }

            var diagramDrop = (IDiagramDropInfo)dropInfo;

            var binaryrelationship = droppedThing as BinaryRelationship;
            if (binaryrelationship != null)
            {
                this.OnBinaryRelationshipDrop(binaryrelationship, diagramDrop.DiagramDropPoint);
            }
            else
            {
                this.CreateDiagramObject(droppedThing, diagramDrop.DiagramDropPoint);
            }
        }

        /// <summary>
        /// create a <see cref="DiagramObject"/> from a dropped thing
        /// </summary>
        /// <param name="depictedThing">The dropped <see cref="Thing"/></param>
        /// <param name="diagramPosition">The position of the <see cref="DiagramObject"/></param>
        /// <returns>The <see cref="DiagramObjectViewModel"/> instantiated</returns>
        private DiagramObjectViewModel CreateDiagramObject(Thing depictedThing, System.Windows.Point diagramPosition)
        {
            var row = this.DiagramObjectCollection.SingleOrDefault(x => x.Thing.DepictedThing == depictedThing);
            if (row != null)
            {
                return row;
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

            var diagramItem = new DiagramObjectViewModel(block, this.Session, this);
            this.DiagramObjectCollection.Add(diagramItem);
            return diagramItem;
        }

        /// <summary>
        /// Handle the drop when the payload is a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="binaryRelationship">The dropped <see cref="BinaryRelationship"/></param>
        /// <param name="dropPosition">The dropped position</param>
        private void OnBinaryRelationshipDrop(BinaryRelationship binaryRelationship, System.Windows.Point dropPosition)
        {
            // draw the source if it does not exist
            var diagramObjectSource = this.DiagramObjectCollection.SingleOrDefault(x => x.Thing.DepictedThing == binaryRelationship.Source);
            if (diagramObjectSource == null)
            {
                var sourceObjectPosition = new System.Windows.Point(dropPosition.X - Cdp4DiagramHelper.DefaultWidth, dropPosition.Y);
                diagramObjectSource = this.CreateDiagramObject(binaryRelationship.Source, sourceObjectPosition);
            }

            // draw the source if it does not exist
            var diagramObjectTarget = this.DiagramObjectCollection.SingleOrDefault(x => x.Thing.DepictedThing == binaryRelationship.Target);
            if (diagramObjectTarget == null)
            {
                var targetObjectPosition = new System.Windows.Point(dropPosition.X + Cdp4DiagramHelper.DefaultWidth, dropPosition.Y);
                diagramObjectTarget = this.CreateDiagramObject(binaryRelationship.Target, targetObjectPosition);
            }

            this.CreateDiagramConnector(binaryRelationship, diagramObjectSource.Thing, diagramObjectTarget.Thing);
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
                DepictedThing = binaryRelationship,
            };

            connectorItem = new DiagramEdgeViewModel(connector, this.Session, this);
            this.DiagramConnectorCollection.Add(connectorItem);

            return connectorItem;
        }

        /// <summary>
        /// Execute the <see cref="GenerateDiagramCommandShallow"/>
        /// </summary>
        private void ExecuteGenerateDiagramCommand(bool extendDeep)
        {
            foreach (var item in this.SelectedItems.OfType<DiagramObjectViewModel>())
            {
                this.GenerateDiagramRelation(item, extendDeep);
            }
        }

        /// <summary>
        /// Generate the diagram connectors from the <see cref="BinaryRelationship"/> associated to the depicted <see cref="Thing"/>
        /// </summary>
        /// <param name="item">The <see cref="DiagramObjectViewModel"/> to start from</param>
        /// <param name="extendDeep">Indicates whether the process shall keep going for the related <see cref="DiagramObjectViewModel"/></param>
        private void GenerateDiagramRelation(DiagramObjectViewModel item, bool extendDeep)
        {
            var iteration = (Iteration)this.Thing.Container;

            var depictedThing = item.Thing.DepictedThing;
            var relationships = iteration.Relationship.OfType<BinaryRelationship>().Where(r => r.Source == depictedThing || r.Target == depictedThing);
            foreach (var binaryRelationship in relationships)
            {
                if (this.DiagramConnectorCollection.Any(x => x.Thing.DepictedThing == binaryRelationship))
                {
                    continue;
                }

                DiagramObjectViewModel associatedViewModel;
                if (binaryRelationship.Source == depictedThing)
                {
                    associatedViewModel = this.CreateDiagramObject(binaryRelationship.Target, new System.Windows.Point(item.Position.X + Cdp4DiagramHelper.DefaultSeparation, item.Position.Y));
                    this.CreateDiagramConnector(binaryRelationship, item.Thing, associatedViewModel.Thing);
                }
                else
                {
                    associatedViewModel = this.CreateDiagramObject(binaryRelationship.Source, new System.Windows.Point(item.Position.X - Cdp4DiagramHelper.DefaultSeparation, item.Position.Y));
                    this.CreateDiagramConnector(binaryRelationship, associatedViewModel.Thing, item.Thing);
                }

                if (extendDeep)
                {
                    this.GenerateDiagramRelation(associatedViewModel, extendDeep);
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

            var deletedDiagramObj = this.Thing.DiagramElement.OfType<DiagramObject>().Except(this.DiagramObjectCollection.Select(x => x.Thing));
            foreach (var diagramObject in deletedDiagramObj)
            {
                transaction.Delete(diagramObject.Clone(false));
            }

            foreach (var diagramObjectViewModel in this.DiagramObjectCollection)
            {
                diagramObjectViewModel.UpdateTransaction(transaction, clone);
            }


            var deletedDiagramConnector = this.Thing.DiagramElement.OfType<DiagramEdge>().Except(this.DiagramConnectorCollection.Select(x => x.Thing));
            foreach (var connector in deletedDiagramConnector)
            {
                transaction.Delete(connector.Clone(false));
            }

            foreach (var diagramEdgeViewModel in this.DiagramConnectorCollection)
            {
                diagramEdgeViewModel.UpdateTransaction(transaction, clone);
            }

            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Handles the <see cref="DiagramDeleteEvent"/> event
        /// </summary>
        /// <param name="deleteEvent">The <see cref="DiagramDeleteEvent"/></param>
        private void OnDiagramDeleteEvent(DiagramDeleteEvent deleteEvent)
        {
            var diagramObjViewModel = deleteEvent.ViewModel as DiagramObjectViewModel;
            if (diagramObjViewModel != null)
            {
                this.DiagramObjectCollection.Remove(diagramObjViewModel);
                var connectors = this.DiagramConnectorCollection.Where(x => x.Source == diagramObjViewModel.Thing || x.Target == diagramObjViewModel.Thing).ToArray();
                foreach (var diagramEdgeViewModel in connectors)
                {
                    this.DiagramConnectorCollection.Remove(diagramEdgeViewModel);
                }
            }

            var connectorViewModel = deleteEvent.ViewModel as DiagramEdgeViewModel;
            if (connectorViewModel != null)
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
            var displayedObjects = this.DiagramObjectCollection.Select(x => x.Thing).ToArray();

            var removedItem = currentObject.Except(displayedObjects).Count();

            this.IsDirty = this.DiagramObjectCollection.Any(x => x.IsDirty) || removedItem > 0;
        }
    }
}