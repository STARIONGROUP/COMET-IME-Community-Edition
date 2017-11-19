// -------------------------------------------------------------------------------------------------
// <copyright file="ModellingAnnotationItemDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ModellingAnnotationItem"/>
    /// </summary>
    public abstract partial class ModellingAnnotationItemDialogViewModel<T> : EngineeringModelDataAnnotationDialogViewModel<T> where T : ModellingAnnotationItem
    {
        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private AnnotationStatusKind status;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="Classification"/>
        /// </summary>
        private AnnotationClassificationKind classification;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedApprovedBy"/>
        /// </summary>
        private ApprovalRowViewModel selectedApprovedBy;


        /// <summary>
        /// Initializes a new instance of the <see cref="ModellingAnnotationItemDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ModellingAnnotationItemDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModellingAnnotationItemDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="modellingAnnotationItem">
        /// The <see cref="ModellingAnnotationItem"/> that is the subject of the current view-model. This is the object
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
        protected ModellingAnnotationItemDialogViewModel(T modellingAnnotationItem, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(modellingAnnotationItem, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the Status
        /// </summary>
        public virtual AnnotationStatusKind Status
        {
            get { return this.status; }
            set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public virtual string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        /// <summary>
        /// Gets or sets the Classification
        /// </summary>
        public virtual AnnotationClassificationKind Classification
        {
            get { return this.classification; }
            set { this.RaiseAndSetIfChanged(ref this.classification, value); }
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
        /// Gets or sets the selected <see cref="ApprovalRowViewModel"/>
        /// </summary>
        public ApprovalRowViewModel SelectedApprovedBy
        {
            get { return this.selectedApprovedBy; }
            set { this.RaiseAndSetIfChanged(ref this.selectedApprovedBy, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Approval"/>
        /// </summary>
        public ReactiveList<ApprovalRowViewModel> ApprovedBy { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="SourceAnnotation"/>s
        /// </summary>
        private ReactiveList<ModellingAnnotationItem> sourceAnnotation;

        /// <summary>
        /// Gets or sets the list of selected <see cref="ModellingAnnotationItem"/>s
        /// </summary>
        public ReactiveList<ModellingAnnotationItem> SourceAnnotation 
        { 
            get { return this.sourceAnnotation; } 
            set { this.RaiseAndSetIfChanged(ref this.sourceAnnotation, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="ModellingAnnotationItem"/> for <see cref="SourceAnnotation"/>
        /// </summary>
        public ReactiveList<ModellingAnnotationItem> PossibleSourceAnnotation { get; protected set; }
        
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
        /// Gets or sets the Create <see cref="ICommand"/> to create a Approval
        /// </summary>
        public ReactiveCommand<object> CreateApprovedByCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Approval
        /// </summary>
        public ReactiveCommand<object> DeleteApprovedByCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Approval
        /// </summary>
        public ReactiveCommand<object> EditApprovedByCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Approval
        /// </summary>
        public ReactiveCommand<object> InspectApprovedByCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateApprovedByCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedApprovedByCommand = this.WhenAny(vm => vm.SelectedApprovedBy, v => v.Value != null);
            var canExecuteEditSelectedApprovedByCommand = this.WhenAny(vm => vm.SelectedApprovedBy, v => v.Value != null && !this.IsReadOnly);

            this.CreateApprovedByCommand = ReactiveCommand.Create(canExecuteCreateApprovedByCommand);
            this.CreateApprovedByCommand.Subscribe(_ => this.ExecuteCreateCommand<Approval>(this.PopulateApprovedBy));

            this.DeleteApprovedByCommand = ReactiveCommand.Create(canExecuteEditSelectedApprovedByCommand);
            this.DeleteApprovedByCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedApprovedBy.Thing, this.PopulateApprovedBy));

            this.EditApprovedByCommand = ReactiveCommand.Create(canExecuteEditSelectedApprovedByCommand);
            this.EditApprovedByCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedApprovedBy.Thing, this.PopulateApprovedBy));

            this.InspectApprovedByCommand = ReactiveCommand.Create(canExecuteInspectSelectedApprovedByCommand);
            this.InspectApprovedByCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedApprovedBy.Thing));
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

            clone.Status = this.Status;
            clone.Title = this.Title;
            clone.Classification = this.Classification;
            clone.ShortName = this.ShortName;
            clone.Owner = this.SelectedOwner;
            clone.SourceAnnotation.Clear();
            clone.SourceAnnotation.AddRange(this.SourceAnnotation);

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
            this.ApprovedBy = new ReactiveList<ApprovalRowViewModel>();
            this.SourceAnnotation = new ReactiveList<ModellingAnnotationItem>();
            this.PossibleSourceAnnotation = new ReactiveList<ModellingAnnotationItem>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Status = this.Thing.Status;
            this.Title = this.Thing.Title;
            this.Classification = this.Thing.Classification;
            this.ShortName = this.Thing.ShortName;
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateApprovedBy();
            this.PopulateSourceAnnotation();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="SourceAnnotation"/> property
        /// </summary>
        protected virtual void PopulateSourceAnnotation()
        {
            this.SourceAnnotation.Clear();

            foreach (var value in this.Thing.SourceAnnotation)
            {
                this.SourceAnnotation.Add(value);
            }
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
        /// Populates the <see cref="ApprovedBy"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateApprovedBy()
        {
            this.ApprovedBy.Clear();
            foreach (var thing in this.Thing.ApprovedBy.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ApprovalRowViewModel(thing, this.Session, this);
                this.ApprovedBy.Add(row);
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
            foreach(var approvedBy in this.ApprovedBy)
            {
                approvedBy.Dispose();
            }
        }
    }
}
