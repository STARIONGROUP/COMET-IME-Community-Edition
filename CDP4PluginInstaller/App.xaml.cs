// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="RHEA System S.A.">
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


namespace CDP4PluginInstaller
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Windows;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;

    using CDP4PluginInstaller.Utilities;
    using CDP4PluginInstaller.ViewModels;
    using CDP4PluginInstaller.Views;

    using CommonServiceLocator;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Holds the windows Elevated rights user claim value
        /// </summary>
        private const string AdministratorClaimsValue = "S-1-5-32-544";

        /// <summary>
        /// The NLog logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds the path to the download folder of plugins
        /// </summary>
        private readonly string downloadFolder;

        /// <summary>
        /// Holds the path to the download folder of plugins
        /// </summary>
        private Version currentImeVersion;

        /// <summary>
        /// Holds an <see cref="IEnumerable{T}"/> of type <code>(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)</code>
        /// of the updatable plugins
        /// </summary>
        public List<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)> UpdatablePlugins = new List<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)>();

        /// <summary>
        /// Holds the main Windows instance
        /// </summary>
        private MainWindow mainWindow;

        /// <summary>
        /// Initiate a new <see cref="App"/>
        /// </summary>
        public App()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.downloadFolder = Path.Combine(appData, "RHEA/CDP4/DownloadCache/Plugins/");
            this.mainWindow = new MainWindow();
        }

        /// <summary>
        /// Fires when the <see cref="App"/> starts
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!this.IsRunningAsAdministrator)
            {
                this.CloseOnError("The CDP4 Plugin Installer requires Administrator Rights to run.");
            }

            try
            {
                if (new Version(e.Args.Single()) is { } imeVersion)
                {
                    this.currentImeVersion = imeVersion;

                    if (this.RetrieveInstallablePlugins())
                    {
                        this.MainWindow.DataContext = new MainWindowViewModel(this.UpdatablePlugins);
                        this.mainWindow.Show();
                    }
                    else
                    {
                        this.logger.Debug("No update found");
                    }
                }
            }
            catch (Exception exception)
            {
                this.CloseOnError($"No IME version provided, the installer cannot run without it. Exception thrown: {exception}");
            }
        }

        /// <summary>
        /// Closes the CDP4PluginInstaller on error and shows the reason
        /// </summary>
        /// <param name="message">The error message to report</param>
        private void CloseOnError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show(message);
                this.logger.Error(message);
            }

            this.mainWindow.Close();
        }

        /// <summary>
        /// Retrieve all plugin that can be installed
        /// </summary>
        /// <returns>Assert whether there is any installable plugin found</returns>
        private bool RetrieveInstallablePlugins()
        {
            if (!Directory.Exists(this.downloadFolder))
            {
                this.logger.Info("Download folder is empty or inexistant, download some plugins update from the IME first");
                return false;
            }
            
            //var currentlyInstalledPluginManifests = PluginUtilities.GetPluginManifests().ToList();
            
            // Loop through all existing download plugin folders
            foreach (var downloadedPluginFolder in Directory.EnumerateDirectories(this.downloadFolder).Select(d => new DirectoryInfo(d)))
            {
                //var correspondingInstalledManifest = currentlyInstalledPluginManifests.SingleOrDefault(p => p.Name == downloadedPluginFolder.Name);

                if (downloadedPluginFolder.EnumerateFiles().FirstOrDefault(f => f.Name.EndsWith(".cdp4ck")) is { } installableCdp4ckFullPath  && installableCdp4ckFullPath.Directory is { } installableCdp4ckBasePath)
                {
                    ZipFile.ExtractToDirectory(installableCdp4ckFullPath.FullName, installableCdp4ckBasePath.FullName, true);

                    if (installableCdp4ckBasePath.EnumerateFiles().FirstOrDefault(f => f.Name.EndsWith(".plugin.manifest")) is { } manifestFile)
                    {
                        var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(manifestFile.FullName)));

                        if (new Version(manifest.CompatibleIMEVersion) <= this.currentImeVersion)
                        {
                            this.UpdatablePlugins.Add((installableCdp4ckFullPath, manifest));
                        }
                        else
                        {
                            this.logger.Debug($"{manifest.CompatibleIMEVersion} is higher than the current IME version please update before installing this plugin update {this.currentImeVersion}");
                        }
                    }
                    else
                    {
                        this.logger.Error($"{downloadedPluginFolder.Name} does not contain any manifest. skipping plugin: {downloadedPluginFolder.Name}");
                    }
                }
                else
                {
                    this.logger.Error($"{downloadedPluginFolder.Name} does not contain any candidate package. skipping plugin: {downloadedPluginFolder.Name}");
                }
            }

            this.logger.Debug($"Found {this.UpdatablePlugins.Count} installable plugins");
            return this.UpdatablePlugins.Count > 0;
        }

        /// <summary>
        /// Get the latest version downloaded of a plugin
        /// </summary>
        /// <param name="downloadedPluginFolder">the absolute path to the plugin download folder containing one or multiple versions</param>
        /// <param name="correspondingInstalledManifest">the currently installed plugin manifest</param>
        /// <returns>Returns null if no candidate are found other wise return the most recent version absolute path string</returns>
        private string GetInstallableCdp4ckBasePath(DirectoryInfo downloadedPluginFolder, Manifest correspondingInstalledManifest = null)
        {
            var allDownloadedVersion = downloadedPluginFolder.EnumerateDirectories().OrderBy(x => x.Name);

            // if a version of the current plugin is already installed
            return correspondingInstalledManifest is { }
                ? allDownloadedVersion.SkipWhile(x => new Version(x.Name) <= new Version(correspondingInstalledManifest.Version)).LastOrDefault()?.FullName
                : allDownloadedVersion.LastOrDefault()?.FullName;
        }

        /// <summary>
        /// Gets an Assert whether this program has been started with elevated rights 
        /// </summary>
        private bool IsRunningAsAdministrator
        {
            get
            {
                var administratorClaim = new WindowsPrincipal(WindowsIdentity.GetCurrent()).UserClaims.FirstOrDefault(x => x.Value.Contains(AdministratorClaimsValue));
                return administratorClaim is { };
            }
        }
    }
}
