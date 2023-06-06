// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XtraReportHandler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.XtraReports.Parameters;
    using DevExpress.XtraReports.UI;

    using CDP4Reporting.Parameters;
    using CDP4Reporting.ReportScript;

    using Parameter = DevExpress.XtraReports.Parameters.Parameter;

    /// <summary>
    /// An implementation of <see cref="IXtraReportHandler{XtraReport,Parameter}"/> to be used for net6 reporting
    /// </summary>
    public class XtraReportHandler : IXtraReportHandler<XtraReport, Parameter>
    {
        /// <summary>
        /// Gets the <see cref="XtraReport"/>
        /// </summary>
        public XtraReport Report { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="XtraReportHandler"/> class
        /// </summary>
        /// <param name="report">The <see cref="XtraReport"/></param>
        public XtraReportHandler(XtraReport report)
        {
            this.Report = report;
        }

        /// <summary>
        /// Sets the <see cref="XtraReport"/>'s DataSource
        /// </summary>
        /// <param name="dataSourceName">The name of the DataSource</param>
        /// <param name="dataSource">The datasource as an <see cref="object"/></param>
        public void SetReportDataSource(string dataSourceName, object dataSource)
        {
            var reportDataSource =
                this.Report.ComponentStorage.OfType<ObjectDataSource>()
                    .FirstOrDefault(x => x.Name.Equals(dataSourceName));

            if (reportDataSource == null)
            {
                // Create new datasource
                reportDataSource = new ObjectDataSource
                {
                    DataSource = dataSource,
                    Name = dataSourceName
                };

                this.Report.ComponentStorage.Add(reportDataSource);
                this.Report.DataSource = reportDataSource;
            }
            else
            {
                reportDataSource.DataSource = dataSource;
                reportDataSource.RebuildResultSchema();
            }
        }

        /// <summary>
        /// Sets the <see cref="XtraReport"/>'s filterstring property
        /// </summary>
        /// <param name="filterString"></param>
        public void SetReportFilterString(string filterString)
        {
            this.Report.FilterString = filterString;
        }

        /// <summary>
        /// Gets the currently available report parameters
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="Parameter"/></returns>
        public IEnumerable<Parameter> GetCurrentParameters()
        {
            foreach (var reportParameter in this.Report.Parameters)
            {
                if (reportParameter.Name.StartsWith(ReportingParameter.NamePrefix))
                {
                    yield return reportParameter;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{string,object}"/> of previously set values of report parameters.
        /// </summary>
        /// <returns>The <see cref="Dictionary{string,object}"/> </returns>
        public Dictionary<string, object> GetPreviouslySetValues()
        {
            var result = new Dictionary<string, object>();

            foreach (var reportParameter in this.Report.Parameters)
            {
                if (reportParameter.Name.StartsWith(ReportingParameter.NamePrefix))
                {
                    result.Add(reportParameter.Name, reportParameter.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <see cref="string"/> that contains all unchanged report parameters
        /// </summary>
        /// <param name="reportingParameters"></param>
        /// <returns>the <see cref="IEnumerable{T}"/> of type <see cref="string"/></returns>
        public IEnumerable<string> GetUnChangedParameters(IEnumerable<IReportingParameter> reportingParameters)
        {
            return this.GetCurrentParameters().Select(x => x.Name)
                .Intersect<string>(reportingParameters.Select(x => x.ParameterName))
                .ToList();
        }

        /// <summary>
        /// Removes a report parameter from the report if it already exists.
        /// </summary>
        /// <param name="reportParameter">The <see cref="Parameter"/></param>
        public void RemoveParameterIfExists(Parameter reportParameter)
        {
            if (this.Report.Parameters.Contains(reportParameter))
            {
                this.Report.Parameters.Remove(reportParameter);
            }
        }

        /// <summary>
        /// Sets the default values all dynamic report parameter based on an <see cref="IReportingParameter"/>
        /// </summary>
        /// <param name="reportingParameter">The <see cref="IReportingParameter"/></param>
        public void SetParameterDefaultValues(IReportingParameter reportingParameter)
        {
            foreach (var parameter in this.Report.Parameters)
            {
                if (parameter.Name == reportingParameter.ParameterName && parameter.Visible == false)
                {
                    this.SetParameterDefaultValue(parameter, reportingParameter.DefaultValue);
                }
            }
        }

        /// <summary>
        /// Sets the default value for a specific report parameter <see cref="Parameter"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        /// <param name="value">The new value for the report parameter</param>
        public void SetParameterDefaultValue(Parameter parameter, object value)
        {
            parameter.Value = value;
        }

        /// <summary>
        /// Adds a new report parameter to the report
        /// </summary>
        /// <param name="newReportParameter">The <see cref="Parameter"/></param>
        public void AddParameter(Parameter newReportParameter)
        {
            this.Report.Parameters.Add(newReportParameter);
        }

        /// <summary>
        /// Gets a new <see cref="Parameter"/> based on an <see cref="IReportingParameter"/>
        /// </summary>
        /// <param name="reportingParameter">The <see cref="IReportingParameter"/></param>
        /// <param name="parameterIsVisible">Boolean indicating if the report parameter is visible and edittable for the user in the report</param>
        /// <returns>The <see cref="Parameter"/></returns>
        public Parameter GetNewParameter(IReportingParameter reportingParameter, bool parameterIsVisible)
        {
            return new Parameter
            {
                Name = reportingParameter.ParameterName,
                Description = reportingParameter.Name,
                Type = reportingParameter.Type,
                Visible = parameterIsVisible
            };
        }

        /// <summary>
        /// Sets all data for a lookup <see cref="Parameter"/> based on an <see cref="IReportingParameter"/>
        /// </summary>
        /// <param name="newReportParameter">The <see cref="Parameter"/></param>
        /// <param name="reportingParameter">The <see cref="IReportingParameter"/></param>
        public void SetParameterStaticLookUpList(Parameter newReportParameter, IReportingParameter reportingParameter)
        {
            var staticListLookupSettings = new StaticListLookUpSettings();
            newReportParameter.LookUpSettings = staticListLookupSettings;

            newReportParameter.MultiValue = reportingParameter.IsMultiValue;

            foreach (var keyValuePair in reportingParameter.LookUpValues)
            {
                staticListLookupSettings.LookUpValues.Add(new LookUpValue(keyValuePair.Key, keyValuePair.Value));
            }
        }

        /// <summary>
        /// Sets a <see cref="Parameter"/>'s visibility property
        /// </summary>
        /// <param name="newReportParameter">The <see cref="Parameter"/></param>
        /// <param name="reportingParameterVisible">A boolean indicating that the <see cref="Parameter"/> is visible and edittable to the user</param>
        public void SetParameterVisibility(Parameter newReportParameter, bool reportingParameterVisible)
        {
            newReportParameter.Visible = reportingParameterVisible;
        }
    }
}
