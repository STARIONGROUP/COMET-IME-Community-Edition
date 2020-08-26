// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImeRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;
    using CDP4Composition.Utilities;

    using CDP4IME.Services;
    using CDP4IME.Views;

    using CDP4UpdateServerDal;
    using CDP4UpdateServerDal.Enumerators;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="PluginRow"/> holding its properties and interaction logic
    /// </summary>
    public class ImeRowViewModel : ReactiveObject, IImeRowViewModel
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
        /// Backing field for the property <see cref="VersionToDisplay"/>
        /// </summary>
        private string versionToDisplay;

        /// <summary>
        /// Gets or Sets the <see cref="versionToDisplay"/> of the represented plugin
        /// </summary>
        public string VersionToDisplay
        {
            get => this.versionToDisplay;
            set => this.RaiseAndSetIfChanged(ref this.versionToDisplay, value);
        }
        
        /// <summary>
        /// Backing field for the property <see cref="Platform"/>
        /// </summary>
        private Platform platform;

        /// <summary>
        /// Gets or Sets the <see cref="platform"/> of the represented plugin
        /// </summary>
        public Platform Platform
        {
            get => this.platform;
            set => this.RaiseAndSetIfChanged(ref this.platform, value);
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
        /// Gets or sets the assert <see cref="isSelected"/> whether the represented plugin will be installed
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
        /// Holds the version as is to be sent to the update server
        /// </summary>
        private readonly string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImeRowViewModel"/> class
        /// </summary>
        /// <param name="version">The represented version</param>
        public ImeRowViewModel(string version)
        {
            this.version = version;
            this.UpdateProperties();
        }

        /// <summary>
        /// Update this <see cref="ImeRowViewModel"/> Properties
        /// </summary>
        private void UpdateProperties()
        {
            this.Platform = ServiceLocator.Current.GetInstance<IAssemblyInformationService>().GetProcessorArchitecture() == ProcessorArchitecture.Amd64 ? Platform.X64 : Platform.X86;
            this.Name = $"CDP4IME-CE";
            this.VersionToDisplay = $"{this.Platform.ToString().ToLower()}-{this.version}";
            this.FileSystem = ServiceLocator.Current.GetInstance<IUpdateFileSystemService>();
        }

        /// <summary>
        /// Downloads this represented IME Installer
        /// </summary>
        /// <param name="client">the Update Server Client to perform request</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task Download(IUpdateServerClient client)
        {
            try
            {
                this.Progress = 0;

                using (var stream = await client.DownloadIme(this.version, this.Platform))
                {
                    this.Progress = 50;
                    
                    using (var fileStream = this.FileSystem.CreateImeMsi($"{this.Name}.{this.VersionToDisplay}.msi"))
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
        /// Called when the download process gets canceled
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task HandlingCancelationOfDownload()
        {
            return Task.Run(() =>
            {
                try
                {
                    this.Progress = -1;
                    this.FileSystem.CleanupDownloadedMsi(this.Name);
                    this.Progress = 0;
                }
                catch (Exception exception)
                {
                    this.logger.Error($"An exception occured: {exception}");
                    throw;
                }
            });
        }
    }
}
