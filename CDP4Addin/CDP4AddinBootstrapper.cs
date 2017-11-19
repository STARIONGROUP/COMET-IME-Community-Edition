// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4AddinBootstrapper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Addin
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using CDP4Composition;

    using Microsoft.Practices.Prism.MefExtensions;
    using Microsoft.Practices.ServiceLocation;

    using NLog;

    /// <summary>
    /// The Class that provides the bootstrapping sequence that registers all the 
    /// Modules of the application
    /// </summary>
    /// <remarks>
    /// There is no shell to be initialized for the addin
    /// </remarks>
    public class CDP4AddinBootstrapper : MefBootstrapper
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// the path if the running assembly
        /// </summary>
        private string currentAssemblyPath;

        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>null due to the fact there is no shell in the addin</returns>
        protected override DependencyObject CreateShell()
        {
            return new DependencyObject();
        }

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            
            logger.Log(LogLevel.Debug, "Loading CDP4 Plugins");

            var pluginLoader = new PluginLoader();

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                this.AggregateCatalog.Catalogs.Add(directoryCatalog);

                logger.Log(LogLevel.Debug, string.Format("DirectoryCatalogue {0} Loaded", directoryCatalog.FullPath));
            }

            logger.Log(LogLevel.Debug, string.Format("{0} CDP4 Plugins Loaded", pluginLoader.DirectoryCatalogues.Count));
        }

        /// <summary>
        /// Configures the <see cref="AggregateCatalog"/> used by MEF.
        /// </summary>
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            this.currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (this.currentAssemblyPath == null)
            {
                throw new InvalidOperationException("Cannot find directory path for " + Assembly.GetExecutingAssembly().FullName);
            }

            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Loading CDP4 Catalogs");
            
            var dllCatalog = new DirectoryCatalog(path: this.currentAssemblyPath, searchPattern: "CDP4*.dll");
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4AddinBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(dllCatalog);


            logger.Debug("CDP4 Catalogs loaded in: {0} [ms]", sw.ElapsedMilliseconds);
        }
    }
}
