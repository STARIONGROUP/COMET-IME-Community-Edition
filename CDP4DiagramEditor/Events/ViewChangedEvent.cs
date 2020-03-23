

namespace CDP4DiagramEditor.Events
{
    using System.Windows.Controls;

    using CDP4DiagramEditor.ViewModels;

    public enum EventKind
    {
        Showing,
        Disappearing,
        Unknown
    }

    /// <summary>
    /// The purpose of the <see cref="T:CDP4Dal.Events.ObjectChangedEvent" /> is to notify an observer
    /// that the referenced <see cref="T:CDP4Common.CommonData.control" /> has changed in some way and what that change is.
    /// </summary>
    public class ViewChangedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewChangedEvent" /> class.
        /// </summary>
        /// <param name="control">
        /// The payload <see cref="ViewChangedEvent" />.
        /// </param>
        /// <param name="eventKind">The event kind.</param>
        public ViewChangedEvent(IViewChangedEventUsingCapable control, EventKind eventKind)
        {
            this.ChangedView = control;
            this.EventKind = eventKind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewChangedEvent" /> class.
        /// </summary>
        /// <param name="control">
        /// The payload <see cref="T:CDP4Common.CommonData.control" />
        /// </param>
        public ViewChangedEvent(IViewChangedEventUsingCapable control)
        {
            this.ChangedView = control;
            this.EventKind = EventKind.Showing;
        }

        /// <summary>
        /// Gets or sets the changed <see cref="T:CDP4Common.CommonData.control" />
        /// </summary>
        public IViewChangedEventUsingCapable ChangedView { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="P:CDP4Dal.Events.ObjectChangedEvent.EventKind" /> to be transported.
        /// </summary>
        public EventKind EventKind { get; set; }
    }
}

