// -------------------------------------------------------------------------------------------------
// <copyright file="IPluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    /// <summary>
    /// Definition of the <see cref="IPluginSettingsService"/> used to load plugin specific settings
    /// </summary>
    public interface IPluginSettingsService
    {
        /// <summary>
        /// Reads the <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">A type of <see cref="PluginSettings"/></typeparam>
        /// <returns>
        /// An instance of <see cref="PluginSettings"/>
        /// </returns>
        T Read<T>() where T : PluginSettings;

        /// <summary>
        /// Writes the <see cref="PluginSettings"/> to disk
        /// </summary>
        /// <param name="pluginSettings">
        /// The <see cref="PluginSettings"/> that will be persisted
        /// </param>
        void Write<T>(T pluginSettings) where T : PluginSettings;
    }
}