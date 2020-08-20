// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRuleDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="BinaryRelationshipRule"/>
    /// </summary>
    public partial class BinaryRelationshipRuleDialogViewModel : RuleDialogViewModel<BinaryRelationshipRule>
    {
        /// <summary>
        /// Backing field for <see cref="ForwardRelationshipName"/>
        /// </summary>
        private string forwardRelationshipName;

        /// <summary>
        /// Backing field for <see cref="InverseRelationshipName"/>
        /// </summary>
        private string inverseRelationshipName;

        /// <summary>
        /// Backing field for <see cref="SelectedRelationshipCategory"/>
        /// </summary>
        private Category selectedRelationshipCategory;

        /// <summary>
        /// Backing field for <see cref="SelectedSourceCategory"/>
        /// </summary>
        private Category selectedSourceCategory;

        /// <summary>
        /// Backing field for <see cref="SelectedTargetCategory"/>
        /// </summary>
        private Category selectedTargetCategory;


        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BinaryRelationshipRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRuleDialogViewModel"/> class
        /// </summary>
        /// <param name="binaryRelationshipRule">
        /// The <see cref="BinaryRelationshipRule"/> that is the subject of the current view-model. This is the object
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
        public BinaryRelationshipRuleDialogViewModel(BinaryRelationshipRule binaryRelationshipRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(binaryRelationshipRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ReferenceDataLibrary",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ForwardRelationshipName
        /// </summary>
        public virtual string ForwardRelationshipName
        {
            get { return this.forwardRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.forwardRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the InverseRelationshipName
        /// </summary>
        public virtual string InverseRelationshipName
        {
            get { return this.inverseRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.inverseRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedRelationshipCategory
        /// </summary>
        public virtual Category SelectedRelationshipCategory
        {
            get { return this.selectedRelationshipCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelationshipCategory, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s for <see cref="SelectedRelationshipCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleRelationshipCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedSourceCategory
        /// </summary>
        public virtual Category SelectedSourceCategory
        {
            get { return this.selectedSourceCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSourceCategory, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s for <see cref="SelectedSourceCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleSourceCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedTargetCategory
        /// </summary>
        public virtual Category SelectedTargetCategory
        {
            get { return this.selectedTargetCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTargetCategory, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s for <see cref="SelectedTargetCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleTargetCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRelationshipCategory"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedRelationshipCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSourceCategory"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedSourceCategoryCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedTargetCategory"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedTargetCategoryCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedRelationshipCategoryCommand = this.WhenAny(vm => vm.SelectedRelationshipCategory, v => v.Value != null);
            this.InspectSelectedRelationshipCategoryCommand = ReactiveCommand.Create(canExecuteInspectSelectedRelationshipCategoryCommand);
            this.InspectSelectedRelationshipCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRelationshipCategory));
            var canExecuteInspectSelectedSourceCategoryCommand = this.WhenAny(vm => vm.SelectedSourceCategory, v => v.Value != null);
            this.InspectSelectedSourceCategoryCommand = ReactiveCommand.Create(canExecuteInspectSelectedSourceCategoryCommand);
            this.InspectSelectedSourceCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSourceCategory));
            var canExecuteInspectSelectedTargetCategoryCommand = this.WhenAny(vm => vm.SelectedTargetCategory, v => v.Value != null);
            this.InspectSelectedTargetCategoryCommand = ReactiveCommand.Create(canExecuteInspectSelectedTargetCategoryCommand);
            this.InspectSelectedTargetCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTargetCategory));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ForwardRelationshipName = this.ForwardRelationshipName;
            clone.InverseRelationshipName = this.InverseRelationshipName;
            clone.RelationshipCategory = this.SelectedRelationshipCategory;
            clone.SourceCategory = this.SelectedSourceCategory;
            clone.TargetCategory = this.SelectedTargetCategory;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleRelationshipCategory = new ReactiveList<Category>();
            this.PossibleSourceCategory = new ReactiveList<Category>();
            this.PossibleTargetCategory = new ReactiveList<Category>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="BinaryRelationshipRule"/>
        /// </summary>
        private void PopulatePossibleContainer()
        {
            this.PossibleContainer.Clear();
            // When creating a new Rule, it can be contained by any ReferenceDataLibrary that is currently loaded
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleContainer.AddRange(this.Session.OpenReferenceDataLibraries.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
                this.Container = this.PossibleContainer.FirstOrDefault();
                return;
            }

            // When inspecting an existing Rule, only it's container needs to be added to the PossibleContainer property (it cannot be changed)
            if (this.dialogKind == ThingDialogKind.Inspect)
            {
                this.PossibleContainer.Add(this.Thing.Container);
                this.Container = this.Thing.Container;
                return;
            }

            // When updating a Rule, the possible ReferenceDataLibrary can only be the ReferenceDataLibrary in the current chain of ReferenceDataLibrary of the Rule
            if (this.dialogKind == ThingDialogKind.Update)
            {
                var containerRdl = (ReferenceDataLibrary)this.Container;
                this.PossibleContainer.Add(containerRdl);
                var chainOfRdls = containerRdl.GetRequiredRdls();
                this.PossibleContainer.AddRange(chainOfRdls.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ForwardRelationshipName = this.Thing.ForwardRelationshipName;
            this.InverseRelationshipName = this.Thing.InverseRelationshipName;
            this.SelectedRelationshipCategory = this.Thing.RelationshipCategory;
            this.PopulatePossibleRelationshipCategory();
            this.SelectedSourceCategory = this.Thing.SourceCategory;
            this.PopulatePossibleSourceCategory();
            this.SelectedTargetCategory = this.Thing.TargetCategory;
            this.PopulatePossibleTargetCategory();
        }

        /// <summary>
        /// Populates the <see cref="PossibleRelationshipCategory"/> property
        /// </summary>
        protected virtual void PopulatePossibleRelationshipCategory()
        {
            this.PossibleRelationshipCategory.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSourceCategory"/> property
        /// </summary>
        protected virtual void PopulatePossibleSourceCategory()
        {
            this.PossibleSourceCategory.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleTargetCategory"/> property
        /// </summary>
        protected virtual void PopulatePossibleTargetCategory()
        {
            this.PossibleTargetCategory.Clear();
        }
    }
}
