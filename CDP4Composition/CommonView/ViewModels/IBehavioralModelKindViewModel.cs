// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBehavioralModelKindViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.CommonView.ViewModels
{
    using System;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    /// <summary>
    /// An interface that represents view models for <see cref="BehavioralModelKind"/> values
    /// </summary>
    public interface IBehavioralModelKindViewModel
    {
        /// <summary>
        /// Returns the if the OK command can be executed for this view model
        /// </summary>
        /// <returns>The ok status</returns>
        bool OkCanExecute();

        /// <summary>
        /// Update the transaction with the <see cref="BehavioralModelKind"/> information represented by this view model
        /// </summary>
        /// <param name="transaction">The transaction for the <see cref="Thing"/></param>
        /// <param name="clone">The <see cref="Behavior"/> for which to update the <see cref="IThingTransaction"/></param>
        void UpdateTransaction(IThingTransaction transaction, Behavior clone);        
    }
}
