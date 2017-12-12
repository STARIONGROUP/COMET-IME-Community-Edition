// -------------------------------------------------------------------------------------------------
// <copyright file="LinearConversionUnitDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    using CDP4CommonView;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="LinearConversionUnitDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="LinearConversionUnit"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.LinearConversionUnit)]
    public class LinearConversionUnitDialogViewModel : CDP4CommonView.LinearConversionUnitDialogViewModel, IThingDialogViewModel
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
        /// Initializes a new instance of the <see cref="LinearConversionUnitDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public LinearConversionUnitDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearConversionUnitDialogViewModel"/> class.
        /// </summary>
        /// <param name="linearConversionUnit">
        /// The <see cref="LinearConversionUnit"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="LinearConversionUnitDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="LinearConversionUnitDialogViewModel"/> performs
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
        public LinearConversionUnitDialogViewModel(LinearConversionUnit linearConversionUnit, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(linearConversionUnit, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossibleReferenceUnit());
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populates the <see cref="ConversionBasedUnitDialogViewModel{T}.PossibleReferenceUnit"/> property
        /// </summary>
        protected override void PopulatePossibleReferenceUnit()
        {
            base.PopulatePossibleReferenceUnit();

            var units = this.GetPossibleMeasurementUnits();
            foreach (var measurementUnit in units)
            {
                this.PossibleReferenceUnit.Add(measurementUnit);
            }

            if (this.SelectedReferenceUnit == null)
            {
                this.SelectedReferenceUnit = this.PossibleReferenceUnit.FirstOrDefault();
            }
        }

        /// <summary>
        /// Queries all the <see cref="MeasurementUnit"/> from the chain of <see cref="ReferenceDataLibrary"/> of the container <see cref="ReferenceDataLibrary"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{MeasurementUnit}"/> ordered by short name.
        /// </returns>
        private IEnumerable<MeasurementUnit> GetPossibleMeasurementUnits()
        {
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return new List<MeasurementUnit>();
            }

            var measurementUnits = new List<MeasurementUnit>(containerRdl.Unit);

            foreach (var rdl in containerRdl.GetRequiredRdls())
            {
                measurementUnits.AddRange(rdl.Unit);
            }

            return measurementUnits.OrderBy(unit => unit.ShortName).ToList();
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
