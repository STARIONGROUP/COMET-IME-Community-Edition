// -------------------------------------------------------------------------------------------------
// <copyright file="MappingToReferenceScaleDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="MappingToReferenceScale"/>
    /// </summary>
    public partial class MappingToReferenceScaleDialogViewModel : DialogViewModelBase<MappingToReferenceScale>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedReferenceScaleValue"/>
        /// </summary>
        private ScaleValueDefinition selectedReferenceScaleValue;

        /// <summary>
        /// Backing field for <see cref="SelectedDependentScaleValue"/>
        /// </summary>
        private ScaleValueDefinition selectedDependentScaleValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="MappingToReferenceScaleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public MappingToReferenceScaleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingToReferenceScaleDialogViewModel"/> class
        /// </summary>
        /// <param name="mappingToReferenceScale">
        /// The <see cref="MappingToReferenceScale"/> that is the subject of the current view-model. This is the object
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
        public MappingToReferenceScaleDialogViewModel(MappingToReferenceScale mappingToReferenceScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(mappingToReferenceScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as MeasurementScale;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type MeasurementScale",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedReferenceScaleValue
        /// </summary>
        public virtual ScaleValueDefinition SelectedReferenceScaleValue
        {
            get { return this.selectedReferenceScaleValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferenceScaleValue, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ScaleValueDefinition"/>s for <see cref="SelectedReferenceScaleValue"/>
        /// </summary>
        public ReactiveList<ScaleValueDefinition> PossibleReferenceScaleValue { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedDependentScaleValue
        /// </summary>
        public virtual ScaleValueDefinition SelectedDependentScaleValue
        {
            get { return this.selectedDependentScaleValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDependentScaleValue, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ScaleValueDefinition"/>s for <see cref="SelectedDependentScaleValue"/>
        /// </summary>
        public ReactiveList<ScaleValueDefinition> PossibleDependentScaleValue { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedReferenceScaleValue"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedReferenceScaleValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDependentScaleValue"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDependentScaleValueCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedReferenceScaleValueCommand = this.WhenAny(vm => vm.SelectedReferenceScaleValue, v => v.Value != null);
            this.InspectSelectedReferenceScaleValueCommand = ReactiveCommand.Create(canExecuteInspectSelectedReferenceScaleValueCommand);
            this.InspectSelectedReferenceScaleValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferenceScaleValue));
            var canExecuteInspectSelectedDependentScaleValueCommand = this.WhenAny(vm => vm.SelectedDependentScaleValue, v => v.Value != null);
            this.InspectSelectedDependentScaleValueCommand = ReactiveCommand.Create(canExecuteInspectSelectedDependentScaleValueCommand);
            this.InspectSelectedDependentScaleValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDependentScaleValue));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ReferenceScaleValue = this.SelectedReferenceScaleValue;
            clone.DependentScaleValue = this.SelectedDependentScaleValue;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleReferenceScaleValue = new ReactiveList<ScaleValueDefinition>();
            this.PossibleDependentScaleValue = new ReactiveList<ScaleValueDefinition>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedReferenceScaleValue = this.Thing.ReferenceScaleValue;
            this.PopulatePossibleReferenceScaleValue();
            this.SelectedDependentScaleValue = this.Thing.DependentScaleValue;
            this.PopulatePossibleDependentScaleValue();
        }

        /// <summary>
        /// Populates the <see cref="PossibleReferenceScaleValue"/> property
        /// </summary>
        protected virtual void PopulatePossibleReferenceScaleValue()
        {
            this.PossibleReferenceScaleValue.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleDependentScaleValue"/> property
        /// </summary>
        protected virtual void PopulatePossibleDependentScaleValue()
        {
            this.PossibleDependentScaleValue.Clear();
        }
    }
}
