// -------------------------------------------------------------------------------------------------
// <copyright file="ImeAppSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Settings
{
    using System.Collections.Generic;

    using CDP4Composition.Services.AppSettingService;
  
    /// <summary>
    /// Class used to hold the settings from application
    /// </summary>
    public class ImeAppSettings : AppSettings
    {
        /// <summary>
        /// Gets or sets the disabled Plugins
        /// </summary>
        public List<string> UpdateServerAddresses { get; set; } = new List<string>();
    }
}
