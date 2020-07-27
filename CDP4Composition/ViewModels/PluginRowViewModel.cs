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


namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.PluginFileSystemService;
    using CDP4Composition.Views;

    using DevExpress.CodeParser.Diagnostics;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="PluginRow"/> holding its properties and interaction logic
    /// </summary>
    public class PluginRowViewModel : ReactiveObject
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

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
        /// Backing field for the property <see cref="Website"/>
        /// </summary>
        private string website;

        /// <summary>
        /// Gets or Sets the <see cref="website"/> page link of the represented plugin
        /// </summary>
        public string Website
        {
            get => this.website;
            set => this.RaiseAndSetIfChanged(ref this.website, value);
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
        /// Backing field for the property <see cref="InstallationProgress"/>
        /// </summary>
        private double installationProgress;

        /// <summary>
        /// Gets or Sets the <see cref="installationProgress"/> of the represented plugin
        /// </summary>
        public double InstallationProgress
        {
            get => this.installationProgress;
            set => this.RaiseAndSetIfChanged(ref this.installationProgress, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="IsSelectedForInstallation"/>
        /// </summary>
        private bool isSelectedForInstallation;
        
        /// <summary>
        /// Gets or sets the assert <see cref="isSelectedForInstallation"/> whether the represented plugin will be installed
        /// </summary>
        public bool IsSelectedForInstallation
        {
            get => this.isSelectedForInstallation;
            set => this.RaiseAndSetIfChanged(ref this.isSelectedForInstallation, value);
        }
        
        /// <summary>
        /// Gets the <see cref="IPluginFileSystemService"/> to operate on
        /// </summary>
        public IPluginFileSystemService FileSystem { get; set; }

        /// <summary>
        /// Gets or sets the Command that open the plugin store web page relative to this represented plugin
        /// </summary>
        public ReactiveCommand<object> WebsiteCommand { get; private set; }
        
        /// <summary>
        /// Gets the plugin download path and the new manifest
        /// </summary>
        public (FileInfo cdp4ckFile, Manifest manifest) Plugin { get; private set; }

        /// <summary>
        /// Instanciate a new <see cref="PluginRowViewModel"/>
        /// </summary>
        /// <param name="plugin">The represented plugin</param>
        /// <param name="pluginFileSystemService">The file system to operate on</param>
        public PluginRowViewModel((FileInfo cdp4ckFile, Manifest manifest) plugin, IPluginFileSystemService pluginFileSystemService = null)
        {
            this.Plugin = plugin;
            this.FileSystem = pluginFileSystemService ?? new PluginFileSystemService(plugin);
            this.UpdateProperties();
            this.InitializeCommands();
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>
        /// </summary>
        private void InitializeCommands()
        {
            this.WebsiteCommand = ReactiveCommand.Create();
            this.WebsiteCommand.Subscribe(_ => Process.Start(new ProcessStartInfo(this.Website)));
        }

        /// <summary>
        /// Update this <see cref="PluginRowViewModel"/> Properties
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Plugin.manifest.Name;
            this.Description = this.Plugin.manifest.Description;
            this.Version = $"version {this.Plugin.manifest.Version}";
            this.Author = this.Plugin.manifest.Author;
            this.Website = this.Plugin.manifest.Website;
            this.ReleaseNote = this.Plugin.manifest.ReleaseNote;
        }

        /// <summary>
        /// Make the installation of the new Plugin
        /// </summary>
        public void Install()
        {
            try
            {
                this.InstallationProgress += 33;
                this.FileSystem.BackUpOldVersion();

                this.InstallationProgress += 33;
                this.FileSystem.InstallNewVersion();

                this.InstallationProgress += 33;
                this.FileSystem.CleanUp();
                this.InstallationProgress = 100;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
            }
        }

        /// <summary>
        /// Called when the install gets canceled
        /// </summary>
        public void HandlingCancelation()
        {
            try
            {
                this.FileSystem.Restore();

                this.InstallationProgress = 0;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
            }
        }
    }
}
