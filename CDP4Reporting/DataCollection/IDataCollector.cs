// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataCollector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataCollection
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// The interface used for collecting data.
    /// </summary>
    public interface IDataCollector
    {
        /// <summary>
        /// Gets the <see cref="Iteration"/>
        /// </summary>
        Iteration Iteration { get; }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/>
        /// </summary>
        DomainOfExpertise DomainOfExpertise { get; }

        /// <summary>
        /// All currently open <see cref="ReferenceDataLibrary"/>s in this <see cref="IDataCollector.Session"/>
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> OpenReferenceDataLibraries { get; }

        /// <summary>
        /// The current <see cref="SiteDirectory"/>s in this <see cref="IDataCollector.Session"/>
        /// </summary>
        public SiteDirectory SiteDirectory { get; }

        /// <summary>
        /// Initializes this DataCollector 
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="session"></param>
        void Initialize(Iteration iteration, ISession session);

        /// <summary>
        /// Creates a new data object instance. Could be anything depending on what the data is used for.
        /// </summary>
        /// <returns>
        /// An object instance.
        /// </returns>
        object CreateDataObject();
    }
}
