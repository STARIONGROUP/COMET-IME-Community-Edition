// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISessionCreator.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
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

using CDP4Composition.Extensions;

namespace CDP4Composition.Services
{
    using CDP4Common.ExceptionHandlerService;

    using CDP4Dal;
    using CDP4Dal.DAL;

    /// <summary>
    /// Defines the properties and methods for the <see cref="SessionCreator"/>
    /// </summary>
    public interface ISessionCreator
    {
        /// <summary>
        /// Creates an <see cref="ISession"/> and tries to run <see cref="ISessionCreationHook"/>s if found.
        /// </summary>
        /// <param name="dal">The <see cref="IDal"/></param>
        /// <param name="credentials">The <see cref="Credentials"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="exceptionHandlerService">The <see cref="IExceptionHandlerService"/></param>
        /// <returns></returns>
        ISession CreateSession(IDal dal, Credentials credentials, ICDPMessageBus messageBus, IExceptionHandlerService exceptionHandlerService);
    }
}
