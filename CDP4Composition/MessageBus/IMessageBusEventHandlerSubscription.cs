﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageBusEventHandlerSubscription.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.MessageBus
{
    using System;

    /// <summary>
    /// Describes the properties and methods of a messagebus handler data object
    /// </summary>
    public interface IMessageBusEventHandlerSubscription : IDisposable
    {
        /// <summary>
        /// Executes discriminator code (sort of Where statement) to check if ExecuteAction is allowed to be executed
        /// </summary>
        /// <param name="messageBusEvent">The type of message bus event</param>
        /// <returns>True if ExecuteAction is allowed, otherwise false</returns>
        bool ExecuteDiscriminator(object messageBusEvent);

        /// <summary>
        /// Executes code that needs to run according to a message bus event
        /// </summary>
        /// <param name="messageBusEvent">The type of message bus event</param>
        void ExecuteAction(object messageBusEvent);
    }
}