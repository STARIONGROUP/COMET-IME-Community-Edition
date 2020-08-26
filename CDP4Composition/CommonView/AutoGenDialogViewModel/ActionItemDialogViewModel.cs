// -------------------------------------------------------------------------------------------------
// <copyright file="ActionItemDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="ActionItem"/>
    /// </summary>
    public partial class ActionItemDialogViewModel : ModellingAnnotationItemDialogViewModel<ActionItem>
    {
        /// <summary>
        /// Backing field for <see cref="DueDate"/>
        /// </summary>
        private DateTime dueDate;

        /// <summary>
        /// Backing field for <see cref="CloseOutDate"/>
        /// </summary>
        private DateTime? closeOutDate;

        /// <summary>
        /// Backing field for <see cref="CloseOutStatement"/>
        /// </summary>
        private string closeOutStatement;

        /// <summary>
        /// Backing field for <see cref="SelectedActionee"/>
        /// </summary>
        private Participant selectedActionee;


        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ActionItemDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemDialogViewModel"/> class
        /// </summary>
        /// <param name="actionItem">
        /// The <see cref="ActionItem"/> that is the subject of the current view-model. This is the object
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
        public ActionItemDialogViewModel(ActionItem actionItem, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(actionItem, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the DueDate
        /// </summary>
        public virtual DateTime DueDate
        {
            get { return this.dueDate; }
            set { this.RaiseAndSetIfChanged(ref this.dueDate, value); }
        }

        /// <summary>
        /// Gets or sets the CloseOutDate
        /// </summary>
        public virtual DateTime? CloseOutDate
        {
            get { return this.closeOutDate; }
            set { this.RaiseAndSetIfChanged(ref this.closeOutDate, value); }
        }

        /// <summary>
        /// Gets or sets the CloseOutStatement
        /// </summary>
        public virtual string CloseOutStatement
        {
            get { return this.closeOutStatement; }
            set { this.RaiseAndSetIfChanged(ref this.closeOutStatement, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedActionee
        /// </summary>
        public virtual Participant SelectedActionee
        {
            get { return this.selectedActionee; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActionee, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Participant"/>s for <see cref="SelectedActionee"/>
        /// </summary>
        public ReactiveList<Participant> PossibleActionee { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedActionee"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedActioneeCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedActioneeCommand = this.WhenAny(vm => vm.SelectedActionee, v => v.Value != null);
            this.InspectSelectedActioneeCommand = ReactiveCommand.Create(canExecuteInspectSelectedActioneeCommand);
            this.InspectSelectedActioneeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActionee));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.DueDate = this.DueDate;
            clone.CloseOutDate = this.CloseOutDate;
            clone.CloseOutStatement = this.CloseOutStatement;
            clone.Actionee = this.SelectedActionee;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleActionee = new ReactiveList<Participant>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.DueDate = this.Thing.DueDate;
            this.CloseOutDate = this.Thing.CloseOutDate;
            this.CloseOutStatement = this.Thing.CloseOutStatement;
            this.SelectedActionee = this.Thing.Actionee;
            this.PopulatePossibleActionee();
        }

        /// <summary>
        /// Populates the <see cref="PossibleActionee"/> property
        /// </summary>
        protected virtual void PopulatePossibleActionee()
        {
            this.PossibleActionee.Clear();
        }
    }
}
