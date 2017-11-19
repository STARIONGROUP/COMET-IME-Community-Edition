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

        /// <summary>
        /// Gets or sets the Word Application
        /// </summary>
        NetOffice.WordApi.Application Word { get; set; }

        /// <summary>
        /// Gets or sets the Visio Application
        /// </summary>
        NetOffice.VisioApi.Application Visio { get; set; }

        /// <summary>
        /// Gets or sets the Project Application
        /// </summary>
        NetOffice.MSProjectApi.Application Project { get; set; }

        /// <summary>
        /// Gets or sets the Outlook Application
        /// </summary>
        NetOffice.OutlookApi.Application Outlook { get; set; }

        /// <summary>
        /// Gets or sets the PowerPoint Application
        /// </summary>
        NetOffice.PowerPointApi.Application PowerPoint { get; set; }
    }
}
