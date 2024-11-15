// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusHandler.cs" company="Starion Group S.A.">
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
    using System.Collections.Concurrent;

    /// <summary>
    /// Handles the creation and usage of <see cref="MessageBusEventHandler{T}"/> classes as an alternative to the creation of many many message bus subscriptions
    /// </summary>
    public class MessageBusHandler : IDisposable
    {
        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// A <see cref="ConcurrentDictionary{TKey, TValue}"/> of type <see cref="Type"/> and <see cref="IMessageBusEventHandlerBase"/>
        /// </summary>
        private readonly ConcurrentDictionary<Type, IMessageBusEventHandlerBase> messageBusEventHandlers = new();

        /// <summary>
        /// Gets, and if not yet present, also adds a <see cref="MessageBusEventHandler{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of message bus event</typeparam>
        /// <returns>The existin, or newly created <see cref="MessageBusEventHandler{T}"/></returns>
        public MessageBusEventHandler<T> GetHandler<T>()
        {
            var messageBusEventHandler = this.messageBusEventHandlers.GetOrAdd(typeof(T), _ => MessageBusEventHandler<T>.CreateHandler<T>());

                return messageBusEventHandler as MessageBusEventHandler<T>;
            }
        }

        /// <summary>
        /// Removes an existing <see cref="MessageBusEventHandler{T}"/> from this class
        /// </summary>
        /// <typeparam name="T">The type of message bus event</typeparam>
        public void RemoveHandlerIfExists<T>()
        {
            if (this.messageBusEventHandlers.TryRemove(typeof(T), out var messageBusEventHandler))
            {
                messageBusEventHandler.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing) //Free any other managed objects here
            {
                foreach (var disposable in this.messageBusEventHandlers.Values)
                {
                    disposable.Dispose();
                }

                this.messageBusEventHandlers.Clear();
            }

            // Indicate that the instance has been disposed.
            this.isDisposed = true;
        }
    }
}