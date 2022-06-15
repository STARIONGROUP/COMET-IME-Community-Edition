// -------------------------------------------------------------------------------------------------
// <copyright file="FileDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
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
    /// dialog-view-model class representing a <see cref="File"/>
    /// </summary>
    public partial class FileDialogViewModel : DialogViewModelBase<File>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedLockedBy"/>
        /// </summary>
        private Person selectedLockedBy;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedFileRevision"/>
        /// </summary>
        private FileRevisionRowViewModel selectedFileRevision;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FileDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogViewModel"/> class
        /// </summary>
        /// <param name="file">
        /// The <see cref="File"/> that is the subject of the current view-model. This is the object
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
        public FileDialogViewModel(File file, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(file, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as FileStore;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type FileStore",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedLockedBy
        /// </summary>
        public virtual Person SelectedLockedBy
        {
            get { return this.selectedLockedBy; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLockedBy, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Person"/>s for <see cref="SelectedLockedBy"/>
        /// </summary>
        public ReactiveList<Person> PossibleLockedBy { get; protected set; }

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
        /// Gets or sets the selected <see cref="FileRevisionRowViewModel"/>
        /// </summary>
        public FileRevisionRowViewModel SelectedFileRevision
        {
            get { return this.selectedFileRevision; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFileRevision, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="FileRevision"/>
        /// </summary>
        public ReactiveList<FileRevisionRowViewModel> FileRevision { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Category"/>s
        /// </summary>
        private ReactiveList<Category> category;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> Category 
        { 
            get { return this.category; } 
            set { this.RaiseAndSetIfChanged(ref this.category, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedLockedBy"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedLockedByCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a FileRevision
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateFileRevisionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a FileRevision
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteFileRevisionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a FileRevision
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditFileRevisionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a FileRevision
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectFileRevisionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateFileRevisionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedFileRevisionCommand = this.WhenAny(vm => vm.SelectedFileRevision, v => v.Value != null);
            var canExecuteEditSelectedFileRevisionCommand = this.WhenAny(vm => vm.SelectedFileRevision, v => v.Value != null && !this.IsReadOnly);

            this.CreateFileRevisionCommand = ReactiveCommandCreator.Create(canExecuteCreateFileRevisionCommand);
            this.CreateFileRevisionCommand.Subscribe(_ => this.ExecuteCreateCommand<FileRevision>(this.PopulateFileRevision));

            this.DeleteFileRevisionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileRevisionCommand);
            this.DeleteFileRevisionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedFileRevision.Thing, this.PopulateFileRevision));

            this.EditFileRevisionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedFileRevisionCommand);
            this.EditFileRevisionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedFileRevision.Thing, this.PopulateFileRevision));

            this.InspectFileRevisionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedFileRevisionCommand);
            this.InspectFileRevisionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFileRevision.Thing));
            var canExecuteInspectSelectedLockedByCommand = this.WhenAny(vm => vm.SelectedLockedBy, v => v.Value != null);
            this.InspectSelectedLockedByCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedLockedByCommand);
            this.InspectSelectedLockedByCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedLockedBy));
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

            clone.LockedBy = this.SelectedLockedBy;
            clone.Owner = this.SelectedOwner;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleLockedBy = new ReactiveList<Person>();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.FileRevision = new ReactiveList<FileRevisionRowViewModel>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedLockedBy = this.Thing.LockedBy;
            this.PopulatePossibleLockedBy();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateFileRevision();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Category"/> property
        /// </summary>
        protected virtual void PopulateCategory()
        {
            this.Category.Clear();

            foreach (var value in this.Thing.Category)
            {
                this.Category.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="FileRevision"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateFileRevision()
        {
            this.FileRevision.Clear();
            foreach (var thing in this.Thing.FileRevision.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new FileRevisionRowViewModel(thing, this.Session, this);
                this.FileRevision.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleLockedBy"/> property
        /// </summary>
        protected virtual void PopulatePossibleLockedBy()
        {
            this.PossibleLockedBy.Clear();
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
            foreach(var fileRevision in this.FileRevision)
            {
                fileRevision.Dispose();
            }
        }
    }
}
