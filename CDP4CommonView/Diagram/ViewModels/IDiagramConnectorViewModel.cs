// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;
    using CDP4Common.DiagramData;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The interface that shall be realized by view-models representing a <see cref="DiagramEdge"/>
    /// </summary>
    public interface IDiagramConnectorViewModel : IRowViewModelBase<DiagramEdge>
    {
        /// <summary>
        /// Gets the connection points for the represented <see cref="DiagramEdge"/>
        /// </summary>
        List<System.Windows.Point> ConnectingPoints { get; }

        /// <summary>
        /// Gets the source of the <see cref="DiagramEdge"/>
        /// </summary>
        DiagramElementThing Source { get; set; }

        /// <summary>
        /// Gets the target of the <see cref="DiagramEdge"/>
        /// </summary>
        DiagramElementThing Target { get; set; }

        /// <summary>
        /// Gets the text to display
        /// </summary>
        string DisplayedText { get; }
    }
}