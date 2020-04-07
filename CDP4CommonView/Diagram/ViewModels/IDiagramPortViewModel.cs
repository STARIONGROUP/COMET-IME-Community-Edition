// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramObjectViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Composition.Mvvm;

    using Point = System.Windows.Point;

    /// <summary>
    /// The interface for view-model that shall be bound to a diagram object
    /// </summary>
    public interface IDiagramPortViewModel : IRowViewModelBase<DiagramObject>
    {
        /// <summary>
        /// Gets or sets the position
        /// </summary>
        System.Windows.Point Position { get; set; }

        PortContainerShapeSide PortContainerShapeSide { get; set; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        double Width { get; set; }

        Bounds ContainerBounds { get; set; }
    }
}