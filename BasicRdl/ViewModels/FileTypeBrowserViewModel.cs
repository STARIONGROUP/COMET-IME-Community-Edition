// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowserViewModel.cs" company="RHEA System S.A.">
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

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="FileTypeBrowserViewModel"/> is to represent the view-model for <see cref="FileType"/>s
    /// </summary>
    public class FileTypeBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDropTarget,
        IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "File Types";

        /// <summary>
        /// Backing field for <see cref="CanWriteFileType"/>
        /// </summary>
        private bool canWriteFileType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated <see cref="ISession"/></param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public FileTypeBrowserViewModel(
            ISession session,
            SiteDirectory siteDir,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService)
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
            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets or sets a value indicating if the current person can create, edit or delete a <see cref="FileType"/>
        /// </summary>
        public bool CanWriteFileType
        {
            get => this.canWriteFileType;
            set => this.RaiseAndSetIfChanged(ref this.canWriteFileType, value);
        }

        /// <summary>
        /// Gets the <see cref="FileTypes"/> rows that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<FileTypeRowViewModel> FileTypes { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(FileType))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Added &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as FileType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddFileTypeRowViewModel);

            this.Disposables.Add(addListener);

            var removeListener =
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(FileType))
                    .Where(
                        objectChange => objectChange.EventKind == EventKind.Removed &&
                                        objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as FileType)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveFileTypeRowViewModel);

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
        }

        /// <summary>
        /// Adds a <see cref="FileTypeRowViewModel"/>
        /// </summary>
        /// <param name="filetype">
        /// The associated <see cref="FileType"/> for which the row is to be added.
        /// </param>
        private void AddFileTypeRowViewModel(FileType filetype)
        {
            if (this.FileTypes.Any(x => x.Thing == filetype))
            {
                return;
            }

            var row = new FileTypeRowViewModel(filetype, this.Session, this);
            this.FileTypes.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="FileTypeRowViewModel"/> from the view model
        /// </summary>
        /// <param name="filetype">
        /// The <see cref="FileType"/> for which the row view model has to be removed
        /// </param>
        private void RemoveFileTypeRowViewModel(FileType filetype)
        {
            var row = this.FileTypes.SingleOrDefault(rowViewModel => rowViewModel.Thing == filetype);

            if (row != null)
            {
                this.FileTypes.RemoveAndDispose(row);
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
            foreach (var filetype in this.FileTypes)
            {
                if (filetype.Thing.Container != rdl)
                {
                    continue;
                }

                if (filetype.ContainerRdl != rdl.ShortName)
                {
                    filetype.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Loads the <see cref="FileType"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.FileTypes = new DisposableReactiveList<FileTypeRowViewModel>();
            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries()
                         .Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var filetype in referenceDataLibrary.FileType)
                {
                    this.AddFileTypeRowViewModel(filetype);
                }
            }

            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<FileType>(), this.WhenAnyValue(x => x.CanWriteFileType));
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanWriteFileType = this.Session.OpenReferenceDataLibraries.Any();
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a File Type",
                    "",
                    this.CreateCommand,
                    MenuItemKind.Create,
                    ClassKind.FileType));
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

            foreach (var fileType in this.FileTypes)
            {
                fileType.Dispose();
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
