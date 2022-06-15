// -------------------------------------------------------------------------------------------------
// <copyright file="PageDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Page"/>
    /// </summary>
    public partial class PageDialogViewModel : DialogViewModelBase<Page>
    {
        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

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
        /// Backing field for <see cref="SelectedNote"/>
        /// </summary>
        private IRowViewModelBase<Note> selectedNote;


        /// <summary>
        /// Backing field for <see cref="SelectedNote"/>Kind
        /// </summary>
        private ClassKind selectedNoteKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PageDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageDialogViewModel"/> class
        /// </summary>
        /// <param name="page">
        /// The <see cref="Page"/> that is the subject of the current view-model. This is the object
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
        public PageDialogViewModel(Page page, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(page, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Section;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Section",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public virtual string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
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
        /// Gets the concrete Note to create
        /// </summary>
        public ClassKind SelectedNoteKind
        {
            get { return this.selectedNoteKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedNoteKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleNoteKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.BinaryNote,
            ClassKind.TextualNote 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<Note> SelectedNote
        {
            get { return this.selectedNote; }
            set { this.RaiseAndSetIfChanged(ref this.selectedNote, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Note"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<Note>> Note { get; protected set; }
        
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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Note
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Note
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Note
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditNoteCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Note
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a Note 
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpNoteCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a Note
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownNoteCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateNoteCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedNoteCommand = this.WhenAny(vm => vm.SelectedNote, v => v.Value != null);
            var canExecuteEditSelectedNoteCommand = this.WhenAny(vm => vm.SelectedNote, v => v.Value != null && !this.IsReadOnly);

            this.CreateNoteCommand = ReactiveCommandCreator.Create(canExecuteCreateNoteCommand);
            this.CreateNoteCommand.Subscribe(_ => this.ExecuteCreateNoteCommand());

            this.DeleteNoteCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNoteCommand);
            this.DeleteNoteCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedNote.Thing, this.PopulateNote));

            this.EditNoteCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNoteCommand);
            this.EditNoteCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedNote.Thing, this.PopulateNote));

            this.InspectNoteCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedNoteCommand);
            this.InspectNoteCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedNote.Thing));

            this.MoveUpNoteCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNoteCommand);
            this.MoveUpNoteCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Note, this.SelectedNote));

            this.MoveDownNoteCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNoteCommand);
            this.MoveDownNoteCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Note, this.SelectedNote));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="Note"/>
        /// </summary>
        protected void ExecuteCreateNoteCommand()
        {
            switch (this.SelectedNoteKind)
            {
                case ClassKind.BinaryNote:
                    this.ExecuteCreateCommand<BinaryNote>(this.PopulateNote);
                    break;
                case ClassKind.TextualNote:
                    this.ExecuteCreateCommand<TextualNote>(this.PopulateNote);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ShortName = this.ShortName;
            clone.Name = this.Name;
            clone.CreatedOn = this.CreatedOn;
            clone.Owner = this.SelectedOwner;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);


            if (!clone.Note.SortedItems.Values.SequenceEqual(this.Note.Select(x => x.Thing)))
            {
                var itemCount = this.Note.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Note[i].Thing;
                    var currentIndex = clone.Note.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Note.Move(currentIndex, i);
                    }
                }
            }
            
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.Note = new ReactiveList<IRowViewModelBase<Note>>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ShortName = this.Thing.ShortName;
            this.Name = this.Thing.Name;
            this.CreatedOn = this.Thing.CreatedOn;
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateNote();
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
        /// Populates the <see cref="Note"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateNote()
        {
            // this method needs to be overriden.
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
            foreach(var note in this.Note)
            {
                note.Dispose();
            }
        }
    }
}
