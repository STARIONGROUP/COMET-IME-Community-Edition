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

        public const string DownloadDirectory = "RHEA/CDP4/DownloadCache/";

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
        /// Retrieves the Plugins directory of the current target plateform 
        /// </summary>
        /// <returns>a <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetPluginDirectory(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
            {
                return null;
            }

            var directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, PluginDirectoryName, pluginName));
            return directoryInfo;
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
        /// Compute and return the <see cref="DirectoryInfo"/> of the temporary folder of the specified plugin
        /// </summary>
        /// <param name="pluginName">the name of the plugin</param>
        /// <returns>a <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetTempDirectoryInfo(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
            {
                return null;
            }

            var appData = Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData);
            var temporaryFolder = Path.Combine(appData, DownloadDirectory, "Temp", pluginName);

            if (!Directory.Exists(temporaryFolder))
            {
                Directory.CreateDirectory(temporaryFolder);
            }

            return new DirectoryInfo(temporaryFolder);
        }

        /// <summary>
        /// Retrieve all plugin that can be installed
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of type <code>(FileInfo cdp4ckFile, Manifest manifest)</code> of the updatable plugins</returns>
        public static IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> GetDownloadedInstallablePluginUpdate()
        {
            var logger = LogManager.GetCurrentClassLogger();

            var appData = Environment.GetFolderPath(folder: Environment.SpecialFolder.ApplicationData);
            var downloadPath = Path.Combine(path1: appData, path2: DownloadDirectory, PluginDirectoryName);

            var updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>();

            if (!Directory.Exists(path: downloadPath))
            {
                logger.Info(message: "Download folder is empty or inexistant, download some plugins update from the IME first");
                return updatablePlugins;
            }
            
            var currentPlateformVersion = Assembly.GetExecutingAssembly().GetName().Version;

            // Loop through all existing download plugin folders
            foreach (var downloadedPluginFolder in Directory.EnumerateDirectories(path: downloadPath).Select(selector: d => new DirectoryInfo(path: d)))
            {
                if (downloadedPluginFolder.EnumerateFiles().FirstOrDefault(predicate: f => f.Name.EndsWith(value: ".cdp4ck")) is { } installableCdp4CkFullPath && installableCdp4CkFullPath.Directory is { } installableCdp4CkBasePath)
                {
                    var manifest = DeserializeManifestFromCdp4Ck(installableCdp4CkFullPath, downloadedPluginFolder);

                    if (manifest is { } && (manifest.MinIMEVersion is null || new Version(version: manifest.MinIMEVersion) <= currentPlateformVersion))
                    {
                        updatablePlugins.Add(item: (installableCdp4CkFullPath, manifest));
                    }
                    else
                    {
                        logger.Debug(message: manifest is { } 
                            ? $"{manifest.MinIMEVersion} is higher than the current IME version please update before installing this plugin update {currentPlateformVersion}"
                            : $"{downloadedPluginFolder.Name} does not contain any manifest. skipping plugin: {downloadedPluginFolder.Name}");
                    }
                }
                else
                {
                    logger.Error(message: $"{downloadedPluginFolder.Name} does not contain any package candidate. skipping plugin: {downloadedPluginFolder.Name}");
                }
            }

            logger.Debug(message: $"Found {updatablePlugins.Count} installable plugins");
            return updatablePlugins;
        }

        /// <summary>
        /// Retrive and deserialize the manifest from the provided cdp4ck
        /// </summary>
        /// <param name="cdp4CkPath">The <see cref="FileInfo"/> of the current plugin cdp4ck file</param>
        /// <param name="downloadedPluginFolder">the current folder containing the plugin update</param>
        /// <returns>the deserialized <see cref="Manifest"/></returns>
        private static Manifest DeserializeManifestFromCdp4Ck(FileInfo cdp4CkPath, DirectoryInfo downloadedPluginFolder)
        {
            using var openArchive = ZipFile.OpenRead(cdp4CkPath.FullName);
            var manifestEntry = openArchive.Entries.FirstOrDefault(z => z.Name.EndsWith(".plugin.manifest"));
            manifestEntry.ExtractToFile(Path.Combine(downloadedPluginFolder.FullName, manifestEntry.Name), true);
            Manifest manifest = null;

            if (downloadedPluginFolder.EnumerateFiles().FirstOrDefault(f => f.Name.EndsWith(".plugin.manifest")) is { } manifestFile)
            {
                manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(manifestFile.FullName)));
            }

            return manifest;
        }
    }
}
