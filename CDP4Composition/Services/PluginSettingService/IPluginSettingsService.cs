// -------------------------------------------------------------------------------------------------
// <copyright file="IPluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Definition of the <see cref="IPluginSettingsService"/> used to load plugin specific settings
    /// </summary>
    public interface IPluginSettingsService
    {
        /// <summary>
        /// Reads the <see cref="TPluginSettings"/>
        /// </summary>
        /// <typeparam name="TPluginSettings">A type of <see cref="PluginSettings"/></typeparam>
        /// <param name="shouldReload">An assert wheter to reload from setting file</param>
        /// <param name="converters">The Json data converters</param>
        /// <returns> An instance of <see cref="PluginSettings"/></returns>
        TPluginSettings Read<TPluginSettings>(bool shouldReload = false, params JsonConverter[] converters) where TPluginSettings : PluginSettings;

        /// <summary>
        /// Writes the <see cref="TPluginSettings"/> to disk
        /// </summary>
        /// <typeparam name="TPluginSettings">A type of <see cref="PluginSettings"/></typeparam>
        /// <param name="pluginSettings"> The <see cref="PluginSettings"/> that will be persisted </param>
        /// <param name="converters">The Json data converters</param>
        void Write<TPluginSettings>(TPluginSettings pluginSettings, params JsonConverter[] converters) where TPluginSettings : PluginSettings;
    }
}