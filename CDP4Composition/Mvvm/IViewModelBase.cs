// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewModelBase.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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

namespace CDP4Composition.Mvvm
{
    using System;

    using CDP4Common.CommonData;

    using CDP4Dal;

    /// <summary>
    /// A Covariant interface that allows the <see cref="Thing"/> to be of a more generic type
    /// </summary>
    /// <typeparam name="T">
    /// The type parameter
    /// </typeparam>
    public interface IViewModelBase<out T> : IDisposable where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IViewModelBase{T}"/>
        /// </summary>
        T Thing { get; }

        /// <summary>
        /// Gets the <see cref="ICDPMessageBus"/>
        /// </summary>
        ICDPMessageBus CDPMessageBus { get; }
    }
}
