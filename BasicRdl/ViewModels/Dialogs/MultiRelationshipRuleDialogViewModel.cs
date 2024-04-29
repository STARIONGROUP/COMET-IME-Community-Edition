// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRuleDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="MultiRelationshipRuleDialogViewModel"/> is to allow an <see cref="CDP4Common.SiteDirectoryData.MultiRelationshipRule"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="CDP4Common.SiteDirectoryData.MultiRelationshipRule"/> will result in an <see cref="CDP4Common.SiteDirectoryData.MultiRelationshipRule"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.MultiRelationshipRule)]
    public class MultiRelationshipRuleDialogViewModel : CDP4CommonView.MultiRelationshipRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public MultiRelationshipRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="multiRelationshipRule">
        /// The <see cref="CDP4Common.SiteDirectoryData.MultiRelationshipRule"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="CDP4Common.CommonData.Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="MultiRelationshipRuleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="MultiRelationshipRuleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The Container <see cref="CDP4Common.CommonData.Thing"/> of the created <see cref="CDP4Common.SiteDirectoryData.MultiRelationshipRule"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public MultiRelationshipRuleDialogViewModel(MultiRelationshipRule multiRelationshipRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(multiRelationshipRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.RepopulatePossibleCategories());
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedRelationshipCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.RelatedCategory).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Repopulate the possible <see cref="CDP4Common.SiteDirectoryData.Category"/>
        /// </summary>
        private void RepopulatePossibleCategories()
        {
            this.PopulatePossibleRelationshipCategory();
            this.PopulatePossibleRelatedCategory();
        }

        /// <summary>
        /// Populates the <see cref="CDP4CommonView.MultiRelationshipRuleDialogViewModel.PossibleRelatedCategory"/> collection
        /// </summary>        
        private void PopulatePossibleRelatedCategory()
        {
            this.PossibleRelatedCategory.Clear();

            var categories = this.GetPossibleCategories();
            foreach (var category in categories)
            {
                this.PossibleRelatedCategory.Add(category);
            }
        }

        /// <summary>
        /// populates the <see cref="CDP4CommonView.MultiRelationshipRuleDialogViewModel.PossibleRelationshipCategory"/> property
        /// </summary>
        protected override void PopulatePossibleRelationshipCategory()
        {
            base.PopulatePossibleRelationshipCategory();

            var categories = this.GetPossibleCategories();
            foreach (var category in categories)
            {
                this.PossibleRelationshipCategory.Add(category);
            }

            if (this.SelectedRelationshipCategory == null)
            {
                this.SelectedRelationshipCategory = this.PossibleRelationshipCategory.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets all the <see cref="CDP4Common.SiteDirectoryData.Category"/> from the chain of <see cref="ReferenceDataLibrary"/> of the container <see cref="ReferenceDataLibrary"/>
        /// </summary>
        /// <returns>The list of <see cref="CDP4Common.SiteDirectoryData.Category"/></returns>
        private IEnumerable<Category> GetPossibleCategories()
        {
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return new List<Category>();
            }

            var allPossibleSuperCategories = new List<Category>(containerRdl.DefinedCategory);

            foreach (var rdl in containerRdl.GetRequiredRdls())
            {
                allPossibleSuperCategories.AddRange(rdl.DefinedCategory);
            }

            return allPossibleSuperCategories.OrderBy(c => c.ShortName).ToList();
        }

        /// <summary>
        /// Updates the <see cref="DialogViewModelBase{T}.OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            var noValidationErrors = !this.ValidationErrors.Any();

            this.OkCanExecute = noValidationErrors 
                && this.Container != null
                && this.SelectedRelationshipCategory != null
                && this.RelatedCategory.Any();
        }
    }
}
