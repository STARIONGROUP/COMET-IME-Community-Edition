// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="CommonFileStoreBrowserViewModel"/> view
    /// </summary>
    public class CommonFileStoreBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Common File Store";

        /// <summary>
        /// the <see cref="CommonFileStoreRowViewModel"/>
        /// </summary>
        private CommonFileStoreRowViewModel commonFileStoreRow;

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
        /// <param name="iteration">
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
        public CommonFileStoreBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
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
        public EngineeringModelSetup CurrentEngineeringModelSetup => (EngineeringModelSetup)this.Thing.IterationSetup.Container;

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel; 
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateFolderCommand"/> can be executed
        /// </summary>
        public bool CanCreateFolder
        {
            get { return this.canCreateFolder; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateFolder, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UploadFileCommand"/> can be executed
        /// </summary>
        public bool CanUploadFile
        {
            get { return this.canUploadFile; }
            private set { this.RaiseAndSetIfChanged(ref this.canUploadFile, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="Folder"/>
        /// </summary>
        public ReactiveCommand<object> CreateFolderCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to upload a <see cref="CDP4Common.EngineeringModelData.File"/>
        /// </summary>
        public ReactiveCommand<object> UploadFileCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateStoreCommand"/> can be executed
        /// </summary>
        public bool CanCreateStore
        {
            get { return this.canCreateStore; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateStore, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="CommonFileStore"/>
        /// </summary>
        public ReactiveCommand<object> CreateStoreCommand { get; private set; }

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

            this.CreateStoreCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateStore));
            this.Disposables.Add(this.CreateStoreCommand.Subscribe(_ => this.ExecuteCreateCommand<CommonFileStore>(this.Thing.Container)));

            this.CreateFolderCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateFolder));
            this.Disposables.Add(this.CreateFolderCommand.Subscribe(_ => this.ExecuteCreateCommandForFolder(this.SelectedThing.Thing)));

            this.UploadFileCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanUploadFile));
            this.Disposables.Add(this.UploadFileCommand.Subscribe(_ => this.ExecuteCreateCommandForFile(this.SelectedThing.Thing)));
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
            var isFile = this.SelectedThing?.Thing is File;
            var isOwner = this.IsOwner();

            this.CanCreateStore = this.PermissionService.CanWrite(ClassKind.CommonFileStore, this.Thing.Container);
            this.CanCreateFolder = (isOwner ?? false) && isContainer && this.PermissionService.CanWrite(ClassKind.Folder, this.SelectedThing.Thing);
            this.CanUploadFile = (isOwner ?? false) && isContainer && this.PermissionService.CanWrite(ClassKind.File, this.SelectedThing.Thing);
        }

        /// <summary>
        /// Checks if the current <see cref="Participant"/>'s contained <see cref="Participant.Domain"/> contains the <see cref="IOwnedThingViewModel"/> instance's <see cref="IOwnedThingViewModel.Owner"/>.
        /// </summary>
        /// <returns>
        /// True if <see cref="IOwnedThingViewModel.Owner"/> is contained in <see cref="Participant.Domain"/>, 
        /// False if <see cref="IOwnedThingViewModel.Owner"/> is not contained in <see cref="Participant.Domain"/>
        /// If the current <see cref="SelectedThing"/> is not an <see cref="IOwnedThingViewModel"/>, it returns null.
        /// </returns>
        private bool? IsOwner()
        {
            if (this.SelectedThing is IOwnedThingViewModel ownedThing)
            {
                return this.Session.QueryDomainOfExpertise(this.Thing).Contains(ownedThing.Owner);
            }

            return null;
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
        //private void UpdateFileStoreRows()
        //{
        //    var updatedStore = ((EngineeringModel)this.Thing.Container).CommonFileStore.FirstOrDefault();
        //    if (updatedStore == null)
        //    {
        //        if (this.commonFileStoreRow != null)
        //        {
        //            this.ContainedRows.RemoveAndDispose(this.commonFileStoreRow);
        //            this.commonFileStoreRow = null;
        //        }

        //        return;
        //    }

        //    if (this.commonFileStoreRow == null)
        //    {
        //        this.commonFileStoreRow = new CommonFileStoreRowViewModel(updatedStore, this.Session, this);
        //        this.ContainedRows.Add(this.commonFileStoreRow);
        //    }
        //    else if (this.commonFileStoreRow.Thing != updatedStore)
        //    {
        //        this.commonFileStoreRow.Dispose();
        //        this.commonFileStoreRow = new CommonFileStoreRowViewModel(updatedStore, this.Session, this);
        //        this.ContainedRows.Add(this.commonFileStoreRow);
        //    }
        //}

        /// <summary>
        /// Update the <see cref="CommonFileStore"/>
        /// </summary>
        private void UpdateFileStoreRows()
        {
            var commonFileStore = ((EngineeringModel)this.Thing.Container).CommonFileStore;

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
            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Container)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties()));

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Folder))
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated))
                    .Subscribe(_ => this.ComputePermission()));

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(File))
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated))
                    .Subscribe(_ => this.ComputePermission()));
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary> 
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";

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
        /// Executes the upload file command
        /// </summary>
        private void ExecuteUploadFile()
        {
            var result = this.fileDialogService.GetSaveFileDialog(string.Empty, string.Empty, string.Empty, string.Empty, 1);
            if (string.IsNullOrEmpty(result))
            {
                return;
            }

            //// TODO on Task T1250: Replace the following 3 lines with an actual call to the server to upload the file 
            //var uploadedFile = new File();
            //var participant = new Participant { Person = new Person() };
            //var fileRevision = new FileRevision { Creator = participant };
            //uploadedFile.FileRevision.Add(fileRevision);
            //var uploadedRow = new FileRowViewModel(uploadedFile, this.Session, this.commonFileStoreRow);
            //this.ContainedRows.Add(uploadedRow);
        }
    }
}