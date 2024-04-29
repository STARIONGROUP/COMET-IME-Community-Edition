// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskbarNotificationEvent.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    /// <summary>
    /// Assertions that determine the kind of notification to show
    /// </summary>
    public enum NotificationKind
    {
        /// <summary>
        /// Asserts that the notification is a basic one (no icon should be displayed)
        /// </summary>
        BASIC,

        /// <summary>
        /// Asserts that the notification is an information (an information icon shall be displayed)
        /// </summary>
        INFO,

        /// <summary>
        /// Asserts that the notification is a warning (a warning icon shall be displayed)
        /// </summary>
        WARNING,

        /// <summary>
        /// Asserts that the notification is an error (an error icon shall be displayed)
        /// </summary>
        ERROR
    }


    /// <summary>
    /// The event to show a notification in the windows task bar
    /// </summary>
    public class TaskbarNotificationEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskbarNotificationEvent"/> class
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">The message of the notification</param>
        /// <param name="notificationKind">The <see cref="NotificationKind"/></param>
        public TaskbarNotificationEvent(string title, string message, NotificationKind notificationKind)
        {
            this.Title = title;
            this.Message = message;
            this.NotificationKind = notificationKind;
        }

        /// <summary>
        /// Gets the title of the notification
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the message of the notification
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the <see cref="NotificationKind"/> of the notification
        /// </summary>
        public NotificationKind NotificationKind { get; private set; }
    }
}