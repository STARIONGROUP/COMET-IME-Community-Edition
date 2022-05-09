// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectChangedMessageBusEventHandler.cs" company="RHEA System S.A.">
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
    using CDP4Dal.Events;

    using System;
    using System.Linq;
    using System.Reactive.Linq;

    /// <summary>
    /// Registers and handles message bus events
    /// </summary>
    public class ObjectChangedMessageBusEventHandler : MessageBusEventHandler<ObjectChangedEvent>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectChangedMessageBusEventHandler"/> class
        /// </summary>
        public ObjectChangedMessageBusEventHandler() : base()
        {
        }

        /// <summary>
        /// Gets the object where the subscription is registered for
        /// </summary>
        /// <returns>The <see cref="object"/></returns>
        protected override object GetSubscriptionObject(IMessageBusEventHandlerSubscription messageBusEventHandlerSubscription)
        {
            if (messageBusEventHandlerSubscription is ObjectChangedMessageBusEventHandlerSubscription objectChangedMessageBusEventHandlerSubscription)
            {
                return objectChangedMessageBusEventHandlerSubscription.Thing as object ?? objectChangedMessageBusEventHandlerSubscription.Type;

            }

            return null;
        }

        /// <summary>
        /// Handles the registered events based on an <see cref="IObservable{T}"/>
        /// </summary>
        /// <param name="listener">The <see cref="IObservable{T}"</param>
        /// <param name="obj">The type of message bus event</param>
        protected override void HandleEvents(IObservable<ObjectChangedEvent> listener, ObjectChangedEvent objectChangedEvent)
        {
            var messageBusHandlerDataDictionary = this.MessageBusHandlerDataList[listener];

            if (messageBusHandlerDataDictionary.TryGetValue(objectChangedEvent.ChangedThing, out var messageBusEventHandlerThingSubscriptions))
            {
                ObjectChangedMessageBusEventHandlerSubscription[] messageBusEventHandlerThingSubscriptionArray 
                    = new ObjectChangedMessageBusEventHandlerSubscription[messageBusEventHandlerThingSubscriptions.Count];
                messageBusEventHandlerThingSubscriptions.CopyTo(messageBusEventHandlerThingSubscriptionArray);
                var list = messageBusEventHandlerThingSubscriptionArray.OfType<ObjectChangedMessageBusEventHandlerSubscription>();

                foreach (var objectChangedMessageBusEventHandlerSubscription in list)
                {
                    if (objectChangedMessageBusEventHandlerSubscription.ExecuteDiscriminator(objectChangedEvent))
                    {
                        objectChangedMessageBusEventHandlerSubscription.ExecuteAction(objectChangedEvent);
                    }
                }
            }

            var type = objectChangedEvent.ChangedThing.GetType();

            foreach (var keyValuePair in messageBusHandlerDataDictionary.Where(x => x.Key is Type))
            {
                if (keyValuePair.Key is Type subscriptionType && (subscriptionType == type || subscriptionType.IsAssignableFrom(type)))
                {
                    var messageBusEventHandlerTypeSubscriptions = keyValuePair.Value;
                    ObjectChangedMessageBusEventHandlerSubscription[] messageBusEventHandlerTypeSubscriptionArray = new ObjectChangedMessageBusEventHandlerSubscription[messageBusEventHandlerTypeSubscriptions.Count];
                    messageBusEventHandlerTypeSubscriptions.CopyTo(messageBusEventHandlerTypeSubscriptionArray);
                    var list = messageBusEventHandlerTypeSubscriptionArray.OfType<ObjectChangedMessageBusEventHandlerSubscription>();

                    foreach (var objectChangedMessageBusEventHandlerSubscription in list)
                    {
                        if (objectChangedMessageBusEventHandlerSubscription.ExecuteDiscriminator(objectChangedEvent))
                        {
                            objectChangedMessageBusEventHandlerSubscription.ExecuteAction(objectChangedEvent);
                        }
                    }
                }
            }
        }
    }
}
