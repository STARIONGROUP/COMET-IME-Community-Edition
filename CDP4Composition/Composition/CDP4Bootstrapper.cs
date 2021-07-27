// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4Bootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    public abstract class CDP4Bootstrapper<T> where T : AppSettings, new()
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// represents the state of the <see cref="CDP4IMEBootstrapperLegacy"/>
        /// </summary>
        protected string status;

        public void Run()
        {
            try
            {
                UpdateBootstrapperStatus("Configuring catalogs");

                var catalog = new AggregateCatalog();
                var container = new CompositionContainer(catalog);

                var sw = Stopwatch.StartNew();
                UpdateBootstrapperStatus("Loading COMET Catalogs");

                AddExecutingAssemblyCatalog(catalog);
                AddCustomCatalogs(catalog);

                UpdateBootstrapperStatus($"COMET Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]");

                ConfigureServiceLocator(container);

                AddPluginCatalogs(catalog);

                UpdateBootstrapperStatus("Composing parts");
                container.ComposeParts();

                UpdateBootstrapperStatus("Initializing modules");
                InitialiseModules(container);

                OnComposed(container);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        /// <summary>
        /// Add the main application catalogues
        /// </summary>
        /// <param name="catalog">The composition  <see cref="AggregateCatalog"/></param>
        private static void AddExecutingAssemblyCatalog(AggregateCatalog catalog)
        {
            var currentAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentAssemblyPath == null)
            {
                throw new InvalidOperationException($"Cannot find directory path for {Assembly.GetExecutingAssembly().FullName}");
            }

            var dllCatalog = new DirectoryCatalog(path: currentAssemblyPath, searchPattern: "CDP4*.dll");

            catalog.Catalogs.Add(dllCatalog);
        }

        /// <summary>
        /// Configures the service locator to the current MEF composition
        /// </summary>
        /// <param name="container"></param>
        private void ConfigureServiceLocator(CompositionContainer container)
        {            
            var serviceLocator = new MefServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        /// <summary>
        /// Adds the main application catalogues
        /// </summary>
        /// <param name="catalog">The <see cref="AggregateCatalog"/></param>
        private void AddPluginCatalogs(AggregateCatalog catalog)
        {
            UpdateBootstrapperStatus("Loading COMET Plugins");

            var pluginLoader = new PluginLoader<T>();

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                catalog.Catalogs.Add(directoryCatalog);
                UpdateBootstrapperStatus($"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");
            }

            UpdateBootstrapperStatus($"{pluginLoader.DirectoryCatalogues.Count} COMET Plugins Loaded");
        }

        /// <summary>
        /// Calls initialize on all <see cref="IModule"/>s in the container/>
        /// </summary>
        /// <param name="container"></param>
        private void InitialiseModules(CompositionContainer container)
        {
            var moduleInitializer = container.GetExportedValue<IModuleInitializer>();
            moduleInitializer.Initialize();
        }

        /// <summary>
        /// Updates the bootstrapper can be overriden in concrete class 
        /// </summary>
        /// <param name="message">The status message</param>
        protected virtual void UpdateBootstrapperStatus(string message)
        {
            status = message;
            logger.Debug(status);
        }

        /// <summary>
        /// Is called after the container has been composed
        /// </summary>
        /// <param name="container"></param>
        protected abstract void OnComposed(CompositionContainer container);

        /// <summary>
        /// Optional override to add custom catalogs in concrete class
        /// </summary>
        /// <param name="catalog"></param>
        protected virtual void AddCustomCatalogs(AggregateCatalog catalog)
        {
        }
    }
}
