﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryBrowserViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using BasicRdl.Views;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents the view-model of the <see cref="CategoryBrowser"/> view
    /// </summary>
    public class CategoryBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Categories Browser";

        /// <summary>
        /// Backing field for the <see cref="Categories"/> property
        /// </summary>
        private readonly DisposableReactiveList<CategoryRowViewModel> categories = new DisposableReactiveList<CategoryRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="thing">The <see cref="SiteDirectory"/> associated</param>
        /// <param name="thingDialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a dialog</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public CategoryBrowserViewModel(
            ISession session,
            SiteDirectory thing,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService)
            : base(
                thing,
                session,
                thingDialogNavigationService,
                panelNavigationService,
                dialogNavigationService,
                pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the rows representing <see cref="Category"/>
        /// </summary>
        public DisposableReactiveList<CategoryRowViewModel> Categories => this.categories;

        /// <summary>
        /// Gets or sets the Highlight Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> HighlightCommand { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a category may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get => this.canCreateRdlElement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value);
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

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

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
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

            this.HighlightCommand = ReactiveCommandCreator.Create(this.ExecuteHighlightCommand, canSelectableCommandsExecute);

            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Category>(), this.WhenAnyValue(x => x.CanCreateRdlElement));
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

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Category",
                    "",
                    this.CreateCommand,
                    MenuItemKind.Create,
                    ClassKind.Category));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Highlight",
                    "",
                    this.HighlightCommand,
                    MenuItemKind.Highlight));
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Category))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Added &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as Category)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.AddCategoryRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Category))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Removed &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as Category)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveCategoryRowViewModel);

            this.Disposables.Add(removeListener);

            var rdlUpdateListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
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
                this.Categories.RemoveAndDispose(row);
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
            this.CDPMessageBus.SendMessage(new CancelHighlightEvent());

            // highlight the selected thing
            this.CDPMessageBus.SendMessage(
                new HighlightByCategoryEvent(this.SelectedThing.Thing as Category),
                this.SelectedThing.Thing);

            this.CDPMessageBus.SendMessage(
                new HighlightByCategoryEvent(this.SelectedThing.Thing as Category),
                null);
        }
    }
}
