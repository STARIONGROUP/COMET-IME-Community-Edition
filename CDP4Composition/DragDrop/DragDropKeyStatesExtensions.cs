// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragDropKeyStatesExtensions.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Jaime Bernar
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Windows;

    using CDP4DalCommon.Protocol.Operations;

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