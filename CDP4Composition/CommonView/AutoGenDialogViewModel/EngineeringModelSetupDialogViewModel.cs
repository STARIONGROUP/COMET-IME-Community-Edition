// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupDialogViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Composition.CommonView.ViewModels;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public partial class EngineeringModelSetupDialogViewModel : DefinedThingDialogViewModel<EngineeringModelSetup>
    {
        /// <summary>
        /// Backing field for <see cref="Kind"/>
        /// </summary>
        private EngineeringModelKind kind;

        /// <summary>
        /// Backing field for <see cref="StudyPhase"/>
        /// </summary>
        private StudyPhaseKind studyPhase;

        /// <summary>
        /// Backing field for <see cref="EngineeringModelIid"/>
        /// </summary>
        private Guid engineeringModelIid;

        /// <summary>
        /// Backing field for <see cref="SourceEngineeringModelSetupIid"/>
        /// </summary>
        private Guid? sourceEngineeringModelSetupIid;

        /// <summary>
        /// Backing field for <see cref="SelectedParticipant"/>
        /// </summary>
        private ParticipantRowViewModel selectedParticipant;

        /// <summary>
        /// Backing field for <see cref="SelectedRequiredRdl"/>
        /// </summary>
        private ModelReferenceDataLibraryRowViewModel selectedRequiredRdl;

        /// <summary>
        /// Backing field for <see cref="SelectedIterationSetup"/>
        /// </summary>
        private IterationSetupRowViewModel selectedIterationSetup;


        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public EngineeringModelSetupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialogViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The <see cref="EngineeringModelSetup"/> that is the subject of the current view-model. This is the object
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
        public EngineeringModelSetupDialogViewModel(EngineeringModelSetup engineeringModelSetup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(engineeringModelSetup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as SiteDirectory;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type SiteDirectory",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Kind
        /// </summary>
        public virtual EngineeringModelKind Kind
        {
            get { return this.kind; }
            set { this.RaiseAndSetIfChanged(ref this.kind, value); }
        }

        /// <summary>
        /// Gets or sets the StudyPhase
        /// </summary>
        public virtual StudyPhaseKind StudyPhase
        {
            get { return this.studyPhase; }
            set { this.RaiseAndSetIfChanged(ref this.studyPhase, value); }
        }

        /// <summary>
        /// Gets or sets the EngineeringModelIid
        /// </summary>
        public virtual Guid EngineeringModelIid
        {
            get { return this.engineeringModelIid; }
            set { this.RaiseAndSetIfChanged(ref this.engineeringModelIid, value); }
        }

        /// <summary>
        /// Gets or sets the SourceEngineeringModelSetupIid
        /// </summary>
        public virtual Guid? SourceEngineeringModelSetupIid
        {
            get { return this.sourceEngineeringModelSetupIid; }
            set { this.RaiseAndSetIfChanged(ref this.sourceEngineeringModelSetupIid, value); }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParticipantRowViewModel"/>
        /// </summary>
        public ParticipantRowViewModel SelectedParticipant
        {
            get { return this.selectedParticipant; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParticipant, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Participant"/>
        /// </summary>
        public ReactiveList<ParticipantRowViewModel> Participant { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ModelReferenceDataLibraryRowViewModel"/>
        /// </summary>
        public ModelReferenceDataLibraryRowViewModel SelectedRequiredRdl
        {
            get { return this.selectedRequiredRdl; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequiredRdl, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ModelReferenceDataLibrary"/>
        /// </summary>
        public ReactiveList<ModelReferenceDataLibraryRowViewModel> RequiredRdl { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="IterationSetupRowViewModel"/>
        /// </summary>
        public IterationSetupRowViewModel SelectedIterationSetup
        {
            get { return this.selectedIterationSetup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIterationSetup, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="IterationSetup"/>
        /// </summary>
        public ReactiveList<IterationSetupRowViewModel> IterationSetup { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ActiveDomain"/>s
        /// </summary>
        private ReactiveList<DomainOfExpertise> activeDomain;

        /// <summary>
        /// Gets or sets the list of selected <see cref="DomainOfExpertise"/>s
        /// </summary>
        public ReactiveList<DomainOfExpertise> ActiveDomain
        { 
            get { return this.activeDomain; } 
            set { this.RaiseAndSetIfChanged(ref this.activeDomain, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="DomainOfExpertise"/> for <see cref="ActiveDomain"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleActiveDomain { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Participant
        /// </summary>
        public ReactiveCommand<object> CreateParticipantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Participant
        /// </summary>
        public ReactiveCommand<object> DeleteParticipantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Participant
        /// </summary>
        public ReactiveCommand<object> EditParticipantCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Participant
        /// </summary>
        public ReactiveCommand<object> InspectParticipantCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ModelReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> CreateRequiredRdlCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ModelReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> DeleteRequiredRdlCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ModelReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> EditRequiredRdlCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ModelReferenceDataLibrary
        /// </summary>
        public ReactiveCommand<object> InspectRequiredRdlCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a IterationSetup
        /// </summary>
        public ReactiveCommand<object> CreateIterationSetupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a IterationSetup
        /// </summary>
        public ReactiveCommand<object> DeleteIterationSetupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a IterationSetup
        /// </summary>
        public ReactiveCommand<object> EditIterationSetupCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a IterationSetup
        /// </summary>
        public ReactiveCommand<object> InspectIterationSetupCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParticipantCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParticipantCommand = this.WhenAny(vm => vm.SelectedParticipant, v => v.Value != null);
            var canExecuteEditSelectedParticipantCommand = this.WhenAny(vm => vm.SelectedParticipant, v => v.Value != null && !this.IsReadOnly);

            this.CreateParticipantCommand = ReactiveCommand.Create(canExecuteCreateParticipantCommand);
            this.CreateParticipantCommand.Subscribe(_ => this.ExecuteCreateCommand<Participant>(this.PopulateParticipant));

            this.DeleteParticipantCommand = ReactiveCommand.Create(canExecuteEditSelectedParticipantCommand);
            this.DeleteParticipantCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParticipant.Thing, this.PopulateParticipant));

            this.EditParticipantCommand = ReactiveCommand.Create(canExecuteEditSelectedParticipantCommand);
            this.EditParticipantCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParticipant.Thing, this.PopulateParticipant));

            this.InspectParticipantCommand = ReactiveCommand.Create(canExecuteInspectSelectedParticipantCommand);
            this.InspectParticipantCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParticipant.Thing));
            
            var canExecuteCreateRequiredRdlCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRequiredRdlCommand = this.WhenAny(vm => vm.SelectedRequiredRdl, v => v.Value != null);
            var canExecuteEditSelectedRequiredRdlCommand = this.WhenAny(vm => vm.SelectedRequiredRdl, v => v.Value != null && !this.IsReadOnly);

            this.CreateRequiredRdlCommand = ReactiveCommand.Create(canExecuteCreateRequiredRdlCommand);
            this.CreateRequiredRdlCommand.Subscribe(_ => this.ExecuteCreateCommand<ModelReferenceDataLibrary>(this.PopulateRequiredRdl));

            this.DeleteRequiredRdlCommand = ReactiveCommand.Create(canExecuteEditSelectedRequiredRdlCommand);
            this.DeleteRequiredRdlCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRequiredRdl.Thing, this.PopulateRequiredRdl));

            this.EditRequiredRdlCommand = ReactiveCommand.Create(canExecuteEditSelectedRequiredRdlCommand);
            this.EditRequiredRdlCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRequiredRdl.Thing, this.PopulateRequiredRdl));

            this.InspectRequiredRdlCommand = ReactiveCommand.Create(canExecuteInspectSelectedRequiredRdlCommand);
            this.InspectRequiredRdlCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRequiredRdl.Thing));
            
            var canExecuteCreateIterationSetupCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedIterationSetupCommand = this.WhenAny(vm => vm.SelectedIterationSetup, v => v.Value != null);
            var canExecuteEditSelectedIterationSetupCommand = this.WhenAny(vm => vm.SelectedIterationSetup, v => v.Value != null && !this.IsReadOnly);

            this.CreateIterationSetupCommand = ReactiveCommand.Create(canExecuteCreateIterationSetupCommand);
            this.CreateIterationSetupCommand.Subscribe(_ => this.ExecuteCreateCommand<IterationSetup>(this.PopulateIterationSetup));

            this.DeleteIterationSetupCommand = ReactiveCommand.Create(canExecuteEditSelectedIterationSetupCommand);
            this.DeleteIterationSetupCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedIterationSetup.Thing, this.PopulateIterationSetup));

            this.EditIterationSetupCommand = ReactiveCommand.Create(canExecuteEditSelectedIterationSetupCommand);
            this.EditIterationSetupCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedIterationSetup.Thing, this.PopulateIterationSetup));

            this.InspectIterationSetupCommand = ReactiveCommand.Create(canExecuteInspectSelectedIterationSetupCommand);
            this.InspectIterationSetupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedIterationSetup.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Kind = this.Kind;
            clone.StudyPhase = this.StudyPhase;
            clone.EngineeringModelIid = this.EngineeringModelIid;
            clone.SourceEngineeringModelSetupIid = this.SourceEngineeringModelSetupIid;
            clone.ActiveDomain.Clear();
            clone.ActiveDomain.AddRange(this.ActiveDomain);
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Participant = new ReactiveList<ParticipantRowViewModel>();
            this.ActiveDomain = new ReactiveList<DomainOfExpertise>();
            this.PossibleActiveDomain = new ReactiveList<DomainOfExpertise>();
            this.RequiredRdl = new ReactiveList<ModelReferenceDataLibraryRowViewModel>();
            this.IterationSetup = new ReactiveList<IterationSetupRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Kind = this.Thing.Kind;
            this.StudyPhase = this.Thing.StudyPhase;
            this.EngineeringModelIid = this.Thing.EngineeringModelIid;
            this.SourceEngineeringModelSetupIid = this.Thing.SourceEngineeringModelSetupIid;
            this.PopulateParticipant();
            this.PopulateRequiredRdl();
            this.PopulateIterationSetup();
            this.PopulateActiveDomain();
        }

        /// <summary>
        /// Populates the <see cref="ActiveDomain"/> property
        /// </summary>
        protected virtual void PopulateActiveDomain()
        {
            this.ActiveDomain.Clear();

            foreach (var value in this.Thing.ActiveDomain)
            {
                this.ActiveDomain.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="Participant"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParticipant()
        {
            this.Participant.Clear();
            foreach (var thing in this.Thing.Participant.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParticipantRowViewModel(thing, this.Session, this);
                this.Participant.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="RequiredRdl"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRequiredRdl()
        {
            this.RequiredRdl.Clear();
            foreach (var thing in this.Thing.RequiredRdl.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ModelReferenceDataLibraryRowViewModel(thing, this.Session, this);
                this.RequiredRdl.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="IterationSetup"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateIterationSetup()
        {
            this.IterationSetup.Clear();
            foreach (var thing in this.Thing.IterationSetup.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new IterationSetupRowViewModel(thing, this.Session, this);
                this.IterationSetup.Add(row);
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
            foreach(var participant in this.Participant)
            {
                participant.Dispose();
            }
            foreach(var requiredRdl in this.RequiredRdl)
            {
                requiredRdl.Dispose();
            }
            foreach(var iterationSetup in this.IterationSetup)
            {
                iterationSetup.Dispose();
            }
        }
    }
}
