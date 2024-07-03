// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DalExtensionMethods.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Extensions
{
    using CDP4Common.ExceptionHandlerService;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CommonServiceLocator;

    /// <summary>
    /// The purpose of these <see cref="DalExtensionMethods"/> is to add functionality to <see cref="IDal"/> instances
    /// </summary>
    public static class DalExtensionMethods
    {
        /// <summary>
        /// Creates an <see cref="ISession"/> and tries to run <see cref="ISessionCreationHook"/>s if found.
        /// </summary>
        /// <param name="dal">The <see cref="IDal"/></param>
        /// <param name="credentials">The <see cref="Credentials"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="exceptionHandlerService">The <see cref="IExceptionHandlerService"/></param>
        /// <returns></returns>
        public static ISession CreateSession(this IDal dal, Credentials credentials, ICDPMessageBus messageBus, IExceptionHandlerService exceptionHandlerService)
        {
            var session = new Session(dal, credentials, messageBus, exceptionHandlerService);

            try
            {
                foreach (var sessionCreationHook in ServiceLocator.Current.GetAllInstances<ISessionCreationHook>())
                {
                    sessionCreationHook.Hook(session);
                }
            }
            catch
            {
                //Probably no ISessionCreationHook implementations, so please continue
            }

            return session;
        }
    }
}