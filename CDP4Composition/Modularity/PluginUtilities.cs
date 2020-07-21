// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
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

namespace CDP4Composition.Modularity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.Utilities;

    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    /// Utility class that help with plugin management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PluginUtilities
    {
        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        public const string PluginDirectoryName = "plugins";

        public const string DownloadDirectory = "RHEA/CDP4/DownloadCache/Plugins/";

        /// <summary>
        /// Gets the plugin <see cref="Manifest"/> present in the IME plugin folder
        /// </summary>
        /// <returns><see cref="IEnumerable{Manifest}"/> containing all founds plugin manifests</returns>
        public static IEnumerable<Manifest> GetPluginManifests()
        {
            var manifests = new List<Manifest>();

#if DEBUG
            var currentConfiguration = "Debug";
#else
            var currentConfiguration = "Release";
#endif

            var directoryInfo = PluginDirectoryExists(out var specificPluginFolderExists);

            if (directoryInfo.Exists)
            {
                foreach (var manifest in directoryInfo.EnumerateFiles("*.plugin.manifest", SearchOption.AllDirectories))
                {
                    if (!specificPluginFolderExists && (!manifest.Directory?.FullName.Contains($"{Path.DirectorySeparatorChar}{currentConfiguration}{Path.DirectorySeparatorChar}") ?? true))
                    {
                        continue;
                    }

                    manifests.Add(JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifest.FullName)));
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

            return manifests;
        }

        /// <summary>
        /// Returns a <see cref="DirectoryInfo"/> object tha contains the root folder where to search for .manifest files
        /// </summary>
        /// <param name="specificPluginFolderExists">States if a specific plugin directory exists</param>
        /// <returns>The <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo PluginDirectoryExists(out bool specificPluginFolderExists)
        {
            var currentPath = ServiceLocator.Current.GetInstance<IAssemblyLocationLoader>().GetLocation();

            var directoryInfo = new DirectoryInfo(Path.Combine(currentPath, PluginDirectoryName));
            specificPluginFolderExists = directoryInfo.Exists;

            if (!specificPluginFolderExists)
            {
                directoryInfo = new DirectoryInfo(currentPath);
            }

            return directoryInfo;
        }

        /// <summary>
        /// Retrieve all plugin that can be installed
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of type <code>(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)</code>of the updatable plugins</returns>
        public static IEnumerable<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)> GetDownloadedInstallablePluginUpdate()
        {
            var logger = LogManager.GetCurrentClassLogger();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var downloadPath = Path.Combine(appData, DownloadDirectory);

            if (!Directory.Exists(downloadPath))
            {
                logger.Info("Download folder is empty or inexistant, download some plugins update from the IME first");
                return default;
            }

            var updatablePlugins = new List<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)>();

            var currentPlateformVersion = Assembly.GetExecutingAssembly().GetName().Version;

            // Loop through all existing download plugin folders
            foreach (var downloadedPluginFolder in Directory.EnumerateDirectories(downloadPath).Select(d => new DirectoryInfo(d)))
            {
                if (downloadedPluginFolder.EnumerateFiles().FirstOrDefault(f => f.Name.EndsWith(".cdp4ck")) is { } installableCdp4CkFullPath && installableCdp4CkFullPath.Directory is { } installableCdp4CkBasePath)
                {
                    ZipFile.ExtractToDirectory(installableCdp4CkFullPath.FullName, installableCdp4CkBasePath.FullName);

                    if (installableCdp4CkBasePath.EnumerateFiles().FirstOrDefault(f => f.Name.EndsWith(".plugin.manifest")) is { } manifestFile)
                    {
                        var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(manifestFile.FullName)));

                        if (manifest.MinIMEVersion is null || new Version(manifest.MinIMEVersion) <= currentPlateformVersion)
                        {
                            updatablePlugins.Add((installableCdp4CkFullPath, manifest));
                        }
                        else
                        {
                            logger.Debug($"{manifest.MinIMEVersion} is higher than the current IME version please update before installing this plugin update {currentPlateformVersion}");
                        }
                    }
                    else
                    {
                        logger.Error($"{downloadedPluginFolder.Name} does not contain any manifest. skipping plugin: {downloadedPluginFolder.Name}");
                    }
                }
                else
                {
                    logger.Error($"{downloadedPluginFolder.Name} does not contain any package candidate. skipping plugin: {downloadedPluginFolder.Name}");
                }
            }

            logger.Debug($"Found {updatablePlugins.Count} installable plugins");
            return updatablePlugins;
        }
    }
}
