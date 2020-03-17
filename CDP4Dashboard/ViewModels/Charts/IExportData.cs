// -------------------------------------------------------------------------------------------------
// <copyright file="IExportData.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CDP4Dashboard.ViewModels.Charts
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface that helps to define data to export to Excel
    /// </summary>
    public interface IExportData
    {
        /// <summary>
        /// Get a list of header strings
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> of <see cref="string"/>s</returns>
        IEnumerable<string> GetExportHeaders();

        /// <summary>
        /// Get a list of rows containing the data
        /// </summary>
        /// <returns><see cref="IEnumerable{IEnumerable{T}}"/></returns>
        IEnumerable<IEnumerable<object>> GetExportData();
    }
}
