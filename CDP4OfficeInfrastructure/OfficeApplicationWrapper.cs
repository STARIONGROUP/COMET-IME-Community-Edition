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
    }
}
