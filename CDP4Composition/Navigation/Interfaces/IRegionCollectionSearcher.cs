// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRegionCollectionSearcher.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Mihail Militaru.
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

namespace CDP4Composition
{
    using System.Collections.Generic;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// Interface for searching views in a <see cref="IRegionCollection"/>
    /// </summary>
    public interface IRegionCollectionSearcher
    {
        /// <summary>
        /// Search a <see cref="IRegionCollection"/> for <see cref="IRegion"/>s that contain a specific <see cref="IPanelView"/>
        /// </summary>
        /// <param name="regionCollection">The <see cref="IRegionCollection"/> to search</param>
        /// <param name="view">The <see cref="IPanelView"/> to search for</param>
        /// <returns>List of <see cref="IRegion"/>s</returns>
        IEnumerable<IRegion> GetRegionsByView(IRegionCollection regionCollection, IPanelView view);
    }
}