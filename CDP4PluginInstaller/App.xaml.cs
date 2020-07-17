using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CDP4PluginInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initiate a new App
        /// </summary>
        public App()
        {
                
        }
        public void App_Startup(object sender, StartupEventArgs e)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.downloadFolder = Path.Combine(appData, "RHEA/CDP4/DownloadCache/Plugins/");
            mainWindow.Show();
        }

        /// <summary>
        /// The NLog logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Holds the path to the download folder of plugins
        /// </summary>
        private readonly string downloadFolder;

        /// <summary>
        /// Holds an <see cref="IEnumerable{T}"/> of type <code>(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)</code>
        /// of the updatable plugins
        /// </summary>
        public List<(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)> UpdatablePlugins = new List<(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)>();
        
        /// <summary>
        /// 
        /// </summary>
        public void CheckAndRunUpdater()
        {
            this.GetInstallablePlugins();

            try
            {
                var updater = Process.Start("", this.UpdatablePlugins);
                updater.EnableRaisingEvents = true;
                updater.WaitForExit();
            }
            catch (Exception exception)
            {
                this.logger.Error($"The CDP4PluginUpdater has failed to start. {exception}");
            }
        }

        /// <summary>
        /// Retrieve all plugin that can be installed
        /// </summary>
        /// <returns>Assert whether there is any installable plugin found</returns>
        private bool GetInstallablePlugins()
        {
            if (!Directory.Exists(this.downloadFolder))
            {
                this.logger.Info("Download folder not yet created, download some plugins update from the IME first");
                return false;
            }

            var currentVersionIme = Assembly.GetExecutingAssembly().GetName().Version;

            var currentlyInstalledPluginManifests = PluginUtilities.GetPluginManifests().ToList();

            foreach (var downloadedPluginFolder in Directory.EnumerateDirectories(this.downloadFolder).Select(d => new DirectoryInfo(d)))
            {
                var correspondingInstalledManifest = currentlyInstalledPluginManifests.SingleOrDefault(p => p.Name == downloadedPluginFolder.Name);

                var installableCdp4ckBasePath = this.GetInstallableCdp4ckBasePath(downloadedPluginFolder, correspondingInstalledManifest);

                if (installableCdp4ckBasePath is { } && Directory.EnumerateFiles(installableCdp4ckBasePath).FirstOrDefault(f => f.EndsWith(".cdp4ck")) is { } installableCdp4ckFullPath)
                {
                    ZipFile.ExtractToDirectory(installableCdp4ckFullPath, installableCdp4ckBasePath);

                    if (Directory.EnumerateFiles(installableCdp4ckBasePath).FirstOrDefault(f => f.EndsWith(".plugin.manifest")) is { } manifestFileName)
                    {
                        var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(installableCdp4ckBasePath, manifestFileName)));
                        this.UpdatablePlugins.Add((installableCdp4ckBasePath, manifest, new Version(manifest.CompatibleIMEVersion) <= currentVersionIme));
                    }
                    else
                    {
                        this.logger.Error($"{downloadedPluginFolder.Name} does not contain any manifest. skipping plugin: {downloadedPluginFolder.Name}");
                    }
                }
                else
                {
                    this.logger.Error($"{downloadedPluginFolder.Name} does not contain any candidate package. skipping plugin: {downloadedPluginFolder.Name}");
                }
            }

            this.logger.Debug($"Found {this.UpdatablePlugins.Count} installable plugins");
            return this.UpdatablePlugins.Count > 0;
        }

        /// <summary>
        /// Get the latest version downloaded of a plugin
        /// </summary>
        /// <param name="downloadedPluginFolder">the absolute path to the plugin download folder containing one or multiple versions</param>
        /// <param name="correspondingInstalledManifest">the currently installed plugin manifest</param>
        /// <returns>Returns null if no candidate are found other wise return the most recent version absolute path string</returns>
        private string GetInstallableCdp4ckBasePath(DirectoryInfo downloadedPluginFolder, Manifest correspondingInstalledManifest = null)
        {
            var allDownloadedVersion = downloadedPluginFolder.EnumerateDirectories().OrderBy(x => x.Name);

            // if a version of the current plugin is already installed
            return correspondingInstalledManifest is { }
                ? allDownloadedVersion.SkipWhile(x => new Version(x.Name) <= new Version(correspondingInstalledManifest.Version)).LastOrDefault()?.FullName
                : allDownloadedVersion.LastOrDefault()?.FullName;
        }
    }
}
