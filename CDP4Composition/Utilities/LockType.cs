// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockType.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System.ComponentModel;

    using CDP4Dal;

    /// <summary>
    /// En enumeration of lock types used to add thread safety to specific processes
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// Locks a the Session Refresh
        /// Typically used to make sure Auto Refresh for a <see cref="Session"/> and opening a browser using a <see cref="BackgroundWorker"/>
        /// do not interfere with each other, which leads to unexpected results and errors regarding thread (un)safety.
        /// </summary>
        SesionRefresh
    }
}
