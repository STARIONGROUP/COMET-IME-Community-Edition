// -------------------------------------------------------------------------------------------------
// <copyright file="ScaleReferenceQuantityValueDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ScaleReferenceQuantityValue"/>
    /// </summary>
    public partial class ScaleReferenceQuantityValueDialogViewModel : DialogViewModelBase<ScaleReferenceQuantityValue>
    {
        /// <summary>
        /// Backing field for <see cref="ScaleReferenceQuantityValue"/>
        /// </summary>
        private string scaleReferenceQuantityValue;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private MeasurementScale selectedScale;


        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleReferenceQuantityValueDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ScaleReferenceQuantityValueDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleReferenceQuantityValueDialogViewModel"/> class
        /// </summary>
        /// <param name="scaleReferenceQuantityValue">
        /// The <see cref="ScaleReferenceQuantityValue"/> that is the subject of the current view-model. This is the object
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
        public ScaleReferenceQuantityValueDialogViewModel(ScaleReferenceQuantityValue scaleReferenceQuantityValue, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(scaleReferenceQuantityValue, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as LogarithmicScale;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type LogarithmicScale",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ScaleReferenceQuantityValue
        /// </summary>
        public virtual string ScaleReferenceQuantityValue
        {
            get { return this.scaleReferenceQuantityValue; }
            set { this.RaiseAndSetIfChanged(ref this.scaleReferenceQuantityValue, value); }
        }

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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedScaleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            this.InspectSelectedScaleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectSelectedScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Value = this.ScaleReferenceQuantityValue;
            clone.Scale = this.SelectedScale;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleScale = new ReactiveList<MeasurementScale>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ScaleReferenceQuantityValue = this.Thing.Value;
            this.SelectedScale = this.Thing.Scale;
            this.PopulatePossibleScale();
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
