// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    /// dialog-view-model class representing a <see cref="ParticipantRole"/>
    /// </summary>
    public partial class ParticipantRoleDialogViewModel : DefinedThingDialogViewModel<ParticipantRole>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedParticipantPermission"/>
        /// </summary>
        private ParticipantPermissionRowViewModel selectedParticipantPermission;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParticipantRoleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleDialogViewModel"/> class
        /// </summary>
        /// <param name="participantRole">
        /// The <see cref="ParticipantRole"/> that is the subject of the current view-model. This is the object
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
        public ParticipantRoleDialogViewModel(ParticipantRole participantRole, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(participantRole, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParticipantPermissionRowViewModel"/>
        /// </summary>
        public ParticipantPermissionRowViewModel SelectedParticipantPermission
        {
            get { return this.selectedParticipantPermission; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParticipantPermission, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParticipantPermission"/>
        /// </summary>
        public ReactiveList<ParticipantPermissionRowViewModel> ParticipantPermission { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParticipantPermission
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParticipantPermissionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParticipantPermission
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteParticipantPermissionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParticipantPermission
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditParticipantPermissionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParticipantPermission
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectParticipantPermissionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParticipantPermissionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParticipantPermissionCommand = this.WhenAny(vm => vm.SelectedParticipantPermission, v => v.Value != null);
            var canExecuteEditSelectedParticipantPermissionCommand = this.WhenAny(vm => vm.SelectedParticipantPermission, v => v.Value != null && !this.IsReadOnly);

            this.CreateParticipantPermissionCommand = ReactiveCommandCreator.Create(canExecuteCreateParticipantPermissionCommand);
            this.CreateParticipantPermissionCommand.Subscribe(_ => this.ExecuteCreateCommand<ParticipantPermission>(this.PopulateParticipantPermission));

            this.DeleteParticipantPermissionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParticipantPermissionCommand);
            this.DeleteParticipantPermissionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParticipantPermission.Thing, this.PopulateParticipantPermission));

            this.EditParticipantPermissionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParticipantPermissionCommand);
            this.EditParticipantPermissionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParticipantPermission.Thing, this.PopulateParticipantPermission));

            this.InspectParticipantPermissionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedParticipantPermissionCommand);
            this.InspectParticipantPermissionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParticipantPermission.Thing));
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
            this.ParticipantPermission = new ReactiveList<ParticipantPermissionRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.PopulateParticipantPermission();
        }

        /// <summary>
        /// Populates the <see cref="ParticipantPermission"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParticipantPermission()
        {
            this.ParticipantPermission.Clear();
            foreach (var thing in this.Thing.ParticipantPermission.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParticipantPermissionRowViewModel(thing, this.Session, this);
                this.ParticipantPermission.Add(row);
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
            foreach(var participantPermission in this.ParticipantPermission)
            {
                participantPermission.Dispose();
            }
        }
    }
}
