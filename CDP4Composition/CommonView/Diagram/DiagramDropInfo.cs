// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramDropInfo.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Windows;
    using CDP4Composition.DragDrop;

    /// <summary>
    /// Extended class specific to diagrams
    /// </summary>
    public class DiagramDropInfo : DropInfo, IDiagramDropInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDropInfo"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the drag event.
        /// </param>
        /// <param name="e">
        /// The drag event
        /// </param>
        public DiagramDropInfo(object sender, DragEventArgs e) : base(sender, e)
        {
        }

        /// <summary>
        /// Gets or sets the drop point relatively to a diagram control
        /// </summary>
        public Point DiagramDropPoint { get; set; }
    }
}