// ------------------------------------------------------------------------------------------------
// <copyright file="officeApplicationWrapper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    using System.ComponentModel.Composition;
    
    /// <summary>
    /// The purpose of the <see cref="OfficeApplicationWrapper"/> is to allow the an office addin to
    /// set a reference to the Excel application
    /// </summary>
    [Export(typeof(IOfficeApplicationWrapper))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OfficeApplicationWrapper : IOfficeApplicationWrapper
    {
        /// <summary>
        /// Gets or sets the Excel Application
        /// </summary>
        public NetOffice.ExcelApi.Application Excel { get; set; }

        /// <summary>
        /// Gets or sets the Word Application
        /// </summary>
        public NetOffice.WordApi.Application Word { get; set; }

        /// <summary>
        /// Gets or sets the Visio Application
        /// </summary>
        public NetOffice.VisioApi.Application Visio { get; set; }

        /// <summary>
        /// Gets or sets the Project Application
        /// </summary>
        public NetOffice.MSProjectApi.Application Project { get; set; }

        /// <summary>
        /// Gets or sets the Outlook Application
        /// </summary>
        public NetOffice.OutlookApi.Application Outlook { get; set; }

        /// <summary>
        /// Gets or sets the PowerPoint Application
        /// </summary>
        public NetOffice.PowerPointApi.Application PowerPoint { get; set; }
    }
}
