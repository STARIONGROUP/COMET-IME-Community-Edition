// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Controls;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
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
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);
            this.ContainedRows = new ReactiveList<IRowViewModelBase<Thing>>();
            this.AddSubscriptions();
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Gets or sets the Contained <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<Thing>> ContainedRows { get; protected set; }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return (EngineeringModelSetup)this.Thing.IterationSetup.Container; }
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
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
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateFolderCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateFolder));
            this.CreateFolderCommand.Subscribe(_ => this.ExecuteCreateCommand<Folder>(this.commonFileStoreRow.Thing));

            this.CreateStoreCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateStore));
            this.CreateStoreCommand.Subscribe(_ => this.ExecuteCreateCommand<CommonFileStore>(this.Thing.TopContainer));

            this.UploadFileCommand = ReactiveCommand.Create();
            this.UploadFileCommand.Subscribe(_ => this.ExecuteUploadFile());
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
            this.CanCreateFolder = this.commonFileStoreRow != null &&
                                   this.PermissionService.CanWrite(ClassKind.Folder, this.commonFileStoreRow.Thing);

            this.CanCreateStore = this.commonFileStoreRow == null;
        }

        /// <summary>
        /// Populates the <see cref="ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Folder", "", this.CreateFolderCommand, MenuItemKind.Create, ClassKind.Folder));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Common File Store", "", this.CreateStoreCommand, MenuItemKind.Create, ClassKind.CommonFileStore));
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
            var updatedStore = ((EngineeringModel)this.Thing.Container).CommonFileStore.FirstOrDefault();
            if (updatedStore == null)
            {
                if (this.commonFileStoreRow != null)
                {
                    this.commonFileStoreRow.Dispose();
                    this.commonFileStoreRow = null;
                    this.ContainedRows.Remove(this.commonFileStoreRow);
                }

                return;
            }

            if (this.commonFileStoreRow == null)
            {
                this.commonFileStoreRow = new CommonFileStoreRowViewModel(updatedStore, this.Session, this);
                this.ContainedRows.Add(this.commonFileStoreRow);
            }
            else if (this.commonFileStoreRow.Thing != updatedStore)
            {
                this.commonFileStoreRow.Dispose();
                this.commonFileStoreRow = new CommonFileStoreRowViewModel(updatedStore, this.Session, this);
                this.ContainedRows.Add(this.commonFileStoreRow);
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(iterationSetupSubscription);

            var modelSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Container)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(modelSubscription);
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
                this.DomainOfExpertise = (iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null)
                                        ? "None"
                                        : string.Format("{0} [{1}]", iterationDomainPair.Value.Item1.Name, iterationDomainPair.Value.Item1.ShortName);
            }

            this.UpdateFileStoreRows();
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

            // TODO on Task T1250: Replace the following 3 lines with an actual call to the server to upload the file 
            var uploadedFile = new File();
            var participant = new Participant { Person = new Person() };
            var fileRevision = new FileRevision { Creator = participant };
            uploadedFile.FileRevision.Add(fileRevision);
            var uploadedRow = new FileRowViewModel(uploadedFile, this.Session, this.commonFileStoreRow);
            this.ContainedRows.Add(uploadedRow);
        }
    }
}