// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelDataAnnotationDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="EngineeringModelDataAnnotation"/>
    /// </summary>
    public abstract partial class EngineeringModelDataAnnotationDialogViewModel<T> : GenericAnnotationDialogViewModel<T> where T : EngineeringModelDataAnnotation
    {
        /// <summary>
        /// Backing field for <see cref="SelectedAuthor"/>
        /// </summary>
        private Participant selectedAuthor;

        /// <summary>
        /// Backing field for <see cref="SelectedPrimaryAnnotatedThing"/>
        /// </summary>
        private ModellingThingReference selectedPrimaryAnnotatedThing;

        /// <summary>
        /// Backing field for <see cref="SelectedRelatedThing"/>
        /// </summary>
        private ModellingThingReferenceRowViewModel selectedRelatedThing;

        /// <summary>
        /// Backing field for <see cref="SelectedDiscussion"/>
        /// </summary>
        private EngineeringModelDataDiscussionItemRowViewModel selectedDiscussion;


        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelDataAnnotationDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected EngineeringModelDataAnnotationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelDataAnnotationDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="engineeringModelDataAnnotation">
        /// The <see cref="EngineeringModelDataAnnotation"/> that is the subject of the current view-model. This is the object
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
        protected EngineeringModelDataAnnotationDialogViewModel(T engineeringModelDataAnnotation, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(engineeringModelDataAnnotation, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the SelectedAuthor
        /// </summary>
        public virtual Participant SelectedAuthor
        {
            get { return this.selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAuthor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Participant"/>s for <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveList<Participant> PossibleAuthor { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedPrimaryAnnotatedThing
        /// </summary>
        public virtual ModellingThingReference SelectedPrimaryAnnotatedThing
        {
            get { return this.selectedPrimaryAnnotatedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPrimaryAnnotatedThing, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ModellingThingReference"/>s for <see cref="SelectedPrimaryAnnotatedThing"/>
        /// </summary>
        public ReactiveList<ModellingThingReference> PossiblePrimaryAnnotatedThing { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ModellingThingReferenceRowViewModel"/>
        /// </summary>
        public ModellingThingReferenceRowViewModel SelectedRelatedThing
        {
            get { return this.selectedRelatedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelatedThing, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ModellingThingReference"/>
        /// </summary>
        public ReactiveList<ModellingThingReferenceRowViewModel> RelatedThing { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="EngineeringModelDataDiscussionItemRowViewModel"/>
        /// </summary>
        public EngineeringModelDataDiscussionItemRowViewModel SelectedDiscussion
        {
            get { return this.selectedDiscussion; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDiscussion, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="EngineeringModelDataDiscussionItem"/>
        /// </summary>
        public ReactiveList<EngineeringModelDataDiscussionItemRowViewModel> Discussion { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedAuthorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedPrimaryAnnotatedThing"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedPrimaryAnnotatedThingCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ModellingThingReference
        /// </summary>
        public ReactiveCommand<object> CreateRelatedThingCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ModellingThingReference
        /// </summary>
        public ReactiveCommand<object> DeleteRelatedThingCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ModellingThingReference
        /// </summary>
        public ReactiveCommand<object> EditRelatedThingCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ModellingThingReference
        /// </summary>
        public ReactiveCommand<object> InspectRelatedThingCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a EngineeringModelDataDiscussionItem
        /// </summary>
        public ReactiveCommand<object> CreateDiscussionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a EngineeringModelDataDiscussionItem
        /// </summary>
        public ReactiveCommand<object> DeleteDiscussionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a EngineeringModelDataDiscussionItem
        /// </summary>
        public ReactiveCommand<object> EditDiscussionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a EngineeringModelDataDiscussionItem
        /// </summary>
        public ReactiveCommand<object> InspectDiscussionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateRelatedThingCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRelatedThingCommand = this.WhenAny(vm => vm.SelectedRelatedThing, v => v.Value != null);
            var canExecuteEditSelectedRelatedThingCommand = this.WhenAny(vm => vm.SelectedRelatedThing, v => v.Value != null && !this.IsReadOnly);

            this.CreateRelatedThingCommand = ReactiveCommand.Create(canExecuteCreateRelatedThingCommand);
            this.CreateRelatedThingCommand.Subscribe(_ => this.ExecuteCreateCommand<ModellingThingReference>(this.PopulateRelatedThing));

            this.DeleteRelatedThingCommand = ReactiveCommand.Create(canExecuteEditSelectedRelatedThingCommand);
            this.DeleteRelatedThingCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRelatedThing.Thing, this.PopulateRelatedThing));

            this.EditRelatedThingCommand = ReactiveCommand.Create(canExecuteEditSelectedRelatedThingCommand);
            this.EditRelatedThingCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRelatedThing.Thing, this.PopulateRelatedThing));

            this.InspectRelatedThingCommand = ReactiveCommand.Create(canExecuteInspectSelectedRelatedThingCommand);
            this.InspectRelatedThingCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRelatedThing.Thing));
            
            var canExecuteCreateDiscussionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedDiscussionCommand = this.WhenAny(vm => vm.SelectedDiscussion, v => v.Value != null);
            var canExecuteEditSelectedDiscussionCommand = this.WhenAny(vm => vm.SelectedDiscussion, v => v.Value != null && !this.IsReadOnly);

            this.CreateDiscussionCommand = ReactiveCommand.Create(canExecuteCreateDiscussionCommand);
            this.CreateDiscussionCommand.Subscribe(_ => this.ExecuteCreateCommand<EngineeringModelDataDiscussionItem>(this.PopulateDiscussion));

            this.DeleteDiscussionCommand = ReactiveCommand.Create(canExecuteEditSelectedDiscussionCommand);
            this.DeleteDiscussionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedDiscussion.Thing, this.PopulateDiscussion));

            this.EditDiscussionCommand = ReactiveCommand.Create(canExecuteEditSelectedDiscussionCommand);
            this.EditDiscussionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedDiscussion.Thing, this.PopulateDiscussion));

            this.InspectDiscussionCommand = ReactiveCommand.Create(canExecuteInspectSelectedDiscussionCommand);
            this.InspectDiscussionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDiscussion.Thing));
            var canExecuteInspectSelectedAuthorCommand = this.WhenAny(vm => vm.SelectedAuthor, v => v.Value != null);
            this.InspectSelectedAuthorCommand = ReactiveCommand.Create(canExecuteInspectSelectedAuthorCommand);
            this.InspectSelectedAuthorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAuthor));
            var canExecuteInspectSelectedPrimaryAnnotatedThingCommand = this.WhenAny(vm => vm.SelectedPrimaryAnnotatedThing, v => v.Value != null);
            this.InspectSelectedPrimaryAnnotatedThingCommand = ReactiveCommand.Create(canExecuteInspectSelectedPrimaryAnnotatedThingCommand);
            this.InspectSelectedPrimaryAnnotatedThingCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPrimaryAnnotatedThing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Author = this.SelectedAuthor;
            clone.PrimaryAnnotatedThing = this.SelectedPrimaryAnnotatedThing;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleAuthor = new ReactiveList<Participant>();
            this.PossiblePrimaryAnnotatedThing = new ReactiveList<ModellingThingReference>();
            this.RelatedThing = new ReactiveList<ModellingThingReferenceRowViewModel>();
            this.Discussion = new ReactiveList<EngineeringModelDataDiscussionItemRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedAuthor = this.Thing.Author;
            this.PopulatePossibleAuthor();
            this.SelectedPrimaryAnnotatedThing = this.Thing.PrimaryAnnotatedThing;
            this.PopulatePossiblePrimaryAnnotatedThing();
            this.PopulateRelatedThing();
            this.PopulateDiscussion();
        }

        /// <summary>
        /// Populates the <see cref="RelatedThing"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRelatedThing()
        {
            this.RelatedThing.Clear();
            foreach (var thing in this.Thing.RelatedThing.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ModellingThingReferenceRowViewModel(thing, this.Session, this);
                this.RelatedThing.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Discussion"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateDiscussion()
        {
            this.Discussion.Clear();
            foreach (var thing in this.Thing.Discussion.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EngineeringModelDataDiscussionItemRowViewModel(thing, this.Session, this);
                this.Discussion.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleAuthor"/> property
        /// </summary>
        protected virtual void PopulatePossibleAuthor()
        {
            this.PossibleAuthor.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossiblePrimaryAnnotatedThing"/> property
        /// </summary>
        protected virtual void PopulatePossiblePrimaryAnnotatedThing()
        {
            this.PossiblePrimaryAnnotatedThing.Clear();
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
            foreach(var relatedThing in this.RelatedThing)
            {
                relatedThing.Dispose();
            }
            foreach(var discussion in this.Discussion)
            {
                discussion.Dispose();
            }
        }
    }
}
