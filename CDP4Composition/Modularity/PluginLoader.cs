// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginLoader.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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

namespace CDP4Composition.Modularity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json;

    /// <summary>
    /// The purpose of the <see cref="PluginLoader"/> is to load the various
    /// CDP4 plugins that are located in the plugins folder.
    /// </summary>
    public class PluginLoader<T> where T : AppSettings, new()
    {
        /// <summary>
        /// Gets or sets a list of disabled plugins
        /// </summary>
        public List<Guid> DisabledPlugins { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets a <see cref="IEnumerable{Manifest}"/> of <see cref="Manifest"/>
        /// </summary>
        public IEnumerable<Manifest> ManifestsList { get; set; }

        /// <summary>
        /// Gets the <see cref="DirectoryCatalog"/>s that are loaded by the CDP4PluginLoader
        /// </summary>
        public List<DirectoryCatalog> DirectoryCatalogues { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoader"/> class.
        /// </summary>
        public PluginLoader()
        {
            this.DirectoryCatalogues = new List<DirectoryCatalog>();
            var currentPath = ServiceLocator.Current.GetInstance<IAssemblyLocationLoader>().GetLocation();
            
            var directoryInfo = new DirectoryInfo(Path.Combine(currentPath, PluginUtilities.PluginDirectoryName));

            if (directoryInfo.Exists)
            {
                this.GetManifests();
                this.GetDisabledPlugins();

                foreach (var manifest in this.ManifestsList.Where(m => !this.DisabledPlugins.Contains(m.ProjectGuid)))
                {
                    var path = Path.Combine(directoryInfo.FullName, manifest.Name);
                    
                    if (Directory.Exists(path))
                    {
                        this.LoadPlugins(path);
                    }
                }
            }
        }
        /// <summary>
        /// Sets the <see cref="DisabledPlugins"/> property
        /// </summary>
        private void GetDisabledPlugins()
        {
            var appSettingsService = ServiceLocator.Current.GetInstance<IAppSettingsService<T>>();
            this.DisabledPlugins = appSettingsService.AppSettings.DisabledPlugins;
        }

        /// <summary>
        /// Sets the <see cref="ManifestsList"/> property
        /// </summary>
        private void GetManifests()
        {
            this.ManifestsList = PluginUtilities.GetPluginManifests();
        }

        /// <summary>
        /// Load the plugins in the specified folder
        /// </summary>
        /// <param name="path">
        /// the folder info that contains the CDP4 plugin
        /// </param>
        private void LoadPlugins(string path)
        {
            var dllCatalog = new DirectoryCatalog(path: path, searchPattern: "*.dll");
            this.DirectoryCatalogues.Add(dllCatalog);
        }
    }
}