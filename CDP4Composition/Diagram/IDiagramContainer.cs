// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramContainer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System.Collections;
    using DevExpress.Xpf.Diagram;
    using Mvvm.Behaviours;
    using ReactiveUI;

    /// <summary>
    /// The interface allows a behavior to pass on a list of items to remove from the items collection in the view model.
    /// </summary>
    public interface IDiagramContainer
    {
        /// <summary>
        /// Gets or sets the behaviour.
        /// </summary>
        IExtendedDiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Get or set the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        DiagramItem SelectedItem { get; set; }

        /// <summary>
        /// Get or set the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        ReactiveList<DiagramItem> SelectedItems { get; set; }

        /// <summary>
        /// Removes items provided by the behavior.
        /// </summary>
        /// <param name="oldItems">The list of items to be removed.</param>
        void RemoveItems(IList oldItems);
    }
}
