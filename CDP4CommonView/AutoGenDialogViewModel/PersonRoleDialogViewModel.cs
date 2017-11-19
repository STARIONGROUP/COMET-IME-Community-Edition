// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="PersonRole"/>
    /// </summary>
    public partial class PersonRoleDialogViewModel : DefinedThingDialogViewModel<PersonRole>
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedPersonPermission"/>
        /// </summary>
        private PersonPermissionRowViewModel selectedPersonPermission;


        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PersonRoleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialogViewModel"/> class
        /// </summary>
        /// <param name="personRole">
        /// The <see cref="PersonRole"/> that is the subject of the current view-model. This is the object
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
        public PersonRoleDialogViewModel(PersonRole personRole, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(personRole, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the selected <see cref="PersonPermissionRowViewModel"/>
        /// </summary>
        public PersonPermissionRowViewModel SelectedPersonPermission
        {
            get { return this.selectedPersonPermission; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPersonPermission, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="PersonPermission"/>
        /// </summary>
        public ReactiveList<PersonPermissionRowViewModel> PersonPermission { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a PersonPermission
        /// </summary>
        public ReactiveCommand<object> CreatePersonPermissionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a PersonPermission
        /// </summary>
        public ReactiveCommand<object> DeletePersonPermissionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a PersonPermission
        /// </summary>
        public ReactiveCommand<object> EditPersonPermissionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a PersonPermission
        /// </summary>
        public ReactiveCommand<object> InspectPersonPermissionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreatePersonPermissionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPersonPermissionCommand = this.WhenAny(vm => vm.SelectedPersonPermission, v => v.Value != null);
            var canExecuteEditSelectedPersonPermissionCommand = this.WhenAny(vm => vm.SelectedPersonPermission, v => v.Value != null && !this.IsReadOnly);

            this.CreatePersonPermissionCommand = ReactiveCommand.Create(canExecuteCreatePersonPermissionCommand);
            this.CreatePersonPermissionCommand.Subscribe(_ => this.ExecuteCreateCommand<PersonPermission>(this.PopulatePersonPermission));

            this.DeletePersonPermissionCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonPermissionCommand);
            this.DeletePersonPermissionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPersonPermission.Thing, this.PopulatePersonPermission));

            this.EditPersonPermissionCommand = ReactiveCommand.Create(canExecuteEditSelectedPersonPermissionCommand);
            this.EditPersonPermissionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPersonPermission.Thing, this.PopulatePersonPermission));

            this.InspectPersonPermissionCommand = ReactiveCommand.Create(canExecuteInspectSelectedPersonPermissionCommand);
            this.InspectPersonPermissionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPersonPermission.Thing));
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
            this.PersonPermission = new ReactiveList<PersonPermissionRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.PopulatePersonPermission();
        }

        /// <summary>
        /// Populates the <see cref="PersonPermission"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePersonPermission()
        {
            this.PersonPermission.Clear();
            foreach (var thing in this.Thing.PersonPermission.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PersonPermissionRowViewModel(thing, this.Session, this);
                this.PersonPermission.Add(row);
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
            foreach(var personPermission in this.PersonPermission)
            {
                personPermission.Dispose();
            }
        }
    }
}
