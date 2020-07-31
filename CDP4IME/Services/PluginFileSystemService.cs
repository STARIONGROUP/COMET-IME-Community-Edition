// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginFileSystemService.cs" company="RHEA System S.A.">
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

namespace CDP4IME.Services
{
    using System.IO;
    using System.IO.Compression;

    using CDP4Composition.Modularity;

    /// <summary>
    /// The purpose of the <see cref="PluginFileSystemService"/> is to provide operations appliable on a file system
    /// </summary>
    public class PluginFileSystemService : IPluginFileSystemService
    {
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
        /// holds the plugin name whose belong to this <see cref="PluginFileSystemService"/> instance
        /// </summary>
        private readonly string pluginName;

        /// <summary>
        /// Instanciate a new <see cref="PluginFileSystemService"/>
        /// </summary>
        /// <param name="plugin"></param>
        public PluginFileSystemService((FileInfo cdp4ckFile, Manifest manifest) plugin)
        {
            var (cdp4CkFile, manifest) = plugin;
            this.pluginName = manifest.Name;

            this.UpdateCdp4CkFileInfo = cdp4CkFile;
            this.InstallationPath = PluginUtilities.GetPluginDirectory(this.pluginName);
            this.TemporaryPath = PluginUtilities.GetTempDirectoryInfo(this.pluginName);
        }

        /// <summary>
        /// Installs the new version into the <see cref="InstallationPath"/>
        /// </summary>
        public void InstallNewVersion()
        {
            if (!this.InstallationPath.Exists)
            {
                this.InstallationPath.Create();
            }

            using (var zip = ZipFile.OpenRead(this.UpdateCdp4CkFileInfo.FullName))
            {
                foreach (var zipArchiveEntry in zip.Entries)
                {
                    var newFile = new FileInfo(Path.Combine(this.InstallationPath.FullName, zipArchiveEntry.FullName));

                    if (newFile.Directory?.Exists == false)
                    {
                        newFile.Directory.Create();
                    }

                    zipArchiveEntry.ExtractToFile(newFile.FullName);
                }
            }
        }

        /// <summary>
        /// Move All files and sub directories from the installation directory to the temporary one
        /// The need to set again <see cref="InstallationPath"/> comes from the <see cref="DirectoryInfo.MoveTo"/>
        /// which path will be set to the new location: in this case the installation path becomes the tenporary path
        /// </summary>
        public void BackUpOldVersion()
        {
            if (this.InstallationPath.Exists)
            {
                this.InstallationPath.MoveTo(this.TemporaryPath.FullName);
                this.InstallationPath = PluginUtilities.GetPluginDirectory(this.pluginName);
            }
        }

        /// <summary>
        /// Cleanup the downloaded files
        /// And the temporary folder
        /// </summary>
        public void CleanUp()
        {
            if (this.UpdateCdp4CkFileInfo.Directory?.Exists == true)
            {
                Directory.Delete(this.UpdateCdp4CkFileInfo.Directory.FullName, true);
            }

            if (this.TemporaryPath.Exists)
            {
                Directory.Delete(this.TemporaryPath.FullName, true);
            }
        }

        /// <summary>
        /// Restores the old version
        /// </summary>
        public void Restore()
        {
            if (this.TemporaryPath.Exists)
            {
                Directory.Delete(this.InstallationPath.FullName);
                this.TemporaryPath.MoveTo(this.InstallationPath.FullName);
            }
        }
    }
}
