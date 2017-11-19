// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="RequirementsSpecification"/>
    /// </summary>
    public partial class RequirementsSpecificationDialogViewModel : RequirementsContainerDialogViewModel<RequirementsSpecification>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedRequirement"/>
        /// </summary>
        private RequirementRowViewModel selectedRequirement;


        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RequirementsSpecificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialogViewModel"/> class
        /// </summary>
        /// <param name="requirementsSpecification">
        /// The <see cref="RequirementsSpecification"/> that is the subject of the current view-model. This is the object
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
        public RequirementsSpecificationDialogViewModel(RequirementsSpecification requirementsSpecification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(requirementsSpecification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Iteration;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Iteration",
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
        /// Gets or sets the selected <see cref="RequirementRowViewModel"/>
        /// </summary>
        public RequirementRowViewModel SelectedRequirement
        {
            get { return this.selectedRequirement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequirement, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Requirement"/>
        /// </summary>
        public ReactiveList<RequirementRowViewModel> Requirement { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Requirement
        /// </summary>
        public ReactiveCommand<object> CreateRequirementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Requirement
        /// </summary>
        public ReactiveCommand<object> DeleteRequirementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Requirement
        /// </summary>
        public ReactiveCommand<object> EditRequirementCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Requirement
        /// </summary>
        public ReactiveCommand<object> InspectRequirementCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateRequirementCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRequirementCommand = this.WhenAny(vm => vm.SelectedRequirement, v => v.Value != null);
            var canExecuteEditSelectedRequirementCommand = this.WhenAny(vm => vm.SelectedRequirement, v => v.Value != null && !this.IsReadOnly);

            this.CreateRequirementCommand = ReactiveCommand.Create(canExecuteCreateRequirementCommand);
            this.CreateRequirementCommand.Subscribe(_ => this.ExecuteCreateCommand<Requirement>(this.PopulateRequirement));

            this.DeleteRequirementCommand = ReactiveCommand.Create(canExecuteEditSelectedRequirementCommand);
            this.DeleteRequirementCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRequirement.Thing, this.PopulateRequirement));

            this.EditRequirementCommand = ReactiveCommand.Create(canExecuteEditSelectedRequirementCommand);
            this.EditRequirementCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRequirement.Thing, this.PopulateRequirement));

            this.InspectRequirementCommand = ReactiveCommand.Create(canExecuteInspectSelectedRequirementCommand);
            this.InspectRequirementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRequirement.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsDeprecated = this.IsDeprecated;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Requirement = new ReactiveList<RequirementRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.PopulateRequirement();
        }

        /// <summary>
        /// Populates the <see cref="Requirement"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRequirement()
        {
            this.Requirement.Clear();
            foreach (var thing in this.Thing.Requirement.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RequirementRowViewModel(thing, this.Session, this);
                this.Requirement.Add(row);
            }
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
            foreach(var requirement in this.Requirement)
            {
                requirement.Dispose();
            }
        }
    }
}
