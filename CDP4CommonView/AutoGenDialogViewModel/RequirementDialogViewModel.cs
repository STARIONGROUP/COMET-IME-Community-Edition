// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="Requirement"/>
    /// </summary>
    public partial class RequirementDialogViewModel : SimpleParameterizableThingDialogViewModel<Requirement>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedGroup"/>
        /// </summary>
        private RequirementsGroup selectedGroup;

        /// <summary>
        /// Backing field for <see cref="SelectedParametricConstraint"/>
        /// </summary>
        private ParametricConstraintRowViewModel selectedParametricConstraint;


        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RequirementDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDialogViewModel"/> class
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that is the subject of the current view-model. This is the object
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
        public RequirementDialogViewModel(Requirement requirement, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(requirement, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as RequirementsSpecification;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type RequirementsSpecification",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedGroup
        /// </summary>
        public virtual RequirementsGroup SelectedGroup
        {
            get { return this.selectedGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGroup, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="RequirementsGroup"/>s for <see cref="SelectedGroup"/>
        /// </summary>
        public ReactiveList<RequirementsGroup> PossibleGroup { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParametricConstraintRowViewModel"/>
        /// </summary>
        public ParametricConstraintRowViewModel SelectedParametricConstraint
        {
            get { return this.selectedParametricConstraint; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParametricConstraint, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParametricConstraint"/>
        /// </summary>
        public ReactiveList<ParametricConstraintRowViewModel> ParametricConstraint { get; protected set; }
        
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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedGroup"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParametricConstraint
        /// </summary>
        public ReactiveCommand<object> CreateParametricConstraintCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParametricConstraint
        /// </summary>
        public ReactiveCommand<object> DeleteParametricConstraintCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParametricConstraint
        /// </summary>
        public ReactiveCommand<object> EditParametricConstraintCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParametricConstraint
        /// </summary>
        public ReactiveCommand<object> InspectParametricConstraintCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a ParametricConstraint 
        /// </summary>
        public ReactiveCommand<object> MoveUpParametricConstraintCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a ParametricConstraint
        /// </summary>
        public ReactiveCommand<object> MoveDownParametricConstraintCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParametricConstraintCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParametricConstraintCommand = this.WhenAny(vm => vm.SelectedParametricConstraint, v => v.Value != null);
            var canExecuteEditSelectedParametricConstraintCommand = this.WhenAny(vm => vm.SelectedParametricConstraint, v => v.Value != null && !this.IsReadOnly);

            this.CreateParametricConstraintCommand = ReactiveCommand.Create(canExecuteCreateParametricConstraintCommand);
            this.CreateParametricConstraintCommand.Subscribe(_ => this.ExecuteCreateCommand<ParametricConstraint>(this.PopulateParametricConstraint));

            this.DeleteParametricConstraintCommand = ReactiveCommand.Create(canExecuteEditSelectedParametricConstraintCommand);
            this.DeleteParametricConstraintCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParametricConstraint.Thing, this.PopulateParametricConstraint));

            this.EditParametricConstraintCommand = ReactiveCommand.Create(canExecuteEditSelectedParametricConstraintCommand);
            this.EditParametricConstraintCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParametricConstraint.Thing, this.PopulateParametricConstraint));

            this.InspectParametricConstraintCommand = ReactiveCommand.Create(canExecuteInspectSelectedParametricConstraintCommand);
            this.InspectParametricConstraintCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParametricConstraint.Thing));

            this.MoveUpParametricConstraintCommand = ReactiveCommand.Create(canExecuteEditSelectedParametricConstraintCommand);
            this.MoveUpParametricConstraintCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.ParametricConstraint, this.SelectedParametricConstraint));

            this.MoveDownParametricConstraintCommand = ReactiveCommand.Create(canExecuteEditSelectedParametricConstraintCommand);
            this.MoveDownParametricConstraintCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.ParametricConstraint, this.SelectedParametricConstraint));
            var canExecuteInspectSelectedGroupCommand = this.WhenAny(vm => vm.SelectedGroup, v => v.Value != null);
            this.InspectSelectedGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedGroupCommand);
            this.InspectSelectedGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGroup));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsDeprecated = this.IsDeprecated;
            clone.Group = this.SelectedGroup;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);


            if (!clone.ParametricConstraint.SortedItems.Values.SequenceEqual(this.ParametricConstraint.Select(x => x.Thing)))
            {
                var itemCount = this.ParametricConstraint.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.ParametricConstraint[i].Thing;
                    var currentIndex = clone.ParametricConstraint.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.ParametricConstraint.Move(currentIndex, i);
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
            this.PossibleGroup = new ReactiveList<RequirementsGroup>();
            this.ParametricConstraint = new ReactiveList<ParametricConstraintRowViewModel>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.SelectedGroup = this.Thing.Group;
            this.PopulatePossibleGroup();
            this.PopulateParametricConstraint();
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
        /// Populates the <see cref="ParametricConstraint"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParametricConstraint()
        {
            this.ParametricConstraint.Clear();
            foreach (ParametricConstraint thing in this.Thing.ParametricConstraint.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParametricConstraintRowViewModel(thing, this.Session, this);
                this.ParametricConstraint.Add(row);
                row.Index = this.Thing.ParametricConstraint.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleGroup"/> property
        /// </summary>
        protected virtual void PopulatePossibleGroup()
        {
            this.PossibleGroup.Clear();
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
            foreach(var parametricConstraint in this.ParametricConstraint)
            {
                parametricConstraint.Dispose();
            }
        }
    }
}
