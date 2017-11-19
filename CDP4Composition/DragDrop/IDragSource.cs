// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDragSource.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;

    /// <summary>
    /// A drag source (view-model) implements this interface to support a drag operation to be started
    /// </summary>
    public interface IDragSource
    {
        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        void StartDrag(IDragInfo dragInfo);
    }
}
