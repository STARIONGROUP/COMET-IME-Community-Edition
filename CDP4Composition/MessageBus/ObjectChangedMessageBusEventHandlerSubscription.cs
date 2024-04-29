// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectChangedMessageBusEventHandlerSubscription.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;

    using CDP4Dal.Events;

    using System;

    /// <summary>
    /// Specific <see cref="MessageBusEventHandlerSubscription{T}"/> class that mandles message bus events for <see cref="ObjectChangedEvent"/> message bus events
    /// </summary>
    public class ObjectChangedMessageBusEventHandlerSubscription : MessageBusEventHandlerSubscription<ObjectChangedEvent>
    {
        /// <summary>
        /// The <see cref="Thing"/> where this <see cref="ObjectChangedMessageBusEventHandlerSubscription"/> was registered for
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> where this <see cref="ObjectChangedMessageBusEventHandlerSubscription"/> was registered for
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ObjectChangedMessageBusEventHandlerSubscription"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <param name="discriminator">The discriminator code</param>
        /// <param name="action">The to be executed code</param>
        public ObjectChangedMessageBusEventHandlerSubscription(Thing thing, Func<ObjectChangedEvent, bool> discriminator, Action<ObjectChangedEvent> action) 
            : base(discriminator, action)
        {
            Thing = thing;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ObjectChangedMessageBusEventHandlerSubscription"/> class
        /// </summary>
        /// <param name="type">The <see cref="Type"/></param>
        /// <param name="discriminator">The discriminator code</param>
        /// <param name="action">The to be executed code</param>
        public ObjectChangedMessageBusEventHandlerSubscription(Type type, Func<ObjectChangedEvent, bool> discriminator, Action<ObjectChangedEvent> action) 
            : base(discriminator, action)
        {
            Type = type;
        }
    }
}
