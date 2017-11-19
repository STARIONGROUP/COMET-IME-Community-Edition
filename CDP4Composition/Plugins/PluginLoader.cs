// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginLoader.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// The purpose of the <see cref="PluginLoader"/> is to load the various
    /// CDP4 plugins that are located in the plugins folder.
    /// </summary>
    public class PluginLoader
    {
        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        private const string PluginDirectoryName = "plugins";

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoader"/> class.
        /// </summary>
        public PluginLoader()
        {
            this.DirectoryCatalogues = new List<DirectoryCatalog>();

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var path = Path.Combine(currentPath, PluginDirectoryName);
            var directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Exists)
            {
                foreach (var dir in directoryInfo.EnumerateDirectories())
                {
                    this.LoadPlugins(dir.FullName);
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
