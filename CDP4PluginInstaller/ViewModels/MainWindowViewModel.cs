// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4PluginInstaller.ViewModels
{
    using System.Collections.Generic;

    using CDP4Composition;
    using CDP4Composition.Modularity;

    using CDP4PluginInstaller.Views;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="MainWindowViewModel"/> is the view model of the <see cref="MainWindow"/> holding it properties its properties and interaction logic
    /// </summary>
    public class MainWindowViewModel : ReactiveObject
    {
        /// <summary>
        /// Gets a <see cref="ReactiveList{T}"/> of type <see cref="PluginRowViewModel"/> that holds the properties for <see cref="PluginRow"/>
        /// </summary>
        public ReactiveList<PluginRowViewModel> AvailablePlugins { get;  } = new ReactiveList<PluginRowViewModel>();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <code>(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)</code>
        /// of the updatable plugins
        /// </summary>
        public List<(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)> UpdatablePlugins { get; } = new List<(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)>();

        /// <summary>
        /// Instanciate a new <see cref="MainWindowViewModel"/>
        /// </summary>
        /// <param name="updatablePlugins"></param>
        public MainWindowViewModel(List<(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)> updatablePlugins)
        {
            this.UpdatablePlugins.AddRange(updatablePlugins);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            
        }
    }
}
