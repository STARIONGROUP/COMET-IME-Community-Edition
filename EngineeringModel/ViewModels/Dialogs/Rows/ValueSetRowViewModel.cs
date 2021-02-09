// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSetRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// A row view model for representation of a <see cref="IValueSet"/> in a combobox.
    /// </summary>
    public class ValueSetRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSetRowViewModel" /> class.
        /// </summary>
        /// <param name="valueSet">The value set</param>
        public ValueSetRowViewModel(IValueSet valueSet)
        {
            this.ValueSet = valueSet;
            this.DisplayName = $"Option: [{valueSet?.ActualOption?.ShortName ?? "-"}]; ActualFiniteState: [{valueSet?.ActualState?.ShortName ?? "-"}]";
        }

        /// <summary>
        /// Gets the value set represented by this row viewmodel
        /// </summary>
        public IValueSet ValueSet { get; private set; }

        /// <summary>
        /// Gets the value set represented by this row viewmodel
        /// </summary>
        public string DisplayName { get; private set; }
    }
}
