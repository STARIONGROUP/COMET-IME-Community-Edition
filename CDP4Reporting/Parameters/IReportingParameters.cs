// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IReportingParameters.cs" company="RHEA System S.A.">
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

    using CDP4Reporting.DataCollection;

    /// <summary>
    /// Interface to be used in the Code editor of <see cref="Views.ReportDesigner"/>.
    /// </summary>
    public interface IReportingParameters
    {
        /// <summary>
        /// Creates a list of report reporting parameter that should dynamically be added to the 
        /// Report Designer's report parameter list.
        /// </summary>
        /// <param name="dataSource">
        /// The already calculated datasource.
        /// </param>
        /// <param name="dataCollector">
        /// The <see cref="IDataCollector"/> used for creating the dataSource.
        /// </param>
        /// <returns>An <see cref="IEnumerable{IReportingParameter}"/></returns>
        IEnumerable<IReportingParameter> CreateParameters(object dataSource, IDataCollector dataCollector);

        /// <summary>
        /// Creates a filterString to be user as a report filter expression.
        /// </summary>
        /// <param name="reportingParameters">
        /// The <see cref="IEnumerable{IReportingParameter}"/>.
        /// </param>
        /// <returns>
        /// The filter expression.
        /// </returns>
        string CreateFilterString(IEnumerable<IReportingParameter> reportingParameters);
    }
}
