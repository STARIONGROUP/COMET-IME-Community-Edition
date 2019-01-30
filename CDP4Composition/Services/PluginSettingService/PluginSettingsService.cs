// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Reflection;
    using Microsoft.Practices.Prism.Modularity;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [Export(typeof(IPluginSettingsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PluginSettingsService : IPluginSettingsService
    {
        /// <summary>
        /// Path to special windows "AppData" folder 
        /// </summary>
        public static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Application configuration folder path.
        /// </summary>
        public static string ConfigurationDirectoryFolder = @"RHEA\CDP4\";

        /// <summary>
        /// a dictionary used to cache the names of assemblies
        /// </summary>
        private readonly Dictionary<IModule, string> assemblyNamesCache;

        /// <summary>
        /// Initializes a new instance of <see cref="PluginSettingsService"/>
        /// </summary>
        public PluginSettingsService()
        {
            this.assemblyNamesCache = new Dictionary<IModule, string>();
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

            var assemblyName = this.QueryAssemblyName(module);

            var path = Path.Combine(ApplicationConfigurationDirectory, assemblyName);

            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                var result = (T)serializer.Deserialize(file, typeof(T));
                return result;
            }
        }

        /// <summary>
        /// Writes the <see cref="PluginSettings"/> for the specified <see cref="IModule"/>
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
            
            var assemblyName = this.QueryAssemblyName(module);

            var path = Path.Combine(ApplicationConfigurationDirectory, assemblyName);

            using (var streamWriter = File.CreateText(path))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                };

                serializer.Serialize(streamWriter, pluginSettings);
            }
        }

        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ApplicationConfigurationDirectory
        {
            get { return Path.Combine(AppDataFolder, ConfigurationDirectoryFolder); }
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