// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4IMEBootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
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

    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;

    using CDP4IME.Settings;

    using DevExpress.Xpf.Core;

    using Microsoft.Practices.Prism.MefExtensions;

    using NLog;

    /// <summary>
    /// The Class that provides the bootstrapping sequence that registers all the 
    /// Modules of the application and initializes the Shell
    /// </summary>
    public class CDP4IMEBootstrapperLegacy : MefBootstrapper
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// represents the state of the <see cref="CDP4IMEBootstrapperLegacy"/>
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

            var moduleInitializer = this.Container.GetExportedValue<IModuleInitializer>();
            moduleInitializer.Initialize();

            this.UpdateBootstrapperState($"{pluginLoader.DirectoryCatalogues.Count} COMET Plugins Loaded");

            this.UpdateBootstrapperState("Initializing the Shell");

            base.InitializeShell();

            var shell = (Shell)this.Shell;
            var dialogNavigationService = this.Container.GetExportedValue<IDialogNavigationService>();
            var dockViewModel = this.Container.GetExportedValue<DockLayoutViewModel>();
            var ribbonBuilder = this.Container.GetExportedValue<IRibbonContentBuilder>();

            //TODO: GH#861 'Make do' solution until the bootstrapper is replaced. Currently injecting the builder into the Shell view is rejected by the composition.
            ribbonBuilder.BuildAndAppendToRibbon(shell.Ribbon);
            shell.DataContext = new ShellViewModel(dialogNavigationService, dockViewModel);

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
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4IMEBootstrapperLegacy).Assembly));
            this.AggregateCatalog.Catalogs.Add(dllCatalog);

            this.UpdateBootstrapperState($"COMET Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]");
        }

        /// <summary>
        /// Updates the state of the <see cref="CDP4IMEBootstrapperLegacy"/> and shows this on the splash screen
        /// and logs it to the logger
        /// </summary>
        /// <param name="message">
        /// the message that reflects the state of the <see cref="CDP4IMEBootstrapperLegacy"/>
        /// </param>
        private void UpdateBootstrapperState(string message)
        {
            this.state = message;
            logger.Debug(this.state);
            DXSplashScreen.SetState(this.state);
        }
    }
}
