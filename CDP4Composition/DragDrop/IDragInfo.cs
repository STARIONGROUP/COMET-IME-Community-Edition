// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDragInfo.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;
    using System.Windows.Input;
    
    /// <summary>
    /// The interface definition of drag information
    /// </summary>
    public interface IDragInfo
    {
        /// <summary>
        /// Gets a value indicating whether the Payload is a the result of a Multi-Select or a Single-Select enabled control
        /// </summary>
        bool IsMultiSelect { get; }

        /// <summary>
        /// Gets or sets the data that is being dragged
        /// </summary>
        object Payload { get; set; }

        /// <summary>
        /// Gets or sets the allowed effects for the drag operation
        /// </summary>
        /// <remarks>
        /// This must be set to a value other than <see cref="DragDropEffects.None"/> by a drag handler in order 
        /// for a drag to start.
        /// </remarks>
        DragDropEffects Effects { get; set; }

        /// <summary>
        /// Gets the mouse button that initiated the drag.
        /// </summary>
        MouseButton MouseButton { get; }

        /// <summary>
        /// Gets the position of the click that initiated the drag, relative to <see cref="VisualSource"/>.
        /// </summary>
        Point DragStartPosition { get; }

        /// <summary>
        /// Gets the control that initiated the drag.
        /// </summary>
        UIElement VisualSource { get; }
    }
}
