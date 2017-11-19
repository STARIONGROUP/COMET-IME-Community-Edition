// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="ParameterSubscription"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ParameterSubscription)]
    public class ParameterSubscriptionDialogViewModel : CDP4CommonView.ParameterSubscriptionDialogViewModel, IThingDialogViewModel
    {
        #region Constructors

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
        /// The parameterSubscription.
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
        /// The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParameterSubscriptionDialogViewModel(ParameterSubscription parameterSubscription, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterSubscription, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.IsNameVisible = this.Thing.ParameterType is CompoundParameterType || this.Thing.IsOptionDependent || this.Thing.StateDependence != null;
            this.CheckValueValidation();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether is owner visible.
        /// </summary>
        public bool IsOwnerVisible
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is name visible.
        /// </summary>
        public bool IsNameVisible { get; private set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterSubscriptionValueSet"/>
        /// </summary>
        public ReactiveList<Dialogs.ParameterSubscriptionRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets the parameter that contains the subscription represented by this view-model.
        /// </summary>
        public string SubcribedParameter
        {
            get
            {
                return string.Format("{0} ({1})", ((ParameterOrOverrideBase)this.Thing.Container).ParameterType.Name, this.Thing.Container.GetContainerOfType<ElementDefinition>().Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter Subscription is state dependent.
        /// </summary>
        public bool IsStateDependent
        {
            get
            {
                return ((ParameterOrOverrideBase)this.Container).StateDependence != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter is option dependent.
        /// </summary>
        public bool IsOptionDependent
        {
            get { return ((ParameterOrOverrideBase)this.Container).IsOptionDependent; }
        }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SubcribedParameter"/>
        /// </summary>
        public ReactiveCommand<object> InspectSubcribedParameterCommand { get; protected set; }

        #endregion

        #region Methods
        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ValueSet = new ReactiveList<Dialogs.ParameterSubscriptionRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteInspectSelectedParameterCommand = this.WhenAny(vm => vm.SubcribedParameter, v => v.Value != null);
            this.InspectSubcribedParameterCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterCommand);
            this.InspectSubcribedParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.Container));
        }

         /// <summary>
         /// Update the properties
         /// </summary>
         protected override void UpdateProperties()
         {
             base.UpdateProperties();
             this.SelectedParameterType = ((ParameterOrOverrideBase)this.Thing.Container).ParameterType;
             this.PossibleOwner.Add(this.SelectedOwner);
             this.PopulateValueSet();
         }

        /// <summary>
        /// Populates the <see cref="ValueSet"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected void PopulateValueSet()
        {
            this.ValueSet.Clear();
            var row = new Dialogs.ParameterSubscriptionRowViewModel(this.Thing, this.Session, this, this.IsReadOnly);

            this.ValueSet.Add(row);
         }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            for (var i = 0; i < this.Thing.ValueSet.Count; i++)
            {
                this.Thing.ValueSet[i] = this.Thing.ValueSet[i].Clone(false);
            }

            this.ValueSet.First().UpdateValueSets(this.Thing.ValueSet.First());

            foreach (var parameterSubscriptionValueSet in this.Thing.ValueSet)
            {
                this.transaction.CreateOrUpdate(parameterSubscriptionValueSet);
            }
        }

        #endregion

        /// <summary>
        /// Check the validation status of the value rows
        /// </summary>
        private void CheckValueValidation()
        {
            foreach (var valueRow in this.ValueSet)
            {
                valueRow.CheckValues(this.Thing.Scale);
            }
        }
    }
}