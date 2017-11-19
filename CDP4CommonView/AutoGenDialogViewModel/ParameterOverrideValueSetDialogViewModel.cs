// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideValueSetDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterOverrideValueSet"/>
    /// </summary>
    public partial class ParameterOverrideValueSetDialogViewModel : ParameterValueSetBaseDialogViewModel<ParameterOverrideValueSet>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedParameterValueSet"/>
        /// </summary>
        private ParameterValueSet selectedParameterValueSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideValueSetDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterOverrideValueSetDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideValueSetDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterOverrideValueSet">
        /// The <see cref="ParameterOverrideValueSet"/> that is the subject of the current view-model. This is the object
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
        public ParameterOverrideValueSetDialogViewModel(ParameterOverrideValueSet parameterOverrideValueSet, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterOverrideValueSet, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ParameterOverride;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ParameterOverride",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedParameterValueSet
        /// </summary>
        public virtual ParameterValueSet SelectedParameterValueSet
        {
            get { return this.selectedParameterValueSet; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterValueSet, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterValueSet"/>s for <see cref="SelectedParameterValueSet"/>
        /// </summary>
        public ReactiveList<ParameterValueSet> PossibleParameterValueSet { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedParameterValueSet"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedParameterValueSetCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedParameterValueSetCommand = this.WhenAny(vm => vm.SelectedParameterValueSet, v => v.Value != null);
            this.InspectSelectedParameterValueSetCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterValueSetCommand);
            this.InspectSelectedParameterValueSetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterValueSet));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ParameterValueSet = this.SelectedParameterValueSet;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleParameterValueSet = new ReactiveList<ParameterValueSet>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedParameterValueSet = this.Thing.ParameterValueSet;
            this.PopulatePossibleParameterValueSet();
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameterValueSet"/> property
        /// </summary>
        protected virtual void PopulatePossibleParameterValueSet()
        {
            this.PossibleParameterValueSet.Clear();
        }
    }
}
