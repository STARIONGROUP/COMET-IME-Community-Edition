// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="EngineeringModel"/>
    /// </summary>
    public partial class EngineeringModelDialogViewModel : TopContainerDialogViewModel<EngineeringModel>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedEngineeringModelSetup"/>
        /// </summary>
        private EngineeringModelSetup selectedEngineeringModelSetup;

        /// <summary>
        /// Backing field for <see cref="SelectedCommonFileStore"/>
        /// </summary>
        private CommonFileStoreRowViewModel selectedCommonFileStore;

        /// <summary>
        /// Backing field for <see cref="SelectedLogEntry"/>
        /// </summary>
        private ModelLogEntryRowViewModel selectedLogEntry;

        /// <summary>
        /// Backing field for <see cref="SelectedIteration"/>
        /// </summary>
        private IterationRowViewModel selectedIteration;

        /// <summary>
        /// Backing field for <see cref="SelectedBook"/>
        /// </summary>
        private BookRowViewModel selectedBook;

        /// <summary>
        /// Backing field for <see cref="SelectedGenericNote"/>
        /// </summary>
        private EngineeringModelDataNoteRowViewModel selectedGenericNote;

        /// <summary>
        /// Backing field for <see cref="SelectedModellingAnnotation"/>
        /// </summary>
        private IRowViewModelBase<ModellingAnnotationItem> selectedModellingAnnotation;


        /// <summary>
        /// Backing field for <see cref="SelectedModellingAnnotationItem"/>Kind
        /// </summary>
        private ClassKind selectedModellingAnnotationItemKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public EngineeringModelDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelDialogViewModel"/> class
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/> that is the subject of the current view-model. This is the object
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
        public EngineeringModelDialogViewModel(EngineeringModel engineeringModel, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(engineeringModel, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the SelectedEngineeringModelSetup
        /// </summary>
        public virtual EngineeringModelSetup SelectedEngineeringModelSetup
        {
            get { return this.selectedEngineeringModelSetup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedEngineeringModelSetup, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="EngineeringModelSetup"/>s for <see cref="SelectedEngineeringModelSetup"/>
        /// </summary>
        public ReactiveList<EngineeringModelSetup> PossibleEngineeringModelSetup { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="CommonFileStoreRowViewModel"/>
        /// </summary>
        public CommonFileStoreRowViewModel SelectedCommonFileStore
        {
            get { return this.selectedCommonFileStore; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCommonFileStore, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="CommonFileStore"/>
        /// </summary>
        public ReactiveList<CommonFileStoreRowViewModel> CommonFileStore { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ModelLogEntryRowViewModel"/>
        /// </summary>
        public ModelLogEntryRowViewModel SelectedLogEntry
        {
            get { return this.selectedLogEntry; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLogEntry, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ModelLogEntry"/>
        /// </summary>
        public ReactiveList<ModelLogEntryRowViewModel> LogEntry { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="IterationRowViewModel"/>
        /// </summary>
        public IterationRowViewModel SelectedIteration
        {
            get { return this.selectedIteration; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIteration, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Iteration"/>
        /// </summary>
        public ReactiveList<IterationRowViewModel> Iteration { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="BookRowViewModel"/>
        /// </summary>
        public BookRowViewModel SelectedBook
        {
            get { return this.selectedBook; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBook, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Book"/>
        /// </summary>
        public ReactiveList<BookRowViewModel> Book { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="EngineeringModelDataNoteRowViewModel"/>
        /// </summary>
        public EngineeringModelDataNoteRowViewModel SelectedGenericNote
        {
            get { return this.selectedGenericNote; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGenericNote, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="EngineeringModelDataNote"/>
        /// </summary>
        public ReactiveList<EngineeringModelDataNoteRowViewModel> GenericNote { get; protected set; }
        
        /// <summary>
        /// Gets the concrete ModellingAnnotationItem to create
        /// </summary>
        public ClassKind SelectedModellingAnnotationItemKind
        {
            get { return this.selectedModellingAnnotationItemKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedModellingAnnotationItemKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleModellingAnnotationItemKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.RequestForWaiver,
            ClassKind.RequestForDeviation,
            ClassKind.ChangeRequest,
            ClassKind.ReviewItemDiscrepancy,
            ClassKind.ActionItem,
            ClassKind.ChangeProposal,
            ClassKind.ContractChangeNotice 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<ModellingAnnotationItem> SelectedModellingAnnotation
        {
            get { return this.selectedModellingAnnotation; }
            set { this.RaiseAndSetIfChanged(ref this.selectedModellingAnnotation, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ModellingAnnotationItem"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<ModellingAnnotationItem>> ModellingAnnotation { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedEngineeringModelSetup"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedEngineeringModelSetupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a CommonFileStore
        /// </summary>
        public ReactiveCommand<object> CreateCommonFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a CommonFileStore
        /// </summary>
        public ReactiveCommand<object> DeleteCommonFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a CommonFileStore
        /// </summary>
        public ReactiveCommand<object> EditCommonFileStoreCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a CommonFileStore
        /// </summary>
        public ReactiveCommand<object> InspectCommonFileStoreCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ModelLogEntry
        /// </summary>
        public ReactiveCommand<object> CreateLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ModelLogEntry
        /// </summary>
        public ReactiveCommand<object> DeleteLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ModelLogEntry
        /// </summary>
        public ReactiveCommand<object> EditLogEntryCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ModelLogEntry
        /// </summary>
        public ReactiveCommand<object> InspectLogEntryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Iteration
        /// </summary>
        public ReactiveCommand<object> CreateIterationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Iteration
        /// </summary>
        public ReactiveCommand<object> DeleteIterationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Iteration
        /// </summary>
        public ReactiveCommand<object> EditIterationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Iteration
        /// </summary>
        public ReactiveCommand<object> InspectIterationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Book
        /// </summary>
        public ReactiveCommand<object> CreateBookCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Book
        /// </summary>
        public ReactiveCommand<object> DeleteBookCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Book
        /// </summary>
        public ReactiveCommand<object> EditBookCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Book
        /// </summary>
        public ReactiveCommand<object> InspectBookCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a Book 
        /// </summary>
        public ReactiveCommand<object> MoveUpBookCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a Book
        /// </summary>
        public ReactiveCommand<object> MoveDownBookCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a EngineeringModelDataNote
        /// </summary>
        public ReactiveCommand<object> CreateGenericNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a EngineeringModelDataNote
        /// </summary>
        public ReactiveCommand<object> DeleteGenericNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a EngineeringModelDataNote
        /// </summary>
        public ReactiveCommand<object> EditGenericNoteCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a EngineeringModelDataNote
        /// </summary>
        public ReactiveCommand<object> InspectGenericNoteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ModellingAnnotationItem
        /// </summary>
        public ReactiveCommand<object> CreateModellingAnnotationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ModellingAnnotationItem
        /// </summary>
        public ReactiveCommand<object> DeleteModellingAnnotationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ModellingAnnotationItem
        /// </summary>
        public ReactiveCommand<object> EditModellingAnnotationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ModellingAnnotationItem
        /// </summary>
        public ReactiveCommand<object> InspectModellingAnnotationCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateCommonFileStoreCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedCommonFileStoreCommand = this.WhenAny(vm => vm.SelectedCommonFileStore, v => v.Value != null);
            var canExecuteEditSelectedCommonFileStoreCommand = this.WhenAny(vm => vm.SelectedCommonFileStore, v => v.Value != null && !this.IsReadOnly);

            this.CreateCommonFileStoreCommand = ReactiveCommand.Create(canExecuteCreateCommonFileStoreCommand);
            this.CreateCommonFileStoreCommand.Subscribe(_ => this.ExecuteCreateCommand<CommonFileStore>(this.PopulateCommonFileStore));

            this.DeleteCommonFileStoreCommand = ReactiveCommand.Create(canExecuteEditSelectedCommonFileStoreCommand);
            this.DeleteCommonFileStoreCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedCommonFileStore.Thing, this.PopulateCommonFileStore));

            this.EditCommonFileStoreCommand = ReactiveCommand.Create(canExecuteEditSelectedCommonFileStoreCommand);
            this.EditCommonFileStoreCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedCommonFileStore.Thing, this.PopulateCommonFileStore));

            this.InspectCommonFileStoreCommand = ReactiveCommand.Create(canExecuteInspectSelectedCommonFileStoreCommand);
            this.InspectCommonFileStoreCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedCommonFileStore.Thing));
            
            var canExecuteCreateLogEntryCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedLogEntryCommand = this.WhenAny(vm => vm.SelectedLogEntry, v => v.Value != null);
            var canExecuteEditSelectedLogEntryCommand = this.WhenAny(vm => vm.SelectedLogEntry, v => v.Value != null && !this.IsReadOnly);

            this.CreateLogEntryCommand = ReactiveCommand.Create(canExecuteCreateLogEntryCommand);
            this.CreateLogEntryCommand.Subscribe(_ => this.ExecuteCreateCommand<ModelLogEntry>(this.PopulateLogEntry));

            this.DeleteLogEntryCommand = ReactiveCommand.Create(canExecuteEditSelectedLogEntryCommand);
            this.DeleteLogEntryCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedLogEntry.Thing, this.PopulateLogEntry));

            this.EditLogEntryCommand = ReactiveCommand.Create(canExecuteEditSelectedLogEntryCommand);
            this.EditLogEntryCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedLogEntry.Thing, this.PopulateLogEntry));

            this.InspectLogEntryCommand = ReactiveCommand.Create(canExecuteInspectSelectedLogEntryCommand);
            this.InspectLogEntryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedLogEntry.Thing));
            
            var canExecuteCreateIterationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedIterationCommand = this.WhenAny(vm => vm.SelectedIteration, v => v.Value != null);
            var canExecuteEditSelectedIterationCommand = this.WhenAny(vm => vm.SelectedIteration, v => v.Value != null && !this.IsReadOnly);

            this.CreateIterationCommand = ReactiveCommand.Create(canExecuteCreateIterationCommand);
            this.CreateIterationCommand.Subscribe(_ => this.ExecuteCreateCommand<Iteration>(this.PopulateIteration));

            this.DeleteIterationCommand = ReactiveCommand.Create(canExecuteEditSelectedIterationCommand);
            this.DeleteIterationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedIteration.Thing, this.PopulateIteration));

            this.EditIterationCommand = ReactiveCommand.Create(canExecuteEditSelectedIterationCommand);
            this.EditIterationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedIteration.Thing, this.PopulateIteration));

            this.InspectIterationCommand = ReactiveCommand.Create(canExecuteInspectSelectedIterationCommand);
            this.InspectIterationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedIteration.Thing));
            
            var canExecuteCreateBookCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedBookCommand = this.WhenAny(vm => vm.SelectedBook, v => v.Value != null);
            var canExecuteEditSelectedBookCommand = this.WhenAny(vm => vm.SelectedBook, v => v.Value != null && !this.IsReadOnly);

            this.CreateBookCommand = ReactiveCommand.Create(canExecuteCreateBookCommand);
            this.CreateBookCommand.Subscribe(_ => this.ExecuteCreateCommand<Book>(this.PopulateBook));

            this.DeleteBookCommand = ReactiveCommand.Create(canExecuteEditSelectedBookCommand);
            this.DeleteBookCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedBook.Thing, this.PopulateBook));

            this.EditBookCommand = ReactiveCommand.Create(canExecuteEditSelectedBookCommand);
            this.EditBookCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedBook.Thing, this.PopulateBook));

            this.InspectBookCommand = ReactiveCommand.Create(canExecuteInspectSelectedBookCommand);
            this.InspectBookCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedBook.Thing));

            this.MoveUpBookCommand = ReactiveCommand.Create(canExecuteEditSelectedBookCommand);
            this.MoveUpBookCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Book, this.SelectedBook));

            this.MoveDownBookCommand = ReactiveCommand.Create(canExecuteEditSelectedBookCommand);
            this.MoveDownBookCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Book, this.SelectedBook));
            
            var canExecuteCreateGenericNoteCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedGenericNoteCommand = this.WhenAny(vm => vm.SelectedGenericNote, v => v.Value != null);
            var canExecuteEditSelectedGenericNoteCommand = this.WhenAny(vm => vm.SelectedGenericNote, v => v.Value != null && !this.IsReadOnly);

            this.CreateGenericNoteCommand = ReactiveCommand.Create(canExecuteCreateGenericNoteCommand);
            this.CreateGenericNoteCommand.Subscribe(_ => this.ExecuteCreateCommand<EngineeringModelDataNote>(this.PopulateGenericNote));

            this.DeleteGenericNoteCommand = ReactiveCommand.Create(canExecuteEditSelectedGenericNoteCommand);
            this.DeleteGenericNoteCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedGenericNote.Thing, this.PopulateGenericNote));

            this.EditGenericNoteCommand = ReactiveCommand.Create(canExecuteEditSelectedGenericNoteCommand);
            this.EditGenericNoteCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedGenericNote.Thing, this.PopulateGenericNote));

            this.InspectGenericNoteCommand = ReactiveCommand.Create(canExecuteInspectSelectedGenericNoteCommand);
            this.InspectGenericNoteCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGenericNote.Thing));
            
            var canExecuteCreateModellingAnnotationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedModellingAnnotationCommand = this.WhenAny(vm => vm.SelectedModellingAnnotation, v => v.Value != null);
            var canExecuteEditSelectedModellingAnnotationCommand = this.WhenAny(vm => vm.SelectedModellingAnnotation, v => v.Value != null && !this.IsReadOnly);

            this.CreateModellingAnnotationCommand = ReactiveCommand.Create(canExecuteCreateModellingAnnotationCommand);
            this.CreateModellingAnnotationCommand.Subscribe(_ => this.ExecuteCreateModellingAnnotationCommand());

            this.DeleteModellingAnnotationCommand = ReactiveCommand.Create(canExecuteEditSelectedModellingAnnotationCommand);
            this.DeleteModellingAnnotationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedModellingAnnotation.Thing, this.PopulateModellingAnnotation));

            this.EditModellingAnnotationCommand = ReactiveCommand.Create(canExecuteEditSelectedModellingAnnotationCommand);
            this.EditModellingAnnotationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedModellingAnnotation.Thing, this.PopulateModellingAnnotation));

            this.InspectModellingAnnotationCommand = ReactiveCommand.Create(canExecuteInspectSelectedModellingAnnotationCommand);
            this.InspectModellingAnnotationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedModellingAnnotation.Thing));
            var canExecuteInspectSelectedEngineeringModelSetupCommand = this.WhenAny(vm => vm.SelectedEngineeringModelSetup, v => v.Value != null);
            this.InspectSelectedEngineeringModelSetupCommand = ReactiveCommand.Create(canExecuteInspectSelectedEngineeringModelSetupCommand);
            this.InspectSelectedEngineeringModelSetupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedEngineeringModelSetup));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="ModellingAnnotationItem"/>
        /// </summary>
        protected void ExecuteCreateModellingAnnotationCommand()
        {
            switch (this.SelectedModellingAnnotationItemKind)
            {
                case ClassKind.RequestForWaiver:
                    this.ExecuteCreateCommand<RequestForWaiver>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.RequestForDeviation:
                    this.ExecuteCreateCommand<RequestForDeviation>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.ChangeRequest:
                    this.ExecuteCreateCommand<ChangeRequest>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.ReviewItemDiscrepancy:
                    this.ExecuteCreateCommand<ReviewItemDiscrepancy>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.ActionItem:
                    this.ExecuteCreateCommand<ActionItem>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.ChangeProposal:
                    this.ExecuteCreateCommand<ChangeProposal>(this.PopulateModellingAnnotation);
                    break;
                case ClassKind.ContractChangeNotice:
                    this.ExecuteCreateCommand<ContractChangeNotice>(this.PopulateModellingAnnotation);
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

            clone.EngineeringModelSetup = this.SelectedEngineeringModelSetup;

            if (!clone.Book.SortedItems.Values.SequenceEqual(this.Book.Select(x => x.Thing)))
            {
                var itemCount = this.Book.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Book[i].Thing;
                    var currentIndex = clone.Book.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Book.Move(currentIndex, i);
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
            this.PossibleEngineeringModelSetup = new ReactiveList<EngineeringModelSetup>();
            this.CommonFileStore = new ReactiveList<CommonFileStoreRowViewModel>();
            this.LogEntry = new ReactiveList<ModelLogEntryRowViewModel>();
            this.Iteration = new ReactiveList<IterationRowViewModel>();
            this.Book = new ReactiveList<BookRowViewModel>();
            this.GenericNote = new ReactiveList<EngineeringModelDataNoteRowViewModel>();
            this.ModellingAnnotation = new ReactiveList<IRowViewModelBase<ModellingAnnotationItem>>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedEngineeringModelSetup = this.Thing.EngineeringModelSetup;
            this.PopulatePossibleEngineeringModelSetup();
            this.PopulateCommonFileStore();
            this.PopulateLogEntry();
            this.PopulateIteration();
            this.PopulateBook();
            this.PopulateGenericNote();
            this.PopulateModellingAnnotation();
        }

        /// <summary>
        /// Populates the <see cref="CommonFileStore"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateCommonFileStore()
        {
            this.CommonFileStore.Clear();
            foreach (var thing in this.Thing.CommonFileStore.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new CommonFileStoreRowViewModel(thing, this.Session, this);
                this.CommonFileStore.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="LogEntry"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateLogEntry()
        {
            this.LogEntry.Clear();
            foreach (var thing in this.Thing.LogEntry.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ModelLogEntryRowViewModel(thing, this.Session, this);
                this.LogEntry.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Iteration"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateIteration()
        {
            this.Iteration.Clear();
            foreach (var thing in this.Thing.Iteration.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new IterationRowViewModel(thing, this.Session, this);
                this.Iteration.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Book"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateBook()
        {
            this.Book.Clear();
            foreach (Book thing in this.Thing.Book.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new BookRowViewModel(thing, this.Session, this);
                this.Book.Add(row);
                row.Index = this.Thing.Book.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populates the <see cref="GenericNote"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateGenericNote()
        {
            this.GenericNote.Clear();
            foreach (var thing in this.Thing.GenericNote.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EngineeringModelDataNoteRowViewModel(thing, this.Session, this);
                this.GenericNote.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ModellingAnnotation"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateModellingAnnotation()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="PossibleEngineeringModelSetup"/> property
        /// </summary>
        protected virtual void PopulatePossibleEngineeringModelSetup()
        {
            this.PossibleEngineeringModelSetup.Clear();
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
            foreach(var commonFileStore in this.CommonFileStore)
            {
                commonFileStore.Dispose();
            }
            foreach(var logEntry in this.LogEntry)
            {
                logEntry.Dispose();
            }
            foreach(var iteration in this.Iteration)
            {
                iteration.Dispose();
            }
            foreach(var book in this.Book)
            {
                book.Dispose();
            }
            foreach(var genericNote in this.GenericNote)
            {
                genericNote.Dispose();
            }
            foreach(var modellingAnnotation in this.ModellingAnnotation)
            {
                modellingAnnotation.Dispose();
            }
        }
    }
}
