// -------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindFactorDialogViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// dialog-view-model class representing a <see cref="QuantityKindFactor"/>
    /// </summary>
    public partial class QuantityKindFactorDialogViewModel : DialogViewModelBase<QuantityKindFactor>
    {
        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="SelectedQuantityKind"/>
        /// </summary>
        private QuantityKind selectedQuantityKind;


        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public QuantityKindFactorDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorDialogViewModel"/> class
        /// </summary>
        /// <param name="quantityKindFactor">
        /// The <see cref="QuantityKindFactor"/> that is the subject of the current view-model. This is the object
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
        public QuantityKindFactorDialogViewModel(QuantityKindFactor quantityKindFactor, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(quantityKindFactor, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as DerivedQuantityKind;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type DerivedQuantityKind",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Exponent
        /// </summary>
        public virtual string Exponent
        {
            get { return this.exponent; }
            set { this.RaiseAndSetIfChanged(ref this.exponent, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedQuantityKind
        /// </summary>
        public virtual QuantityKind SelectedQuantityKind
        {
            get { return this.selectedQuantityKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedQuantityKind, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="QuantityKind"/>s for <see cref="SelectedQuantityKind"/>
        /// </summary>
        public ReactiveList<QuantityKind> PossibleQuantityKind { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedQuantityKind"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedQuantityKindCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedQuantityKindCommand = this.WhenAny(vm => vm.SelectedQuantityKind, v => v.Value != null);
            this.InspectSelectedQuantityKindCommand = ReactiveCommand.Create(canExecuteInspectSelectedQuantityKindCommand);
            this.InspectSelectedQuantityKindCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedQuantityKind));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Exponent = this.Exponent;
            clone.QuantityKind = this.SelectedQuantityKind;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleQuantityKind = new ReactiveList<QuantityKind>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Exponent = this.Thing.Exponent;
            this.SelectedQuantityKind = this.Thing.QuantityKind;
            this.PopulatePossibleQuantityKind();
        }

        /// <summary>
        /// Populates the <see cref="PossibleQuantityKind"/> property
        /// </summary>
        protected virtual void PopulatePossibleQuantityKind()
        {
            this.PossibleQuantityKind.Clear();
        }
    }
}
