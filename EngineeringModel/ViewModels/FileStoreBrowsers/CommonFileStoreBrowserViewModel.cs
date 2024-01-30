// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="CommonFileStoreBrowserViewModel"/> view
    /// </summary>
    public class CommonFileStoreBrowserViewModel : BrowserViewModelBase<EngineeringModel>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Common File Store";

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="CancreateFolder"/>
        /// </summary>
        private bool canCreateFolder;

        /// <summary>
        /// Backing field for <see cref="CanCreateStore"/>
        /// </summary>
        private bool canCreateStore;

        /// <summary>
        /// Backing field for <see cref="CanUploadFile"/>
        /// </summary>
        private bool canUploadFile;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService fileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowserViewModel"/> class.
        /// </summary>
        /// <param name="engineeringModel">
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
        public CommonFileStoreBrowserViewModel(EngineeringModel engineeringModel, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(engineeringModel, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.EngineeringModelSetup.Name}";
            this.ToolTip = $"{this.Thing.EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
            this.ContainedRows = new DisposableReactiveList<IRowViewModelBase<Thing>>();
            this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Contained <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public DisposableReactiveList<IRowViewModelBase<Thing>> ContainedRows { get; protected set; }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.EngineeringModelSetup;

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateFolderCommand"/> can be executed
        /// </summary>
        public bool CanCreateFolder
        {
            get => this.canCreateFolder;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateFolder, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UploadFileCommand"/> can be executed
        /// </summary>
        public bool CanUploadFile
        {
            get => this.canUploadFile;
            private set => this.RaiseAndSetIfChanged(ref this.canUploadFile, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="Folder"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateFolderCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to upload a <see cref="CDP4Common.EngineeringModelData.File"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> UploadFileCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateStoreCommand"/> can be executed
        /// </summary>
        public bool CanCreateStore
        {
            get => this.canCreateStore;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateStore, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="CommonFileStore"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateStoreCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateStoreCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateCommand<CommonFileStore>(this.Thing.TopContainer),
                this.WhenAnyValue(x => x.CanCreateStore));

            this.CreateFolderCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateCommandForFolder(this.SelectedThing.Thing),
                this.WhenAnyValue(x => x.CanCreateFolder));

            this.UploadFileCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateCommandForFile(this.SelectedThing.Thing),
                this.WhenAnyValue(x => x.CanUploadFile));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            var isContainer = this.SelectedThing?.Thing is CommonFileStore || this.SelectedThing?.Thing is Folder;

            this.CanCreateStore = this.PermissionService.CanWrite(ClassKind.CommonFileStore, this.Thing);
            this.CanCreateFolder = isContainer && this.PermissionService.CanWrite(ClassKind.Folder, this.SelectedThing.Thing);
            this.CanUploadFile = isContainer && this.PermissionService.CanWrite(ClassKind.File, this.SelectedThing.Thing);
        }

        /// <summary>
        /// Populates the <see cref="ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Common File Store", "", this.CreateStoreCommand, MenuItemKind.Create, ClassKind.CommonFileStore));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Folder", "", this.CreateFolderCommand, MenuItemKind.Create, ClassKind.Folder));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Upload a File to the File Store", "", this.UploadFileCommand, MenuItemKind.Create, ClassKind.File));
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

            foreach (var row in this.ContainedRows)
            {
                row.Dispose();
            }
        }

        /// <summary>
        /// Update the <see cref="CommonFileStore"/>
        /// </summary>
        private void UpdateFileStoreRows()
        {
            var commonFileStore = this.Thing.CommonFileStore;

            var currentDomainFileStores = this.ContainedRows.Select(x => x.Thing).OfType<CommonFileStore>().ToList();
            var newDomainFileStores = commonFileStore.Except(currentDomainFileStores);
            var oldDomainFileStores = currentDomainFileStores.Except(commonFileStore);

            foreach (var domainFileStore in oldDomainFileStores)
            {
                this.RemoveDomainFileStoreRow(domainFileStore);
            }

            foreach (var domainFileStore in newDomainFileStores)
            {
                this.AddDomainFileStoreRow(domainFileStore);
            }
        }

        /// <summary>
        /// Add the row of the associated <see cref="CommonFileStore"/>
        /// </summary>
        /// <param name="commonFileStore">The <see cref="CommonFileStore"/> to add</param>
        private void AddDomainFileStoreRow(CommonFileStore commonFileStore)
        {
            this.ContainedRows.Add(new CommonFileStoreRowViewModel(commonFileStore, this.Session, this));
        }

        /// <summary>
        /// Remove the row of the associated <see cref="CommonFileStore"/>
        /// </summary>
        /// <param name="commonFileStore">The <see cref="CommonFileStore"/> to remove</param>
        private void RemoveDomainFileStoreRow(CommonFileStore commonFileStore)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == commonFileStore);

            if (row != null)
            {
                this.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.EngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Container)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Folder))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .Subscribe(_ => this.ComputePermission()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(File))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .Subscribe(_ => this.ComputePermission()));
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary> 
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.UpdateFileStoreRows();
        }

        /// <summary>
        /// Execute the generic <see cref="CreateCommand"/> for a <see cref="Folder"/>
        /// </summary>
        /// <param name="container">
        /// The container of the <see cref="Folder"/> that is to be created
        /// </param>
        protected void ExecuteCreateCommandForFolder(Thing container = null)
        {
            var thing = new Folder();

            if (container is Folder folder)
            {
                thing.ContainingFolder = folder;

                var realContainer = folder.GetContainerOfType(typeof(CommonFileStore));

                this.ExecuteCreateCommand(thing, realContainer);
                return;
            }

            this.ExecuteCreateCommand(thing, container);
        }

        /// <summary>
        /// Execute the generic <see cref="CreateCommand"/> for a <see cref="File"/>
        /// </summary>
        /// <param name="container">
        /// The container of the <see cref="File"/> that is to be created
        /// </param>
        protected void ExecuteCreateCommandForFile(Thing container = null)
        {
            var file = new File
            {
                Container = this.Thing
            };

            if (container is Folder fileFolder)
            {
                var realContainer = fileFolder.GetContainerOfType(typeof(CommonFileStore));
                file.CurrentContainingFolder = fileFolder;

                this.ExecuteCreateCommand(file, realContainer);
                return;
            }

            this.ExecuteCreateCommand(file, container);
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
            try
            {
                logger.Trace("drag over {0}", dropInfo.TargetItem);

                if (dropInfo.TargetItem is IDropTarget droptarget)
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
            if (dropInfo.TargetItem is IDropTarget droptarget)
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
    }
}
