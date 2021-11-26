// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="RequirementsGroupDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="RequirementsGroup"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.RequirementsGroup)]
    public class RequirementsGroupDialogViewModel : CDP4CommonView.RequirementsGroupDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The Required Referance-Data-library for the current <see cref="Iteration"/>
        /// </summary>
        private ModelReferenceDataLibrary mRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RequirementsGroupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupDialogViewModel"/> class.
        /// </summary>
        /// <param name="requirementsGroup">
        /// The <see cref="RequirementsGroup"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The <see cref="RequirementsSpecificationDialogViewModel.container"/> for this <see cref="RequirementsSpecificationDialogViewModel.Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public RequirementsGroupDialogViewModel(RequirementsGroup requirementsGroup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(requirementsGroup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Initializes the properties of this <see cref="Requirement"/>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var iteration = this.Container.GetContainerOfType<Iteration>();
            var model = (EngineeringModel)iteration.Container;

            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/> properties.
        /// </summary>
        protected override void PopulateCategory()
        {
            var categories = this.mRdl.DefinedCategory.Where(x => x.PermissibleClass.Contains(ClassKind.RequirementsGroup)).ToList();
            categories.AddRange(this.mRdl.GetRequiredRdls().SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.RequirementsGroup)));

            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="RequirementsGroupDialogViewModel.PossibleOwner"/>
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var engineeringModel = (EngineeringModel)this.Container.TopContainer;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
            this.SelectedOwner = this.PossibleOwner.FirstOrDefault(x => this.Session.ActivePerson?.DefaultDomain != null && x.Iid == this.Session.ActivePerson.DefaultDomain.Iid) ?? this.PossibleOwner.First();
        }
    }
}
