// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusContainerCases.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System.Collections.Generic;

    /// <summary>
    /// Helper class to support direct <see cref="CDPMessageBus"/> subscriptions and subscription using <see cref="MessageBusHandler"/>s
    /// </summary>
    public static class MessageBusContainerCases
    {
        /// <summary>
        /// Get the MessageBus cases
        /// </summary>
        /// <returns>The <see cref="IEnumerable{object}"/></returns>
        public static IEnumerable<object> GetCases()
        {
            yield return new object [] { null, "Direct Subscriptions" };
            yield return new object [] { new TestMessageBusHandlerContainerViewModel(), "MessageBus Handler" };
        }
    }
}
