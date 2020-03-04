// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsMetaData.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    using CDP4Composition.Attributes;
    using Microsoft.Practices.Prism.Modularity;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using DevExpress.Utils;

    /// <summary>
    /// Metadata of the plugin settings
    /// </summary>
    public class PluginSettingsMetaData : SettingsMetaData
    {
        /// <summary>
        /// Class settings <see cref="PluginSettingsMetaData"/> used to serilize/deserialize plugin configurations
        /// </summary>
        public PluginSettingsMetaData()
        {
        }

        /// <summary>
        /// Initializing Imodule to retrieve metadata information
        /// </summary>
        public PluginSettingsMetaData(IModule module, ICollection<string> newPlugins)
        {
            var fileName = module.GetType().GetAssembly().Location.ToUpper();
            var locationInfo = new DirectoryInfo(Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException());
            var location = locationInfo.Name;

            if (newPlugins.Contains(location))
            {
                var assemblyName = ((AssemblyTitleAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute))).Title;

                var nameAttribute = (ModuleExportNameAttribute)Attribute.GetCustomAttribute(module.GetType(), typeof(ModuleExportNameAttribute));

                var name = nameAttribute == null ? string.Empty : nameAttribute.Name;
                var isMandatory = nameAttribute?.IsMandatory ?? false;
                var description = ((AssemblyDescriptionAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute))).Description;
                var version = ((AssemblyFileVersionAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
                var company = ((AssemblyCompanyAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute))).Company;

                this.Key = location;
                this.Name = name;
                this.Assembly = assemblyName;
                this.Description = description;
                this.Version = version;
                this.Company = company;
                this.IsMandatory = isMandatory;
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

        /// <summary>
        /// Gets or sets if the plugin is mandatory
        /// </summary>
        public bool IsMandatory { get; set; } = false;
    }
}
