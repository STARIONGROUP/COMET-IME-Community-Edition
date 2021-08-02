// --------------------------------------------------------------------------------------------------------------------
// <copyright file="COMETBootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
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

    using CDP4Composition.Exceptions;
    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    /// <summary>
    /// Base class that provides basic bootstrapping using MEF
    /// </summary>
    /// <typeparam name="T">The <see cref="AppSettings"/></typeparam>
    public abstract class COMETBootstrapper<T> where T : AppSettings, new()
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Represents the state of the <see cref="COMETBootstrapper"/>
        /// </summary>
        protected string status;

        /// <summary>
        /// Runs the bootstrapper
        /// </summary>
        public void Run()
        {
            this.UpdateBootstrapperStatus("Configuring catalogs");

            var catalog = new AggregateCatalog();
            var container = new CompositionContainer(catalog);

            var sw = Stopwatch.StartNew();
            this.UpdateBootstrapperStatus("Loading COMET Catalogs");

            this.AddExecutingAssemblyCatalog(catalog);
            this.AddCustomCatalogs(catalog);

            this.UpdateBootstrapperStatus($"COMET Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]");

            this.ConfigureServiceLocator(container);

            this.AddPluginCatalogs(catalog);

            this.UpdateBootstrapperStatus("Composing parts");
            container.ComposeParts();

            this.UpdateBootstrapperStatus("Initializing modules");
            this.InitializeModules(container);

            this.OnComposed(container);
        }

        /// <summary>
        /// Add the main application catalogues
        /// </summary>
        /// <param name="catalog">The composition  <see cref="AggregateCatalog"/></param>
        private void AddExecutingAssemblyCatalog(AggregateCatalog catalog)
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
        /// <param name="container">The <see cref="CompositionContainer"/></param>
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
            this.UpdateBootstrapperStatus("Loading COMET Plugins");

            var pluginLoader = new PluginLoader<T>();

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                try
                {
                    catalog.Catalogs.Add(directoryCatalog);
                    this.UpdateBootstrapperStatus($"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");
                }
                catch (ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    throw new CometReflectionTypeLoadException(reflectionTypeLoadException);
                }
            }

            this.UpdateBootstrapperStatus($"{pluginLoader.DirectoryCatalogues.Count} COMET Plugins Loaded");
        }

        /// <summary>
        /// Calls initialize on all <see cref="IModule"/>s in the container/>
        /// </summary>
        /// <param name="container">The <see cref="CompositionContainer"/></param>
        private void InitializeModules(CompositionContainer container)
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
            this.status = message;
            this.logger.Debug(this.status);
        }

        /// <summary>
        /// Is called after the container has been composed
        /// </summary>
        /// <param name="container">The <see cref="CompositionContainer"/></param>
        protected abstract void OnComposed(CompositionContainer container);

        /// <summary>
        /// Optional override to add custom catalogs in concrete class
        /// </summary>
        /// <param name="catalog">The <see cref="AggregateCatalog"/></param>
        protected virtual void AddCustomCatalogs(AggregateCatalog catalog)
        {
        }
    }
}
