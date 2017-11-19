// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Composition.Navigation;
    using Views;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="SiteRdlOpeningDialog"/> Dialog
    /// </summary>
    public class SiteRdlSelectionDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlSelectionDialogResult"/> class
        /// </summary>
        /// <param name="res">
        /// The response.
        /// </param>
        public SiteRdlSelectionDialogResult(bool? res)
            : base(res)
        {
        }

    }
}