// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisposableExtensionMethods.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The purpose of these <see cref="DisposableExtensionMethods"/> is to add functionality to <see cref="IDisposable"/> and <see cref="List{IDisposable}"/> instances
    /// </summary>
    public static class DisposableExtensionMethods
    {
        /// <summary>
        /// Checks is an <see cref="IDisposable"/> is already contained in a <see cref="List{IDisposable}"/>.
        /// If not the <see cref="IDisposable"/> is added to the <see cref="List{IDisposable}"/>
        /// </summary>
        /// <param name="disposables">The <see cref="List{IDisposable}"/></param>
        /// <param name="disposable">The <see cref="IDisposable"/></param>
        public static void AddIfNotExists(this List<IDisposable> disposables, IDisposable disposable)
        {
            if (!disposables.Contains(disposable))
            {
                disposables.Add(disposable);
            }
        }
    }
}