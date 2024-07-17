// ------------------------------------------------------------------------------------------------
// <copyright file="IDialogNavigationService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using CDP4Common.CommonData;
    using CDP4Dal.Composition;
    using Interfaces;

    /// <summary>
    /// The Interface for Dialog Navigation classes
    /// </summary>
    public interface IDialogNavigationService
    {
        /// <summary>
        /// Navigates to the dialog associated with the <see cref="IDialogViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IDialogViewModel"/> associated to the Dialog Box to navigate to</param>
        /// <returns>The <see cref="IDialogResult"/></returns>
        IDialogResult NavigateModal(IDialogViewModel viewModel);

        /// <summary>
        /// Navigates to the dialog associated to the <see cref="IDialogViewModel"/> which has its <see cref="INameMetaData.Name"/> equals to the <see cref="viewModelName"/>.
        /// </summary>
        /// <param name="viewModelName">The name we want to compare to the <see cref="INameMetaData.Name"/> of the view-models.</param>
        /// <returns>The <see cref="IDialogResult"/>.</returns>
        IDialogResult NavigateModal(string viewModelName);

        /// <summary>
        /// Navigates to the floating dialog given a <see cref="IFloatingDialogViewModel{Thing}"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IFloatingDialogViewModel{Thing}"/></param>
        void NavigateFloating(IFloatingDialogViewModel<Thing> viewModel);

        /// <summary>
        /// Clear the <see cref="IDialogNavigationService"/> from the <see cref="IDialogView"/> that is being closed
        /// </summary>
        /// <param name="view">The closing <see cref="IDialogView"/></param>
        void ClosingFloatingWindow(IDialogView view);
    }
}