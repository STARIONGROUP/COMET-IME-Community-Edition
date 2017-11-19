// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDropInfo.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// The interface definition of drop information
    /// </summary>
    public interface IDropInfo
    {
        /// <summary>
        /// Gets the data that is being dropped
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// Gets the mouse position relative to the VisualTarget
        /// </summary>
        Point DropPosition { get; }

        /// <summary>
        /// Gets or sets the class of drop target to display.
        /// </summary>
        /// <remarks>
        /// The standard drop target adorner classes are held in the <see cref="DropTargetAdorners"/>
        /// class.
        /// </remarks>
        Type DropTargetAdorner { get; set; }

        /// <summary>
        /// Gets or sets the allowed effects for the drag operation
        /// </summary>
        /// <remarks>
        /// This must be set to a value other than <see cref="DragDropEffects.None"/> by a drag handler in order 
        /// for a drag to start.
        /// </remarks>
        DragDropEffects Effects { get; set; }

        /// <summary>
        /// Gets the object that the current drop target is bound to.
        /// </summary>
        /// <remarks>
        /// If the current drop target is unbound or not an ItemsControl, this will be null.
        /// </remarks>
        object TargetItem { get; }

        /// <summary>
        /// Gets the current group target.
        /// </summary>
        /// <remarks>
        /// If the drag is currently over an ItemsControl with groups, describes the group that
        /// the drag is currently over.
        /// </remarks>
        CollectionViewGroup TargetGroup { get; }

        /// <summary>
        /// Gets the control that is the current drop target.
        /// </summary>
        UIElement VisualTarget { get; }

        /// <summary>
        /// Gets the orientation of the current drop target.
        /// </summary>
        Orientation VisualTargetOrientation { get; }

        /// <summary>
        /// Gets the current state of the modifier keys (SHIFT, CTRL, and ALT), as well as the state of the mouse buttons.
        /// </summary>
        DragDropKeyStates KeyStates { get; }
    }
}
