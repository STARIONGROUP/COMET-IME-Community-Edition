// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateInstaller.cs" company="RHEA System S.A.">
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

namespace CDP4IME.Modularity
{
    using System.Linq;

    using CDP4Composition.Modularity;
    using CDP4Composition.ViewModels;
    using CDP4Composition.Views;

    using CDP4IME.Services;
    using CDP4IME.ViewModels;
    using CDP4IME.Views;

    /// <summary>
    /// The <see cref="UpdateInstaller"/> is responsible to check all the CDP4 download folders and to install/update the availables user-selected plugins 
    /// </summary>
    public static class UpdateInstaller
    {
        /// <summary>
        /// Check for any update available and run the plugin installer
        /// </summary>
        public static void CheckAndInstall(IPluginInstallerViewInvokerService viewInvoker = null)
        {
            var updatablePlugins = PluginUtilities.GetDownloadedInstallablePluginUpdate().ToList();

            if (updatablePlugins.Any())
            {
                var pluginInstallerView = new UpdateDownloaderInstaller() { DataContext = new UpdateDownloaderInstallerViewModel(updatablePlugins) };
                (viewInvoker ?? new PluginInstallerViewInvokerService()).ShowDialog(pluginInstallerView);
            }
        }
    }
}
