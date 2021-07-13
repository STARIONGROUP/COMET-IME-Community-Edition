// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramSelectEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;

    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;

    using DevExpress.Xpf.Diagram;

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
        public DiagramSelectEvent(IEnumerable<DiagramContentItem> selectedViewModels)
        {
            this.SelectedViewModels = new ReactiveList<DiagramContentItem>() { };
            this.SelectedViewModels.AddRange(selectedViewModels);
        }

        /// <summary>
        /// Gets the view-model that should be deleted
        /// </summary>
        public ReactiveList<DiagramContentItem> SelectedViewModels { get; private set; }
    }
}