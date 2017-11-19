// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.Poco;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// Represents the view-model of the <see cref="CategoryBrowser"/> view
    /// </summary>
    public class CategoryBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Categories Browser";

        /// <summary>
        /// Backing field for the <see cref="Categories"/> property
        /// </summary>
        private readonly ReactiveList<CategoryRowViewModel> categories = new ReactiveList<CategoryRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="thing">The <see cref="SiteDirectory"/> associated</param>
        /// <param name="thingDialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog</param>
        public CategoryBrowserViewModel(ISession session, SiteDirectory thing, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            this.Caption = string.Format("{0}, {1}", PanelCaption, this.Thing.Name);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.AddSubscriptions();            
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the rows representing <see cref="Category"/>
        /// </summary>
        public ReactiveList<CategoryRowViewModel> Categories
        {
            get
            {
                return this.categories;
            }
        }

        /// <summary>
        /// Gets or sets the Highlight Command
        /// </summary>
        public ReactiveCommand<object> HighlightCommand { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a category may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get { return this.canCreateRdlElement; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value); }
        }
        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var category in this.Categories)
            {
                category.Dispose();
            }
        }
        
        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);
            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var category in referenceDataLibrary.DefinedCategory)
                {
                    this.AddCategoryRowViewModel(category);
                }
            }
        }

        /// <summary>
        /// Initialize the <see cref="ICommand{T}"/>s
        /// </summary>       
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canSelectableCommandsExecute =
                this.WhenAnyValue(x => x.SelectedThing).Select(x => x != null && !(x.Thing is NotThing));

            this.HighlightCommand = ReactiveCommand.Create(canSelectableCommandsExecute);
            this.HighlightCommand.Subscribe(_ => this.ExecuteHighlightCommand());

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Category>());
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateRdlElement = this.Session.OpenReferenceDataLibraries.Any();
        }

        /// <summary>
        /// Populate the context menu for this browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Category", "", this.CreateCommand, MenuItemKind.Create, ClassKind.Category));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Highlight", "", this.HighlightCommand, MenuItemKind.Highlight));
        }

        #region Private Methods
        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Category))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Category)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddCategoryRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Category))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Category)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveCategoryRowViewModel);
            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshContainerName);
            this.Disposables.Add(rdlUpdateListener);
        }

        /// <summary>
        /// Adds a <see cref="CategoryRowViewModel"/>
        /// </summary>
        /// <param name="category">The associated <see cref="Category"/></param>
        private void AddCategoryRowViewModel(Category category)
        {
            if (this.Categories.Any(x => x.Thing == category))
            {
                return;
            }

            var row = new CategoryRowViewModel(category, this.Session, this);
            this.Categories.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="CategoryRowViewModel"/> from the view model
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> for which the row view model has to be removed
        /// </param>
        private void RemoveCategoryRowViewModel(Category category)
        {
            var row = this.Categories.SingleOrDefault(rowViewModel => rowViewModel.Thing == category);
            if (row != null)
            {
                this.Categories.Remove(row);
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
            foreach (var categoryRowViewModel in this.Categories)
            {
                if (categoryRowViewModel.Thing.Container != rdl)
                {
                    continue;
                }

                if (categoryRowViewModel.ContainerRdl != rdl.ShortName)
                {
                    categoryRowViewModel.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="HighlightCommand"/>
        /// </summary>
        private void ExecuteHighlightCommand()
        {
            // clear all highlights
            CDPMessageBus.Current.SendMessage(new CancelHighlightEvent());

            // highlight the selected thing
            CDPMessageBus.Current.SendMessage(new HighlightByCategoryEvent(this.SelectedThing.Thing as Category), this.SelectedThing.Thing);
        }
        #endregion
    }
}
