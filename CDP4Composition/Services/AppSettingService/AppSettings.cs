// -------------------------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class from which all <see cref="AppSettings"/> shall derive
    /// </summary>
    public abstract class AppSettings
    {
        /// <summary>
        /// Updating plugin settings
        /// </summary>
        /// <param name="pluginSettings">
        ///The plugin settings to be updated
        /// </param>
        public void UpdatePlugin(PluginSettingsMetaData pluginSettings)
        {
            if (pluginSettings == null)
            {
                return;
            }

            var plugin = this.Plugins.FirstOrDefault(x => x.Key == pluginSettings.Key);

            if (plugin != null)
            {
                plugin.IsEnabled = pluginSettings.IsEnabled;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Plugins"/>
        /// </summary>
        [JsonProperty]
        public List<PluginSettingsMetaData> Plugins { get; set; } = new List<PluginSettingsMetaData>();
    }
}