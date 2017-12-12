// -------------------------------------------------------------------------------------------------
// <copyright file="ReferencerRuleDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="ReferencerRule"/>
    /// </summary>
    public partial class ReferencerRuleDialogViewModel : RuleDialogViewModel<ReferencerRule>
    {
        /// <summary>
        /// Backing field for <see cref="MinReferenced"/>
        /// </summary>
        private int minReferenced;

        /// <summary>
        /// Backing field for <see cref="MaxReferenced"/>
        /// </summary>
        private int maxReferenced;

        /// <summary>
        /// Backing field for <see cref="SelectedReferencingCategory"/>
        /// </summary>
        private Category selectedReferencingCategory;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencerRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ReferencerRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencerRuleDialogViewModel"/> class
        /// </summary>
        /// <param name="referencerRule">
        /// The <see cref="ReferencerRule"/> that is the subject of the current view-model. This is the object
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
        public ReferencerRuleDialogViewModel(ReferencerRule referencerRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(referencerRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the MinReferenced
        /// </summary>
        public virtual int MinReferenced
        {
            get { return this.minReferenced; }
            set { this.RaiseAndSetIfChanged(ref this.minReferenced, value); }
        }

        /// <summary>
        /// Gets or sets the MaxReferenced
        /// </summary>
        public virtual int MaxReferenced
        {
            get { return this.maxReferenced; }
            set { this.RaiseAndSetIfChanged(ref this.maxReferenced, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedReferencingCategory
        /// </summary>
        public virtual Category SelectedReferencingCategory
        {
            get { return this.selectedReferencingCategory; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferencingCategory, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s for <see cref="SelectedReferencingCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleReferencingCategory { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ReferencedCategory"/>s
        /// </summary>
        private ReactiveList<Category> referencedCategory;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> ReferencedCategory 
        { 
            get { return this.referencedCategory; } 
            set { this.RaiseAndSetIfChanged(ref this.referencedCategory, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="ReferencedCategory"/>
        /// </summary>
        public ReactiveList<Category> PossibleReferencedCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedReferencingCategory"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedReferencingCategoryCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedReferencingCategoryCommand = this.WhenAny(vm => vm.SelectedReferencingCategory, v => v.Value != null);
            this.InspectSelectedReferencingCategoryCommand = ReactiveCommand.Create(canExecuteInspectSelectedReferencingCategoryCommand);
            this.InspectSelectedReferencingCategoryCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferencingCategory));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.MinReferenced = this.MinReferenced;
            clone.MaxReferenced = this.MaxReferenced;
            clone.ReferencingCategory = this.SelectedReferencingCategory;
            clone.ReferencedCategory.Clear();
            clone.ReferencedCategory.AddRange(this.ReferencedCategory);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleReferencingCategory = new ReactiveList<Category>();
            this.ReferencedCategory = new ReactiveList<Category>();
            this.PossibleReferencedCategory = new ReactiveList<Category>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="ReferencerRule"/>
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
            this.MinReferenced = this.Thing.MinReferenced;
            this.MaxReferenced = this.Thing.MaxReferenced;
            this.SelectedReferencingCategory = this.Thing.ReferencingCategory;
            this.PopulatePossibleReferencingCategory();
            this.PopulateReferencedCategory();
        }

        /// <summary>
        /// Populates the <see cref="ReferencedCategory"/> property
        /// </summary>
        protected virtual void PopulateReferencedCategory()
        {
            this.ReferencedCategory.Clear();

            foreach (var value in this.Thing.ReferencedCategory)
            {
                this.ReferencedCategory.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossibleReferencingCategory"/> property
        /// </summary>
        protected virtual void PopulatePossibleReferencingCategory()
        {
            this.PossibleReferencingCategory.Clear();
        }
    }
}
