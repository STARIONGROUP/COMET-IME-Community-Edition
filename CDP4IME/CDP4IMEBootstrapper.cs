// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4IMEBootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
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

namespace CDP4IME
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using CDP4Composition.Adapters;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.ViewModels;

    using CDP4IME.Settings;

    using DevExpress.Xpf.Core;
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
            this.UpdateBootstrapperState("Loading COMET Plugins");

            var pluginLoader = new PluginLoader<ImeAppSettings>();

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                this.AggregateCatalog.Catalogs.Add(directoryCatalog);

                this.UpdateBootstrapperState($"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");
            }

            this.UpdateBootstrapperState($"{pluginLoader.DirectoryCatalogues.Count} COMET Plugins Loaded");

            this.UpdateBootstrapperState("Initializing the Shell");

            base.InitializeShell();
            
            var shell = (Shell)this.Shell;
            var dialogNavigationService = this.Container.GetExportedValue<IDialogNavigationService>();
            var dockViewModel = this.Container.GetExportedValue<DockLayoutViewModel>();

            shell.DataContext = new ShellViewModel(dialogNavigationService, dockViewModel);

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
            var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentAssemblyPath == null)
            {
                throw new InvalidOperationException("Cannot find directory path for " + Assembly.GetExecutingAssembly().FullName);
            }

            var sw = new Stopwatch();
            sw.Start();    
            this.UpdateBootstrapperState("Loading COMET Catalogs");

            var dllCatalog = new DirectoryCatalog(path: currentAssemblyPath, searchPattern: "CDP4*.dll");
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4IMEBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(dllCatalog);

            this.UpdateBootstrapperState($"COMET Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]");
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