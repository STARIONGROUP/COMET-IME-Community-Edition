// -------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
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
    /// The purpose of the <see cref="UnitPrefixDialogViewModel"/> is to allow a <see cref="UnitPrefix"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="UnitPrefix"/> will result in an <see cref="UnitPrefix"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.UnitPrefix)]
    public class UnitPrefixDialogViewModel : CDP4CommonView.UnitPrefixDialogViewModel, IThingDialogViewModel
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPrefixDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public UnitPrefixDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryDialogViewModel"/> class.
        /// </summary>
        /// <param name="unitPrefix">
        /// The <see cref="UnitPrefix"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public UnitPrefixDialogViewModel(UnitPrefix unitPrefix, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(unitPrefix, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        #endregion
    }
}
