﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusEventHandlerSubscription.cs" company="Starion Group S.A.">
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
    /// Handles message bus events for a specific <see cref="Type"/>
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of message bus events</typeparam>
    public class MessageBusEventHandlerSubscription<T> : IMessageBusEventHandlerSubscription where T : class
    {
        /// <summary>
        /// The discriminator code (sort of Where statement) to check if ExecuteAction is allowed to be executed
        /// </summary>
        private Func<T, bool> Discriminator { get; }

        /// <summary>
        /// The code that needs to be executed according to a message bus event
        /// </summary>
        private Action<T> Action { get; }

        /// <summary>
        /// Gets or sets a value indicating that this instance is disposed, hence not usable anymore
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="MessageBusHandlerData"/> class
        /// </summary>
        /// <param name="discriminator">The discriminator code</param>
        /// <param name="action">The to be executed code</param>
        public MessageBusEventHandlerSubscription(Func<T, bool> discriminator, Action<T> action)
        {
            Discriminator = discriminator;
            Action = action;
        }

        /// <summary>
        /// Executes discriminator code (sort of Where statement) to check if ExecuteAction is allowed to be executed
        /// </summary>
        /// <param name="messageBusEvent">The type of message bus event</param>
        /// <returns>True if ExecuteAction is allowed, otherwise false</returns>
        public virtual bool ExecuteDiscriminator(object messageBusEvent)
        {
            return !this.IsDisposed && (Discriminator?.Invoke(messageBusEvent as T) ?? true);
        }

        /// <summary>
        /// Executes code that needs to run according to a message bus event
        /// </summary>
        /// <param name="messageBusEvent">The type of message bus event</param>
        public virtual void ExecuteAction(object messageBusEvent)
        {
            this.Action?.Invoke(messageBusEvent as T);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.IsDisposed = true;
        }
    }
}
