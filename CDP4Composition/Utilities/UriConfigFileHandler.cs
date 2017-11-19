// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriConfigFileHandler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;    
    using Newtonsoft.Json;

    // <summary>
    // The purpose of the class is to handle the read and write of uri configurations
    // </summary>
    public class UriConfigFileHandler
    {
        /// <summary>
        /// Application configuration folder
        /// </summary>
        public static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Application configuration folder path.
        /// </summary>
        public static string ConfigFileRelativeFolder = @"RHEA\cdp4\";

        /// <summary>
        /// Application configuration file name.
        /// </summary>
        public static string ConfigFileName = @"uriConfig.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="UriConfigFileHandler"/> class.
        /// </summary>
        public UriConfigFileHandler()
        {
        }

        /// <summary>
        /// Configuration file path
        /// </summary>
        public string ConfigurationFilePath
        {
            get { return Path.Combine(AppDataFolder, ConfigFileRelativeFolder, ConfigFileName); }
        }

        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ConfigurationFileDir
        {
            get { return Path.Combine(AppDataFolder, ConfigFileRelativeFolder); }
        }

        /// <summary>
        /// Read the uris to the JSON configuration file
        /// </summary>
        /// <returns>
        /// an <see cref="IEnumerable{UriConfig}"/>
        /// </returns>
        public IEnumerable<UriConfig> Read()
        {
            List<UriConfig> configFileClientsList;

            if (File.Exists(this.ConfigurationFilePath))
            {
                // Read from config app data folder
                var configFileContent = File.ReadAllText(this.ConfigurationFilePath);
                configFileClientsList = JsonConvert.DeserializeObject<List<UriConfig>>(configFileContent);
            }
            else
            {
                // Read from embedded resource
                configFileClientsList = new List<UriConfig>();
            }
            
            return configFileClientsList;
        }

        /// <summary>
        /// Write the uris to the JSON configuration file
        /// </summary>
        /// <param name="uris">
        /// The <see cref="IEnumerable{UriConfig}"/>s that needs to be saved
        /// </param>
        public void Write(IEnumerable<UriConfig> uris)
        {
            if (uris == null)
            {
                throw new ArgumentNullException("uris", "The clients must not be null");
            }

            string json = JsonConvert.SerializeObject(uris, Formatting.Indented);

            if (!Directory.Exists(this.ConfigurationFileDir))
            {
                Directory.CreateDirectory(this.ConfigurationFileDir);
            }

            File.WriteAllText(this.ConfigurationFilePath, json);
        }
    }
}
