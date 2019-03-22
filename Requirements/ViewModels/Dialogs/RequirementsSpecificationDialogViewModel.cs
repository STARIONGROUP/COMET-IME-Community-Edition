// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;
    using System.Collections.Generic;
    using System.Linq;

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
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
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
