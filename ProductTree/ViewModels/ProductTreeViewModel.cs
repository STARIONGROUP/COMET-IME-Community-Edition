// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ProductTree.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The view model for the Product Tree
    /// </summary>
    public class ProductTreeViewModel : BrowserViewModelBase<Option>, IPanelViewModel, IDropTarget, IHaveMessageBusHandler
    {
        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="CurrentOption"/>
        /// </summary>
        private string currentOption;

        /// <summary>
        /// Backing field for <see cref="IsDisplayShortNamesOn"/>
        /// </summary>
        private bool isDisplayShortNamesOn;

        /// <summary>
        /// The active <see cref="Participant"/>
        /// </summary>
        public readonly Participant ActiveParticipant;

        /// <summary>
        /// The <see cref="EngineeringModelSetup"/> that is referenced by the <see cref="EngineeringModel"/> that contains the current <see cref="Option"/>
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup"/> that is referenced by the container <see cref="Iteration"/> of the current <see cref="Option"/>.
        /// </summary>
        private readonly IterationSetup iterationSetup;

        /// <summary>
        /// Backing field for <see cref="CanCreateSubscription"/>
        /// </summary>
        private bool canCreateSubscription;

        /// <summary>
        /// Backing field for <see cref="CanDeleteSubscription"/>
        /// </summary>
        private bool canDeleteSubscription;

        /// <summary>
        /// Backing field for the <see cref="CanCreateParameterGroup"/> property
        /// </summary>
        private bool canCreateParameterGroup;

        /// <summary>
        /// Backing field for <see cref="CanCreateOverride"/>
        /// </summary>
        private bool canCreateOverride;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Product Tree";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTreeViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> of which this browser is of</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public ProductTreeViewModel(Option option, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(option, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();

            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.TopElement = new DisposableReactiveList<ElementDefinitionRowViewModel>();
            var model = (EngineeringModel)this.Thing.TopContainer;
            this.modelSetup = model.EngineeringModelSetup;

            var iteration = (Iteration)this.Thing.Container;
            this.iterationSetup = iteration.IterationSetup;

            this.ActiveParticipant = this.modelSetup.Participant.Single(x => x.Person == this.Session.ActivePerson);

            var iterationSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(iteration)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.SetTopElement);

            this.Disposables.Add(iterationSubscription);

            this.ExecuteLongRunningDispatcherAction(
                () =>
                {
                    this.AddSubscriptions();
                    this.SetTopElement(iteration);
                    this.UpdateProperties();
                    stopWatch.Stop();
                    logger.Info("The Product Tree loaded in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));
                },
                $"Loading {this.Caption}");
        }

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
        /// Gets the current option caption to be displayed in the browser
        /// </summary>
        public string CurrentOption
        {
            get => this.currentOption;
            private set => this.RaiseAndSetIfChanged(ref this.currentOption, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="ParameterSubscription"/> can be created
        /// </summary>
        public bool CanCreateSubscription
        {
            get => this.canCreateSubscription;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateSubscription, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="ParameterSubscription"/> can be created
        /// </summary>
        public bool CanDeleteSubscription
        {
            get => this.canDeleteSubscription;
            private set => this.RaiseAndSetIfChanged(ref this.canDeleteSubscription, value);
        }

        /// <summary>
        /// Gets a value indicating whether ShortNames of <see cref="ElementUsage"/>s are displayed instead of Names
        /// </summary>
        public bool IsDisplayShortNamesOn
        {
            get => this.isDisplayShortNamesOn;
            private set => this.RaiseAndSetIfChanged(ref this.isDisplayShortNamesOn, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ParameterGroup"/>
        /// </summary>
        public bool CanCreateParameterGroup
        {
            get => this.canCreateParameterGroup;
            set => this.RaiseAndSetIfChanged(ref this.canCreateParameterGroup, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to create a <see cref="ParameterSubscription"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to Copy Model Code to clipboard <see cref="ParameterRowViewModel"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyModelCodeToClipboardCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to Copy Path to clipboard
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyPathToClipboardCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to delete a <see cref="ParameterSubscription"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to toggle <see cref="ElementUsage"/>s between Name and ShortName
        /// </summary>
        public ReactiveCommand<Unit, Unit> ToggleUsageNamesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ParameterGroup"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterGroup { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the create override command shall be enabled
        /// </summary>
        public bool CanCreateOverride
        {
            get => this.canCreateOverride;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateOverride, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to create a <see cref="ParameterOverride"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateOverrideCommand { get; private set; }

        /// <summary>
        /// Gets the Top <see cref="ElementDefinition"/> for this <see cref="Option"/>
        /// </summary>
        /// <remarks>
        /// This has to be a list in order to display the tree
        /// </remarks>
        public DisposableReactiveList<ElementDefinitionRowViewModel> TopElement { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets the <see cref="MessageBusHandler"/>
        /// </summary>
        public new MessageBusHandler MessageBusHandler { get; } = new MessageBusHandler();

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
            try
            {
                logger.Trace("drag over {0}", dropInfo.TargetItem);
                var droptarget = dropInfo.TargetItem as IDropTarget;

                if (droptarget != null)
                {
                    droptarget.DragOver(dropInfo);
                    return;
                }

                dropInfo.Effects = DragDropEffects.None;
            }
            catch (Exception ex)
            {
                dropInfo.Effects = DragDropEffects.None;
                logger.Error(ex, "drag-over caused an error");
                throw;
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
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget != null)
            {
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

        /// <summary>
        /// Initialize the <see cref="ReactiveCommandCreator"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateSubscriptionCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteCreateSubscriptionCommand, this.WhenAnyValue(x => x.CanCreateSubscription));
            this.CopyModelCodeToClipboardCommand = ReactiveCommandCreator.Create(this.ExecuteCopyModelCodeToClipboardCommand);
            this.CopyPathToClipboardCommand = ReactiveCommandCreator.Create(this.ExecuteCopyPathToClipboardCommand);

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            this.DeleteSubscriptionCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteDeleteCommand(
                    ((ParameterOrOverrideBase)this.SelectedThing.Thing).ParameterSubscription
                    .Single(s => tuple != null && s.Owner.Equals(tuple.Item1))),
                this.WhenAnyValue(x => x.CanDeleteSubscription));

            this.CreateParameterGroup = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateCommand<ParameterGroup>(this.SelectedThing.Thing.GetContainerOfType<ElementDefinition>()),
                this.WhenAnyValue(vm => vm.CanCreateParameterGroup));

            this.CreateOverrideCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteCreateParameterOverride, this.WhenAnyValue(vm => vm.CanCreateOverride));

            this.ToggleUsageNamesCommand = ReactiveCommandCreator.Create(this.ToggleDisplayShortNamesOn);
        }

        /// <summary>
        /// Handles the <see cref="DomainChangedEvent"/>
        /// </summary>
        /// <param name="domainChangeEvent">The <see cref="DomainChangedEvent"/></param>
        protected override void UpdateDomain(DomainChangedEvent domainChangeEvent)
        {
            base.UpdateDomain(domainChangeEvent);
            this.TopElement.ClearAndDispose();
            this.SetTopElement(this.Thing.Container as Iteration);
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
            this.CurrentOption = this.Thing.Name;
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            this.CanCreateParameterGroup = this.PermissionService.CanWrite(ClassKind.ParameterGroup, this.Thing);

            var parameterOrOverrideRow = this.SelectedThing as ParameterOrOverrideBaseRowViewModel;

            if (parameterOrOverrideRow == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            if (tuple != null)
            {
                var activeDomain = tuple.Item1;

                this.CanCreateSubscription = parameterOrOverrideRow.Thing.ParameterSubscription.All(ps => ps.Owner != activeDomain)
                                             && parameterOrOverrideRow.Thing.Owner != activeDomain
                                             && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, parameterOrOverrideRow.Thing);

                this.CanDeleteSubscription = parameterOrOverrideRow.Thing.ParameterSubscription.Any(ps => ps.Owner == activeDomain)
                                             && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, parameterOrOverrideRow.Thing);

                this.CanCreateOverride = parameterOrOverrideRow.Thing is Parameter
                                         && (parameterOrOverrideRow.Thing.Owner == activeDomain || ((Parameter)parameterOrOverrideRow.Thing).AllowDifferentOwnerOfOverride)
                                         && !this.TopElement.Contains(parameterOrOverrideRow.ContainerViewModel)
                                         && this.PermissionService.CanWrite(ClassKind.ParameterOverride, this.SelectedThing.ContainerViewModel.Thing);
            }
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is ElementDefinitionRowViewModel || this.SelectedThing is ParameterGroupRowViewModel)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Parameter Group", "", this.CreateParameterGroup, MenuItemKind.Create, ClassKind.ParameterGroup));
            }

            if (this.SelectedThing is IHavePath)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Copy Model Code to Clipboard", "", this.CopyModelCodeToClipboardCommand, MenuItemKind.None, ClassKind.NotThing));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Copy Path to Clipboard", "", this.CopyPathToClipboardCommand, MenuItemKind.None, ClassKind.NotThing));
            }

            var parameterOrOverrideRow = this.SelectedThing as ParameterOrOverrideBaseRowViewModel;

            if (parameterOrOverrideRow == null)
            {
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Subscription", "", this.CreateSubscriptionCommand, MenuItemKind.Create, ClassKind.ParameterSubscription));

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            if (tuple != null)
            {
                var owner = tuple.Item1;

                if (parameterOrOverrideRow.Thing.ParameterSubscription.Any(s => s.Owner.Equals(owner)))
                {
                    this.ContextMenu.Add(new ContextMenuItemViewModel("Delete Subscription", "", this.DeleteSubscriptionCommand, MenuItemKind.Delete, ClassKind.ParameterSubscription));
                }
            }

            var parameter = parameterOrOverrideRow.Thing as Parameter;

            if (parameter != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Override this Parameter", "", this.CreateOverrideCommand, MenuItemKind.Create, ClassKind.ParameterOverride));
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
            var topElement = this.TopElement.SingleOrDefault();

            if (topElement == null)
            {
                return;
            }

            topElement.Dispose();

            this.MessageBusHandler.Dispose();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.modelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.iterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Sets the top element for this product tree
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> associated to this <see cref="Option"/></param>
        private void SetTopElement(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var existingTopElement = this.TopElement.SingleOrDefault();
            var topElement = iteration.TopElement;

            if (topElement == null && existingTopElement != null)
            {
                this.TopElement.ClearAndDispose();
            }
            else if (topElement != null && (existingTopElement == null || existingTopElement.Thing != topElement))
            {
                if (existingTopElement != null)
                {
                    this.TopElement.ClearAndDispose();
                }

                var row = new ElementDefinitionRowViewModel(iteration.TopElement, this.Thing, this.Session, this);
                this.TopElement.Add(row);
            }
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.modelSetup.Name;
            this.CurrentIteration = this.iterationSetup.IterationNumber;
            this.CurrentOption = this.Thing.Name;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Thing.Container);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
        }

        /// <summary>
        /// Execute the <see cref="CopyModelCodeToClipboardCommand"/>
        /// </summary>
        private void ExecuteCopyModelCodeToClipboardCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is IHavePath havePath)
            {
                Clipboard.SetText(havePath.ModelCode);
            }
        }

        /// <summary>
        /// Execute the <see cref="CopyPathToClipboardCommand"/>
        /// </summary>
        private void ExecuteCopyPathToClipboardCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is IHavePath pathRow)
            {
                Clipboard.SetText(pathRow.GetPath());
            }
        }

        /// <summary>
        /// Executes the <see cref="CreateSubscriptionCommand"/>
        /// </summary>
        private async Task ExecuteCreateSubscriptionCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var parameterOrOverrideRow = this.SelectedThing as ParameterOrOverrideBaseRowViewModel;

            if (parameterOrOverrideRow == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            if (tuple != null)
            {
                var subscription = new ParameterSubscription
                {
                    Owner = tuple.Item1
                };

                var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(transactionContext);

                var clone = parameterOrOverrideRow.Thing.Clone(false);
                transaction.Create(subscription);
                transaction.CreateOrUpdate(clone);
                clone.ParameterSubscription.Add(subscription);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Execute the <see cref="CreateOverrideCommand"/>
        /// </summary>
        private async Task ExecuteCreateParameterOverride()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var elementUsage = this.SelectedThing.ContainerViewModel.Thing as ElementUsage;

            if (elementUsage == null)
            {
                return;
            }

            var parameter = this.SelectedThing.Thing as Parameter;

            if (parameter == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            if (tuple != null)
            {
                var parameterOverride = new ParameterOverride
                {
                    Parameter = parameter,
                    Owner = tuple.Item1
                };

                var transactionContext = TransactionContextResolver.ResolveContext(elementUsage);
                var transaction = new ThingTransaction(transactionContext);

                transaction.Create(parameterOverride);

                var clone = elementUsage.Clone(false);
                transaction.CreateOrUpdate(clone);
                clone.ParameterOverride.Add(parameterOverride);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Execute the <see cref="ToggleUsageNamesCommand"/>
        /// </summary>
        private void ToggleDisplayShortNamesOn()
        {
            this.IsDisplayShortNamesOn = !this.IsDisplayShortNamesOn;
        }
    }
}
