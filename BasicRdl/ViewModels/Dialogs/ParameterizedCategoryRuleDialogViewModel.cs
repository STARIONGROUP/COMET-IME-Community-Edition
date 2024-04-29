// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterizedCategoryRuleDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ParameterizedCategoryRuleDialogViewModel" /> is to allow an
    /// <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" /> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" /> will result in an
    /// <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" /> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ParameterizedCategoryRule)]
    public class ParameterizedCategoryRuleDialogViewModel : CDP4CommonView.ParameterizedCategoryRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The (injected) <see cref="IFilterStringService" />
        /// </summary>
        private IFilterStringService filterStringService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialogViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterizedCategoryRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialogViewModel" /> class.
        /// </summary>
        /// <param name="parameterizedCategoryRule">
        /// The <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" /> that is the subject of the current
        /// view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction" /> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="CDP4Common.CommonData.Thing" /> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ParameterizedCategoryRuleDialogViewModel" /> is the root of all
        /// <see cref="IThingDialogViewModel" />
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ParameterizedCategoryRuleDialogViewModel" /> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService" /> that is used to navigate to a dialog of a specific
        /// <see cref="Thing" />.
        /// </param>
        /// <param name="container">
        /// The Container <see cref="CDP4Common.CommonData.Thing" /> of the created
        /// <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" />
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container" /> argument
        /// </param>
        public ParameterizedCategoryRuleDialogViewModel(ParameterizedCategoryRule parameterizedCategoryRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterizedCategoryRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.filterStringService = ServiceLocator.Current.GetInstance<IFilterStringService>();

            this.WhenAnyValue(vm => vm.Container).Subscribe(
                _ =>
                {
                    this.PopulatePossibleCategory();
                    this.PopulatePossibleParameterTypes();
                    this.UpdateOkCanExecute();
                });

            this.WhenAnyValue(vm => vm.ParameterType.Count).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Updates the <see cref="DialogViewModelBase{T}.OkCanExecute" /> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && (this.Container != null) && (this.SelectedCategory != null) && this.ParameterType.Any();
        }

        /// <summary>
        /// Populates the <see cref="PopulatePossibleCategory" /> property
        /// </summary>
        protected override void PopulatePossibleCategory()
        {
            base.PopulatePossibleCategory();

            if (this.Container is ReferenceDataLibrary containerRdl)
            {
                var rdls = containerRdl.AggregatedReferenceDataLibrary;

                var allCategories = rdls.SelectMany(x => x.DefinedCategory);
                this.PossibleCategory.AddRange(allCategories.OrderBy(c => c.ShortName));
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulatePossibleParameterTypes();
        }

        /// <summary>
        /// The populate possible parameter types.
        /// </summary>
        private void PopulatePossibleParameterTypes()
        {
            this.PossibleParameterType.Clear();
            var containerRdl = this.Container as ReferenceDataLibrary;

            if (containerRdl == null)
            {
                return;
            }

            var allParameterTypes = containerRdl.ParameterType.ToList();
            var allParameterTypesFromChainedRdl = containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType);
            allParameterTypes.AddRange(allParameterTypesFromChainedRdl);

            if (this.filterStringService == null)
            {
                this.filterStringService = ServiceLocator.Current.GetInstance<IFilterStringService>();
            }

            if (!this.filterStringService.ShowDeprecatedThings)
            {
                allParameterTypes = allParameterTypes.Where(pt => !pt.IsDeprecated).ToList();
            }

            this.PossibleParameterType.AddRange(this.ParameterType.OrderBy(c => c.ShortName));
            this.PossibleParameterType.AddRange(allParameterTypes.Except(this.ParameterType).OrderBy(c => c.ShortName));
        }
    }
}
