// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramSelectEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using CDP4Common.DiagramData;
    using CDP4Composition.Mvvm;
    using EventAggregator;

    using ReactiveUI;

    /// <summary>
    /// A select event for the diagramming tool
    /// </summary>
    public class DiagramSelectEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramSelectEvent"/> class
        /// </summary>
        /// <param name="selectedViewModels">The selected view-models</param>
        public DiagramSelectEvent(ReactiveList<DiagramElementThing> selectedViewModels)
        {
            this.SelectedViewModels = selectedViewModels;
        }

        /// <summary>
        /// Gets the view-model that should be deleted
        /// </summary>
        public ReactiveList<DiagramElementThing> SelectedViewModels { get; private set; }
    }
}