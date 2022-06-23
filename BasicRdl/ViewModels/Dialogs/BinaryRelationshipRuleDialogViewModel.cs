// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRuleDialogViewModel.cs" company="RHEA System S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="BinaryRelationshipRuleDialogViewModel"/> is to allow an <see cref="BinaryRelationshipRule"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="BinaryRelationshipRule"/> will result in an <see cref="BinaryRelationshipRule"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.BinaryRelationshipRule)]
    public class BinaryRelationshipRuleDialogViewModel : CDP4CommonView.BinaryRelationshipRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BinaryRelationshipRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="binaryRelationshipRule">
        /// The <see cref="BinaryRelationshipRule"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="MultiRelationshipRuleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="MultiRelationshipRuleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public BinaryRelationshipRuleDialogViewModel(BinaryRelationshipRule binaryRelationshipRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(binaryRelationshipRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => { this.RepopulatePossibleCategories(); this.UpdateOkCanExecute(); });
            this.WhenAnyValue(vm => vm.SelectedRelationshipCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedSourceCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.SelectedTargetCategory).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// populates the <see cref="BinaryRelationshipRuleDialogViewModel.PossibleRelationshipCategory"/>
        /// </summary>
        protected override void PopulatePossibleRelationshipCategory()
        {
            base.PopulatePossibleRelationshipCategory();

            var categories = this.GetPossibleCategories().Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship));
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
        /// populates the <see cref="BinaryRelationshipRuleDialogViewModel.PossibleSourceCategory"/> property
        /// </summary>
        protected override void PopulatePossibleSourceCategory()
        {
            base.PopulatePossibleSourceCategory();
            var categories = this.GetPossibleCategories();
            foreach (var category in categories)
            {
                this.PossibleSourceCategory.Add(category);
            }

            if (this.SelectedSourceCategory == null)
            {
                this.SelectedSourceCategory = this.PossibleSourceCategory.First();
            }
        }

        /// <summary>
        /// Populates the <see cref="BinaryRelationshipRuleDialogViewModel.PossibleTargetCategory"/>
        /// </summary>
        protected override void PopulatePossibleTargetCategory()
        {
            base.PopulatePossibleTargetCategory();
            var categories = this.GetPossibleCategories();
            foreach (var category in categories)
            {
                this.PossibleTargetCategory.Add(category);
            }

            if (this.SelectedTargetCategory == null)
            {
                this.SelectedTargetCategory = this.PossibleTargetCategory.First();
            }
        }

        /// <summary>
        /// Update the <see cref="BinaryRelationshipRuleDialogViewModel.OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.Container != null && this.SelectedRelationshipCategory != null &&
                                this.SelectedSourceCategory != null && this.SelectedTargetCategory != null;
        }

        /// <summary>
        /// Gets all the <see cref="Category"/> from the chain of <see cref="ReferenceDataLibrary"/> of the container <see cref="ReferenceDataLibrary"/>
        /// </summary>
        /// <returns>The list of <see cref="Category"/></returns>
        private IEnumerable<Category> GetPossibleCategories()
        {
            var rdlContainer = this.Container as ReferenceDataLibrary;

            if (rdlContainer == null)
            {
                throw new NullReferenceException("The selected Container is not a RDL.");
            }

            var allPossibleSuperCategories = new List<Category>(rdlContainer.DefinedCategory);

            foreach (var rdl in rdlContainer.GetRequiredRdls())
            {
                allPossibleSuperCategories.AddRange(rdl.DefinedCategory);
            }

            return allPossibleSuperCategories.OrderBy(c => c.ShortName).ToList();
        }

        /// <summary>
        /// Repopulate the possible <see cref="Category"/>
        /// </summary>
        private void RepopulatePossibleCategories()
        {
            this.PopulatePossibleRelationshipCategory();
            this.PopulatePossibleSourceCategory();
            this.PopulatePossibleTargetCategory();
        }
    }
}