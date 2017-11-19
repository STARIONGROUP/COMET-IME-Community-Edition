// ------------------------------------------------------------------------------------------------
// <copyright file="ConfirmationEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.EventAggregator
{
    /// <summary>
    /// The confirmation event
    /// </summary>
    public class ConfirmationEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationEvent"/> class
        /// </summary>
        /// <param name="isConfirmed">The confirmation value</param>
        public ConfirmationEvent(bool isConfirmed)
        {
            this.IsConfirmed = isConfirmed;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this event carry a confirmation of cancellation
        /// </summary>
        public bool IsConfirmed { get; set; }
    }
}