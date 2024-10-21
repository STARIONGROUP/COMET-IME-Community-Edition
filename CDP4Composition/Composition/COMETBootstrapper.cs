// --------------------------------------------------------------------------------------------------------------------
// <copyright file="COMETBootstrapper.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using CDP4Composition.Exceptions;
    using CDP4Composition.Modularity;
    using CDP4Composition.Services;
    using CDP4Composition.Services.AppSettingService;

    using CommonServiceLocator;

    using DynamicData;

    using NLog;

    using ReactiveUI;

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
        /// The [INJECTED] <see cref="IMessageBoxService"/>
        /// </summary>
        private IMessageBoxService messageBoxService;

        /// <summary>
        /// Runs the bootstrapper
        /// </summary>
        public void Run()
        {
            this.UpdateBootstrapperStatus("Configuring catalogs");

            var catalog = new AggregateCatalog();
            var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);

            var sw = Stopwatch.StartNew();
            this.UpdateBootstrapperStatus("Loading CDP4-COMET Catalogs");
            this.ShowStatusMessage("Loading CDP4-COMET IME Components...");

            this.AddExecutingAssemblyCatalog(catalog);
            this.AddCustomCatalogs(catalog);

            this.UpdateBootstrapperStatus($"CDP4-COMET Catalogs loaded in: {sw.ElapsedMilliseconds} [ms]");

            this.messageBoxService = new MessageBoxService();

            this.ShowStatusMessage("Loading CDP4-COMET IME Plugins...");

            this.ConfigureServiceLocator(container);

            this.AddPluginCatalogs(catalog);

            this.UpdateBootstrapperStatus("Composing parts");
            this.ResetStatusProgress();
            this.ShowStatusMessage("Initializing CDP4-COMET IME Plugins...");

            container.ComposeParts();

            this.UpdateBootstrapperStatus("Initializing modules");
            this.InitializeModules(container);

            RxApp.DefaultExceptionHandler = new RxAppObservableExceptionHandler();

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

            var excludeAssemblies = new [] { "CDP4-COMET.dll", "System.Data.SqlClient.dll", "CDP4-COMETAddinCE.comhost.dll" };

            var files = Directory.GetFiles(currentAssemblyPath, "CDP4*.dll");

            foreach (var excludeAssembly in excludeAssemblies)
            {
                files = files
                    .Where(f => !f.EndsWith(excludeAssembly)).ToArray();
            }

            // Create a new catalog with the remaining assemblies
            foreach (var file in files)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(file));
            }
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
            return;

            this.UpdateBootstrapperStatus("Loading CDP4-COMET Plugins");

            var pluginLoader = new PluginLoader<T>();
            this.ResetStatusProgress();
            var counter = 1;

            foreach (var directoryCatalog in pluginLoader.DirectoryCatalogues)
            {
                try
                {
                    this.SetStatusProgress(counter, pluginLoader.DirectoryCatalogues.Count);
                    this.ShowStatusMessage($"Loading Plugin: {Path.GetFileName(directoryCatalog.FullPath)}");

                    catalog.Catalogs.Add(directoryCatalog);
                    this.UpdateBootstrapperStatus($"DirectoryCatalogue {directoryCatalog.FullPath} Loaded");

                    counter++;
                }
                catch (ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    var newError = new CometReflectionTypeLoadException(reflectionTypeLoadException);
                    var messageText = $"Plugin {directoryCatalog} had a loading error:\n\n{newError.Message}\n\nContinue loading plugins?";

                    if (this.messageBoxService.ShowAlwaysOnTop(messageText, "Continue loading plugins?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                    {
                        throw newError;
                    }
                    else
                    {
                        this.logger.Error(newError);
                    }
                }
            }

            this.UpdateBootstrapperStatus($"{pluginLoader.DirectoryCatalogues.Count} CDP4-COMET Plugins Loaded");
            this.ShowStatusMessage($"{pluginLoader.DirectoryCatalogues.Count} CDP4-COMET Plugins Loaded");
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
        /// Show a status message to user, more display friendly than the logs. To be used for splash screens etc.
        /// </summary>
        /// <param name="message">The message to show</param>
        protected virtual void ShowStatusMessage(string message)
        {
            // do nothing in base class
        }

        /// <summary>
        /// Resets any progress bars to a determinate or indeterminate state and applies max value
        /// </summary>
        protected virtual void ResetStatusProgress()
        {
            // do nothing in base class
        }

        /// <summary>
        /// Sets the progress bar indicator to value
        /// </summary>
        /// <param name="value">The value to set the progress bar to</param>
        /// <param name="maxValue">The max value of the progress bar</param>
        protected virtual void SetStatusProgress(int value, int maxValue = 100)
        {
            // do nothing in base class
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
