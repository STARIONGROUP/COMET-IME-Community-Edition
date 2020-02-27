// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModuleSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    /// <summary>
    /// A setting class for the requirement module
    /// </summary>
    public class RequirementsModuleSettings : PluginSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsModuleSettings"/> class
        /// </summary>
        public RequirementsModuleSettings()
        {
            this.OrderSettings = new OrderSettings();
        }

        /// <summary>
        /// Gets or sets the order-settings
        /// </summary>
        public OrderSettings OrderSettings { get; set; }
    }
}
