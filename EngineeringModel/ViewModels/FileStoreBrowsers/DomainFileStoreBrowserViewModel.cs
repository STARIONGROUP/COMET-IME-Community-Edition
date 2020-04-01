// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
    using System.Collections.Generic;
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
    /// The view-model for the <see cref="DomainFileStoreBrowserViewModel"/> view
    /// </summary>
    public class DomainFileStoreBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Domain File Store";

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
        /// Initializes a new instance of the <see cref="DomainFileStoreBrowserViewModel"/> class.
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
        public DomainFileStoreBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, 
            IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
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
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateStoreCommand"/> can be executed
        /// </summary>
        public bool CanCreateStore
        {
            get => this.canCreateStore;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateStore, value);
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
        /// Gets the <see cref="ICommand"/> to create a <see cref="DomainFileStore"/>
        /// </summary>
        public ReactiveCommand<object> CreateStoreCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="Folder"/>
        /// </summary>
        public ReactiveCommand<object> CreateFolderCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to upload a <see cref="CDP4Common.EngineeringModelData.File"/>
        /// </summary>
        public ReactiveCommand<object> UploadFileCommand { get; private set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateFolderCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateFolder));
            this.CreateFolderCommand.Subscribe(_ => this.ExecuteCreateCommandForFolder(this.SelectedThing.Thing));

            this.CreateStoreCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateStore));
            this.CreateStoreCommand.Subscribe(_ => this.ExecuteCreateCommand<DomainFileStore>(this.Thing));

            this.UploadFileCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanUploadFile));
            this.UploadFileCommand.Subscribe(_ => this.ExecuteCreateCommandForFile(this.SelectedThing.Thing));
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

            var isContainer = this.SelectedThing?.Thing is DomainFileStore || this.SelectedThing?.Thing is Folder;

            this.CanCreateStore = this.PermissionService.CanWrite(ClassKind.DomainFileStore, this.Thing.Container);
            this.CanCreateFolder = isContainer && this.PermissionService.CanWrite(ClassKind.Folder, this.SelectedThing.Thing);
            this.CanUploadFile = isContainer && this.PermissionService.CanWrite(ClassKind.File, this.SelectedThing.Thing);
        }

        /// <summary>
        /// Populates the <see cref="ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Folder", "", this.CreateFolderCommand, MenuItemKind.Create, ClassKind.Folder));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Domain File Store", "", this.CreateStoreCommand, MenuItemKind.Create, ClassKind.DomainFileStore));
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
        /// Update the <see cref="DomainFileStore"/>
        /// </summary>
        private void UpdateFileStoreRows()
        {
            var currentDomainFileStores = this.ContainedRows.Select(x => x.Thing).OfType<DomainFileStore>().ToList();
            var newDomainFileStores = this.Thing.DomainFileStore.Except(currentDomainFileStores);
            var oldDomainFileStores = currentDomainFileStores.Except(this.Thing.DomainFileStore);

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
        /// Add the row of the associated <see cref="DomainFileStore"/>
        /// </summary>
        /// <param name="domainFileStore">The <see cref="DomainFileStore"/> to add</param>
        private void AddDomainFileStoreRow(DomainFileStore domainFileStore)
        {
            this.ContainedRows.Add(new DomainFileStoreRowViewModel(domainFileStore, this.Session, this));
        }

        /// <summary>
        /// Remove the row of the associated <see cref="DomainFileStore"/>
        /// </summary>
        /// <param name="domainFileStore">The <see cref="DomainFileStore"/> to remove</param>
        private void RemoveDomainFileStoreRow(DomainFileStore domainFileStore)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == domainFileStore);

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
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);

            var modelSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Container)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
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
                this.DomainOfExpertise = iterationDomainPair.Value?.Item1 == null
                    ? "None"
                    : $"{iterationDomainPair.Value.Item1.Name} [{iterationDomainPair.Value.Item1.ShortName}]";
            }

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
                var realContainer = folder.GetContainerOfType(typeof(DomainFileStore));

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
            var thing = new File();

            thing.FileRevision.Add(new FileRevision());

            if (container is Folder fileFolder)
            {
                var realContainer = fileFolder.GetContainerOfType(typeof(DomainFileStore));

                this.ExecuteCreateCommand(thing, realContainer);
                return;
            }

            this.ExecuteCreateCommand(thing, container);
        }

        /// <summary>
        /// Executes the upload file command
        /// </summary>
        private void ExecuteUploadFile()
        {
            var result = this.fileDialogService.GetOpenFileDialog(false, false, false, string.Empty, string.Empty, string.Empty, 1);

            if (result.Length != 1)
            {
                return;
            }

            // TODO on Task T1250: Replace the following 3 lines with an actual call to the server to upload the file 
            //var uploadedFile = new File();
            //var participant = new Participant { Person = new Person() };
            //var fileRevision = new FileRevision { Creator = participant };
            //uploadedFile.FileRevision.Add(fileRevision);
            //var uploadedRow = new DomainFileStoreRowViewModel(((Iteration)this.Thing).DomainFileStore.FirstOrDefault(), this.Session, this.domainFileStoreRow);
            //this.ContainedRows.Add(uploadedRow);
        }
    }
}
