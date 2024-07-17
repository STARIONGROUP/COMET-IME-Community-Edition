﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameterActualFiniteStateListApplicationBatchService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4EngineeringModel.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="IParameterActualFiniteStateListApplicationBatchService"/> is to apply a <see cref="ActualFiniteStateList"/> to
    /// a multiple <see cref="Parameters"/> as a batch operation. The <see cref="Parameter"/>s that are to be updated are selected based on provided
    /// <see cref="ParameterType"/>s, <see cref="Category"/>s and owning <see cref="DomainOfExpertise"/>
    /// </summary>
    public interface IParameterActualFiniteStateListApplicationBatchService
    {
        /// <summary>
        /// Updates multiple Parameters with <see cref="ActualFiniteStateList"/> application in one batch operation
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> in which the parameteres are to be updated
        /// </param>
        /// <param name="actualFiniteStateList">
        /// The <see cref="ActualFiniteStateList"/> that needs to be applied to the <see cref="Parameter"/>s
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// </param>
        /// <param name="domainOfExpertises"></param>
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// </param>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        Task Update(ISession session, Iteration iteration, ActualFiniteStateList actualFiniteStateList, bool isUncategorizedIncluded, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises, IEnumerable<ParameterType> parameterTypes);
    }
}
