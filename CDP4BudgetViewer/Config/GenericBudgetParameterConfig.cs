// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetParameterConfig.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
// --------------------------------------------------------------------------------------------------------------------

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetParameterConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using CDP4Common.SiteDirectoryData;
    using Config;

    /// <summary>
    /// The cost budget parameter configuration class
    /// </summary>
    public class GenericBudgetParameterConfig : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBudgetParameterConfig"/> class
        /// </summary>
        /// <param name="generic">The <see cref="BudgetParameterMarginPair"/> for the cost</param>
        public GenericBudgetParameterConfig(BudgetParameterMarginPair generic)
        {
            this.GenericTuple = generic;
        }

        /// <summary>
        /// Gets the parameter and margin
        /// </summary>
        public BudgetParameterMarginPair GenericTuple { get; private set; }
    }
}
