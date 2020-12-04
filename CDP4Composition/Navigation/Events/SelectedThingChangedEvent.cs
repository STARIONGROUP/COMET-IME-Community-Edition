// ------------------------------------------------------------------------------------------------
// <copyright file="SelectedThingChangedEvent.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Events
{
    using CDP4Common.CommonData;

    using CDP4Dal;

    /// <summary>
    /// The event carrying information for a selected thing that was changed.
    /// </summary>
    /// <typeparam name="T">The type of the payload for this event</typeparam>
    public class SelectedThingChangedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedThingChangedEvent"/> class
        /// </summary>
        /// <param name="selectedThing">
        /// The selected <see cref="Thing"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        public SelectedThingChangedEvent(Thing selectedThing, ISession session)
        {
            this.Session = session;
            this.SelectedThing = selectedThing;
        }

        /// <summary>
        /// Gets the selected <see cref="Thing"/>
        /// </summary>
        public Thing SelectedThing { get; private set; }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; }
    }
}
