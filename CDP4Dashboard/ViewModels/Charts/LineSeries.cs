// -------------------------------------------------------------------------------------------------
// <copyright file="LineSeries.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Charts
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// A generic object that is used to show a collection of <see cref="Line"/> values in a chart
    /// </summary>
    public class LineSeries : IExportData
    {
        /// <summary>
        /// Sets or gets the name of the <see cref="Parameter"/> or <see cref="ParameterOverride"/>
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Sets or gets the name of the <see cref="Option"/>
        /// </summary>
        public string OptionName { get; set; }

        /// <summary>
        /// Sets or gets the name of the <see cref="ActualFiniteState"/>
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Gets the name of this line serie
        /// </summary>
        public string LineName => $"{this.ParameterName ?? ""} {this.OptionName ?? ""} {this.StateName ?? ""}".Trim();

        /// <summary>
        /// The collection of <see cref="Line"/> objects
        /// </summary>
        public IEnumerable<Line> Lines { get; set; }

        /// <summary>
        /// Get a list of header strings
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="string"/>s</returns>
        public IEnumerable<string> GetExportHeaders()
        {
            var headers = new List<string>
            {
                nameof(this.ParameterName),
                nameof(this.OptionName),
                nameof(this.StateName)
            };

            headers.AddRange(new Line().GetExportHeaders());

            return headers;
        }

        /// <summary>
        /// Get a list of rows containing the data
        /// </summary>
        /// <returns><see cref="IEnumerable{IEnumerable{T}}"/></returns>
        public IEnumerable<IEnumerable<object>> GetExportData()
        {
            foreach (var line in this.Lines)
            {
                foreach (var lineData in line.GetExportData())
                {
                    var data = new List<object>
                    {
                        this.ParameterName,
                        this.OptionName,
                        this.StateName
                    };

                    data.AddRange(lineData);

                    yield return data;
                }
            }
        }
    }
}
