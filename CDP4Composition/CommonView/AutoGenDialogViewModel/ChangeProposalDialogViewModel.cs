﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ChangeProposalDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.ReportingData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ChangeProposal"/>
    /// </summary>
    public partial class ChangeProposalDialogViewModel : ModellingAnnotationItemDialogViewModel<ChangeProposal>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedChangeRequest"/>
        /// </summary>
        private ChangeRequest selectedChangeRequest;


        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeProposalDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ChangeProposalDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeProposalDialogViewModel"/> class
        /// </summary>
        /// <param name="changeProposal">
        /// The <see cref="ChangeProposal"/> that is the subject of the current view-model. This is the object
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
        public ChangeProposalDialogViewModel(ChangeProposal changeProposal, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(changeProposal, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the SelectedChangeRequest
        /// </summary>
        public virtual ChangeRequest SelectedChangeRequest
        {
            get { return this.selectedChangeRequest; }
            set { this.RaiseAndSetIfChanged(ref this.selectedChangeRequest, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ChangeRequest"/>s for <see cref="SelectedChangeRequest"/>
        /// </summary>
        public ReactiveList<ChangeRequest> PossibleChangeRequest { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedChangeRequest"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedChangeRequestCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedChangeRequestCommand = this.WhenAny(vm => vm.SelectedChangeRequest, v => v.Value != null);
            this.InspectSelectedChangeRequestCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedChangeRequestCommand);
            this.InspectSelectedChangeRequestCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedChangeRequest));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ChangeRequest = this.SelectedChangeRequest;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleChangeRequest = new ReactiveList<ChangeRequest>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedChangeRequest = this.Thing.ChangeRequest;
            this.PopulatePossibleChangeRequest();
        }

        /// <summary>
        /// Populates the <see cref="PossibleChangeRequest"/> property
        /// </summary>
        protected virtual void PopulatePossibleChangeRequest()
        {
            this.PossibleChangeRequest.Clear();
        }
    }
}
