// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToggleTooltipEvent.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Events
{
    /// <summary>
    /// The purpose of the <see cref="ToggleTooltipEvent"/> is to notify an observer
    /// whether tooltip shall be displayed
    /// </summary>
    public class ToggleTooltipEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleTooltipEvent"/> class
        /// </summary>
        /// <param name="shouldShow">a value indicating whether the tooltip should be displayed</param>
        public ToggleTooltipEvent(bool shouldShow)
        {
            this.ShouldShow = shouldShow;
        }

        /// <summary>
        /// Gets a value indicating whether the tooltip should be displayed
        /// </summary>
        public bool ShouldShow { get; private set; }
    }
}
