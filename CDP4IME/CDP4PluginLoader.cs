// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4PluginLoader.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME
{
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
        /// Initializes a new instance of the <see cref="CDP4PluginLoader"/> class.
        /// </summary>
        public CDP4PluginLoader(IAppSettingsService<ImeAppSettings> appSettingsService)
        {
            this.DirectoryCatalogues = new List<DirectoryCatalog>();

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(currentPath, PluginDirectoryName);
            var directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Exists)
            {
                foreach (var dir in directoryInfo.EnumerateDirectories())
                {
                    var fileName = Path.GetFileName(dir.FullName);

                    if (!appSettingsService.AppSettings.DisabledPlugins.Select(x => x.ToUpper()).ToList().Contains(fileName.ToUpper()))
                    {
                        this.LoadPlugins(dir.FullName);
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
        private void LoadPlugins(string folder)
        {
            var dllCatalog = new DirectoryCatalog(path: folder, searchPattern: "*.dll");
            this.DirectoryCatalogues.Add(dllCatalog);
        }
    }
}
