// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

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

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.Views;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="OptionBrowser"/> view
    /// </summary>
    public class OptionBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The row associated to the default <see cref="Option"/>
        /// </summary>
        private OptionRowViewModel defaultOptionRow;

        /// <summary>
        /// Backing field for <see cref="CanToggleDefaultOption"/>
        /// </summary>
        private bool canToggleDefaultOption;

        /// <summary>
        /// Backing field for <see cref="CanCreateOption"/>
        /// </summary>
        private bool canCreateOption;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Options";

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public OptionBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.UpdateOptions();

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
        /// Gets or sets a value indicating whether the set default option command is enabled
        /// </summary>
        public bool CanToggleDefaultOption
        {
            get => this.canToggleDefaultOption;
            set => this.RaiseAndSetIfChanged(ref this.canToggleDefaultOption, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the create option command is enabled
        /// </summary>
        public bool CanCreateOption
        {
            get => this.canCreateOption;
            set => this.RaiseAndSetIfChanged(ref this.canCreateOption, value);
        }

        /// <summary>
        /// Gets a <see cref="ICommand"/> to set the current option as the default one
        /// </summary>
        public ReactiveCommand<object> ToggleDefaultCommand { get; private set; }

        /// <summary>
        /// Gets the rows representing <see cref="Option"/>s
        /// </summary>
        public DisposableReactiveList<OptionRowViewModel> Options { get; private set; }

        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Options = new DisposableReactiveList<OptionRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateOption));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Option>(this.Thing));

            this.ToggleDefaultCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanToggleDefaultOption));
            this.ToggleDefaultCommand.Subscribe(_ => this.ExecuteToggleDefaultCommand());
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateOptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Compute the permissions to create an <see cref="Option"/>
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.CurrentEngineeringModelSetup.Kind == EngineeringModelKind.MODEL_CATALOGUE && this.Thing.Option.Count > 0)
            {
                this.CanCreateOption = false;
            }
            else
            {
                this.CanCreateOption = this.PermissionService.CanWrite(ClassKind.Option, this.Thing);
            }

            if (!(this.SelectedThing is OptionRowViewModel))
            {
                this.CanToggleDefaultOption = false;
                return;
            }

            this.CanToggleDefaultOption = this.PermissionService.CanWrite(this.Thing);
        }

        /// <summary>
        /// Populates the <see cref="OptionBrowserViewModel.ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Option", "", this.CreateCommand, MenuItemKind.Create, ClassKind.Option));

            if (this.SelectedThing != null)
            {
                var setOrUnsetText = this.SelectedThing == this.defaultOptionRow ? "Unset" : "Set";

                this.ContextMenu.Add(new ContextMenuItemViewModel($"{setOrUnsetText} as Default", "", this.ToggleDefaultCommand, MenuItemKind.Edit, ClassKind.Iteration));
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

            foreach (var iteration in this.Options)
            {
                iteration.Dispose();
            }
        }

        /// <summary>
        /// Update the <see cref="Options"/> List
        /// </summary>
        private void UpdateOptions()
        {
            var newOptions = this.Thing.Option.Except(this.Options.Select(x => x.Thing)).ToList();
            var oldOptions = this.Options.Select(x => x.Thing).Except(this.Thing.Option).ToList();

            foreach (var option in newOptions)
            {
                var row = new OptionRowViewModel(option, this.Session, this);
                row.Index = this.Thing.Option.IndexOf(option);

                this.Options.Add(row);
            }

            foreach (var option in oldOptions)
            {
                var row = this.Options.SingleOrDefault(x => x.Thing == option);

                if (row != null)
                {
                    this.Options.RemoveAndDispose(row);
                }
            }

            this.Options.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
            this.UpdateDefaultOption();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
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

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);

            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = iterationDomainPair.Value?.Item1 == null
                    ? "None"
                    : $"{iterationDomainPair.Value.Item1.Name} [{iterationDomainPair.Value.Item1.ShortName}]";
            }
        }

        /// <summary>
        /// Set the default <see cref="Option"/>
        /// </summary>
        private void UpdateDefaultOption()
        {
            var defaultOption = this.Thing.DefaultOption;

            if (this.defaultOptionRow != null)
            {
                this.defaultOptionRow.IsDefaultOption = false;
            }

            if (defaultOption == null)
            {
                this.defaultOptionRow = null;
                return;
            }

            var row = this.Options.SingleOrDefault(x => x.Thing == defaultOption);

            if (row != null)
            {
                this.defaultOptionRow = row;
                row.IsDefaultOption = true;
            }
        }

        /// <summary>
        /// Executes the <see cref="ToggleDefaultCommand"/>
        /// </summary>
        private async Task ExecuteToggleDefaultCommand()
        {
            if (!(this.SelectedThing?.Thing is Option option))
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext);
            var clone = this.Thing.Clone(false);

            clone.DefaultOption = option.IsDefault ? null : option;

            transaction.CreateOrUpdate(clone);
            await this.DalWrite(transaction);
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
