// -------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Composition.Navigation;

    /// <summary>
    /// The purpose of the <see cref="SubmitConfirmationDialogResult"/> is to return a value
    /// that specifies that the parameter sheet changes shall be submitted, and the change
    /// SubmitMessage.
    /// </summary>
    public class SubmitConfirmationDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitConfirmationDialogResult"/> class.
        /// </summary>
        /// <param name="result">
        /// The result (true or false)
        /// </param>
        /// <param name="submitMessage">
        /// The SubmitMessage that describes the changes that were made..
        /// </param>
        /// <param name="clones">
        /// The the <see cref="Thing"/> clones that need to be submitted.
        /// </param>
        public SubmitConfirmationDialogResult(bool? result, string submitMessage, IEnumerable<Thing> clones)
            : base(result)
        {
            this.SubmitMessage = submitMessage;
            this.Clones = clones;
        }

        /// <summary>
        /// Gets the SubmitMessage that describes the changes that were made.
        /// </summary>
        public string SubmitMessage { get; private set; }

        /// <summary>
        /// Gets the <see cref="Thing"/> clones that need to be submitted.
        /// </summary>
        public IEnumerable<Thing> Clones { get; private set; }
    }
}
