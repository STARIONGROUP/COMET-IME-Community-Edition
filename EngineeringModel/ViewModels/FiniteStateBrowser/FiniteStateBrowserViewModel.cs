﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels.Dialogs;
    using CDP4EngineeringModel.Views;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="FiniteStateBrowser"/> view
    /// </summary>
    public class FiniteStateBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The <see cref="IMessageBoxService"/> used to show user messages.
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Finite States";

        /// <summary>
        /// The Actual List Short Name
        /// </summary>
        private const string ActualListShortName = "Actual List";

        /// <summary>
        /// The Possible List Short Name
        /// </summary>
        private const string PossibleListShortName = "Possible List";

        /// <summary>
        /// The intermediate folder containing <see cref="PossibleFiniteStateList"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel possibleFiniteStateListFolder;

        /// <summary>
        /// The intermediate folder containing <see cref="ActualFiniteStateList"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel actualFiniteStateListFolder;

        /// <summary>
        /// Backing field for <see cref="CanCreatePossibleState"/>
        /// </summary>
        private bool canCreatePossibleState;

        /// <summary>
        /// Backing field for <see cref="CanExecuteCreatePossibleFiniteStateListCommand"/>
        /// </summary>
        private bool canExecuteCreatePossibleFiniteStateListCommand;

        /// <summary>
        /// Backing field for <see cref="CanExecuteCreateActualFiniteStateListCommand"/>
        /// </summary>
        private bool canExecuteCreateActualFiniteStateListCommand;

        /// <summary>
        /// Backing field for <see cref="CanUpdateBatchParameters"/>
        /// </summary>
        private bool canUpdateBatchParameters;

        /// <summary>
        /// Backing field for <see cref="CanSetAsDefault"/>
        /// </summary>
        private bool canSetAsDefault;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The <see cref="IParameterActualFiniteStateListApplicationBatchService"/> used to update multiple <see cref="Parameter"/>s to set the <see cref="ActualFiniteStateList"/> in a batch operation
        /// </summary>
        private readonly IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteStateBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The iteration.
        /// </param>
        /// <param name="session">
        /// The session
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// the <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// the <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/>
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="parameterActualFiniteStateListApplicationBatchService">
        /// The <see cref="IParameterActualFiniteStateListApplicationBatchService"/> used to update multiple <see cref="Parameter"/>s to set the <see cref="ActualFiniteStateList"/> in a batch operation
        /// </param>
        public FiniteStateBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.parameterActualFiniteStateListApplicationBatchService = parameterActualFiniteStateListApplicationBatchService;

            this.UpdatePossibleFiniteStatesList();
            this.UpdateActualFiniteStatesList();
            this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
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
        /// Gets the rows containing the finite state lists
        /// </summary>
        public DisposableReactiveList<CDP4Composition.FolderRowViewModel> FiniteStateList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="CreatePossibleFiniteStateListCommand"/> can be executed
        /// </summary>
        public bool CanExecuteCreatePossibleFiniteStateListCommand
        {
            get => this.canExecuteCreatePossibleFiniteStateListCommand;
            set => this.RaiseAndSetIfChanged(ref this.canExecuteCreatePossibleFiniteStateListCommand, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="CreateActualFiniteStateListCommand"/> can be executed
        /// </summary>
        public bool CanExecuteCreateActualFiniteStateListCommand
        {
            get => this.canExecuteCreateActualFiniteStateListCommand;
            set => this.RaiseAndSetIfChanged(ref this.canExecuteCreateActualFiniteStateListCommand, value);
        }

        /// <summary>
        /// Gets a value indicating whether the set as default <see cref="ICommand"/> can be executed
        /// </summary>
        public bool CanSetAsDefault
        {
            get => this.canSetAsDefault;
            private set => this.RaiseAndSetIfChanged(ref this.canSetAsDefault, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreatePossibleStateCommand"/> can be executed
        /// </summary>
        public bool CanCreatePossibleState
        {
            get => this.canCreatePossibleState;
            private set => this.RaiseAndSetIfChanged(ref this.canCreatePossibleState, value);
        }

        /// <summary>
        /// Gets a value indicating whether the batch update parameters command shall be enabled
        /// </summary>
        public bool CanUpdateBatchParameters
        {
            get => this.canUpdateBatchParameters;
            private set => this.RaiseAndSetIfChanged(ref this.canUpdateBatchParameters, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreatePossibleFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateActualFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to set a <see cref="PossibleFiniteState"/> as default
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetDefaultStateCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="PossibleFiniteState"/> 
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreatePossibleStateCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to update multiple <see cref="Parameter"/>s in batch operation mode
        /// </summary>
        public ReactiveCommand<Unit, Unit> BatchUpdateParameterCommand { get; private set; }

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
            this.FiniteStateList = new DisposableReactiveList<CDP4Composition.FolderRowViewModel>();
            this.possibleFiniteStateListFolder = new CDP4Composition.FolderRowViewModel(PossibleListShortName, "Possible Finite State List", this.Session, this);
            this.actualFiniteStateListFolder = new CDP4Composition.FolderRowViewModel(ActualListShortName, "Actual Finite State List", this.Session, this);
            this.FiniteStateList.Add(this.possibleFiniteStateListFolder);
            this.FiniteStateList.Add(this.actualFiniteStateListFolder);
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreatePossibleFiniteStateListCommand =
                ReactiveCommandCreator.Create(
                    () => this.ExecuteCreateCommand<PossibleFiniteStateList>(this.Thing),
                    this.WhenAnyValue(x => x.CanExecuteCreatePossibleFiniteStateListCommand));

            this.CreateActualFiniteStateListCommand =
                ReactiveCommandCreator.Create(
                    () => this.ExecuteCreateCommand<ActualFiniteStateList>(this.Thing),
                    this.WhenAnyValue(x => x.CanExecuteCreateActualFiniteStateListCommand));

            this.SetDefaultStateCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteSetDefaultCommand, this.WhenAnyValue(x => x.CanSetAsDefault));

            this.CreatePossibleStateCommand =
                ReactiveCommandCreator.Create(
                    () => this.ExecuteCreateCommand<PossibleFiniteState>(this.SelectedThing.Thing.GetContainerOfType<PossibleFiniteStateList>()),
                    this.WhenAnyValue(x => x.CanCreatePossibleState));

            this.BatchUpdateParameterCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteBatchUpdateParameterCommand, this.WhenAnyValue(x => x.CanUpdateBatchParameters));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdatePossibleFiniteStatesList();
            this.UpdateActualFiniteStatesList();
            this.UpdateProperties();
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            this.CanExecuteCreatePossibleFiniteStateListCommand = this.PermissionService.CanWrite(ClassKind.PossibleFiniteStateList, this.Thing);
            this.CanExecuteCreateActualFiniteStateListCommand = this.PermissionService.CanWrite(ClassKind.ActualFiniteStateList, this.Thing);

            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;

            if (possibleStateRow != null)
            {
                var possibleList = possibleStateRow.Thing.Container;
                this.CanSetAsDefault = this.PermissionService.CanWrite(possibleList) && !possibleStateRow.IsDefault;
                return;
            }

            var possibleListRow = this.SelectedThing as PossibleFiniteStateListRowViewModel;

            if (possibleListRow != null)
            {
                var possibleList = possibleListRow.Thing;
                this.CanCreatePossibleState = this.PermissionService.CanWrite(possibleList);
            }

            var actualFiniteStateListRowViewModel = this.SelectedThing as ActualFiniteStateListRowViewModel;

            if (actualFiniteStateListRowViewModel != null)
            {
                this.CanUpdateBatchParameters = this.PermissionService.CanWrite(ClassKind.Parameter, this.Thing);
            }

            this.CanSetAsDefault = false;
        }

        /// <summary>
        /// Populates the <see cref="ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing is CDP4Composition.FolderRowViewModel finiteRow)
            {
                switch (finiteRow.ShortName)
                {
                    case PossibleListShortName:
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State List", "", this.CreatePossibleFiniteStateListCommand, MenuItemKind.Create, ClassKind.PossibleFiniteStateList));
                        break;
                    case ActualListShortName:
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Actual Finite State List", "", this.CreateActualFiniteStateListCommand, MenuItemKind.Create, ClassKind.ActualFiniteStateList));
                        break;
                }
            }

            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;

            if (possibleStateRow != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State", "", this.CreatePossibleStateCommand, MenuItemKind.Create, ClassKind.PossibleFiniteState));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Set as default", "", this.SetDefaultStateCommand, MenuItemKind.Edit, ClassKind.PossibleFiniteStateList));
                return;
            }

            var possibleListRow = this.SelectedThing as PossibleFiniteStateListRowViewModel;

            if (possibleListRow != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State", "", this.CreatePossibleStateCommand, MenuItemKind.Create, ClassKind.PossibleFiniteState));
            }

            var actualFiniteStateListRowViewModel = this.SelectedThing as ActualFiniteStateListRowViewModel;

            if (actualFiniteStateListRowViewModel != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Apply Actual Finite State List to multiple Parameters", "", this.BatchUpdateParameterCommand, MenuItemKind.Edit, ClassKind.NotThing));
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

            foreach (var folderRowViewModel in this.FiniteStateList)
            {
                folderRowViewModel.Dispose();
            }
        }

        /// <summary>
        /// Update the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        private void UpdatePossibleFiniteStatesList()
        {
            var currentPossibleStateLists = this.possibleFiniteStateListFolder.ContainedRows.Select(x => (PossibleFiniteStateList)x.Thing).ToList();
            var updatedPossibleStateLists = this.Thing.PossibleFiniteStateList.ToList();

            var newPossibleStateList = updatedPossibleStateLists.Except(currentPossibleStateLists).ToList();
            var oldPossibleStateList = currentPossibleStateLists.Except(updatedPossibleStateLists).ToList();

            foreach (var statelist in newPossibleStateList)
            {
                var row = new PossibleFiniteStateListRowViewModel(statelist, this.Session, this);
                this.possibleFiniteStateListFolder.ContainedRows.Add(row);
            }

            foreach (var statelist in oldPossibleStateList)
            {
                var row = this.possibleFiniteStateListFolder.ContainedRows.SingleOrDefault(x => x.Thing == statelist);

                if (row != null)
                {
                    this.possibleFiniteStateListFolder.ContainedRows.RemoveAndDispose(row);
                }
            }
        }

        /// <summary>
        /// Update the <see cref="ActualFiniteStateList"/>
        /// </summary>
        private void UpdateActualFiniteStatesList()
        {
            var currentActualStateLists = this.actualFiniteStateListFolder.ContainedRows.Select(x => (ActualFiniteStateList)x.Thing).ToList();
            var updatedActualStateLists = this.Thing.ActualFiniteStateList.ToList();

            var newActualStateList = updatedActualStateLists.Except(currentActualStateLists).ToList();
            var oldActualStateList = currentActualStateLists.Except(updatedActualStateLists).ToList();

            foreach (var statelist in newActualStateList)
            {
                var row = new ActualFiniteStateListRowViewModel(statelist, this.Session, this);
                this.actualFiniteStateListFolder.ContainedRows.Add(row);
            }

            foreach (var statelist in oldActualStateList)
            {
                var row = this.actualFiniteStateListFolder.ContainedRows.SingleOrDefault(x => x.Thing == statelist);

                if (row != null)
                {
                    this.actualFiniteStateListFolder.ContainedRows.RemoveAndDispose(row);
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="SetDefaultCommand"/> command
        /// </summary>
        private async Task ExecuteSetDefaultCommand()
        {
            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;

            if (possibleStateRow == null)
            {
                return;
            }

            var list = (PossibleFiniteStateList)possibleStateRow.Thing.Container;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext);
            var clone = list.Clone(false);
            clone.DefaultState = possibleStateRow.Thing.Clone(false);

            transaction.CreateOrUpdate(clone);
            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Execute the <see cref="BatchUpdateParameterCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async Task ExecuteBatchUpdateParameterCommand()
        {
            var actualFiniteStateListRowViewModel = this.SelectedThing as ActualFiniteStateListRowViewModel;

            if (actualFiniteStateListRowViewModel == null)
            {
                return;
            }

            var model = (EngineeringModel)this.Thing.Container;

            var requiredRls = model.RequiredRdls;
            var allowedCategories = requiredRls.SelectMany(rdl => rdl.DefinedCategory).Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition));
            var allowedParameterTypes = requiredRls.SelectMany(rdl => rdl.ParameterType);

            var categoryDomainParameterTypeSelectorDialogViewModel = new CategoryDomainParameterTypeSelectorDialogViewModel(allowedParameterTypes, allowedCategories, model.EngineeringModelSetup.ActiveDomain);

            categoryDomainParameterTypeSelectorDialogViewModel.DialogTitle = "Apply ActualFiniteState to Parameters";

            var result = this.DialogNavigationService.NavigateModal(categoryDomainParameterTypeSelectorDialogViewModel) as CategoryDomainParameterTypeSelectorResult;

            if (result == null || !result.Result.HasValue || !result.Result.Value)
            {
                return;
            }

            try
            {
                this.IsBusy = true;

                await this.parameterActualFiniteStateListApplicationBatchService.Update(this.Session, this.Thing, actualFiniteStateListRowViewModel.Thing, result.IsUncategorizedIncluded, result.Categories, result.DomainOfExpertises, result.ParameterTypes);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when applying an ActualFiniteState to multiple Parameters in a batch operation");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
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

        /// <summary>
        /// Check if the Delete Command asociated to this ViewModel is allowed.
        /// </summary>
        /// <returns>True if the Delete Command is allowed, false otherwise</returns>
        protected override bool IsDeleteCommandAllowed()
        {
            var iterationElements = this.Thing.Element;
            var ParametersWithStateDependencies = 0;
            var FirstPhrase = "";

            switch (this.SelectedThing.Thing)
            {
                case ActualFiniteStateList actualFiniteStateList:

                    ParametersWithStateDependencies = iterationElements.SelectMany(elem => elem.Parameter.Where(param => param.StateDependence == actualFiniteStateList)).Count();
                    FirstPhrase = $"Deleting an {nameof(ActualFiniteStateList)} will delete ALL parameter values that may be dependent on this.";
                    break;

                case PossibleFiniteStateList possibleFiniteStateList:
                    ParametersWithStateDependencies = iterationElements.SelectMany(
                        elem =>
                            elem.Parameter.Where(
                                param =>
                                    param.StateDependence != null).SelectMany(
                                param =>
                                    param.StateDependence.PossibleFiniteStateList.Where(
                                        finiteStateList =>
                                            finiteStateList == possibleFiniteStateList))).Count();

                    FirstPhrase = $"Deleting a {nameof(PossibleFiniteStateList)} List will delete ALL parameter values that may be dependent on this through {nameof(ActualFiniteStateList)} that use it.";
                    break;

                case PossibleFiniteState possibleFiniteState:
                    ParametersWithStateDependencies = iterationElements.SelectMany(
                        elem =>
                            elem.Parameter.Where(
                                param =>
                                    param.StateDependence != null).SelectMany(
                                param =>
                                    param.StateDependence.PossibleFiniteStateList.SelectMany(
                                        finiteStateList =>
                                            finiteStateList.PossibleState.Where(
                                                finiteState =>
                                                    finiteState == possibleFiniteState)))).Count();

                    FirstPhrase = $"Deleting a {nameof(PossibleFiniteState)} will delete ALL parameter values that may be dependent on this through {nameof(ActualFiniteState)} that use it.";
                    break;
            }

            if (ParametersWithStateDependencies > 0)
            {
                var message = FirstPhrase +
                              "\r\n\r\nCare should be taken not to delete states and their dependent parameter values in the product tree inadvertently." +
                              "\r\n\r\nAre you sure you want to delete these?" +
                              $"\r\n\r\n{ParametersWithStateDependencies} parameter(s) will be affected by this deletion";

                if (this.messageBoxService.Show(message, "Deleting Finite State", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
