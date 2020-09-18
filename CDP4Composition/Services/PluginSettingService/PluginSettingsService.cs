// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    
    using CDP4Composition.Exceptions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    
    using NLog;

    /// <summary>
    /// Implementation of a <see cref="IPluginSettingsService"/>
    /// </summary>
    [Export(typeof(IPluginSettingsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PluginSettingsService : IPluginSettingsService
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Path to special windows "AppData" folder 
        /// </summary>
        public readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Application configuration folder path.
        /// </summary>
        public readonly string Cdp4ConfigurationDirectoryFolder = $"RHEA{Path.DirectorySeparatorChar}CDP4{Path.DirectorySeparatorChar}";

        /// <summary>
        /// The setting file extension
        /// </summary>
        public const string SettingFileExtension = ".settings.json";

        /// <summary>
        /// A dictionary used to store the user plugin-setting of the application
        /// </summary>
        private readonly Dictionary<string, PluginSettings> applicationUserPluginSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="PluginSettingsService"/>
        /// </summary>
        public PluginSettingsService()
        {
            this.applicationUserPluginSettings = new Dictionary<string, PluginSettings>();
        }
        
        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ApplicationConfigurationDirectory => Path.Combine(this.AppDataFolder, this.Cdp4ConfigurationDirectoryFolder);

        /// <summary>
        /// Reads the <see cref="TPluginSettings"/> in settings
        /// </summary>
        /// <typeparam name="TPluginSettings">A type of <see cref="PluginSettings"/></typeparam>
        /// <returns> An instance of <see cref="TPluginSettings"/> </returns>
        public TPluginSettings Read<TPluginSettings>() where TPluginSettings : PluginSettings
        {
            var assemblyName = this.QueryAssemblyTitle(typeof(TPluginSettings));

            if (this.applicationUserPluginSettings.TryGetValue(assemblyName, out var result))
            {
                return (TPluginSettings)result;
            }

            this.CheckApplicationConfigurationDirectory();

            var path = Path.Combine(this.ApplicationConfigurationDirectory, assemblyName);

            this.logger.Debug("Read pluggin settings for {0} from {1}", assemblyName, path);

            try
            {
                using (var file = File.OpenText($"{path}{SettingFileExtension}"))
                {
                    var serializer = new JsonSerializer();
                    result = (TPluginSettings) serializer.Deserialize(file, typeof(TPluginSettings));

                    // once the settings have been read from disk, add them to the cache for fast access
                    this.applicationUserPluginSettings.Add(assemblyName, result);

                    return (TPluginSettings) result;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "The PluginSettings could not be read");

                throw new PluginSettingsException("The PluginSettings could not be read", ex);
            }
        }

        /// <summary>
        /// Checks for the existence of the <see cref="PluginSettingsService.ApplicationConfigurationDirectory"/>
        /// </summary>
        public void CheckApplicationConfigurationDirectory()
        {
            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                this.logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                this.logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }
        }

        /// <summary>
        /// Writes the <see cref="pluginSettings"/> to disk
        /// </summary>
        /// <typeparam name="TPluginSettings">A type of <see cref="PluginSettings"/></typeparam>
        /// <param name="pluginSettings"> The <see cref="PluginSettings"/> that will be persisted </param>
        /// <param name="converters"></param>
        public void Write<TPluginSettings>(TPluginSettings pluginSettings, params JsonConverter[] converters) where TPluginSettings : PluginSettings
        {
            if (pluginSettings == null)
            {
                throw new ArgumentNullException(nameof(pluginSettings), "The pluginSettings may not be null");
            }

            var assemblyName = this.QueryAssemblyTitle(pluginSettings.GetType());

            this.CheckApplicationConfigurationDirectory();

            var path = Path.Combine(this.ApplicationConfigurationDirectory, $"{assemblyName}{SettingFileExtension}");

            this.logger.Debug("write settings to for {0} to {1}", assemblyName, path);
            //Json DataTypeConverter custom converter
            using (var streamWriter = File.CreateText(path))
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = converters.ToList(),
                    Formatting = Formatting.Indented
                };

                var serializer = JsonSerializer.Create(serializerSettings);

                serializer.Serialize(streamWriter, pluginSettings);
            }
            
            this.applicationUserPluginSettings[assemblyName] = pluginSettings;
        }

        /// <summary>
        /// Queries the name of the assembly that contains the <see cref="Type"/>
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> that is contained in the assembly for which the name is queried.
        /// </param>
        /// <returns>
        /// A string that contains the name of the assembly
        /// </returns>
        private string QueryAssemblyTitle(Type type)
        {
            return ((AssemblyTitleAttribute)type.Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)))
                .Title;
        }
    }
}