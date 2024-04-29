// -------------------------------------------------------------------------------------------------
// <copyright file="FileStoreDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="FileStore"/>
    /// </summary>
    public abstract partial class FileStoreDialogViewModel<T> : DialogViewModelBase<T> where T : FileStore
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedFolder"/>
        /// </summary>
        private FolderRowViewModel selectedFolder;

        /// <summary>
        /// Backing field for <see cref="SelectedFile"/>
        /// </summary>
        private FileRowViewModel selectedFile;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileStoreDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected FileStoreDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStoreDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="fileStore">
        /// The <see cref="FileStore"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        protected FileStoreDialogViewModel(T fileStore, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(fileStore, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public virtual DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedOwner
        /// </summary>
        public virtual DomainOfExpertise SelectedOwner
        {
            get { return this.selectedOwner; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOwner, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleOwner { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="FolderRowViewModel"/>
        /// </summary>
        public FolderRowViewModel SelectedFolder
        {
            get { return this.selectedFolder; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFolder, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Folder"/>
        /// </summary>
        public ReactiveList<FolderRowViewModel> Folder { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="FileRowViewModel"/>
        /// </summary>
        public FileRowViewModel SelectedFile
        {
            get { return this.selectedFile; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFile, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="File"/>
        /// </summary>
        public ReactiveList<FileRowViewModel> File { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Folder
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateFolderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Folder
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteFolderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Folder
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditFolderCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Folder
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectFolderCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a File
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateFileCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a File
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteFileCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a File
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditFileCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a File
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectFileCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateFolderCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedFolderCommand = this.WhenAny(vm => vm.SelectedFolder, v => v.Value != null);
            var canExecuteEditSelectedFolderCommand = this.WhenAny(vm => vm.SelectedFolder, v => v.Value != null && !this.IsReadOnly);

            this.CreateFolderCommand = ReactiveCommandCreator.Create(canExecuteCreateFolderCommand);
            this.CreateFolderCommand.Subscribe(_ => this.ExecuteCreateCommand<Folder>(this.PopulateFolder));

            this.DeleteFolderCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFolderCommand);
            this.DeleteFolderCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedFolder.Thing, this.PopulateFolder));

            this.EditFolderCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFolderCommand);
            this.EditFolderCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedFolder.Thing, this.PopulateFolder));

            this.InspectFolderCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFolderCommand);
            this.InspectFolderCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFolder.Thing));
            
            var canExecuteCreateFileCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedFileCommand = this.WhenAny(vm => vm.SelectedFile, v => v.Value != null);
            var canExecuteEditSelectedFileCommand = this.WhenAny(vm => vm.SelectedFile, v => v.Value != null && !this.IsReadOnly);

            this.CreateFileCommand = ReactiveCommandCreator.Create(canExecuteCreateFileCommand);
            this.CreateFileCommand.Subscribe(_ => this.ExecuteCreateCommand<File>(this.PopulateFile));

            this.DeleteFileCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileCommand);
            this.DeleteFileCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedFile.Thing, this.PopulateFile));

            this.EditFileCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileCommand);
            this.EditFileCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedFile.Thing, this.PopulateFile));

            this.InspectFileCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFileCommand);
            this.InspectFileCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFile.Thing));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Name = this.Name;
            clone.CreatedOn = this.CreatedOn;
            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.Folder = new ReactiveList<FolderRowViewModel>();
            this.File = new ReactiveList<FileRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Name = this.Thing.Name;
            this.CreatedOn = this.Thing.CreatedOn;
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateFolder();
            this.PopulateFile();
        }

        /// <summary>
        /// Populates the <see cref="Folder"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateFolder()
        {
            this.Folder.Clear();
            foreach (var thing in this.Thing.Folder.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new FolderRowViewModel(thing, this.Session, this);
                this.Folder.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="File"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateFile()
        {
            this.File.Clear();
            foreach (var thing in this.Thing.File.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new FileRowViewModel(thing, this.Session, this);
                this.File.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/> property
        /// </summary>
        protected virtual void PopulatePossibleOwner()
        {
            this.PossibleOwner.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var folder in this.Folder)
            {
                folder.Dispose();
            }
            foreach(var file in this.File)
            {
                file.Dispose();
            }
        }
    }
}
