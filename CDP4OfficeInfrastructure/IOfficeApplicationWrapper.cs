// ------------------------------------------------------------------------------------------------
// <copyright file="IOfficeApplicationWrapper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    /// <summary>
    /// Interface that allows to set the different Office Applications
    /// </summary>
    public interface IOfficeApplicationWrapper
    {
        /// <summary>
        /// Gets or sets the Excel Application
        /// </summary>
        NetOffice.ExcelApi.Application Excel { get; set; }
    }
}
