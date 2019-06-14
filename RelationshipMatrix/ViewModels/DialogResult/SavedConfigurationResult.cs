// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedConfigurationResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels.DialogResult
{
    using CDP4Composition.Navigation;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="SavedConfiguration"/> dialog
    /// </summary>
    public class SavedConfigurationResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavedConfigurationResult"/> class
        /// </summary>
        /// <param name="res">An instance of <see cref="Nullable{T}"/> that gives the action of the user</param>
        public SavedConfigurationResult(bool? res)
            : base(res)
        {
        }
    }
}
