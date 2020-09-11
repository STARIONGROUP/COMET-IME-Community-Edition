// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4ReportingDataSource.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DataSource
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    public abstract class ReportingDataSource : IReportingDataSource
    {
        /// <summary>
        /// Gets or sets the <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise DomainOfExpertise { get; private set; }

        /// <summary>
        /// The list of available <see cref="Option"/>s
        /// </summary>
        public IEnumerable<Option> Options => this.Iteration?.Option as IEnumerable<Option> ?? new List<Option>();

        internal void Initialize(Iteration iteration, ISession session)
        {
            this.Iteration = iteration;
            this.Session = session;
            this.DomainOfExpertise = session.QueryCurrentDomainOfExpertise();
        }

        /// <summary>
        /// Creates a new data source instance.
        /// </summary>
        /// <returns>
        /// An object instance.
        /// </returns>
        public abstract object CreateDataSource();
    }
}
