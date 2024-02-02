// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramBrowserViewModel.cs" company="RHEA System S.A.">
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
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
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4DiagramEditor.ViewModels.Rows;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DiagramBrowserViewModel" /> is to represent the view-model for <see cref="Diagram" />s
    /// </summary>
    public class DiagramBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Diagrams";

        /// <summary>
        /// Backing field for <see cref="CanCreateDiagram" />
        /// </summary>
        private bool canCreateDiagram;

        /// <summary>
        /// Backing field for <see cref="CanCreateArchitectureDiagram" />
        /// </summary>
        private bool canCreateArchitectureDiagram;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="CurrentModel" />
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramBrowserViewModel" /> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel" /></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService" /></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService" /></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService" /></param>
        public DiagramBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.UpdateDiagrams();

            this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup" />
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
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
        /// Gets or sets a value indicating whether the create Diagram command is enabled
        /// </summary>
        public bool CanCreateDiagram
        {
            get => this.canCreateDiagram;
            set => this.RaiseAndSetIfChanged(ref this.canCreateDiagram, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the create Architecture Diagram command is enabled
        /// </summary>
        public bool CanCreateArchitectureDiagram
        {
            get => this.canCreateArchitectureDiagram;
            set => this.RaiseAndSetIfChanged(ref this.canCreateArchitectureDiagram, value);
        }

        /// <summary>
        /// Gets the rows representing Diagrams
        /// </summary>
        public ReactiveList<DiagramCanvasRowViewModel> Diagrams { get; private set; }

        /// <summary>
        /// Gets or sets the Create Architecture Diagram Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateArchitectureDiagramCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Open Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Hide Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> HideCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Ready For Publication Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> ReadyForPublicationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Publish Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> PublishCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Diagrams = new ReactiveList<DiagramCanvasRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand" />s
        /// </summary>
        protected override void InitializeCommands()
        {
            var canDelete = this.WhenAnyValue(
                vm => vm.SelectedThing,
                vm => vm.CanWriteSelectedThing,
                (selection, canWrite) => selection != null && canWrite);

            this.CreateCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateCommand<DiagramCanvas>(this.Thing),
                this.WhenAnyValue(vm => vm.CanCreateDiagram));

            this.DeleteCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteDeleteCommand(this.SelectedThing.Thing),
                canDelete);

            this.UpdateCommand = ReactiveCommandCreator.Create(
                this.ExecuteUpdateCommand,
                canDelete);

            this.CreateArchitectureDiagramCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ArchitectureDiagram>(this.Thing), this.WhenAnyValue(vm => vm.CanCreateArchitectureDiagram));

            this.DeleteCommand = ReactiveCommandCreator.Create(() => this.ExecuteDeleteCommand(this.SelectedThing.Thing), canDelete);
            this.UpdateCommand = ReactiveCommandCreator.Create(this.ExecuteUpdateCommand, canDelete);
            this.OpenCommand = ReactiveCommandCreator.Create(this.ExecuteOpenCommand, canDelete);

            // publication of diagrams
            this.HideCommand = ReactiveCommandCreator.CreateAsyncTask(async () => await this.ExecuteHideCommand(this.SelectedThing.Thing as DiagramCanvas));

            this.HideCommand.ThrownExceptions
                .Subscribe(this.LogAsyncException);

            this.ReadyForPublicationCommand = ReactiveCommandCreator.CreateAsyncTask(async () => await this.ExecuteReadyForPublicationCommand(this.SelectedThing.Thing as DiagramCanvas));

            this.ReadyForPublicationCommand.ThrownExceptions
                .Subscribe(this.LogAsyncException);

            this.PublishCommand = ReactiveCommandCreator.CreateAsyncTask(async () => await this.ExecutePublishCommand(this.SelectedThing.Thing as DiagramCanvas));

            this.PublishCommand.ThrownExceptions
                .Subscribe(this.LogAsyncException);

            this.InspectCommand = ReactiveCommandCreator.Create(this.ExecuteUpdateCommand, this.WhenAnyValue(x => x.SelectedThing).Select(x => x != null));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateDiagrams();
            this.UpdateProperties();
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateDiagram = this.PermissionService.CanWrite(ClassKind.DiagramCanvas, this.Thing);
            this.CanCreateArchitectureDiagram = this.PermissionService.CanWrite(ClassKind.ArchitectureDiagram, this.Thing);
            this.CanWriteSelectedThing = true;
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Diagram", "", this.CreateCommand, MenuItemKind.Create, ClassKind.DiagramCanvas));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Architecture Diagram", "", this.CreateArchitectureDiagramCommand, MenuItemKind.Create, ClassKind.ArchitectureDiagram));

            if (this.SelectedThing is null)
            {
                return;
            }

            if (this.SelectedThing.CanEditThing)
            {
                // publication
                switch (((DiagramCanvas)this.SelectedThing.Thing).PublicationState)
                {
                    case PublicationState.Hidden:
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Ready for Publication", "", this.ReadyForPublicationCommand, MenuItemKind.Review));
                        break;
                    case PublicationState.ReadyForPublish:
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Hide", "CTRL+H", this.HideCommand, MenuItemKind.Hide));
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Publish", "CTRL+P", this.PublishCommand, MenuItemKind.Publish));
                        break;
                    case PublicationState.Published:
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Hide", "CTRL+H", this.HideCommand, MenuItemKind.Hide));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.ContextMenu.Add(new ContextMenuItemViewModel("Open Diagram", "CTRL+O", this.OpenCommand, MenuItemKind.Open));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var iteration in this.Diagrams)
            {
                iteration.Dispose();
            }
        }

        /// <summary>
        /// Logs the async exception
        /// </summary>
        /// <param name="ex">The exception</param>
        private void LogAsyncException(Exception ex)
        {
            logger.Error(ex.Message);
            this.Feedback = ex.Message;
        }

        /// <summary>
        /// Change the publication status of the 
        /// </summary>
        /// <param name="selectedThing">The selected diagram</param>
        /// <param name="state">The state to change to.</param>
        private async Task ChangePublicationStatus(DiagramCanvas selectedThing, PublicationState state)
        {
            if (selectedThing == null)
            {
                return;
            }

            var clone = selectedThing.Clone(false);

            clone.PublicationState = state;

            var transactionContext = TransactionContextResolver.ResolveContext(selectedThing);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.CreateOrUpdate(clone);

            try
            {
                this.IsBusy = true;
                var operationContainer = transaction.FinalizeTransaction();

                await this.Session.Write(operationContainer);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Publishes the diagram
        /// </summary>
        /// <param name="selectedThing">The selected diagram</param>
        private async Task ExecutePublishCommand(DiagramCanvas selectedThing)
        {
            if (selectedThing.PublicationState != PublicationState.ReadyForPublish)
            {
                return;
            }

            await this.ChangePublicationStatus(selectedThing, PublicationState.Published);
        }

        /// <summary>
        /// Marks the diagram ready for publication
        /// </summary>
        /// <param name="selectedThing">The selected diagram</param>
        private async Task ExecuteReadyForPublicationCommand(DiagramCanvas selectedThing)
        {
            if (selectedThing.PublicationState != PublicationState.Hidden)
            {
                return;
            }

            await this.ChangePublicationStatus(selectedThing, PublicationState.ReadyForPublish);
        }

        /// <summary>
        /// Hides the diagram
        /// </summary>
        /// <param name="selectedThing">The selected diagram</param>
        private async Task ExecuteHideCommand(DiagramCanvas selectedThing)
        {
            if (selectedThing.PublicationState == PublicationState.Hidden)
            {
                return;
            }

            await this.ChangePublicationStatus(selectedThing, PublicationState.Hidden);
        }

        /// <summary>
        /// Update the <see cref="Diagrams" /> List
        /// </summary>
        private void UpdateDiagrams()
        {
            var newDiagrams = this.Thing.DiagramCanvas.Except(this.Diagrams.Select(x => x.Thing)).ToList();
            var oldDiagrams = this.Diagrams.Select(x => x.Thing).Except(this.Thing.DiagramCanvas).ToList();

            foreach (var diagram in newDiagrams)
            {
                DiagramCanvasRowViewModel row;

                if (diagram is ArchitectureDiagram architectureDiagram)
                {
                    row = new ArchitectureDiagramRowViewModel(architectureDiagram, this.Session, this)
                    {
                        Index = this.Thing.DiagramCanvas.IndexOf(diagram)
                    };
                }
                else
                {
                    row = new DiagramCanvasRowViewModel(diagram, this.Session, this)
                    {
                        Index = this.Thing.DiagramCanvas.IndexOf(diagram)
                    };
                }

                this.Diagrams.Add(row);
            }

            foreach (var diagram in oldDiagrams)
            {
                var row = this.Diagrams.SingleOrDefault(x => x.Thing == diagram);

                if (row != null)
                {
                    this.Diagrams.Remove(row);
                }
            }

            // filter out the unpublished diagrams that are not owned
            foreach (var row in this.Diagrams.Where(d => d.Thing.PublicationState != PublicationState.Published).ToList())
            {
                if (!this.CanReadDiagram(row.Thing))
                {
                    this.Diagrams.Remove(row);
                }
            }

            this.Diagrams.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
        }

        /// <summary>
        /// Evaluates whether the diagram is visible to the user
        /// </summary>
        /// <param name="rowThing">The <see cref="DiagramCanvas"/> to evaluate</param>
        /// <returns>True if it should remain visible</returns>
        private bool CanReadDiagram(DiagramCanvas rowThing)
        {
            var ownedDiagram = rowThing as IOwnedThing;

            if (ownedDiagram is null)
            {
                // diagram is not owned. Can be read.
                return true;
            }

            var currentDomain = this.Session.QuerySelectedDomainOfExpertise(this.Thing);

            // Person is owner, can read
            if (ownedDiagram.Owner.Equals(currentDomain))
            {
                return true;
            }

            // Person is not owner, depends on access right kind
            return this.Session.PermissionService.CanWrite(rowThing);
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);

            var diagramSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DiagramCanvas))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateDiagrams());

            this.Disposables.Add(diagramSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);

            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null
                    ? "None"
                    : $"{iterationDomainPair.Value.Item1.Name} [{iterationDomainPair.Value.Item1.ShortName}]";
            }
        }

        /// <summary>
        /// Execute the <see cref="OpenCommand" /> on the <see cref="DiagramBrowserViewModel.SelectedThing" />
        /// </summary>
        private void ExecuteOpenCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var thing = this.SelectedThing.Thing;
            var vm = new DiagramEditorViewModel((DiagramCanvas)thing, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
            this.PanelNavigationService.OpenInDock(vm);
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
            logger.Trace("drag over {0}", dropInfo.TargetItem);
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            droptarget.DragOver(dropInfo);
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                return;
            }

            try
            {
                this.IsBusy = true;
                await droptarget.Drop(dropInfo);
            }
            catch (Exception ex)
            {
                this.Feedback = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}
