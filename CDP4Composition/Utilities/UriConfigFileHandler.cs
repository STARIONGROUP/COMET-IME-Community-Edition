// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriConfigFileHandler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
        public static string ConfigFileRelativeFolder = $"RHEA{Path.DirectorySeparatorChar}cdp4{Path.DirectorySeparatorChar}";

        /// <summary>
        /// Application configuration file name.
        /// </summary>
        public static string ConfigFileName = @"uriConfig.json";

        /// <summary>
        /// Configuration file path
        /// </summary>
        public string ConfigurationFilePath => Path.Combine(AppDataFolder, ConfigFileRelativeFolder, ConfigFileName);

        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ConfigurationFileDir => Path.Combine(AppDataFolder, ConfigFileRelativeFolder);

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
                throw new ArgumentNullException(nameof(uris), "The clients must not be null");
            }

            var json = JsonConvert.SerializeObject(uris, Formatting.Indented);

            if (!Directory.Exists(this.ConfigurationFileDir))
            {
                Directory.CreateDirectory(this.ConfigurationFileDir);
            }

            File.WriteAllText(this.ConfigurationFilePath, json);
        }
    }
}
