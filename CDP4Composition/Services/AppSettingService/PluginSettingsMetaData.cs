// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsMetaData.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Services.AppSettingService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The purpose of the <see cref="PluginSettingsMetaData"/> is to serialize/deserialize plugin configurations
    /// </summary>
    public class PluginSettingsMetaData : SettingsMetaData
    {
        /// <summary>
        /// Initializes an instance of <see cref="PluginSettingsMetaData"/> class
        /// </summary>
        public PluginSettingsMetaData()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="PluginSettingsMetaData"/> class based on the provided <see cref="IModule"/>
        /// </summary>
        /// <param name="module">
        /// The subject <see cref="IModule"/> on the basis of whuch the <see cref="PluginSettingsMetaData"/> is instantiated
        /// </param>
        /// <param name="newPlugins">
        /// A list of names of plugins
        /// </param>
        public PluginSettingsMetaData(IModule module, ICollection<string> newPlugins)
        {
            var moduleAssembly = module.GetType().Assembly;
            var fileName = moduleAssembly.Location.ToUpper();
            var locationInfo = new DirectoryInfo(Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException());
            var location = locationInfo.Name;

            if (newPlugins.Contains(location))
            {
                var assemblyName = ((AssemblyTitleAttribute)moduleAssembly.GetCustomAttribute(typeof(AssemblyTitleAttribute))).Title;
                var nameAttribute = (ModuleExportNameAttribute)Attribute.GetCustomAttribute(module.GetType(), typeof(ModuleExportNameAttribute));
                var name = nameAttribute == null ? string.Empty : nameAttribute.Name;
                var description = ((AssemblyDescriptionAttribute) moduleAssembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute))).Description;
                var version = ((AssemblyFileVersionAttribute) moduleAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
                var company = ((AssemblyCompanyAttribute) moduleAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute))).Company;

                this.Key = location;
                this.Name = name;
                this.Assembly = assemblyName;
                this.Description = description;
                this.Version = version;
                this.Company = company;
            }
        }

        /// <summary>
        /// Unique key of the plugin settings. It is the folder name.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description name of the plugin
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the comapnay name of the plugin
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the asembly name of the plugin
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// Gets or sets the version name of the plugin
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets if the plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}