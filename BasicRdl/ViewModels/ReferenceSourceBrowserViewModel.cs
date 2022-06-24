// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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

    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ReferenceSourceBrowserViewModel"/> is to represent the view-model for <see cref="ReferenceSource"/>s
    /// </summary>
    public class ReferenceSourceBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDropTarget, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "ReferenceSources";

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated <see cref="ISession"/></param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public ReferenceSourceBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="ReferenceSourceRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<ReferenceSourceRowViewModel> ReferenceSources { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
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
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ReferenceSource>(), this.WhenAnyValue(x => x.CanCreateRdlElement));
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

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a ReferenceSource", "", this.CreateCommand, MenuItemKind.Create, ClassKind.ReferenceSource));
        }

        /// <summary>
        /// Loads the <see cref="ReferenceSource"/>es from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ReferenceSources = new DisposableReactiveList<ReferenceSourceRowViewModel>();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(x => x.Iid);
            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var referenceSource in referenceDataLibrary.ReferenceSource)
                {
                    this.AddReferenceSourceRowViewModel(referenceSource);
                }
            }

            this.ReferenceSources.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
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
            foreach (var referenceSource in this.ReferenceSources)
            {
                referenceSource.Dispose();
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceSource))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceSource)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddReferenceSourceRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceSource))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceSource)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveReferenceSourceRowViewModel);
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
        /// Adds a <see cref="ReferenceSourceRowViewModel"/>
        /// </summary>
        /// <param name="referenceSource">
        /// The associated <see cref="ReferenceSource"/> for which the row is to be added.
        /// </param>
        private void AddReferenceSourceRowViewModel(ReferenceSource referenceSource)
        {
            if (this.ReferenceSources.Any(x => x.Thing == referenceSource))
            {
                return;
            }

            var row = new ReferenceSourceRowViewModel(referenceSource, this.Session, this);
            this.ReferenceSources.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="ReferenceSourceRowViewModel"/> from the view model
        /// </summary>
        /// <param name="referenceSource">
        /// The <see cref="ReferenceSource"/> for which the row view model has to be removed
        /// </param>
        private void RemoveReferenceSourceRowViewModel(ReferenceSource referenceSource)
        {
            var row = this.ReferenceSources.SingleOrDefault(rowViewModel => rowViewModel.Thing == referenceSource);
            if (row != null)
            {
                this.ReferenceSources.RemoveAndDispose(row);
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
            foreach (var referenceSource in this.ReferenceSources)
            {
                if (referenceSource.Thing.Container != rdl)
                {
                    continue;
                }

                if (referenceSource.ContainerRdl != rdl.ShortName)
                {
                    referenceSource.ContainerRdl = rdl.ShortName;
                }
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
            if (droptarget != null)
            {
                droptarget.DragOver(dropInfo);
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
                    Logger.Error(ex);
                    this.Feedback = ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }
    }
}