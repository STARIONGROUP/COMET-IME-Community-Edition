// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElementBaseRowViewModel.cs" company="Starion Group S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Collections.Generic;

    using CDP4Composition.Mvvm;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The interface for rows that contains rows representing a <see cref="ParameterGroup" /> or <see cref="ParameterBase" />
    /// </summary>
    public interface IElementBaseRowViewModel : IHaveModelCode
    {
        /// <summary>
        /// Gets an <see cref="IEnumerable{Category}" /> containing all applied categories for this <see cref="ElementBase" />.
        /// </summary>
        IEnumerable<Category> Category { get; }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase" />
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase" /></param>
        void UpdateParameterBasePosition(ParameterBase parameterBase);

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterGroup" />
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup" /></param>
        void UpdateParameterGroupPosition(ParameterGroup parameterGroup);
    }
}
