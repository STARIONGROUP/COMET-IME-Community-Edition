// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="ParameterTypeComponent"/>
    /// </summary>
    public partial class ParameterTypeComponentDialogViewModel : DialogViewModelBase<ParameterTypeComponent>
    {
        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private MeasurementScale selectedScale;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeComponentDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterTypeComponentDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeComponentDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterTypeComponent">
        /// The <see cref="ParameterTypeComponent"/> that is the subject of the current view-model. This is the object
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
        public ParameterTypeComponentDialogViewModel(ParameterTypeComponent parameterTypeComponent, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterTypeComponent, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as CompoundParameterType;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type CompoundParameterType",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public virtual string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedParameterType
        /// </summary>
        public virtual ParameterType SelectedParameterType
        {
            get { return this.selectedParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterType"/>s for <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedScale
        /// </summary>
        public virtual MeasurementScale SelectedScale
        {
            get { return this.selectedScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedScale, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementScale"/>s for <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedScaleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null);
            this.InspectSelectedParameterTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterTypeCommand);
            this.InspectSelectedParameterTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterType));
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            this.InspectSelectedScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectSelectedScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ShortName = this.ShortName;
            clone.ParameterType = this.SelectedParameterType;
            clone.Scale = this.SelectedScale;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleParameterType = new ReactiveList<ParameterType>();
            this.PossibleScale = new ReactiveList<MeasurementScale>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ShortName = this.Thing.ShortName;
            this.SelectedParameterType = this.Thing.ParameterType;
            this.PopulatePossibleParameterType();
            this.SelectedScale = this.Thing.Scale;
            this.PopulatePossibleScale();
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameterType"/> property
        /// </summary>
        protected virtual void PopulatePossibleParameterType()
        {
            this.PossibleParameterType.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleScale"/> property
        /// </summary>
        protected virtual void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();
        }
    }
}
