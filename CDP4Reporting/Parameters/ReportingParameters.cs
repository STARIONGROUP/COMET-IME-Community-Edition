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
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A class that is used to build Report Parameters and optional a specific filter string at the report level. 
    /// </summary>
    public abstract class ReportingParameters : IReportingParameters
    {
        /// <summary>
        /// Creates a list of report reporting parameter that should dynamically be added to the 
        /// Report Designer's report parameter list.
        /// </summary>
        /// <param name="dataSource">The already calculated datasource.</param>
        /// <returns>An <see cref="IEnumerable{IReportingParameter}"/></returns>
        public abstract IEnumerable<IReportingParameter> CreateParameters(object dataSource);

        /// <summary>
        /// Build a filter string that contains a concatination of all IReportingParameter's in the
        /// parameters parameter, which are built in this class'
        /// If this method returns a non-empty string, then the Report's FilterExpression will be
        /// overwritten with this string.
        /// </summary>
        /// <param name="reportingParameters">The <see cref="IEnumerable{IReportingParameter}"/> </param>
        /// <returns>The filterstring to be set in the report definition.</returns>
        public string CreateFilterString(IEnumerable<IReportingParameter> reportingParameters)
        {
            var stringBuilder = new StringBuilder();

            foreach (var parameter in reportingParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.FilterExpression))
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(" Or ");
                    }

                    stringBuilder.Append("(");
                    stringBuilder.Append(parameter.FilterExpression);
                    stringBuilder.Append(")");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
