// -------------------------------------------------------------------------------------------------
// <copyright file="DecompositionRuleDialogViewModel.cs" company="RHEA System S.A.">
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
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DecompositionRule"/>
    /// </summary>
    public partial class DecompositionRuleDialogViewModel : RuleDialogViewModel<DecompositionRule>
    {
        /// <summary>
        /// Backing field for <see cref="MinContained"/>
        /// </summary>
        private int minContained;

        /// <summary>
        /// Backing field for <see cref="MaxContained"/>
        /// </summary>
        private int? maxContained;

        /// <summary>
        /// Backing field for <see cref="SelectedContainingCategory"/>
        /// </summary>
        private Category selectedContainingCategory;


        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DecompositionRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionRuleDialogViewModel"/> class
        /// </summary>
        /// <param name="decompositionRule">
        /// The <see cref="DecompositionRule"/> that is the subject of the current view-model. This is the object
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
        public DecompositionRuleDialogViewModel(DecompositionRule decompositionRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(decompositionRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the MinContained
        /// </summary>
        public virtual int MinContained
        {
            get { return this.minContained; }
            set { this.RaiseAndSetIfChanged(ref this.minContained, value); }
        }

        /// <summary>
        /// Gets or sets the MaxContained
        /// </summary>
        public virtual int? MaxContained
        {
            get { return this.maxContained; }
            set { this.RaiseAndSetIfChanged(ref this.maxContained, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedContainingCategory
        /// </summary>
        public virtual Category SelectedContainingCategory
        {
            get { return this.selectedContainingCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedContainingCategory, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s for <see cref="SelectedContainingCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleContainingCategory { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ContainedCategory"/>s
        /// </summary>
        private ReactiveList<Category> containedCategory;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> ContainedCategory 
        { 
            get { return this.containedCategory; } 
            set { this.RaiseAndSetIfChanged(ref this.containedCategory, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="ContainedCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleContainedCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedContainingCategory"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedContainingCategoryCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedContainingCategoryCommand = this.WhenAny(vm => vm.SelectedContainingCategory, v => v.Value != null);
            this.InspectSelectedContainingCategoryCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedContainingCategoryCommand);
            this.InspectSelectedContainingCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedContainingCategory));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.MinContained = this.MinContained;
            clone.MaxContained = this.MaxContained;
            clone.ContainingCategory = this.SelectedContainingCategory;
            clone.ContainedCategory.Clear();
            clone.ContainedCategory.AddRange(this.ContainedCategory);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleContainingCategory = new ReactiveList<Category>();
            this.ContainedCategory = new ReactiveList<Category>();
            this.PossibleContainedCategory = new ReactiveList<Category>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="DecompositionRule"/>
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
            this.MinContained = this.Thing.MinContained;
            this.MaxContained = this.Thing.MaxContained;
            this.SelectedContainingCategory = this.Thing.ContainingCategory;
            this.PopulatePossibleContainingCategory();
            this.PopulateContainedCategory();
        }

        /// <summary>
        /// Populates the <see cref="ContainedCategory"/> property
        /// </summary>
        protected virtual void PopulateContainedCategory()
        {
            this.ContainedCategory.Clear();

            foreach (var value in this.Thing.ContainedCategory)
            {
                this.ContainedCategory.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossibleContainingCategory"/> property
        /// </summary>
        protected virtual void PopulatePossibleContainingCategory()
        {
            this.PossibleContainingCategory.Clear();
        }
    }
}
