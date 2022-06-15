// -------------------------------------------------------------------------------------------------
// <copyright file="BookDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="Book"/>
    /// </summary>
    public partial class BookDialogViewModel : DialogViewModelBase<Book>
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
        /// Backing field for <see cref="SelectedSection"/>
        /// </summary>
        private SectionRowViewModel selectedSection;


        /// <summary>
        /// Initializes a new instance of the <see cref="BookDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BookDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookDialogViewModel"/> class
        /// </summary>
        /// <param name="book">
        /// The <see cref="Book"/> that is the subject of the current view-model. This is the object
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
        public BookDialogViewModel(Book book, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(book, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as EngineeringModel;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type EngineeringModel",
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
        /// Gets or sets the selected <see cref="SectionRowViewModel"/>
        /// </summary>
        public SectionRowViewModel SelectedSection
        {
            get { return this.selectedSection; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSection, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Section"/>
        /// </summary>
        public ReactiveList<SectionRowViewModel> Section { get; protected set; }
        
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
        /// Gets or sets the Create <see cref="ICommand"/> to create a Section
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSectionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Section
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteSectionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Section
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditSectionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Section
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSectionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a Section 
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpSectionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a Section
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownSectionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateSectionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSectionCommand = this.WhenAny(vm => vm.SelectedSection, v => v.Value != null);
            var canExecuteEditSelectedSectionCommand = this.WhenAny(vm => vm.SelectedSection, v => v.Value != null && !this.IsReadOnly);

            this.CreateSectionCommand = ReactiveCommandCreator.Create(canExecuteCreateSectionCommand);
            this.CreateSectionCommand.Subscribe(_ => this.ExecuteCreateCommand<Section>(this.PopulateSection));

            this.DeleteSectionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSectionCommand);
            this.DeleteSectionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSection.Thing, this.PopulateSection));

            this.EditSectionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSectionCommand);
            this.EditSectionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSection.Thing, this.PopulateSection));

            this.InspectSectionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedSectionCommand);
            this.InspectSectionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSection.Thing));

            this.MoveUpSectionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSectionCommand);
            this.MoveUpSectionCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.Section, this.SelectedSection));

            this.MoveDownSectionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSectionCommand);
            this.MoveDownSectionCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.Section, this.SelectedSection));
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

            clone.ShortName = this.ShortName;
            clone.Name = this.Name;
            clone.CreatedOn = this.CreatedOn;
            clone.Owner = this.SelectedOwner;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);


            if (!clone.Section.SortedItems.Values.SequenceEqual(this.Section.Select(x => x.Thing)))
            {
                var itemCount = this.Section.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.Section[i].Thing;
                    var currentIndex = clone.Section.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.Section.Move(currentIndex, i);
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
            this.Section = new ReactiveList<SectionRowViewModel>();
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
            this.PopulateSection();
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
        /// Populates the <see cref="Section"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateSection()
        {
            this.Section.Clear();
            foreach (Section thing in this.Thing.Section.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SectionRowViewModel(thing, this.Session, this);
                this.Section.Add(row);
                row.Index = this.Thing.Section.IndexOf(thing);
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
            foreach(var section in this.Section)
            {
                section.Dispose();
            }
        }
    }
}
