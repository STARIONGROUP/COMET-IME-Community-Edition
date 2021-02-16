// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObfuscationService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed.
// 
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Services
{
    using System;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    /// <summary>
    /// Service used for resolving obfuscated rows
    /// </summary>
    public interface IObfuscationService : IDisposable
    {
        /// <summary>
        /// Determines whether a row should be obfuscated based on the provided <see cref="Thing" /> of that row.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <param name="session">The session</param>
        /// <returns>True if row is obfuscated.</returns>
        bool IsRowObfuscated(Thing thing);

        /// <summary>
        /// Initializes the obfuscation service for a particular iteration
        /// </summary>
        /// <param name="iteration">The Iteration this service is valid for.</param>
        /// <param name="session">The session.</param>
        void Initialize(Iteration thing, ISession session);
    }
}
