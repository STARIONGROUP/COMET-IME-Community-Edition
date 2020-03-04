// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4PluginLoader.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.Services.AppSettingService;
    using CDP4IME.Settings;

    /// <summary>
    /// The CDP4 Plugin Module Catalog that loads the CDP4 Plugins
    /// </summary>
    public class CDP4PluginLoader
    {
        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        private const string PluginDirectoryName = "plugins";

        /// <summary>
        /// The name of the bew plugins to be added to the app settings
        /// </summary>
        public readonly List<string> NewPlugins;

        /// <summary>   
        /// Initializes a new instance of the <see cref="CDP4PluginLoader"/> class.
        /// </summary>
        public CDP4PluginLoader(IAppSettingsService<ImeAppSettings> appSettingsService)
        {
            this.DirectoryCatalogues = new List<DirectoryCatalog>();

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(currentPath, PluginDirectoryName);
            var directoryInfo = new DirectoryInfo(path);
            this.NewPlugins = new List<string>();

            if (directoryInfo.Exists)
            {
                foreach (var dir in directoryInfo.EnumerateDirectories())
                {
                    var pluginSettings = appSettingsService.AppSettings.Plugins.FirstOrDefault(x => (x.Key != null) && (string.Equals(x.Key, dir.Name, StringComparison.CurrentCultureIgnoreCase)));

                    if ((pluginSettings == null) || pluginSettings.IsEnabled)
                    {
                        this.LoadPlugins(dir, pluginSettings);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="DirectoryCatalog"/>s that are loaded by the CDP4PluginLoader
        /// </summary>
        public List<DirectoryCatalog> DirectoryCatalogues { get; private set; }

        /// <summary>
        /// Load the plugins in the specified folder
        /// </summary>
        /// <param name="dir">
        /// the folder info that contains the CDP4 plugin
        /// </param>
        /// <param name="pluginSettings">
        /// the plugin to be loaded if it is new
        /// </param>
        private void LoadPlugins(DirectoryInfo dir, PluginSettingsMetaData pluginSettings)
        {
            var dllCatalog = new DirectoryCatalog(path: dir.FullName, searchPattern: "*.dll");
            this.DirectoryCatalogues.Add(dllCatalog);

            if (pluginSettings == null)
            {
                this.NewPlugins.Add(dir.Name.ToUpper());
            }
        }
    }
}
