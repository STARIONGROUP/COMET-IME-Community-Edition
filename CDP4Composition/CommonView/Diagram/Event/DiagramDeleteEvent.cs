// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramDeleteEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using CDP4Common.CommonData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;
    using EventAggregator;

    /// <summary>
    /// A delete event for the diagramming tool
    /// </summary>
    public class DiagramDeleteEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDeleteEvent"/> class
        /// </summary>
        /// <param name="viewModelDeleted">The view-model instance to delete</param>
        public DiagramDeleteEvent(object viewModelDeleted)
        {
            this.ViewModel = viewModelDeleted;
        }

        /// <summary>
        /// Gets the view-model that should be deleted
        /// </summary>
        public object ViewModel { get; private set; }
    }
}