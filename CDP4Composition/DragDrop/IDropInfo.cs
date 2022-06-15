// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDropInfo.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using Microsoft.Xaml.Behaviors;

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

        /// <summary>
        /// Gets the mouse position relative to the drop target
        /// </summary>
        Point RelativePositionToDrop { get; }

        /// <summary>
        /// Gets a value indicating whether the item is dropped after this <see cref="TargetItem"/> (false means before)
        /// </summary>
        bool IsDroppedAfter { get; }

        /// <summary>
        /// Indicates that Drop/Drag functionality is handled by another class. See <see cref="DragEventArgs.Handled"/>.
        /// </summary>
        /// <remarks>
        /// The property was added at a later time so it is only implemented at specific places in the application!!!
        /// It was introduced specifically in case a complicated Drag/Drop event firing sequence is used.
        /// For example, when the Diagram Editor is used combined with a <see cref="Behavior{T}"/>.
        /// It doesn't make the event firing sequence less complex, but can help to prioritise specific functionality
        /// created in Preview* and normal event handlers like PreviewDrop and Drop.
        /// </remarks>
        bool Handled { get; set; }
    }
}
