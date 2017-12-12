// ------------------------------------------------------------------------------------------------
// <copyright file="IThingDialogNavigationService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The Interface for Dialog Navigation classes.
    /// </summary>
    public interface IThingDialogNavigationService
    {
        /// <summary>
        /// Navigates to the dialog associated to the specified <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> for which a dialog window needs to be opened
        /// </param>
        /// <param name="transaction">
        /// The transaction that is used to record changes on the <see cref="Thing"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> which is used to persist the changes recorded in the <see cref="transaction"/>
        /// </param>
        /// <param name="isRoot">
        /// Assert if the <see cref="IThingDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation the <see cref="IThingDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> for the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        /// <returns>
        /// true if the dialog is confirmed, false if otherwise.
        /// </returns>
        bool? Navigate(Thing thing, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null);
    }
}
