// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4IMEBootstrapper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using CDP4Composition.Adapters;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.AppSettingService;
    using CDP4IME.Settings;
    //using CDP4Composition.Services.AppSettingService;
    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Ribbon;

    using Microsoft.Practices.Prism.MefExtensions;
    using Microsoft.Practices.Prism.Regions;

    using NLog;

    /// <summary>
    /// The Class that provides the bootstrapping sequence that registers all the 
    /// Modules of the application and initializes the Shell
    /// </summary>
    public class CDP4IMEBootstrapper : MefBootstrapper
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// represents the state of the <see cref="CDP4IMEBootstrapper"/>
        /// </summary>
        private string state;

        /// <summary>
        /// the path if the running assembly
        /// </summary>
        private string currentAssemblyPath;

        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>The shell of the application.</returns>
        protected override DependencyObject CreateShell()
        {
            logger.Trace("Creating Shell");
            return this.Container.GetExportedValue<Shell>();
        }

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        protected override void InitializeShell()
        {
            var appSettingsService = this.Container.GetExportedValue<IAppSettingsService<ImeAppSettings>>();

            this.UpdateBootstrapperState("Loading CDP4 Plugins");
            var pluginCatalog = new CDP4PluginLoader(appSettingsService);
            foreach (var directoryCatalog in pluginCatalog.DirectoryCatalogues)
            {
                this.AggregateCatalog.Catalogs.Add(directoryCatalog);

                this.UpdateBootstrapperState($"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");
            }

            this.UpdateBootstrapperState($"{pluginCatalog.DirectoryCatalogues.Count} CDP4 Plugins Loaded");

            this.UpdateBootstrapperState("Initializing the Shell");
            base.InitializeShell();
            var shell = (Shell)this.Shell;
            var dialogNavigationService = this.Container.GetExportedValue<IDialogNavigationService>();

            shell.DataContext = new ShellViewModel(dialogNavigationService);

            this.UpdateBootstrapperState("Setting up Regions");
            var regionmanager = this.Container.GetExportedValue<IRegionManager>();
            foreach (var region in regionmanager.Regions)
            {
                this.UpdateBootstrapperState($"Loaded Region: {region.Name} ");
            }

            Application.Current.MainWindow = shell;
        }

        /// <summary>
        /// Configures the <see cref="AggregateCatalog"/> used by MEF.
        /// </summary>
        protected override void ConfigureAggregateCatalog()
        {
            this.UpdateBootstrapperState("Configuring catalogs");

            base.ConfigureAggregateCatalog();
            this.currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (this.currentAssemblyPath == null)
            {
                throw new InvalidOperationException("Cannot find directory path for " + Assembly.GetExecutingAssembly().FullName);
            }

            var sw = new Stopwatch();
            sw.Start();
            this.UpdateBootstrapperState("Loading CDP4 Catalogs");

            var dllCatalog = new DirectoryCatalog(path: this.currentAssemblyPath, searchPattern: "CDP4*.dll");
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4IMEBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(dllCatalog);

            var message = $"CDP4 Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]";
            this.UpdateBootstrapperState(message);
        }

        /// <summary>
        /// Register the custom Region Adapters
        /// </summary>
        /// <returns>
        /// an instance of <see cref="RegionAdapterMappings"/> containing the registered Region Adapters
        /// </returns>
        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            this.UpdateBootstrapperState("Configuring Region Mappings");

            var mappings = base.ConfigureRegionAdapterMappings();

            mappings.RegisterMapping(typeof(LayoutPanel), this.Container.GetExportedValue<LayoutPanelAdapter>());
            mappings.RegisterMapping(typeof(LayoutGroup), this.Container.GetExportedValue<LayoutGroupAdapter>());
            mappings.RegisterMapping(typeof(DocumentGroup), this.Container.GetExportedValue<DocumentGroupAdapter>());
            mappings.RegisterMapping(typeof(TabbedGroup), this.Container.GetExportedValue<TabbedGroupAdapter>());
            mappings.RegisterMapping(typeof(RibbonControl), this.Container.GetExportedValue<RibbonAdapter>());

            return mappings;
        }

        /// <summary>
        /// Updates the state of the <see cref="CDP4IMEBootstrapper"/> and shows this on the splash screen
        /// and logs it to the logger
        /// </summary>
        /// <param name="message">
        /// the message that reflects the state of the <see cref="CDP4IMEBootstrapper"/>
        /// </param>
        private void UpdateBootstrapperState(string message)
        {
            this.state = message;
            logger.Debug(this.state);
            DXSplashScreen.SetState(this.state);
        }
    }
}
