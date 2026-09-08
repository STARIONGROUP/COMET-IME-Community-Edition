// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVGateState.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Services
{
    /// <summary>
    /// What a stage gate review can say about one requirement at one stage gate.
    /// </summary>
    public enum VandVGateState
    {
        /// <summary>
        /// Nobody has said what happens to this requirement here. Either no V&amp;V is planned for it at all, or V&amp;V
        /// is planned but no item declares that it closes the requirement out, so the plan has no end. This is the
        /// state a planning review exists to drive to zero.
        /// </summary>
        Undefined,

        /// <summary>
        /// The requirement cannot be verified at this gate: nothing is planned here, but V&amp;V is planned at a later
        /// gate. Derived, never entered by hand.
        /// </summary>
        Deferred,

        /// <summary>
        /// V&amp;V is planned at this gate but has not concluded yet.
        /// </summary>
        Planned,

        /// <summary>
        /// V&amp;V at this gate failed, or showed a shortfall that no accepted concession covers.
        /// </summary>
        Failed,

        /// <summary>
        /// V&amp;V at this gate concluded positively, but the requirement is not finished: further V&amp;V is owed at a
        /// later gate.
        /// </summary>
        Verified,

        /// <summary>
        /// V&amp;V at this gate concluded positively and closes the requirement out. Nothing further is owed.
        /// </summary>
        ClosedOut,

        /// <summary>
        /// Nothing is owed at this gate because the requirement was already closed out at an earlier one.
        /// </summary>
        Complete
    }
}
