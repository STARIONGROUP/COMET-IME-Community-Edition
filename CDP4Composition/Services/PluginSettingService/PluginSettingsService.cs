// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CDP4Composition.Exceptions;
    using Microsoft.Practices.Prism.Modularity;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;

    [Export(typeof(IPluginSettingsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PluginSettingsService : IPluginSettingsService
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Path to special windows "AppData" folder 
        /// </summary>
        public static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Application configuration folder path.
        /// </summary>
        public const string ConfigurationDirectoryFolder = @"RHEA\CDP4\";

        /// <summary>
        /// The setting file extension
        /// </summary>
        public const string SETTING_FILE_EXTENSION = ".settings.json";

        /// <summary>
        /// a dictionary used to cache the names of assemblies
        /// </summary>
        private readonly Dictionary<IModule, string> assemblyNamesCache;

        /// <summary>
        /// A dictionary used to store the user plugin-setting of the application
        /// </summary>
        private readonly Dictionary<IModule, PluginSettings> applicationUserPluginSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="PluginSettingsService"/>
        /// </summary>
        public PluginSettingsService()
        {
            this.assemblyNamesCache = new Dictionary<IModule, string>();
            this.applicationUserPluginSettings = new Dictionary<IModule, PluginSettings>();
        }
        
        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ApplicationConfigurationDirectory
        {
            get { return Path.Combine(AppDataFolder, ConfigurationDirectoryFolder); }
        }

        /// <summary>
        /// Reads the <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">A type of <see cref="PluginSettings"/></typeparam>
        /// <returns>
        /// An instance of <see cref="PluginSettings"/>
        /// </returns>
        public T Read<T>() where T : PluginSettings
        {
            return this.applicationUserPluginSettings.Values.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Reads the <see cref="PluginSettings"/> for the specified <see cref="IModule"/>
        /// </summary>
        /// <param name="module">
        /// The <see cref="IModule"/> for which the settings need to be read
        /// </param>
        /// <returns>
        /// An instance of <see cref="PluginSettings"/>
        /// </returns>
        public T Read<T>(IModule module) where T : PluginSettings
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "The module may not be null");
            }

            PluginSettings result;
            if (this.applicationUserPluginSettings.TryGetValue(module, out result))
            {
                return result as T;
            }

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }
            
            var assemblyName = this.QueryAssemblyName(module);

            var path = Path.Combine(this.ApplicationConfigurationDirectory, assemblyName);

            logger.Debug("Read pluggin settings for {0} from {1}", assemblyName, path);

            try
            {
                using (var file = File.OpenText($"{path}{SETTING_FILE_EXTENSION}"))
                {
                    var serializer = new JsonSerializer();
                    result = (T)serializer.Deserialize(file, typeof(T));

                    // once the settings have been read from disk, add them to the cache for fast access
                    this.applicationUserPluginSettings.Add(module, result);

                    return (T)result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The PluginSettings could not be read");

                throw new PluginSettingsException("The PluginSettings could not be read", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="PluginSettings"/> for the specified <see cref="IModule"/> to disks and adds to the cache
        /// </summary>
        /// <param name="pluginSettings">
        /// The <see cref="PluginSettings"/> that will be persisted
        /// </param>
        /// <param name="module">
        /// The <see cref="IModule"/> for which the <see cref="PluginSettings"/> are written.
        /// </param>
        public void Write<T>(T pluginSettings, IModule module) where T : PluginSettings
        {
            if (pluginSettings == null)
            {
                throw new ArgumentNullException(nameof(pluginSettings), "The pluginSettings may not be null");
            }

            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "The module may not be null");
            }
            
            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var assemblyName = this.QueryAssemblyName(module);
            
            var path = Path.Combine(this.ApplicationConfigurationDirectory, $"{assemblyName}{SETTING_FILE_EXTENSION}");

            logger.Debug("write settings to for {0} to {1}", assemblyName, path);
            
            using (var streamWriter = File.CreateText(path))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                };

                serializer.Serialize(streamWriter, pluginSettings);
            }

            // once the settings have been written to disk, add them to the cache for fast access, or update the cache if the item is already present
            if (this.applicationUserPluginSettings.ContainsKey(module))
            {
                this.applicationUserPluginSettings[module] = pluginSettings;
            }
            else
            {
                this.applicationUserPluginSettings.Add(module, pluginSettings);
            }
        }

        /// <summary>
        /// Queries the name of the assembly that contains the <see cref="IModule"/>
        /// </summary>
        /// <param name="module">
        /// The <see cref="IModule"/> that is contained in the assembly for which the name is queried.
        /// </param>
        /// <returns>
        /// A string that contains the name of the assembly
        /// </returns>
        private string QueryAssemblyName(IModule module)
        {
            string assemblyName;

            if (!this.assemblyNamesCache.TryGetValue(module, out assemblyName))
            {
                assemblyName =
                    ((AssemblyTitleAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)))
                    .Title;

                this.assemblyNamesCache.Add(module, assemblyName);
            }
            
            return assemblyName;
        }
    }
}