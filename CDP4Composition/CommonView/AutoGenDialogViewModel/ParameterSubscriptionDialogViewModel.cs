// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.EngineeringModelData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterSubscription"/>
    /// </summary>
    public partial class ParameterSubscriptionDialogViewModel : ParameterBaseDialogViewModel<ParameterSubscription>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedValueSet"/>
        /// </summary>
        private ParameterSubscriptionValueSetRowViewModel selectedValueSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterSubscriptionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterSubscription">
        /// The <see cref="ParameterSubscription"/> that is the subject of the current view-model. This is the object
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
        public ParameterSubscriptionDialogViewModel(ParameterSubscription parameterSubscription, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterSubscription, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ParameterOrOverrideBase;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ParameterOrOverrideBase",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterSubscriptionValueSetRowViewModel"/>
        /// </summary>
        public ParameterSubscriptionValueSetRowViewModel SelectedValueSet
        {
            get { return this.selectedValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueSet, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterSubscriptionValueSet"/>
        /// </summary>
        public ReactiveList<ParameterSubscriptionValueSetRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterSubscriptionValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateValueSetCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterSubscriptionValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteValueSetCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterSubscriptionValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditValueSetCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterSubscriptionValueSet
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectValueSetCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateValueSetCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedValueSetCommand = this.WhenAny(vm => vm.SelectedValueSet, v => v.Value != null);
            var canExecuteEditSelectedValueSetCommand = this.WhenAny(vm => vm.SelectedValueSet, v => v.Value != null && !this.IsReadOnly);

            this.CreateValueSetCommand = ReactiveCommandCreator.Create(canExecuteCreateValueSetCommand);
            this.CreateValueSetCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterSubscriptionValueSet>(this.PopulateValueSet));

            this.DeleteValueSetCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedValueSetCommand);
            this.DeleteValueSetCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedValueSet.Thing, this.PopulateValueSet));

            this.EditValueSetCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedValueSetCommand);
            this.EditValueSetCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedValueSet.Thing, this.PopulateValueSet));

            this.InspectValueSetCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedValueSetCommand);
            this.InspectValueSetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueSet.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ValueSet = new ReactiveList<ParameterSubscriptionValueSetRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateValueSet();
        }

        /// <summary>
        /// Populates the <see cref="ValueSet"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateValueSet()
        {
            this.ValueSet.Clear();
            foreach (var thing in this.Thing.ValueSet.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterSubscriptionValueSetRowViewModel(thing, this.Session, this);
                this.ValueSet.Add(row);
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
            foreach(var valueSet in this.ValueSet)
            {
                valueSet.Dispose();
            }
        }
    }
}
