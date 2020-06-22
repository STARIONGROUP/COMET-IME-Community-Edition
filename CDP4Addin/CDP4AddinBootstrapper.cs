// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4AddinBootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
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

namespace CDP4AddinCE
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using CDP4AddinCE.Settings;

    using CDP4Composition.Modularity;

    using Microsoft.Practices.Prism.MefExtensions;

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
            logger.Log(LogLevel.Debug, "Loading CDP4 Plugins");

            var pluginLoader = new PluginLoader<AddinAppSettings>();

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                this.AggregateCatalog.Catalogs.Add(directoryCatalog);
                logger.Log(LogLevel.Debug, $"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");
            }

            logger.Log(LogLevel.Debug, $"{pluginLoader.DirectoryCatalogues.Count} CDP4 Plugins Loaded");

            base.InitializeShell();
        }

        /// <summary>
        /// Configures the <see cref="AggregateCatalog"/> used by MEF.
        /// </summary>
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();
            var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentAssemblyPath == null)
            {
                throw new InvalidOperationException("Cannot find directory path for " + Assembly.GetExecutingAssembly().FullName);
            }

            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("Loading CDP4 Catalogs");

            var dllCatalog = new DirectoryCatalog(path: currentAssemblyPath, searchPattern: "CDP4*.dll");
            this.AggregateCatalog.Catalogs.Add(dllCatalog);

            logger.Debug("CDP4 Catalogs loaded in: {0} [ms]", sw.ElapsedMilliseconds);
        }
    }
}