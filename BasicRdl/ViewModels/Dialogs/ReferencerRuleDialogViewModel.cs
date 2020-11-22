// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferencerRuleDialogViewModel.cs" company="RHEA System S.A.">
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
    /// The purpose of the <see cref="ReferencerRuleDialogViewModel"/> is to allow an <see cref="CDP4Common.SiteDirectoryData.ReferencerRule"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="CDP4Common.SiteDirectoryData.ReferencerRule"/> will result in an <see cref="CDP4Common.SiteDirectoryData.ReferencerRule"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ReferencerRule)]
    public class ReferencerRuleDialogViewModel : CDP4CommonView.ReferencerRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencerRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ReferencerRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencerRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="referencerRule">
        /// The <see cref="CDP4Common.SiteDirectoryData.ReferencerRule"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="CDP4Common.CommonData.Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ReferencerRuleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ReferencerRuleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The Container <see cref="CDP4Common.CommonData.Thing"/> of the created <see cref="CDP4Common.SiteDirectoryData.ReferencerRule"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ReferencerRuleDialogViewModel(ReferencerRule referencerRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(referencerRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Initializes the commands and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.WhenAnyValue(x => x.Container).Subscribe(_ =>
            {
                this.PopulatePossibleReferencedCategory();
                this.PopulatePossibleReferencingCategory();
                this.UpdateOkCanExecute();
            });
            this.WhenAnyValue(x => x.ReferencedCategory.Count).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.SelectedReferencingCategory).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.MinReferenced).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.MaxReferenced).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.ReferencerRuleDialogViewModel.PossibleReferencingCategory"/> property
        /// </summary>
        protected override void PopulatePossibleReferencingCategory()
        {
            base.PopulatePossibleReferencingCategory();
            if (this.Container == null)
            {
                return;
            }

            var rdlContainer = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdlContainer.GetRequiredRdls()) { rdlContainer };
            var categories = rdls.SelectMany(x => x.DefinedCategory)
                .Where(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)).OrderBy(x => x.Name);

            this.PossibleReferencingCategory.AddRange(categories);
            if (this.SelectedReferencingCategory == null)
            {
                this.SelectedReferencingCategory = this.PossibleReferencingCategory.FirstOrDefault();
            }
        }

        /// <summary>
        /// Populate the <see cref="CDP4CommonView.ReferencerRuleDialogViewModel.PossibleReferencedCategory"/> property
        /// </summary>
        protected void PopulatePossibleReferencedCategory()
        {
            this.PossibleReferencedCategory.Clear();
            if (this.Container == null)
            {
                return;
            }

            var rdlContainer = (ReferenceDataLibrary)this.Container;
            var rdls = new List<ReferenceDataLibrary>(rdlContainer.GetRequiredRdls()) { rdlContainer };
            var categories = rdls.SelectMany(x => x.DefinedCategory)
                .Where(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)).OrderBy(x => x.Name);

            this.PossibleReferencedCategory.AddRange(categories);
            this.PopulateReferencedCategory();
        }

        /// <summary>
        /// Update the <see cref="DialogViewModelBase{T}.OkCanExecute"/> value
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            var isReferenceAmountOk = this.MaxReferenced >= this.MinReferenced && this.ReferencedCategory.Count > 0 && this.ReferencedCategory.Count >= this.MinReferenced;
            this.OkCanExecute = this.OkCanExecute && this.SelectedReferencingCategory != null && isReferenceAmountOk;
        }
    }
}