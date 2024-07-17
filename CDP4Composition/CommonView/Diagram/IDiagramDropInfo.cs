// -------------------------------------------------------------------------------------------------
// <copyright file="IDiagramDropInfo.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Windows;
    using CDP4Composition.DragDrop;

    /// <summary>
    /// The <see cref="IDropInfo"/> interface specific to diagrams
    /// </summary>
    public interface IDiagramDropInfo : IDropInfo
    {
        /// <summary>
        /// Gets the drop position relatively to a diagram
        /// </summary>
        Point DiagramDropPoint { get; set; }
    }
}