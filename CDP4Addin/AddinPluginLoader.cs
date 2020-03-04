// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinPluginLoader.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using System.Linq;

    using CDP4Composition.Services.AppSettingService;
    using CDP4AddinCE.Settings;

    /// <summary>
    /// The purpose of the <see cref="AddinPluginLoader"/> is to load the various
    /// CDP4 plugins that are located in the plugins folder.
    /// </summary>
    public class AddinPluginLoader
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
        /// Initializes a new instance of the <see cref="AddinPluginLoader"/> class.
        /// </summary>
        public AddinPluginLoader(IAppSettingsService<AddinAppSettings> appSettingsService)
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
                    var pluginName = dir.FullName.ToUpper();
                    var locationInfo = new DirectoryInfo(pluginName);
                    var pluginSettingsList = appSettingsService.AppSettings.Plugins.Where(x => (x.Key != null) && (x.Key.ToUpper() == locationInfo.Name)).ToList();
                    var isNewPlugin = pluginSettingsList.Count == 0;

                    if (isNewPlugin || pluginSettingsList.Single().IsEnabled)
                    {
                        this.LoadPlugins(dir.FullName, isNewPlugin);
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
        /// <param name="folder">
        /// the folder that contains the CDP4 plugin
        /// </param>
        /// <param name="isNewPlugin">
        /// the flag telling is plugin is new to the settings
        /// </param> 
        private void LoadPlugins(string folder, bool isNewPlugin)
        {
            var dllCatalog = new DirectoryCatalog(path: folder, searchPattern: "*.dll");
            this.DirectoryCatalogues.Add(dllCatalog);

            if (isNewPlugin)
            {
                var locationInfo = new DirectoryInfo(folder);
                this.NewPlugins.Add(locationInfo.Name.ToUpper());
            }
        }
    }
}
