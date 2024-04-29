﻿// -------------------------------------------------------------------------------------------------
// <copyright file="AppSettingsService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    using System;
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Exceptions;
    using CDP4Composition.Settings;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using NLog;

    /// <summary>
    /// Class service <see cref="AppSettingsService{T}"/> used to read and write the application configuration file
    /// </summary>
    public abstract class AppSettingsService<T> : IAppSettingsService<T> where T : AppSettings, new()
    {
        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        private const string PluginDirectoryName = "plugins";

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
        public const string ConfigurationDirectoryFolder = @"STARION\CDP4-COMET\";

        /// <summary>
        /// The setting file extension
        /// </summary>
        public const string SettingFileExtension = ".settings.json";

        /// <summary>
        /// Initializes a new instance of <see cref="AppSettingsService{T}"/>
        /// </summary>
        protected AppSettingsService()
        {
            try
            {
                this.AppSettings = this.Read<T>();
            }
            catch (AppSettingsException appSettingsException)
            {
                var appSettings = new T();

                this.Save();

                logger.Error(appSettingsException);

                this.AppSettings = appSettings;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw;
            }
        }

        /// <summary>
        /// Holder of the application settings
        /// </summary>
        public T AppSettings { get; }

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
        private T Read<T>() where T : AppSettings
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? this.QueryAssemblyTitle(typeof(T));

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4-COMET settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4-COMET settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var path = Path.Combine(this.ApplicationConfigurationDirectory, assemblyName);

            logger.Debug("Read application settings for {0} from {1}", assemblyName, path);

            try
            {
                var fileSettings = $"{path}{SettingFileExtension}";

                if (!File.Exists(fileSettings))
                {
                    this.Save();
                }

                using (var file = File.OpenText(fileSettings))
                {
                    var serializer = new JsonSerializer();
                    var result = (T)serializer.Deserialize(file, typeof(T));

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The AppSettings could not be read");

                throw new AppSettingsException("The AppSettings could not be read", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="AppSettings"/> to disk
        /// </summary>
        public void Save()
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? this.QueryAssemblyTitle(typeof(T));

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4-COMET settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4-COMET settings folder {0} has been created", this.ApplicationConfigurationDirectory);
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

                if (this.AppSettings == null)
                {
                    serializer.Serialize(streamWriter, new ShellAppSettings());
                }
                else
                {
                    serializer.Serialize(streamWriter, this.AppSettings);
                }
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
            return ((AssemblyTitleAttribute)type.Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute))).Title;
        }
    }
}