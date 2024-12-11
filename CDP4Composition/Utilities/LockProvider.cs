// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockProvider.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Provides locking capabilities for making specific code thread safe
    /// </summary>
    public static class LockProvider
    {
        /// <summary>
        /// ConcurrentDictionary ensures thread-safe access to lock objects
        /// </summary>
        private static readonly ConcurrentDictionary<LockType, object> _locks = new();

        /// <summary>
        /// Enter a locked status for a specific key (<see cref="LockType"/>)
        /// </summary>
        /// <param name="key">The <see cref="LockType"/></param>
        public static void EnterLock(LockType key)
        {
            var lockObject = _locks.GetOrAdd(key, _ => new object());
            Monitor.Enter(lockObject);
        }

        /// <summary>
        /// Exists from a locked status for a specific key (<see cref="LockType"/>)
        /// </summary>
        /// <param name="key">The <see cref="LockType"/></param>
        public static void ExitLock(LockType key)
        {
            if (_locks.TryGetValue(key, out var lockObject))
            {
                Monitor.Exit(lockObject);
            }
            else
            {
                throw new InvalidOperationException($"Lock not found for the specified key: {key}.");
            }
        }
    }
}
