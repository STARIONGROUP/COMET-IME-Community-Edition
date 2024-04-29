// -------------------------------------------------------------------------------------------------
// <copyright file="MappingDialogNavigationResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Composition.Navigation;

    /// <summary>
    /// The dialog result for the ReqIF import mapping dialog.
    /// </summary>
    public class MappingDialogNavigationResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingDialogNavigationResult"/> class
        /// </summary>
        /// <param name="goNext">A value indicating whether the next dialog to open shall be the next mapping step or the previous one</param>
        /// <param name="result">The dialog result</param>
        public MappingDialogNavigationResult(bool? goNext, bool? result) : base(result)
        {
            this.GoNext = goNext;
        }

        /// <summary>
        /// Gets a value indicating whether the next dialog to open shall be the next mapping step or the previous one
        /// </summary>
        public bool? GoNext { get; private set; }
    }
}