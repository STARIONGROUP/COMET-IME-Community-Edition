// -------------------------------------------------------------------------------------------------
// <copyright file="DerivedUnitDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DerivedUnitDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="DerivedUnit"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.DerivedUnit)]
    public class DerivedUnitDialogViewModel : CDP4CommonView.DerivedUnitDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedUnitDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DerivedUnitDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedUnitDialogViewModel"/> class.
        /// </summary>
        /// <param name="derivedUnit">
        /// The <see cref="DerivedUnit"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DerivedUnitDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DerivedUnitDialogViewModel"/> performs
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
        public DerivedUnitDialogViewModel(DerivedUnit derivedUnit, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(derivedUnit, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            var canCreateCondition = this.WhenAnyValue(vm => vm.Container).Select(x => x != null);
            this.CreateUnitFactorCommand = ReactiveCommand.Create(canCreateCondition);
            this.CreateUnitFactorCommand.Subscribe(_ => this.ExecuteCreateCommand<UnitFactor>(this.PopulateUnitFactor));

            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.UnitFactor.CountChanged.Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>e
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.UnitFactor.Count > 0;

            if (!this.OkCanExecute)
            {
                return;
            }

            // check if all unitFactor's unit are contained in the selected chain of Rdls
            var areFactorsValid = true;
            foreach (UnitFactor unitFactor in this.Thing.UnitFactor)
            {
                var rdl = (ReferenceDataLibrary) this.Container;
                var unitAvailable = rdl.Unit.ToList();

                foreach (var referenceDataLibrary in rdl.GetRequiredRdls())
                {
                    unitAvailable.AddRange(referenceDataLibrary.Unit);
                }

                areFactorsValid = unitAvailable.Contains(unitFactor.Unit);
                if (!areFactorsValid)
                {
                    break;
                }
            }

            this.OkCanExecute = this.OkCanExecute && areFactorsValid;
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "RDLShortName")]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "RDLName")]
        public override string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }
    }
}
