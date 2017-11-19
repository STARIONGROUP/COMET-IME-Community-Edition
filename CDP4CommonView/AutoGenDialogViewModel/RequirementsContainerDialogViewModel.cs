// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsContainerDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="RequirementsContainer"/>
    /// </summary>
    public abstract partial class RequirementsContainerDialogViewModel<T> : DefinedThingDialogViewModel<T> where T : RequirementsContainer
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedGroup"/>
        /// </summary>
        private RequirementsGroupRowViewModel selectedGroup;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterValue"/>
        /// </summary>
        private RequirementsContainerParameterValueRowViewModel selectedParameterValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected RequirementsContainerDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="requirementsContainer">
        /// The <see cref="RequirementsContainer"/> that is the subject of the current view-model. This is the object
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
        protected RequirementsContainerDialogViewModel(T requirementsContainer, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(requirementsContainer, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
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
        /// Gets or sets the selected <see cref="RequirementsGroupRowViewModel"/>
        /// </summary>
        public RequirementsGroupRowViewModel SelectedGroup
        {
            get { return this.selectedGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGroup, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RequirementsGroup"/>
        /// </summary>
        public ReactiveList<RequirementsGroupRowViewModel> Group { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="RequirementsContainerParameterValueRowViewModel"/>
        /// </summary>
        public RequirementsContainerParameterValueRowViewModel SelectedParameterValue
        {
            get { return this.selectedParameterValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterValue, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RequirementsContainerParameterValue"/>
        /// </summary>
        public ReactiveList<RequirementsContainerParameterValueRowViewModel> ParameterValue { get; protected set; }
        
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
        public ReactiveCommand<object> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RequirementsGroup
        /// </summary>
        public ReactiveCommand<object> CreateGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RequirementsGroup
        /// </summary>
        public ReactiveCommand<object> DeleteGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RequirementsGroup
        /// </summary>
        public ReactiveCommand<object> EditGroupCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RequirementsGroup
        /// </summary>
        public ReactiveCommand<object> InspectGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RequirementsContainerParameterValue
        /// </summary>
        public ReactiveCommand<object> CreateParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RequirementsContainerParameterValue
        /// </summary>
        public ReactiveCommand<object> DeleteParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RequirementsContainerParameterValue
        /// </summary>
        public ReactiveCommand<object> EditParameterValueCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RequirementsContainerParameterValue
        /// </summary>
        public ReactiveCommand<object> InspectParameterValueCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateGroupCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedGroupCommand = this.WhenAny(vm => vm.SelectedGroup, v => v.Value != null);
            var canExecuteEditSelectedGroupCommand = this.WhenAny(vm => vm.SelectedGroup, v => v.Value != null && !this.IsReadOnly);

            this.CreateGroupCommand = ReactiveCommand.Create(canExecuteCreateGroupCommand);
            this.CreateGroupCommand.Subscribe(_ => this.ExecuteCreateCommand<RequirementsGroup>(this.PopulateGroup));

            this.DeleteGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedGroupCommand);
            this.DeleteGroupCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedGroup.Thing, this.PopulateGroup));

            this.EditGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedGroupCommand);
            this.EditGroupCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedGroup.Thing, this.PopulateGroup));

            this.InspectGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedGroupCommand);
            this.InspectGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGroup.Thing));
            
            var canExecuteCreateParameterValueCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterValueCommand = this.WhenAny(vm => vm.SelectedParameterValue, v => v.Value != null);
            var canExecuteEditSelectedParameterValueCommand = this.WhenAny(vm => vm.SelectedParameterValue, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterValueCommand = ReactiveCommand.Create(canExecuteCreateParameterValueCommand);
            this.CreateParameterValueCommand.Subscribe(_ => this.ExecuteCreateCommand<RequirementsContainerParameterValue>(this.PopulateParameterValue));

            this.DeleteParameterValueCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterValueCommand);
            this.DeleteParameterValueCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterValue.Thing, this.PopulateParameterValue));

            this.EditParameterValueCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterValueCommand);
            this.EditParameterValueCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterValue.Thing, this.PopulateParameterValue));

            this.InspectParameterValueCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterValueCommand);
            this.InspectParameterValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterValue.Thing));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommand.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

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
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.Group = new ReactiveList<RequirementsGroupRowViewModel>();
            this.ParameterValue = new ReactiveList<RequirementsContainerParameterValueRowViewModel>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateGroup();
            this.PopulateParameterValue();
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
        /// Populates the <see cref="Group"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateGroup()
        {
            this.Group.Clear();
            foreach (var thing in this.Thing.Group.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RequirementsGroupRowViewModel(thing, this.Session, this);
                this.Group.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ParameterValue"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterValue()
        {
            this.ParameterValue.Clear();
            foreach (var thing in this.Thing.ParameterValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RequirementsContainerParameterValueRowViewModel(thing, this.Session, this);
                this.ParameterValue.Add(row);
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
            foreach(var group in this.Group)
            {
                group.Dispose();
            }
            foreach(var parameterValue in this.ParameterValue)
            {
                parameterValue.Dispose();
            }
        }
    }
}
