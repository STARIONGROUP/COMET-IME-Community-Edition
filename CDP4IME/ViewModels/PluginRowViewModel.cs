// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-Plugin Installer Community Edition. 
//    The CDP4-Plugin Installer Community Edition is the RHEA Plugin Installer for the CDP4-IME Community Edition.
//
//    The CDP4-Plugin Installer Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-Plugin Installer Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;

    using CDP4IME.Services;
    using CDP4IME.Views;

    using CDP4UpdateServerDal;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;
    
    /// <summary>
    /// Represents a <see cref="PluginRow"/> holding its properties and interaction logic
    /// </summary>
    public class PluginRowViewModel : ReactiveObject, IPluginRowViewModel
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the property <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or Sets the <see cref="name"/> of the represented plugin
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Version"/>
        /// </summary>
        private string version;

        /// <summary>
        /// Gets or Sets the <see cref="version"/> of the represented plugin
        /// </summary>
        public string Version
        {
            get => this.version;
            set => this.RaiseAndSetIfChanged(ref this.version, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or Sets the <see cref="description"/> of the represented plugin
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Author"/>
        /// </summary>
        private string author;

        /// <summary>
        /// Gets or Sets the <see cref="author"/> of the represented plugin
        /// </summary>
        public string Author
        {
            get => this.author;
            set => this.RaiseAndSetIfChanged(ref this.author, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="ReleaseNote"/>
        /// </summary>
        private string releaseNote;

        /// <summary>
        /// Gets or Sets the <see cref="author"/> of the represented plugin
        /// </summary>
        public string ReleaseNote
        {
            get => this.releaseNote;
            set => this.RaiseAndSetIfChanged(ref this.releaseNote, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Progress"/>
        /// </summary>
        private double progress;

        /// <summary>
        /// Gets or Sets the <see cref="progress"/> of the represented plugin
        /// </summary>
        public double Progress
        {
            get => this.progress;
            set => this.RaiseAndSetIfChanged(ref this.progress, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Gets or sets the assert <see cref="isSelected"/> whether the represented plugin will be installed or downloaded
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }
        
        /// <summary>
        /// Gets the <see cref="IUpdateFileSystemService"/> to operate on
        /// </summary>
        public IUpdateFileSystemService FileSystem { get; set; }

        /// <summary>
        /// Gets the plugin download path and the new manifest
        /// </summary>
        public (FileInfo cdp4ckFile, Manifest manifest) Plugin { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginRowViewModel"/> class
        /// </summary>
        /// <param name="plugin">The represented plugin</param>
        /// <param name="updateFileSystemService">The file system to operate on</param>
        public PluginRowViewModel((FileInfo cdp4ckFile, Manifest manifest) plugin, IUpdateFileSystemService updateFileSystemService = null)
        {
            this.Plugin = plugin;
            this.FileSystem = updateFileSystemService ?? new UpdateFileSystemService(plugin);
            this.UpdateProperties();
        }

        /// <summary>
        /// Initalizes a new instance of the <see cref="PluginRowViewModel"/> class
        /// </summary>
        /// <param name="plugin"> A <code>(string ThingName, string Version)</code> containing the name and the version</param>
        public PluginRowViewModel((string ThingName, string Version) plugin)
        {
            this.Name = plugin.ThingName;
            this.Version = plugin.Version;
            this.FileSystem = ServiceLocator.Current.GetInstance<IUpdateFileSystemService>();
        }

        /// <summary>
        /// Update this <see cref="PluginRowViewModel"/> Properties
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Plugin.manifest.Name;
            this.Description = this.Plugin.manifest.Description;
            this.Version = $"Version {this.Plugin.manifest.Version}";
            this.Author = this.Plugin.manifest.Author;
            this.ReleaseNote = this.Plugin.manifest.ReleaseNote;
        }

        /// <summary>
        /// Make the installation of the new Plugin
        /// <param name="token">Cancelation Token</param>
        /// </summary>
        public Task Install(CancellationToken token)
        {
            return Task.Run(
            () =>
            {
                try
                {
                    this.Progress += 33;
                    this.FileSystem.BackUpOldVersion();

                    this.Progress += 33;
                    this.FileSystem.InstallNewVersion();

                    this.Progress += 33;
                    this.FileSystem.CleanUp();
                    this.Progress = 100;
                }
                catch (Exception exception)
                {
                    this.logger.Error($"An exception occured: {exception}");
                    throw;
                }
            },
            token);
        }

        /// <summary>
        /// Downloads this represented plugin
        /// </summary>
        /// <param name="client">the Update Server Client to perform request</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task Download(IUpdateServerClient client)
        {
            try
            {
                this.Progress = 0;

                using (var stream = await client.DownloadPlugin(this.Name, this.Version))
                {
                    this.Progress = 50;
                    
                    using (var fileStream = this.FileSystem.CreateCdp4Ck(this.Name))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(fileStream);
                    }
                }
                
                this.Progress = 100;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
                throw;
            }
        }
        
        /// <summary>
        /// Called when the install gets canceled
        /// </summary>
        public void HandlingCancelationOfInstallation()
        {
            try
            {
                this.Progress = -1;

                this.FileSystem.Restore();

                this.Progress = 0;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
                throw;
            }
        }

        /// <summary>
        /// Handles the cancelation of the download process
        /// </summary>
        public void HandlingCancelationOfDownload()
        {
            try
            {
                this.Progress = -1;

                this.FileSystem.CleanupDownloadedPlugin(this.Name);

                this.Progress = 0;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
                throw;
            }
        }
    }
}
