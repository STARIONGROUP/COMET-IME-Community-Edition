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
        /// Gets or Sets the <see cref="name"/> name of the represented object
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
        /// Gets or Sets the <see cref="name"/> name of the represented object
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
        /// Gets or Sets the <see cref="website"/> name of the represented object
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
        /// Gets or Sets the <see cref="description"/> name of the represented object
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
        /// Gets or Sets the <see cref="author"/> name of the represented object
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
        /// Gets or Sets the <see cref="author"/> name of the represented object
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
        /// Gets or Sets the <see cref="installationProgress"/> name of the represented object
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

        public ReactiveCommand<object> WebsiteCommand { get; private set; }

        /// <summary>
        /// Gets or sets the path were the cdp4ck to be installed sits 
        /// </summary>
        public FileInfo UpdateCdp4CkFileInfo { get; set; }

        /// <summary>
        /// Gets or sets the path where the old version of the plugin will be temporaly kept in case anything goes wrong with the installation
        /// </summary>
        public DirectoryInfo TemporaryPath { get; set; }

        /// <summary>
        /// Gets or sets the path where the updated plugin should be installed
        /// </summary>
        public DirectoryInfo InstallationPath { get; set; }
        
        /// <summary>
        /// Gets the plugin download path and the new manifest
        /// </summary>
        public (FileInfo cdp4ckFile, Manifest manifest) Plugin { get; private set; }

        /// <summary>
        /// Instanciate a new <see cref="PluginRowViewModel"/>
        /// </summary>
        /// <param name="plugin"></param>
        public PluginRowViewModel((FileInfo cdp4ckFile, Manifest manifest) plugin)
        {
            this.Plugin = plugin;
            this.UpdateProperties();
        }

        /// <summary>
        /// Update this <see cref="PluginRowViewModel"/> Properties
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateCdp4CkFileInfo = this.Plugin.cdp4ckFile;
            this.InstallationPath = PluginUtilities.GetPluginDirectory(this.Plugin.manifest.Name);
            this.TemporaryPath = PluginUtilities.GetTempDirectoryInfo(this.Plugin.manifest.Name);

            this.WebsiteCommand = ReactiveCommand.Create();
            this.WebsiteCommand.Subscribe(_ => Process.Start(new ProcessStartInfo(this.Website)));

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
                var existingFiles = this.InstallationPath.EnumerateFiles("*", SearchOption.AllDirectories).ToList();
                
                using var zip = ZipFile.OpenRead(this.UpdateCdp4CkFileInfo.FullName);
                var stepping = 100 / ((existingFiles.Count() * 2) + zip.Entries.Count + 2);

                this.MoveExistingFiles(existingFiles, stepping);
                
                //Install the new version
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    zipArchiveEntry.ExtractToFile(Path.Combine(this.InstallationPath.FullName, zipArchiveEntry.FullName));
                    this.InstallationProgress += stepping;
                }
                
                zip.Dispose();

                //Cleanup
                foreach (var oldFile in this.TemporaryPath.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    File.Delete(Path.Combine(this.TemporaryPath.FullName, oldFile.Name));
                    this.InstallationProgress += stepping;
                }

                Directory.Delete(this.TemporaryPath.FullName);
                
                //Cleanup the download file
                if (this.UpdateCdp4CkFileInfo.Directory?.Exists == true)
                {
                    foreach (var oldFile in this.UpdateCdp4CkFileInfo.Directory.EnumerateFiles())
                    {
                        File.Delete(Path.Combine(this.UpdateCdp4CkFileInfo.Directory.FullName, oldFile.Name));
                        this.InstallationProgress += stepping;
                    }

                    Directory.Delete(this.UpdateCdp4CkFileInfo.Directory.FullName);
                }

                this.InstallationProgress = 100;
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
            }
        }

        /// <summary>
        /// Move the old version to a temporary folder
        /// </summary>
        /// <param name="fileInfos">An <see cref="IEnumerable{T}"/> of <see cref="FileInfo"/></param>
        /// <param name="stepping">the progress stepping</param>
        private void MoveExistingFiles(IEnumerable<FileInfo> fileInfos, int stepping)
        {
            foreach (var oldFile in fileInfos)
            {
                File.Move(oldFile.FullName, Path.Combine(this.TemporaryPath.FullName, oldFile.Name));
                this.InstallationProgress += stepping;
            }
        }

        /// <summary>
        /// Called when the install gets canceled
        /// </summary>
        public void HandlingCancelation()
        {
            try
            {
                foreach (var oldFile in this.TemporaryPath.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    File.Move(Path.Combine(this.TemporaryPath.FullName, oldFile.Name), this.InstallationPath.FullName);
                    this.InstallationProgress = 0;
                }
            }
            catch (Exception exception)
            {
                this.logger.Error($"An exception occured: {exception}");
            }
        }
    }
}
