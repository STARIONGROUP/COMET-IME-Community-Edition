// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPluginFileSystemService.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Services.PluginFileSystemService
{
    using System.IO;

    /// <summary>
    /// Defines methods and properties to provide operations appliable on a file system
    /// </summary>
    public interface IPluginFileSystemService
    {
        /// <summary>
        /// Gets or sets the path were the cdp4ck to be installed sits 
        /// </summary>
        FileInfo UpdateCdp4CkFileInfo { get; }

        /// <summary>
        /// Gets or sets the path where the old version of the plugin will be temporaly kept in case anything goes wrong with the installation
        /// </summary>
        DirectoryInfo TemporaryPath { get; }

        /// <summary>
        /// Gets or sets the path where the updated plugin should be installed
        /// </summary>
        DirectoryInfo InstallationPath { get; }

        /// <summary>
        /// Installs the new version into the <see cref="InstallationPath"/>
        /// </summary>
        void InstallNewVersion();

        /// <summary>
        /// Move All files and sub directories from the installation directory to the temporary one
        /// </summary>
        void BackUpOldVersion();

        /// <summary>
        /// Cleanup the downloaded files
        /// And the temporary folder
        /// </summary>
        void CleanUp();

        /// <summary>
        /// Restores the old version
        /// </summary>
        void Restore();
    }
}
