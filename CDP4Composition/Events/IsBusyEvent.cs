// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsBusyEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    /// <summary>
    /// The purpose of the <see cref="IsBusyEvent"/> is to notify an observer
    /// that the loading notification should be displayed to prevent the application to freeze
    /// </summary>
    public class IsBusyEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsBusyEvent"/> class.
        /// </summary>
        /// <param name="isBusy">
        /// The payload
        /// </param>
        /// <param name="message">
        /// The optional message
        /// </param>
        public IsBusyEvent(bool isBusy, string message = "")
        {
            this.IsBusy = isBusy;
            this.Message = message;
        }
        
        /// <summary>
        /// Gets or sets the IsBusy status
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }
    }
}