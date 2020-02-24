// -------------------------------------------------------------------------------------------------
// <copyright file="AppSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Reflection;
    using CDP4Composition.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;

    [Export(typeof(IAppSettingsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppSettingsService : IAppSettingsService
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
        public const string SettingFileExtension = ".settings.json";

        /// <summary>
        /// A dictionary used to store the user plugin-setting of the application
        /// </summary>
        private readonly Dictionary<string, AppSettings> applicationUserAppSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="AppSettingsService"/>
        /// </summary>
        public AppSettingsService()
        {
            //this.assemblyNamesCache = new Dictionary<IModule, string>();
            this.applicationUserAppSettings = new Dictionary<string, AppSettings>();
        }

        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ApplicationConfigurationDirectory
        {
            get { return Path.Combine(AppDataFolder, ConfigurationDirectoryFolder); }
        }

        /// <summary>
        /// Reads the <see cref="T"/> plug in settings
        /// </summary>
        /// <typeparam name="T">A type of <see cref="AppSettings"/></typeparam>
        /// <returns>
        /// An instance of <see cref="AppSettings"/>
        /// </returns>
        public T Read<T>() where T : AppSettings
        {
            var assemblyName = this.QueryAssemblyTitle(typeof(T));

            if (this.applicationUserAppSettings.TryGetValue(assemblyName, out var result))
            {
                return result as T;
            }

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var path = Path.Combine(this.ApplicationConfigurationDirectory, assemblyName);

            logger.Debug("Read pluggin settings for {0} from {1}", assemblyName, path);

            try
            {
                using (var file = File.OpenText($"{path}{SettingFileExtension}"))
                {
                    var serializer = new JsonSerializer();
                    result = (T)serializer.Deserialize(file, typeof(T));

                    // once the settings have been read from disk, add them to the cache for fast access
                    this.applicationUserAppSettings.Add(assemblyName, result);

                    return (T)result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The AppSettings could not be read");

                throw new SettingsException("The AppSettings could not be read", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="AppSettings"/> to disk
        /// </summary>
        /// <param name="appSettings">
        /// The <see cref="appSettings"/> that will be persisted
        /// </param>
        public void Write<T>(T appSettings) where T : AppSettings
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings), "The AppSettings may not be null");
            }

            var assemblyName = this.QueryAssemblyTitle(appSettings.GetType());

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var path = Path.Combine(this.ApplicationConfigurationDirectory, $"{assemblyName}{SettingFileExtension}");

            logger.Debug("write settings to for {0} to {1}", assemblyName, path);

            using (var streamWriter = File.CreateText(path))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                };

                serializer.Serialize(streamWriter, appSettings);
            }

            if (this.applicationUserAppSettings.ContainsKey(assemblyName))
            {
                this.applicationUserAppSettings[assemblyName] = appSettings;
            }
            else
            {
                this.applicationUserAppSettings.Add(assemblyName, appSettings);
            }
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