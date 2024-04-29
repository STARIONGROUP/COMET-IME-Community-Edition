// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypesBrowserViewModel.cs" company="Starion Group S.A.">
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.Services.FavoritesService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ParameterTypesBrowserViewModel"/> is to represent the view-model for <see cref="ParameterType"/>s
    /// </summary>
    public class ParameterTypesBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDropTarget,
        IFavoritesBrowserViewModel, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Parameter Types";

        /// <summary>
        /// Backing field for the <see cref="CanCreateParameterType"/> property
        /// </summary>
        private bool canCreateParameterType;

        /// <summary>
        /// The backing field for <see cref="ShowOnlyFavorites"/> property.
        /// </summary>
        private bool showOnlyFavorites;

        /// <summary>
        /// The <see cref="IFavoritesService"/> used to work with favorite Things.
        /// </summary>
        private readonly IFavoritesService favoritesService;

        /// <summary>
        /// The (injected) <see cref="IFilterStringService"/>
        /// </summary>
        private IFilterStringService filterStringService;

        /// <summary>
        /// Backing field for the <see cref="ParameterTypes"/> property
        /// </summary>
        private readonly DisposableReactiveList<ParameterTypeRowViewModel> parameterTypes =
            new DisposableReactiveList<ParameterTypeRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypesBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">The associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="favoritesService">The <see cref="IFavoritesService"/>.</param>
        public ParameterTypesBrowserViewModel(
            ISession session,
            SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService,
            IFavoritesService favoritesService)
            : base(
                siteDir,
                session,
                thingDialogNavigationService,
                panelNavigationService,
                dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.favoritesService = favoritesService;

            this.RefreshFavorites(this.favoritesService.GetFavoriteItemsCollectionByType(this.Session, typeof(ParameterType)));

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="ParameterTypeRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<ParameterTypeRowViewModel> ParameterTypes => this.parameterTypes;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ParameterType"/>
        /// </summary>
        public bool CanCreateParameterType
        {
            get => this.canCreateParameterType;
            set => this.RaiseAndSetIfChanged(ref this.canCreateParameterType, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display favorites.
        /// </summary>
        public bool ShowOnlyFavorites
        {
            get => this.showOnlyFavorites;
            set => this.RaiseAndSetIfChanged(ref this.showOnlyFavorites, value);
        }

        /// <summary>
        /// Gets the list of Iids that correspond to favorite <see cref="ParameterType"/>.
        /// </summary>
        public HashSet<Guid> FavoriteParameterTypeIids { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="TextParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateTextParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="BooleanParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateBooleanParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DateParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDateParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="EnumerationParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateEnumerationParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="TimeOfDayParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateTimeOfDayParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DateTimeParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDateTimeParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SimpleQuantityKind"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSimpleQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SpecializedQuantityKind"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSpecializedQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DerivedQuantityKind"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateDerivedQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CompoundParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateCompoundParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ArrayParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateArrayParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SampledFunctionParameterType"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSampledFunctionParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to toggle the state of <see cref="IsFavorite"/> for a perticulat row.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var favoritesToggleListener = this.WhenAnyValue(vm => vm.ShowOnlyFavorites).Subscribe(_ => this.ExecuteToggleShowFavoriteCommand());
            this.Disposables.Add(favoritesToggleListener);

            var addListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Added &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddParameterTypeRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Removed &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveParameterTypeRowViewModel);

            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Updated &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshContainerName);

            this.Disposables.Add(rdlUpdateListener);

            var favoritesListener =
                this.favoritesService.SubscribeToChanges(this.Session, typeof(ParameterType), this.RefreshFavorites);

            this.Disposables.Add(favoritesListener);
        }

        /// <summary>
        /// Initializes the create <see cref="ReactiveCommand"/> that allow a user to create the different kinds of <see cref="ParameterType"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateTextParameterType = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<TextParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateBooleanParameterType =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<BooleanParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateDateParameterType = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<DateParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateDateTimeParameterType =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<DateTimeParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateEnumerationParameterType =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<EnumerationParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateTimeOfDayParameterType =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<TimeOfDayParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateSimpleQuantityKind = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<SimpleQuantityKind>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateSpecializedQuantityKind =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<SpecializedQuantityKind>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateDerivedQuantityKind = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<DerivedQuantityKind>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateCompoundParameterType =
                ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<CompoundParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateArrayParameterType = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ArrayParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.CreateSampledFunctionParameterType = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<SampledFunctionParameterType>(), this.WhenAnyValue(vm => vm.CanCreateParameterType));

            this.ToggleFavoriteCommand = ReactiveCommandCreator.Create(this.ExecuteToggleFavoriteCommand);
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateParameterType = this.Session.OpenReferenceDataLibraries.Any();
        }

        /// <summary>
        /// Populate the <see cref="ContextMenuItemViewModel"/>s of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing is ParameterTypeRowViewModel selectedParameterTypeRow)
            {
                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        !selectedParameterTypeRow.IsFavorite ? "Add to Favorites" : "Remove from Favorites",
                        "",
                        this.ToggleFavoriteCommand,
                        MenuItemKind.Favorite));
            }

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create an Array Parameter Type",
                    "",
                    this.CreateArrayParameterType,
                    MenuItemKind.Create,
                    ClassKind.ArrayParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Boolean Parameter Type",
                    "",
                    this.CreateBooleanParameterType,
                    MenuItemKind.Create,
                    ClassKind.BooleanParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Compound Parameter Type",
                    "",
                    this.CreateCompoundParameterType,
                    MenuItemKind.Create,
                    ClassKind.CompoundParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Date Parameter Type",
                    "",
                    this.CreateDateParameterType,
                    MenuItemKind.Create,
                    ClassKind.DateParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Date Time Parameter Type",
                    "",
                    this.CreateDateTimeParameterType,
                    MenuItemKind.Create,
                    ClassKind.DateTimeParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Derived Quantity Kind",
                    "",
                    this.CreateDerivedQuantityKind,
                    MenuItemKind.Create,
                    ClassKind.DerivedQuantityKind));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create an Enumeration Parameter Type",
                    "",
                    this.CreateEnumerationParameterType,
                    MenuItemKind.Create,
                    ClassKind.EnumerationParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Sampled Function Parameter Type",
                    "",
                    this.CreateSampledFunctionParameterType,
                    MenuItemKind.Create,
                    ClassKind.SampledFunctionParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Simple Quantity Kind",
                    "",
                    this.CreateSimpleQuantityKind,
                    MenuItemKind.Create,
                    ClassKind.SimpleQuantityKind));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Specialized Quantity Kind",
                    "",
                    this.CreateSpecializedQuantityKind,
                    MenuItemKind.Create,
                    ClassKind.SpecializedQuantityKind));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Text Parameter Type",
                    "",
                    this.CreateTextParameterType,
                    MenuItemKind.Create,
                    ClassKind.TextParameterType));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Time of Day Parameter Type",
                    "",
                    this.CreateTimeOfDayParameterType,
                    MenuItemKind.Create,
                    ClassKind.TimeOfDayParameterType));
        }

        /// <summary>
        /// Toggles showing only the the favorite items.
        /// </summary>
        private void ExecuteToggleShowFavoriteCommand()
        {
            if (this.filterStringService == null)
            {
                this.filterStringService = ServiceLocator.Current.GetInstance<IFilterStringService>();
            }

            this.filterStringService.RefreshFavoriteBrowser(this);
        }

        /// <summary>
        /// Refreshes the list of favorite <see cref="ParameterType"/>
        /// </summary>
        private void RefreshFavorites(HashSet<Guid> iidList)
        {
            this.FavoriteParameterTypeIids = iidList;

            // update row status
            foreach (var parameterTypeRowViewModel in this.ParameterTypes)
            {
                parameterTypeRowViewModel.SetFavoriteStatus(this.FavoriteParameterTypeIids.Contains(parameterTypeRowViewModel.Thing.Iid));
            }
        }

        /// <summary>
        /// Execute the <see cref="ToggleFavoriteCommand"/>
        /// </summary>
        private void ExecuteToggleFavoriteCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is ParameterTypeRowViewModel row)
            {
                this.ToggleFavorite(row);
            }
        }

        /// <summary>
        /// Toggles the favorite status of the thing on the server.
        /// </summary>
        /// <param name="row">The row.</param>
        private void ToggleFavorite(ParameterTypeRowViewModel row)
        {
            Task.Run(() => this.favoritesService.ToggleFavorite<ParameterType>(this.Session, row.Thing));
        }

        /// <summary>
        /// Adds a <see cref="ParameterTypeRowViewModel"/>
        /// </summary>
        /// <param name="parameterType">
        /// The associated <see cref="ParameterType"/> for which the row is to be added.
        /// </param>
        private void AddParameterTypeRowViewModel(ParameterType parameterType)
        {
            var row = new ParameterTypeRowViewModel(parameterType, this.Session, this);

            if (this.FavoriteParameterTypeIids != null)
            {
                row.SetFavoriteStatus(this.FavoriteParameterTypeIids.Contains(row.Thing.Iid));
            }

            this.ParameterTypes.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="ParameterTypeRowViewModel"/> from the view model
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> for which the row view model has to be removed
        /// </param>
        private void RemoveParameterTypeRowViewModel(ParameterType parameterType)
        {
            var row = this.ParameterTypes.SingleOrDefault(rowViewModel => rowViewModel.Thing == parameterType);

            if (row != null)
            {
                this.ParameterTypes.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary"/>.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            foreach (var parameter in this.parameterTypes)
            {
                if (parameter.Thing.Container != rdl)
                {
                    continue;
                }

                if (parameter.ContainerRdl != rdl.ShortName)
                {
                    parameter.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var parameterType in referenceDataLibrary.ParameterType)
                {
                    this.AddParameterTypeRowViewModel(parameterType);
                }
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

            foreach (var type in this.ParameterTypes)
            {
                type.Dispose();
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
