// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="UnitPrefixBrowserViewModel"/> is to represent the view-model for <see cref="UnitPrefix"/>es
    /// </summary>
    public class UnitPrefixBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDeprecatableBrowserViewModel, IPanelFilterableDataGridViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "UnitPrefixes";

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Baking field for <see cref="FilterString"/>
        /// </summary>
        private string filterString;

        /// <summary>
        /// Baking field for <see cref="IsFilterEnabled"/>
        /// </summary>
        private bool isFilterEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPrefixBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated <see cref="ISession"/></param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public UnitPrefixBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.UnitPrefixes.ChangeTrackingEnabled = true;

            this.AddSubscriptions();
        }
        
        /// <summary>
        /// Gets the <see cref="UnitPrefixRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<UnitPrefixRowViewModel> UnitPrefixes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get { return this.canCreateRdlElement; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value); }
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        ///<inheritdoc/>
        public string FilterString
        {
            get { return this.filterString; }
            set { this.RaiseAndSetIfChanged(ref this.filterString, value); }
        }

        ///<inheritdoc/>
        public bool IsFilterEnabled
        {
            get { return this.isFilterEnabled; }
            set { this.RaiseAndSetIfChanged(ref this.isFilterEnabled, value); }
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<UnitPrefix>());
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
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a UnitPrefix", "", this.CreateCommand, MenuItemKind.Create, ClassKind.UnitPrefix));
        }

        /// <summary>
        /// Loads the <see cref="UnitPrefix"/>es from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.UnitPrefixes = new DisposableReactiveList<UnitPrefixRowViewModel>();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(x => x.Iid);
            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var unitPrefix in referenceDataLibrary.UnitPrefix)
                {
                    this.AddUnitPrefixRowViewModel(unitPrefix);
                }
            }

            this.UnitPrefixes.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
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
            foreach (var unitPrefix in this.UnitPrefixes)
            {
                unitPrefix.Dispose();
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(UnitPrefix))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as UnitPrefix)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddUnitPrefixRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(UnitPrefix))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as UnitPrefix)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveUnitPrefixRowViewModel);
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
        /// Adds a <see cref="UnitPrefixRowViewModel"/>
        /// </summary>
        /// <param name="unitPrefix">
        /// The associated <see cref="UnitPrefix"/> for which the row is to be added.
        /// </param>
        private void AddUnitPrefixRowViewModel(UnitPrefix unitPrefix)
        {
            if (this.UnitPrefixes.Any(x => x.Thing == unitPrefix))
            {
                return;
            }

            var row = new UnitPrefixRowViewModel(unitPrefix, this.Session, this);
            this.UnitPrefixes.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="UnitPrefixRowViewModel"/> from the view model
        /// </summary>
        /// <param name="unitPrefix">
        /// The <see cref="UnitPrefix"/> for which the row view model has to be removed
        /// </param>
        private void RemoveUnitPrefixRowViewModel(UnitPrefix unitPrefix)
        {
            var row = this.UnitPrefixes.SingleOrDefault(rowViewModel => rowViewModel.Thing == unitPrefix);
            if (row != null)
            {
                this.UnitPrefixes.RemoveAndDispose(row);
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
            foreach (var unitPrefixe in this.UnitPrefixes)
            {
                if (unitPrefixe.Thing.Container != rdl)
                {
                    continue;
                }

                if (unitPrefixe.ContainerRdl != rdl.ShortName)
                {
                    unitPrefixe.ContainerRdl = rdl.ShortName;
                }
            }
        }
    }
}