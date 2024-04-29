// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageHelper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    /// <summary>
    /// A utility class containing different messages than can be used in the application
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// The Pabnel closing confirmation message
        /// </summary>
        public static readonly string ClosingPanelConfirmation = "The content of this panel has not been saved." + System.Environment.NewLine + "Would you like to close and discard the changes anyway?";
    }
}