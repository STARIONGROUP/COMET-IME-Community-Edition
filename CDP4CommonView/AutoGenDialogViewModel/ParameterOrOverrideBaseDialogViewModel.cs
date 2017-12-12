// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract partial class ParameterOrOverrideBaseDialogViewModel<T> : ParameterBaseDialogViewModel<T> where T : ParameterOrOverrideBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedParameterSubscription"/>
        /// </summary>
        private ParameterSubscriptionRowViewModel selectedParameterSubscription;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ParameterOrOverrideBaseDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterOrOverrideBase">
        /// The <see cref="ParameterOrOverrideBase"/> that is the subject of the current view-model. This is the object
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
        protected ParameterOrOverrideBaseDialogViewModel(T parameterOrOverrideBase, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterOrOverrideBase, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterSubscriptionRowViewModel"/>
        /// </summary>
        public ParameterSubscriptionRowViewModel SelectedParameterSubscription
        {
            get { return this.selectedParameterSubscription; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterSubscription, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterSubscription"/>
        /// </summary>
        public ReactiveList<ParameterSubscriptionRowViewModel> ParameterSubscription { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterSubscription
        /// </summary>
        public ReactiveCommand<object> CreateParameterSubscriptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterSubscription
        /// </summary>
        public ReactiveCommand<object> DeleteParameterSubscriptionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterSubscription
        /// </summary>
        public ReactiveCommand<object> EditParameterSubscriptionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterSubscription
        /// </summary>
        public ReactiveCommand<object> InspectParameterSubscriptionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParameterSubscriptionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterSubscriptionCommand = this.WhenAny(vm => vm.SelectedParameterSubscription, v => v.Value != null);
            var canExecuteEditSelectedParameterSubscriptionCommand = this.WhenAny(vm => vm.SelectedParameterSubscription, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterSubscriptionCommand = ReactiveCommand.Create(canExecuteCreateParameterSubscriptionCommand);
            this.CreateParameterSubscriptionCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterSubscription>(this.PopulateParameterSubscription));

            this.DeleteParameterSubscriptionCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterSubscriptionCommand);
            this.DeleteParameterSubscriptionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterSubscription.Thing, this.PopulateParameterSubscription));

            this.EditParameterSubscriptionCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterSubscriptionCommand);
            this.EditParameterSubscriptionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterSubscription.Thing, this.PopulateParameterSubscription));

            this.InspectParameterSubscriptionCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterSubscriptionCommand);
            this.InspectParameterSubscriptionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterSubscription.Thing));
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
            this.ParameterSubscription = new ReactiveList<ParameterSubscriptionRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateParameterSubscription();
        }

        /// <summary>
        /// Populates the <see cref="ParameterSubscription"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterSubscription()
        {
            this.ParameterSubscription.Clear();
            foreach (var thing in this.Thing.ParameterSubscription.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterSubscriptionRowViewModel(thing, this.Session, this);
                this.ParameterSubscription.Add(row);
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
            foreach(var parameterSubscription in this.ParameterSubscription)
            {
                parameterSubscription.Dispose();
            }
        }
    }
}
