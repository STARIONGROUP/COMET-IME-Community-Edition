// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="Participant"/>
    /// </summary>
    public partial class ParticipantDialogViewModel : DialogViewModelBase<Participant>
    {
        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="SelectedPerson"/>
        /// </summary>
        private Person selectedPerson;

        /// <summary>
        /// Backing field for <see cref="SelectedRole"/>
        /// </summary>
        private ParticipantRole selectedRole;

        /// <summary>
        /// Backing field for <see cref="SelectedSelectedDomain"/>
        /// </summary>
        private DomainOfExpertise selectedSelectedDomain;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParticipantDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialogViewModel"/> class
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is the subject of the current view-model. This is the object
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
        public ParticipantDialogViewModel(Participant participant, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(participant, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as EngineeringModelSetup;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type EngineeringModelSetup",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public virtual bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedPerson
        /// </summary>
        public virtual Person SelectedPerson
        {
            get { return this.selectedPerson; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPerson, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Person"/>s for <see cref="SelectedPerson"/>
        /// </summary>
        public ReactiveList<Person> PossiblePerson { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedRole
        /// </summary>
        public virtual ParticipantRole SelectedRole
        {
            get { return this.selectedRole; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRole, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParticipantRole"/>s for <see cref="SelectedRole"/>
        /// </summary>
        public ReactiveList<ParticipantRole> PossibleRole { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedSelectedDomain
        /// </summary>
        public virtual DomainOfExpertise SelectedSelectedDomain
        {
            get { return this.selectedSelectedDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSelectedDomain, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedSelectedDomain"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleSelectedDomain { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Domain"/>s
        /// </summary>
        private ReactiveList<DomainOfExpertise> domain;

        /// <summary>
        /// Gets or sets the list of selected <see cref="DomainOfExpertise"/>s
        /// </summary>
        public ReactiveList<DomainOfExpertise> Domain 
        { 
            get { return this.domain; } 
            set { this.RaiseAndSetIfChanged(ref this.domain, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="DomainOfExpertise"/> for <see cref="Domain"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleDomain { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedPerson"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedPersonCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRole"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedRoleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSelectedDomain"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedSelectedDomainCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedPersonCommand = this.WhenAny(vm => vm.SelectedPerson, v => v.Value != null);
            this.InspectSelectedPersonCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedPersonCommand);
            this.InspectSelectedPersonCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPerson));
            var canExecuteInspectSelectedRoleCommand = this.WhenAny(vm => vm.SelectedRole, v => v.Value != null);
            this.InspectSelectedRoleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRoleCommand);
            this.InspectSelectedRoleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRole));
            var canExecuteInspectSelectedSelectedDomainCommand = this.WhenAny(vm => vm.SelectedSelectedDomain, v => v.Value != null);
            this.InspectSelectedSelectedDomainCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedSelectedDomainCommand);
            this.InspectSelectedSelectedDomainCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSelectedDomain));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsActive = this.IsActive;
            clone.Person = this.SelectedPerson;
            clone.Role = this.SelectedRole;
            clone.SelectedDomain = this.SelectedSelectedDomain;
            clone.Domain.Clear();
            clone.Domain.AddRange(this.Domain);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossiblePerson = new ReactiveList<Person>();
            this.PossibleRole = new ReactiveList<ParticipantRole>();
            this.PossibleSelectedDomain = new ReactiveList<DomainOfExpertise>();
            this.Domain = new ReactiveList<DomainOfExpertise>();
            this.PossibleDomain = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsActive = this.Thing.IsActive;
            this.SelectedPerson = this.Thing.Person;
            this.PopulatePossiblePerson();
            this.SelectedRole = this.Thing.Role;
            this.PopulatePossibleRole();
            this.SelectedSelectedDomain = this.Thing.SelectedDomain;
            this.PopulatePossibleSelectedDomain();
            this.PopulateDomain();
        }

        /// <summary>
        /// Populates the <see cref="Domain"/> property
        /// </summary>
        protected virtual void PopulateDomain()
        {
            this.Domain.Clear();

            foreach (var value in this.Thing.Domain)
            {
                this.Domain.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossiblePerson"/> property
        /// </summary>
        protected virtual void PopulatePossiblePerson()
        {
            this.PossiblePerson.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleRole"/> property
        /// </summary>
        protected virtual void PopulatePossibleRole()
        {
            this.PossibleRole.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSelectedDomain"/> property
        /// </summary>
        protected virtual void PopulatePossibleSelectedDomain()
        {
            this.PossibleSelectedDomain.Clear();
        }
    }
}
