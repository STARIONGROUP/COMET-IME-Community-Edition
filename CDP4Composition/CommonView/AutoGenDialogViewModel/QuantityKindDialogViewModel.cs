// -------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;
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
    /// dialog-view-model class representing a <see cref="QuantityKind"/>
    /// </summary>
    public abstract partial class QuantityKindDialogViewModel<T> : ScalarParameterTypeDialogViewModel<T> where T : QuantityKind
    {
        /// <summary>
        /// Backing field for <see cref="QuantityDimensionSymbol"/>
        /// </summary>
        private string quantityDimensionSymbol;

        /// <summary>
        /// Backing field for <see cref="SelectedDefaultScale"/>
        /// </summary>
        private MeasurementScale selectedDefaultScale;


        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected QuantityKindDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="quantityKind">
        /// The <see cref="QuantityKind"/> that is the subject of the current view-model. This is the object
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
        protected QuantityKindDialogViewModel(T quantityKind, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(quantityKind, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ReferenceDataLibrary",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the QuantityDimensionSymbol
        /// </summary>
        public virtual string QuantityDimensionSymbol
        {
            get { return this.quantityDimensionSymbol; }
            set { this.RaiseAndSetIfChanged(ref this.quantityDimensionSymbol, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedDefaultScale
        /// </summary>
        public virtual MeasurementScale SelectedDefaultScale
        {
            get { return this.selectedDefaultScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultScale, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementScale"/>s for <see cref="SelectedDefaultScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleDefaultScale { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="PossibleScale"/>s
        /// </summary>
        private ReactiveList<MeasurementScale> possibleScale;

        /// <summary>
        /// Gets or sets the list of selected <see cref="MeasurementScale"/>s
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale 
        { 
            get { return this.possibleScale; } 
            set { this.RaiseAndSetIfChanged(ref this.possibleScale, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="MeasurementScale"/> for <see cref="PossibleScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossiblePossibleScale { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultScale"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedDefaultScaleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedDefaultScaleCommand = this.WhenAny(vm => vm.SelectedDefaultScale, v => v.Value != null);
            this.InspectSelectedDefaultScaleCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedDefaultScaleCommand);
            this.InspectSelectedDefaultScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultScale));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.QuantityDimensionSymbol = this.QuantityDimensionSymbol;
            clone.DefaultScale = this.SelectedDefaultScale;
            clone.PossibleScale.Clear();
            clone.PossibleScale.AddRange(this.PossibleScale);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleDefaultScale = new ReactiveList<MeasurementScale>();
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.PossiblePossibleScale = new ReactiveList<MeasurementScale>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="QuantityKind"/>
        /// </summary>
        private void PopulatePossibleContainer()
        {
            this.PossibleContainer.Clear();
            // When creating a new Rule, it can be contained by any ReferenceDataLibrary that is currently loaded
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleContainer.AddRange(this.Session.OpenReferenceDataLibraries.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
                this.Container = this.PossibleContainer.FirstOrDefault();
                return;
            }

            // When inspecting an existing Rule, only it's container needs to be added to the PossibleContainer property (it cannot be changed)
            if (this.dialogKind == ThingDialogKind.Inspect)
            {
                this.PossibleContainer.Add(this.Thing.Container);
                this.Container = this.Thing.Container;
                return;
            }

            // When updating a Rule, the possible ReferenceDataLibrary can only be the ReferenceDataLibrary in the current chain of ReferenceDataLibrary of the Rule
            if (this.dialogKind == ThingDialogKind.Update)
            {
                var containerRdl = (ReferenceDataLibrary)this.Container;
                this.PossibleContainer.Add(containerRdl);
                var chainOfRdls = containerRdl.GetRequiredRdls();
                this.PossibleContainer.AddRange(chainOfRdls.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.QuantityDimensionSymbol = this.Thing.QuantityDimensionSymbol;
            this.SelectedDefaultScale = this.Thing.DefaultScale;
            this.PopulatePossibleDefaultScale();
            this.PopulatePossibleScale();
        }

        /// <summary>
        /// Populates the <see cref="PossibleScale"/> property
        /// </summary>
        protected virtual void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();

            foreach (var value in this.Thing.PossibleScale)
            {
                this.PossibleScale.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossibleDefaultScale"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultScale()
        {
            this.PossibleDefaultScale.Clear();
        }
    }
}
