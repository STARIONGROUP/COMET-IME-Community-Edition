﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Enumeration that specifies what combination of <see cref="Parameter"/>, <see cref="ParameterOverride"/> and <see cref="ParameterSubscription"/>
    /// needs to be taken into account.
    /// </summary>
    public enum ValueSetKind
    {
        /// <summary>
        /// Assertion that Paramters, Overrides and Subscriptions need to be taken into account
        /// </summary>
        All,

        /// <summary>
        /// Assertion that Parameters and Overrides need to be taken into account
        /// </summary>
        ParameterAndOrverride,

        /// <summary>
        /// Assertion that ParameterSubscriptions need to be taken into account
        /// </summary>
        ParameterSubscription
    }
}
