// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionValueSetDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterSubscriptionValueSet"/>
    /// </summary>
    public partial class ParameterSubscriptionValueSetDialogViewModel : DialogViewModelBase<ParameterSubscriptionValueSet>
    {
        /// <summary>
        /// Backing field for <see cref="ValueSwitch"/>
        /// </summary>
        private ParameterSwitchKind valueSwitch;

        /// <summary>
        /// Backing field for <see cref="SelectedSubscribedValueSet"/>
        /// </summary>
        private ParameterValueSetBase selectedSubscribedValueSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionValueSetDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterSubscriptionValueSetDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionValueSetDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> that is the subject of the current view-model. This is the object
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
        public ParameterSubscriptionValueSetDialogViewModel(ParameterSubscriptionValueSet parameterSubscriptionValueSet, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterSubscriptionValueSet, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ParameterSubscription;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ParameterSubscription",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ValueSwitch
        /// </summary>
        public virtual ParameterSwitchKind ValueSwitch
        {
            get { return this.valueSwitch; }
            set { this.RaiseAndSetIfChanged(ref this.valueSwitch, value); }
        }

        /// <summary>
        /// Backing field for Manual
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> manual;

        /// <summary>
        /// Gets or sets the Manual
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Manual
        {
            get { return this.manual; }
            set { this.RaiseAndSetIfChanged(ref this.manual, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedSubscribedValueSet
        /// </summary>
        public virtual ParameterValueSetBase SelectedSubscribedValueSet
        {
            get { return this.selectedSubscribedValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSubscribedValueSet, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterValueSetBase"/>s for <see cref="SelectedSubscribedValueSet"/>
        /// </summary>
        public ReactiveList<ParameterValueSetBase> PossibleSubscribedValueSet { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSubscribedValueSet"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedSubscribedValueSetCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedSubscribedValueSetCommand = this.WhenAny(vm => vm.SelectedSubscribedValueSet, v => v.Value != null);
            this.InspectSelectedSubscribedValueSetCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedSubscribedValueSetCommand);
            this.InspectSelectedSubscribedValueSetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSubscribedValueSet));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ValueSwitch = this.ValueSwitch;
            clone.Manual = new ValueArray<string>(this.Manual.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.SubscribedValueSet = this.SelectedSubscribedValueSet;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Manual = new ReactiveList<PrimitiveRow<string>>();
            this.PossibleSubscribedValueSet = new ReactiveList<ParameterValueSetBase>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ValueSwitch = this.Thing.ValueSwitch;
            this.PopulateManual();
            this.SelectedSubscribedValueSet = this.Thing.SubscribedValueSet;
            this.PopulatePossibleSubscribedValueSet();
        }

        /// <summary>
        /// Populates the <see cref="Manual"/> property
        /// </summary>
        protected virtual void PopulateManual()
        {
            this.Manual.Clear();
            foreach(var value in this.Thing.Manual)
            {
                this.Manual.Add(new PrimitiveRow<string> { Index = this.Manual.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleSubscribedValueSet"/> property
        /// </summary>
        protected virtual void PopulatePossibleSubscribedValueSet()
        {
            this.PossibleSubscribedValueSet.Clear();
        }
    }
}
