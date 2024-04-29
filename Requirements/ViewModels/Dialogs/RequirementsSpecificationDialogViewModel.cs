// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="RequirementsSpecificationDialogViewModel"/> is to allow a <see cref="Person"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Person"/> will result in an <see cref="Person"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.RequirementsSpecification)]
    public class RequirementsSpecificationDialogViewModel : CDP4CommonView.RequirementsSpecificationDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The Required Referance-Data-library for the current <see cref="Iteration"/>
        /// </summary>
        private ModelReferenceDataLibrary mRdl;

        /// <summary>
        /// Backing field for <see cref="ShortName"/> property
        /// </summary>
        private string shortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RequirementsSpecificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes the properties of this <see cref="Requirement"/>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var iteration = (Iteration)this.Container ?? this.ChainOfContainer.OfType<Iteration>().Single();
            var model = (EngineeringModel)iteration.Container;

            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/> properties.
        /// </summary>
        protected override void PopulateCategory()
        {
            var categories = this.mRdl.DefinedCategory.Where(x => x.PermissibleClass.Contains(ClassKind.RequirementsSpecification)).ToList();
            categories.AddRange(this.mRdl.GetRequiredRdls().SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.RequirementsSpecification)));

            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialogViewModel"/> class
        /// </summary>
        /// <param name="requirementsSpecification">
        /// The <see cref="RequirementsSpecification"/> that is the subject of the current view-model. This is the object
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
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The <see cref="RequirementsSpecificationDialogViewModel.container"/> for this <see cref="RequirementsSpecificationDialogViewModel.Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public RequirementsSpecificationDialogViewModel(RequirementsSpecification requirementsSpecification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(requirementsSpecification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "RequirementShortName")]
        public override string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// The populate possible owner.
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = (EngineeringModel)this.Container.Container;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
            this.SelectedOwner = this.PossibleOwner.FirstOrDefault(x => this.Session.ActivePerson?.DefaultDomain != null && x.Iid == this.Session.ActivePerson.DefaultDomain.Iid) ?? this.PossibleOwner.First();
        }
    }
}
