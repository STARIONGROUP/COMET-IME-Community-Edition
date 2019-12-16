// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypesBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.Services.FavoritesService;
    using CDP4Dal;
    using CDP4Dal.Events;
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
        /// Backing field for the <see cref="ParameterTypes"/> property
        /// </summary>
        private readonly ReactiveList<ParameterTypeRowViewModel> parameterTypes =
            new ReactiveList<ParameterTypeRowViewModel>();

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
        public ParameterTypesBrowserViewModel(ISession session, SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService,
            IFavoritesService favoritesService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.favoritesService = favoritesService;

            this.parameterTypes.ChangeTrackingEnabled = true;

            this.RefreshFavorites(
                this.favoritesService.GetFavoriteItemsCollectionByType(this.Session, typeof(ParameterType)));

            this.WhenAnyValue(vm => vm.ShowOnlyFavorites).Subscribe(_ => this.ExecuteToggleShowFavoriteCommand());

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="ParameterTypeRowViewModel"/> that are contained by this view-model
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> ParameterTypes
        {
            get { return this.parameterTypes; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ParameterType"/>
        /// </summary>
        public bool CanCreateParameterType
        {
            get { return this.canCreateParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateParameterType, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display favorites.
        /// </summary>
        public bool ShowOnlyFavorites
        {
            get { return this.showOnlyFavorites; }
            set { this.RaiseAndSetIfChanged(ref this.showOnlyFavorites, value); }
        }

        /// <summary>
        /// Gets the list of Iids that correspond to favorite <see cref="ParameterType"/>.
        /// </summary>
        public HashSet<Guid> FavoriteParameterTypeIids { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="TextParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateTextParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="BooleanParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateBooleanParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DateParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateDateParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="EnumerationParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateEnumerationParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="TimeOfDayParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateTimeOfDayParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DateTimeParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateDateTimeParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SimpleQuantityKind"/>
        /// </summary>
        public ReactiveCommand<object> CreateSimpleQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="SpecializedQuantityKind"/>
        /// </summary>
        public ReactiveCommand<object> CreateSpecializedQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DerivedQuantityKind"/>
        /// </summary>
        public ReactiveCommand<object> CreateDerivedQuantityKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="CompoundParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateCompoundParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ArrayParameterType"/>
        /// </summary>
        public ReactiveCommand<object> CreateArrayParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to toggle the state of <see cref="IsFavorite"/> for a perticulat row.
        /// </summary>
        public ReactiveCommand<object> ToggleFavoriteCommand { get; private set; }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddParameterTypeRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed &&
                                           objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveParameterTypeRowViewModel);
            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
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

            this.CreateTextParameterType = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateTextParameterType.Subscribe(_ => this.ExecuteCreateCommand<TextParameterType>());

            this.CreateBooleanParameterType =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateBooleanParameterType.Subscribe(_ => this.ExecuteCreateCommand<BooleanParameterType>());

            this.CreateDateParameterType = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateDateParameterType.Subscribe(_ => this.ExecuteCreateCommand<DateParameterType>());

            this.CreateDateTimeParameterType =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateDateTimeParameterType.Subscribe(_ => this.ExecuteCreateCommand<DateTimeParameterType>());

            this.CreateEnumerationParameterType =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateEnumerationParameterType.Subscribe(_ => this.ExecuteCreateCommand<EnumerationParameterType>());

            this.CreateTimeOfDayParameterType =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateTimeOfDayParameterType.Subscribe(_ => this.ExecuteCreateCommand<TimeOfDayParameterType>());

            this.CreateSimpleQuantityKind = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateSimpleQuantityKind.Subscribe(_ => this.ExecuteCreateCommand<SimpleQuantityKind>());

            this.CreateSpecializedQuantityKind =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateSpecializedQuantityKind.Subscribe(_ => this.ExecuteCreateCommand<SpecializedQuantityKind>());

            this.CreateDerivedQuantityKind = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateDerivedQuantityKind.Subscribe(_ => this.ExecuteCreateCommand<DerivedQuantityKind>());

            this.CreateCompoundParameterType =
                ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateCompoundParameterType.Subscribe(_ => this.ExecuteCreateCommand<CompoundParameterType>());

            this.CreateArrayParameterType = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterType));
            this.CreateArrayParameterType.Subscribe(_ => this.ExecuteCreateCommand<ArrayParameterType>());

            this.ToggleFavoriteCommand = ReactiveCommand.Create();
            this.ToggleFavoriteCommand.Subscribe(_ => this.ExecuteToggleFavoriteCommand());
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
                this.ContextMenu.Add(new ContextMenuItemViewModel(
                    !selectedParameterTypeRow.IsFavorite ? "Add to Favorites" : "Remove from Favorites", "",
                    this.ToggleFavoriteCommand, MenuItemKind.Favorite));
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Array Parameter Type", "",
                this.CreateArrayParameterType, MenuItemKind.Create, ClassKind.ArrayParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Boolean Parameter Type", "",
                this.CreateBooleanParameterType, MenuItemKind.Create, ClassKind.BooleanParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Compound Parameter Type", "",
                this.CreateCompoundParameterType, MenuItemKind.Create, ClassKind.CompoundParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Date Parameter Type", "",
                this.CreateDateParameterType, MenuItemKind.Create, ClassKind.DateParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Date Time Parameter Type", "",
                this.CreateDateTimeParameterType, MenuItemKind.Create, ClassKind.DateTimeParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Derived Quantity Kind", "",
                this.CreateDerivedQuantityKind, MenuItemKind.Create, ClassKind.DerivedQuantityKind));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Enumeration Parameter Type", "",
                this.CreateEnumerationParameterType, MenuItemKind.Create, ClassKind.EnumerationParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Simple Quantity Kind", "",
                this.CreateSimpleQuantityKind, MenuItemKind.Create, ClassKind.SimpleQuantityKind));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Specialized Quantity Kind", "",
                this.CreateSpecializedQuantityKind, MenuItemKind.Create, ClassKind.SpecializedQuantityKind));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Text Parameter Type", "",
                this.CreateTextParameterType, MenuItemKind.Create, ClassKind.TextParameterType));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Time of Day Parameter Type", "",
                this.CreateTimeOfDayParameterType, MenuItemKind.Create, ClassKind.TimeOfDayParameterType));
        }

        /// <summary>
        /// Toggles showing only the the favorite items.
        /// </summary>
        private void ExecuteToggleShowFavoriteCommand()
        {
            FilterStringService.FilterString.RefreshFavoriteBrowser(this);
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
                parameterTypeRowViewModel.SetFavoriteStatus(
                    this.FavoriteParameterTypeIids.Contains(parameterTypeRowViewModel.Thing.Iid));
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
                this.ParameterTypes.Remove(row);
                row.Dispose();
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