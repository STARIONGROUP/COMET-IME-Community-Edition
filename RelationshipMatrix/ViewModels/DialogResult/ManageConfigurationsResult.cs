// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageConfigurationsResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels.DialogResult
{
    using CDP4Composition.Navigation;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="ManageConfigurationsDialogViewModel"/> dialog
    /// </summary>
    public class ManageConfigurationsResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConfigurationsResult"/> class
        /// </summary>
        /// <param name="res">An instance of <see cref="Nullable{T}"/> that gives the action of the user</param>
        public ManageConfigurationsResult(bool? res)
            : base(res)
        {
        }
    }
}
