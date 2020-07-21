// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerViewModel.cs" company="RHEA System S.A.">
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
    using System.IO;

    using CDP4Composition.Behaviors;
    using CDP4Composition.Modularity;
    using CDP4Composition.Views;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="PluginInstallerViewModel"/> is the view model of the <see cref="PluginInstallerViewModel"/> holding it properties its properties and interaction logic
    /// </summary>
    public class PluginInstallerViewModel : ReactiveObject, IPluginInstallerViewModel
    {
        /// <summary>
        /// The attached Behavior
        /// </summary>
        public IPluginUpdateInstallerBehavior Behavior { get; set; }

        /// <summary>
        /// Gets the Command that will cancel the update operation if any and close the view
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets a <see cref="ReactiveList{T}"/> of type <see cref="PluginRowViewModel"/> that holds the properties for <see cref="PluginRow"/>
        /// </summary>
        public List<PluginRowViewModel> AvailablePlugins { get; } = new List<PluginRowViewModel>();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <code>(FileInfo pluginDownloadFullPath, Manifest theNewManifest)</code>
        /// of the updatable plugins
        /// </summary>
        public IEnumerable<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)> UpdatablePlugins { get; }

        /// <summary>
        /// Instanciate a new <see cref="PluginInstallerViewModel"/>
        /// </summary>
        /// <param name="updatablePlugins"></param>
        public PluginInstallerViewModel(IEnumerable<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)> updatablePlugins)
        {
            this.UpdatablePlugins = updatablePlugins;
            this.UpdateProperties();

            this.InitializeCommand();
        }
        
        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/>
        /// </summary>
        private void InitializeCommand()
        {
            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.CancelCommandExecute());
        }

        private void CancelCommandExecute()
        {
            this.Behavior.Close();
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            foreach (var plugin in this.UpdatablePlugins)
            {
                this.AvailablePlugins.Add(new PluginRowViewModel(plugin));
            }
        }
    }
}
