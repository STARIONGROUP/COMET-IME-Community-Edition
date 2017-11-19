// -------------------------------------------------------------------------------------------------
// <copyright file="OrganizationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="OrganizationDialogViewModel"/> is to allow an <see cref="Organization"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Organization"/> will result in an <see cref="Organization"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Organization)]
    public class OrganizationDialogViewModel : CDP4CommonView.OrganizationDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public OrganizationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDialogViewModel"/> class.
        /// </summary>
        /// <param name="organization">
        /// The <see cref="Organization"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="OrganizationDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="OrganizationDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public OrganizationDialogViewModel(Organization organization, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(organization, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
    }
}