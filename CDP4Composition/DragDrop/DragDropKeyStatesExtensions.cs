// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragDropKeyStatesExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;
    using CDP4Dal.Operations;

    /// <summary>
    /// The <see cref="DragDropKeyStatesExtensions"/> class specifies Extension methods for the <see cref="DragDropKeyStates"/>
    /// </summary>
    public static class DragDropKeyStatesExtensions
    {
        /// <summary>
        /// Gets the <see cref="OperationKind"/> related to a <see cref="DragDropKeyStates"/> when a drag-and-drop operation is performed
        /// </summary>
        /// <param name="keyStates">
        /// The <see cref="DragDropKeyStates"/></param> for
        /// <returns>
        /// The <see cref="OperationKind"/> or null
        /// </returns>
        public static OperationKind? GetCopyOperationKind(this DragDropKeyStates keyStates)
        {
            if (keyStates == Constants.DryCopy)
            {
                return OperationKind.CopyDefaultValuesChangeOwner;
            }

            if (keyStates == Constants.CtrlCopy)
            {
                return OperationKind.CopyKeepValuesChangeOwner;
            }

            if (keyStates == Constants.ShiftCopy)
            {
                return OperationKind.Copy;
            }

            if (keyStates == Constants.CtlrShiftCopy)
            {
                return OperationKind.CopyKeepValues;
            }

            return null;
        }
    }
}