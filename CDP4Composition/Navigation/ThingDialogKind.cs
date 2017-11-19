// ------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// enum used for <see cref="IThingDialogViewModel"/> to identify the kind of operation performed by the dialog
    /// </summary>
    public enum ThingDialogKind
    {
        /// <summary>
        /// The <see cref="IThingDialogViewModel"/> performs a creation
        /// </summary>
        Create,

        /// <summary>
        /// The <see cref="IThingDialogViewModel"/> performs an update
        /// </summary>
        Update,

        /// <summary>
        /// The <see cref="IThingDialogViewModel"/> performs an inspection
        /// </summary>
        Inspect
    }
}