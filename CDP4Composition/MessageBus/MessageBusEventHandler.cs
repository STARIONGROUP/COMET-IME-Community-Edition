// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusEventHandler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using ReactiveUI;

    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Registers and handles message bus events
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of message bus event</typeparam>
    public class MessageBusEventHandler<T> : IMessageBusEventHandler<T>
    {
         /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// A <see cref="List{T}"/> of type <see cref="IDisposable"/> that contains disposable items
        /// </summary>
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> of type <see cref="IObservable{T}"/> and <see cref="HashSet{IMessageBusEventHandlerData}}"/>
        /// that holds all <see cref="IMessageBusEventHandlerSubscription"/> classes per <see cref="IObservable{T}"/>.
        /// </summary>
        private Dictionary<IObservable<T>, HashSet<IMessageBusEventHandlerSubscription>> MessageBusHandlerDataList { get; } 
            = new Dictionary<IObservable<T>, HashSet<IMessageBusEventHandlerSubscription>>();

        /// <summary>
        /// Register a message bus event handler
        /// </summary>
        /// <param name="listener">The <see cref="IObservable{T}"/> to listen to for message bus messages</param>
        /// <param name="messageBusHandlerData">The <see cref="IMessageBusEventHandlerSubscription"/> instance that holds information about the event handler</param>
        /// <returns>A <see cref="MessageBusEventHandlerDisposer"/> as an <see cref="IDisposable"/></returns>
        public IDisposable RegisterEventHandler(IObservable<T> listener, IMessageBusEventHandlerSubscription messageBusHandlerData)
        {
            return Task.Run
                (() =>
                    { 
                        if (!this.MessageBusHandlerDataList.TryGetValue(listener, out var messageBusHandlerDataList))
                        {
                            messageBusHandlerDataList = new HashSet<IMessageBusEventHandlerSubscription>();
                            this.MessageBusHandlerDataList.Add(listener, messageBusHandlerDataList);

                            // At least one subscription, otherwise the CDPMessageBus could remove the listener unexpectedly
                            this.disposables.Add(
                                listener
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe(
                                    x => 
                                    this.HandleEvent(listener, x)));
                        }

                        messageBusHandlerDataList.Add(messageBusHandlerData);

                        Action cleanUpAction = () =>
                        {
                            if (messageBusHandlerDataList.Contains(messageBusHandlerData))
                            {
                                messageBusHandlerDataList.Remove(messageBusHandlerData);
                            }
                        };

                        return new MessageBusEventHandlerDisposer(cleanUpAction);
                    }
                ).Result;
        }

        /// <summary>
        /// Handles the registered events based on an <see cref="IObservable{T}"/>
        /// </summary>
        /// <param name="listener">The <see cref="IObservable{T}"</param>
        /// <param name="obj">The type of message bus event</param>
        private void HandleEvent(IObservable<T> listener, T obj)
        {
            var messageBusHandlerDataHashSet = this.MessageBusHandlerDataList[listener];
            IMessageBusEventHandlerSubscription[] messageBusHandlerDataArray = new IMessageBusEventHandlerSubscription[messageBusHandlerDataHashSet.Count];
            messageBusHandlerDataHashSet.CopyTo(messageBusHandlerDataArray);

            foreach (var messageBusHandlerData in messageBusHandlerDataArray)
            {
                if (messageBusHandlerData.ExecuteDiscriminator(obj))
                {
                    messageBusHandlerData.ExecuteAction(obj);
                }
            }
        }

        /// <summary>
        /// Unregisters a registered <see cref="IMessageBusEventHandlerSubscription"/>
        /// </summary>
        /// <param name="messageBusHandlerData">The <see cref="IMessageBusEventHandlerSubscription"/></param>
        public void UnregisterEventHandler(IMessageBusEventHandlerSubscription messageBusHandlerData)
        {
            foreach (var keyValuePair in this.MessageBusHandlerDataList)
            {
                if (keyValuePair.Value.Contains(messageBusHandlerData))
                {
                    keyValuePair.Value.Remove(messageBusHandlerData);
                }
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
                foreach (var disposable in this.disposables)
                {
                    disposable.Dispose();
                }
            }

            // Indicate that the instance has been disposed.
            this.isDisposed = true;
        }
    }
}
