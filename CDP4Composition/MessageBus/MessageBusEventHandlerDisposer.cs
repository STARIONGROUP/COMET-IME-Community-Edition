// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusEventHandlerDisposer.cs" company="RHEA System S.A.">
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
    using System;

    /// <summary>
    /// A wrapper class that can help to execute some code when it gets disposed
    /// </summary>
    public class MessageBusEventHandlerDisposer : IDisposable
    {
        /// <summary>
        /// The <see cref="Action"/> to be executed on disposal
        /// </summary>
        private readonly Action action;

        /// <summary>
        /// Creates a new instance of the <see cref="MessageBusEventHandlerDisposer"/> class
        /// </summary>
        /// <param name="action"></param>
        public MessageBusEventHandlerDisposer(Action action)
        {
            this.action = action;
        }

        /// <summary>
        /// Disposes this class
        /// </summary>
        public void Dispose()
        {
            this.action.Invoke();
        }
    }
}
