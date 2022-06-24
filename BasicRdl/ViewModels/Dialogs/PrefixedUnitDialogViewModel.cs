// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrefixedUnitDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="PrefixedUnitDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="PrefixedUnit"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.PrefixedUnit)]
    public class PrefixedUnitDialogViewModel : CDP4CommonView.PrefixedUnitDialogViewModel, IThingDialogViewModel
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
        /// Initializes a new instance of the <see cref="PrefixedUnitDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PrefixedUnitDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixedUnitDialogViewModel"/> class.
        /// </summary>
        /// <param name="prefixedUnit">
        /// The <see cref="PrefixedUnit"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="PrefixedUnitDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="PrefixedUnitDialogViewModel"/> performs
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
        public PrefixedUnitDialogViewModel(PrefixedUnit prefixedUnit, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(prefixedUnit, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossiblePrefix());
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossibleReferenceUnit());

            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedPrefix).Subscribe(_ => this.SetShortNameAndName());
            this.WhenAnyValue(vm => vm.SelectedPrefix).Subscribe(_ => this.UpdateOkCanExecute());

            this.WhenAnyValue(vm => vm.SelectedReferenceUnit).Subscribe(_ => this.SetShortNameAndName());
            this.WhenAnyValue(vm => vm.SelectedReferenceUnit).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public override string this[string columnName]
        {
            get
            {
                if (columnName == "ShortName" || columnName == "Name")
                {
                    return null;
                }

                return ValidationService.ValidateProperty(columnName, this);
            } 
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "RDLShortName")]
        public override string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "RDLName")]
        public override string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && 
                this.SelectedPrefix != null &&
                this.SelectedReferenceUnit != null;
        }

        /// <summary>
        /// Populates the <see cref="PrefixedUnitDialogViewModel.PossiblePrefix"/> property
        /// </summary>
        protected override void PopulatePossiblePrefix()
        {
            base.PopulatePossiblePrefix();

            var prefixes = this.GetPossiblePrefixes();
            foreach (var unitPrefix in prefixes)
            {
                this.PossiblePrefix.Add(unitPrefix);
            }

            if (this.SelectedPrefix == null)
            {
                this.SelectedPrefix = this.PossiblePrefix.FirstOrDefault();
            }
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
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            // No call of the base method to avoid setting derived properties
            var clone = this.Thing;

            clone.IsDeprecated = this.IsDeprecated;
            clone.ReferenceUnit = this.SelectedReferenceUnit;
            clone.Prefix = this.SelectedPrefix;
        }

        /// <summary>
        /// Queries all the <see cref="UnitPrefix"/> from the chain of <see cref="ReferenceDataLibrary"/> of the container <see cref="ReferenceDataLibrary"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{UnitPrefix}"/> ordered by Conversion factor.
        /// </returns>
        private IEnumerable<UnitPrefix> GetPossiblePrefixes()
        {
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return new List<UnitPrefix>();
            }

            var allPrefixes = new List<UnitPrefix>(containerRdl.UnitPrefix);

            foreach (var rdl in containerRdl.GetRequiredRdls())
            {
                allPrefixes.AddRange(rdl.UnitPrefix);
            }

            return allPrefixes.OrderBy(prefix => prefix.ShortName).ToList();
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
        /// Sets the <see cref="DefinedThingDialogViewModel{T}.ShortName"/> and <see cref="DefinedThingDialogViewModel{T}.Name"/>
        /// which are derived from the <see cref="PrefixedUnitDialogViewModel.SelectedPrefix"/> and <see cref="ConversionBasedUnitDialogViewModel{T}.SelectedReferenceUnit"/> properties.
        /// </summary>
        private void SetShortNameAndName()
        {
            if (this.SelectedPrefix != null && this.SelectedReferenceUnit != null)
            {
                this.ShortName = $"{this.SelectedPrefix.ShortName}{this.SelectedReferenceUnit.ShortName}";
                this.Name = $"{this.SelectedPrefix.Name}{this.SelectedReferenceUnit.Name}";
            }
            else
            {
                this.ShortName = string.Empty;
                this.Name = string.Empty;
            }
        }
    }
}
