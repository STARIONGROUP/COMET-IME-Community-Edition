// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
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

    using CommonServiceLocator;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    /// Utility class that help with plugin management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PluginUtilities
    {
        /// <summary>
        /// The NLog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(typeof(PluginUtilities));

        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        public const string PluginDirectoryName = "plugins";

        /// <summary>
        /// The name of the Ime msi directory
        /// </summary>
        public const string ImeDirectoryName = "ime";

        /// <summary>
        /// The directory of the downloaded plugin
        /// </summary>
        public const string DownloadDirectory = "DownloadCache";

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
            Logger.Info($"Reading plugin manifests for {currentConfiguration} configuration");
            var directoryInfo = PluginDirectoryExists(out var specificPluginFolderExists);

            if (directoryInfo.Exists)
            {
                foreach (var manifest in directoryInfo.EnumerateFiles("*.plugin.manifest", SearchOption.AllDirectories))
                {
                    if (!specificPluginFolderExists && (!manifest.Directory?.FullName.Contains($"{Path.DirectorySeparatorChar}{currentConfiguration}{Path.DirectorySeparatorChar}") ?? true))
                    {
                        Logger.Info($"Plugin manifest {manifest.Name} skipped");
                        continue;
                    }

                    manifests.Add(JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifest.FullName)));
                    Logger.Info($"Plugin manifest {manifest.Name} added");
                }
            }
            else
            {
                Logger.Error($"Plugin directory does not exist");
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
            var currentPath = ServiceLocator.Current.GetInstance<IAssemblyInformationService>().GetLocation();

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
        /// <returns>A <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetTempDirectoryInfo(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
            {
                return null;
            }

            var temporaryFolder = Path.Combine(GetAppDataPath(), DownloadDirectory, "Temp");

            if (!Directory.Exists(temporaryFolder))
            {
                Directory.CreateDirectory(temporaryFolder);
            }

            temporaryFolder = Path.Combine(temporaryFolder, pluginName);
            Logger.Info($"Plugin temp folder found {temporaryFolder}");

            return new DirectoryInfo(temporaryFolder);
        }

        /// <summary>
        /// Computes and returns the <see cref="DirectoryInfo"/> of the download folder
        /// </summary>
        /// <param name="isItForPlugins">An assert whether it should return the plugin download directory or the IME msi one</param>
        /// <param name="pluginName">The plugin name</param>
        /// <returns>A <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetDownloadDirectory(bool isItForPlugins = true, string pluginName = null)
        {
            var path3 = isItForPlugins ? PluginDirectoryName : ImeDirectoryName;
            var path4 = isItForPlugins && !string.IsNullOrWhiteSpace(pluginName) ? pluginName : string.Empty;

            var downloadPath = new DirectoryInfo(Path.Combine(path1: GetAppDataPath(), path2: DownloadDirectory, path3, path4));

            if (!downloadPath.Exists)
            {
                downloadPath.Create();
            }

            Logger.Info($"Plugin download folder located {downloadPath.FullName}");

            return downloadPath;
        }

        /// <summary>
        /// Retrieve all plugin that can be installed
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of type <code>(FileInfo cdp4ckFile, Manifest manifest)</code> of the updatable plugins</returns>
        public static IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> GetDownloadedInstallablePluginUpdate()
        {
            var updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>();

            var currentPlateformVersion = GetVersion();

            // Loop through all existing download plugin folders
            foreach (var downloadedPluginFolder in GetDownloadDirectory().EnumerateDirectories())
            {
                if (GetPlugin(currentPlateformVersion, downloadedPluginFolder) is { manifest: { }, cdp4ckFile: { } } plugin)
                {
                    updatablePlugins.Add(plugin);
                    Logger.Info($"Added installable plugin {plugin.manifest.Name}");
                }
            }

            Logger.Info(message: $"Found {updatablePlugins.Count} installable plugins");
            return updatablePlugins;
        }

        ///<summary>
        /// Check if the found plugin can be installed
        /// </summary>
        /// <param name="imeVersion">the IME version</param>
        /// <param name="downloadedPluginFolder">the found folder of downloaded plugin</param>
        /// <returns>Returns a represented plugin by a <code>(FileInfo cdp4ckFile, Manifest manifest)</code> </returns>
        private static (FileInfo cdp4ckFile, Manifest manifest) GetPlugin(Version imeVersion, DirectoryInfo downloadedPluginFolder)
        {
            if (downloadedPluginFolder.EnumerateFiles().FirstOrDefault(predicate: f => f.Name.EndsWith(value: ".cdp4ck")) is { } installableCdp4CkFullPath && installableCdp4CkFullPath.Directory is { } installableCdp4CkBasePath)
            {
                var manifest = DeserializeManifestFromCdp4Ck(installableCdp4CkFullPath, downloadedPluginFolder);

                if (manifest is { } && (manifest.MinIMEVersion is null || new Version(version: manifest.MinIMEVersion) <= imeVersion))
                {
                    Logger.Info($"Found installable plugin {manifest.Name}");
                    return (installableCdp4CkFullPath, manifest);
                }

                Logger.Info(
                    manifest is { }
                    ? $"{manifest.MinIMEVersion} is higher than the current IME version please update before installing this plugin update {imeVersion}"
                    : $"{downloadedPluginFolder.Name} does not contain any manifest. skipping plugin: {downloadedPluginFolder.Name}");
            }
            else
            {
                Logger.Error(message: $"{downloadedPluginFolder.Name} does not contain any package candidate. skipping plugin: {downloadedPluginFolder.Name}");
            }

            return default;
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
                Logger.Info($"Deserialized manifest of installable plugin {manifest?.Name}");
            }

            return manifest;
        }

        /// <summary>
        /// Gets the path of the AppData directory
        /// </summary>
        /// <returns>the Path the AppData folder</returns>
        public static string GetAppDataPath()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RHEA", "CDP4-COMET");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            Logger.Info($"AppData folder located {appDataPath}");

            return appDataPath;
        }

        /// <summary>
        /// Gets the current assembly version (Composition) which matches the IME one
        /// </summary>
        /// <returns>the version</returns>
        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
