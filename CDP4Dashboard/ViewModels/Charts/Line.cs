// -------------------------------------------------------------------------------------------------
// <copyright file="Line.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CDP4Dashboard.ViewModels.Charts
{
    using System.Collections.Generic;

    /// <summary>
    /// Class that helps define an individual line in a chart
    /// </summary>
    public class Line : IExportData
    {
        /// <summary>
        /// Sets or gets the line's revision number
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Sets or gets the line's value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Get a list of header strings
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="string"/>s</returns>
        public IEnumerable<string> GetExportHeaders()
        {
            var headers = new[]
            {
                nameof(this.RevisionNumber),
                nameof(this.Value)
            };

            return headers;
        }

        /// <summary>
        /// Get a list of rows containing the data
        /// </summary>
        /// <returns><see cref="IEnumerable{IEnumerable{T}}"/></returns>
        public IEnumerable<IEnumerable<object>> GetExportData()
        {
            yield return new[]
            {
                this.RevisionNumber.ToString(),
                this.Value
            };
        }
    }
}
