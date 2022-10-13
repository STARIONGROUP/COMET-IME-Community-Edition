// ------------------------------------------------------------------------------------------------
// <copyright file="IOfficeApplicationWrapper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
