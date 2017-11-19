// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    /// <summary>
    /// Enumeration that specifies what combination of <see cref="Parameter"/>, <see cref="ParameterOverride"/> and <see cref="ParameterSubscription"/>
    /// needs to be taken into account.
    /// </summary>
    public enum ValueSetKind
    {
        /// <summary>
        /// Assertion that Paramters, Overrides and Subscriptions need to be taken into account
        /// </summary>
        All = 0,

        /// <summary>
        /// Assertion that Parameters and Overrides need to be taken into account
        /// </summary>
        ParameterAndOrverride = 1,

        /// <summary>
        /// Assertion that ParameterSubscriptions need to be taken into account
        /// </summary>
        ParameterSubscription = 2
    }
}
