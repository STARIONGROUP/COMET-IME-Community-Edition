// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingToReferenceScaleDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="MappingToReferenceScaleDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="MappingToReferenceScale"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.MappingToReferenceScale)]
    public class MappingToReferenceScaleDialogViewModel : CDP4CommonView.MappingToReferenceScaleDialogViewModel, IThingDialogViewModel
    {
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
        /// Initializes a new instance of the <see cref="MappingToReferenceScaleDialogViewModel"/> class.
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
        /// Assert if this <see cref="MappingToReferenceScaleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="MappingToReferenceScaleDialogViewModel"/> performs
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
        public MappingToReferenceScaleDialogViewModel(MappingToReferenceScale mappingToReferenceScale, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(mappingToReferenceScale, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedReferenceScaleValue).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedDependentScaleValue).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populates the <see cref="ConversionBasedUnitDialogViewModel{T}.PossibleReferenceUnit"/> property
        /// </summary>
        protected override void PopulatePossibleReferenceScaleValue()
        {
            base.PopulatePossibleReferenceScaleValue();

            var siteDir = this.Session.RetrieveSiteDirectory();
            var containerMeasurementScale = this.Container as MeasurementScale;
            var allScales = siteDir.SiteReferenceDataLibrary.SelectMany(rdl => rdl.Scale);
            this.PossibleReferenceScaleValue.AddRange(allScales.SelectMany(s => s.ValueDefinition)
                .Where(s => s.Container.Iid != containerMeasurementScale.Iid).OrderBy(s => s.Name));

            if (this.SelectedReferenceScaleValue == null)
            {
                this.SelectedReferenceScaleValue = this.PossibleReferenceScaleValue.FirstOrDefault();
            }
        }

        /// <summary>
        /// Populates the <see cref="ConversionBasedUnitDialogViewModel{T}.PossibleReferenceUnit"/> property
        /// </summary>
        protected override void PopulatePossibleDependentScaleValue()
        {
            base.PopulatePossibleDependentScaleValue();

            var clone = this.Container as MeasurementScale;
            this.PossibleDependentScaleValue.AddRange(clone.ValueDefinition.OrderBy(d => d.Name));

            if (this.SelectedDependentScaleValue == null)
            {
                this.SelectedDependentScaleValue = this.PossibleDependentScaleValue.FirstOrDefault();
            }
        }

        /// <summary>
        /// Updates the <see cref="DialogViewModelBase{T}.OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedReferenceScaleValue != null && this.SelectedDependentScaleValue != null;
        }
    }
}
