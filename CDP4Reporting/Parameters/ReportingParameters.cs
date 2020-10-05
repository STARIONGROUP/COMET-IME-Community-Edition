// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingParameters.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Parameters
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// A class that is used to build Report Parameters and optional a specific filter string at the report level. 
    /// </summary>
    public abstract class ReportingParameters : IReportingParameters
    {
        /// <summary>
        /// Gets the <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise DomainOfExpertise { get; private set; }

        /// <summary>
        /// All currently open <see cref="ReferenceDataLibrary"/>s in this <see cref="IReportingParameters.Session"/>
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> OpenReferenceDataLibraries { get; private set; }

        /// <summary>
        /// The current <see cref="SiteDirectory"/>s in this <see cref="IReportingParameters.Session"/>
        /// </summary>
        public SiteDirectory SiteDirectory { get; private set; }

        /// <summary>
        /// Initializes this DataCollector 
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="session"></param>
        public void Initialize(Iteration iteration, ISession session)
        {
            this.Iteration = iteration;
            this.Session = session;
            this.DomainOfExpertise = session.QueryCurrentDomainOfExpertise();
            this.OpenReferenceDataLibraries = session.OpenReferenceDataLibraries;
            this.SiteDirectory = session.RetrieveSiteDirectory();
        }

        /// <summary>
        /// Creates a list of report reporting parameter that should dynamically be added to the 
        /// Report Designer's report parameter list.
        /// </summary>
        /// <param name="dataSource">The already calculated datasource.</param>
        /// <returns>An <see cref="IEnumerable{IReportingParameter}"/></returns>
        public abstract IEnumerable<IReportingParameter> CreateParameters(object dataSource);

        /// <summary>
        /// Build a filter string that contains a concatination of all IReportingParameter's in the
        /// parameters parameter, which are built in this class.
        /// </summary>
        /// <param name="reportingParameters">The <see cref="IEnumerable{IReportingParameter}"/> </param>
        /// <returns>The filterstring, built according to the DevExpress reporting guidelines for filtercriteria.</returns>
        public string CreateFilterString(IEnumerable<IReportingParameter> reportingParameters)
        {
            return string.Join(" Or ", reportingParameters
                .Where(x => !string.IsNullOrWhiteSpace(x.FilterExpression))
                .Select(x => $"({x.FilterExpression})"));
        }
    }
}
