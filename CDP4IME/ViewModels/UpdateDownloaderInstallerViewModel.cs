// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-Plugin Installer Community Edition. 
//    The CDP4-Plugin Installer Community Edition is the RHEA Plugin Installer for the CDP4-IME Community Edition.
//
//    The CDP4-Plugin Installer Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-Plugin Installer Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4CommonView.ViewModels;
    using CDP4CommonView.Views;

    using CDP4Composition.Attributes;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;

    using CDP4IME.Behaviors;
    using CDP4IME.Modularity;
    using CDP4IME.Services;
    using CDP4IME.Settings;
    using CDP4IME.Views;

    using CDP4UpdateServerDal;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="UpdateDownloaderInstallerViewModel"/> is the view model of the <see cref="UpdateDownloaderInstallerViewModel"/> holding it properties its properties and interaction logic
    /// </summary>
    [DialogViewModelExport(nameof(UpdateDownloaderInstaller), "Plugin Installer")]
    public class UpdateDownloaderInstallerViewModel : ReactiveObject, IPluginInstallerViewModel, IDialogViewModel
    {
        /// <summary>
        /// Holds the Download button text
        /// </summary>
        private const string DownloadText = "Download";
        
        /// <summary>
        /// Holds the doownload button text when restart option is cheked
        /// </summary>
        private const string DownloadAndRestartText = "Download and Restart";

        /// <summary>
        /// The NLog logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="IsInstallationOrDownloadInProgress"/> property
        /// </summary>
        private bool isInstallationOrDownloadInProgress;

        /// <summary>
        /// The <see cref="IObservable{T}"/> of bool that can tell whether a download or an installation is already in progress
        /// </summary>
        private IObservable<bool> isInstallationOrDownloadInProgressObservable;

        /// <summary>
        /// Holds the app setting service instance
        /// </summary>
        private IAppSettingsService<ImeAppSettings> appSettingService;

        /// <summary>
        /// Backing field for the <see cref="IsCheckingApi"/> property
        /// </summary>
        private bool isCheckingApi;

        /// <summary>
        /// Backing field for <see cref="IsInDownloadMode"/> property
        /// </summary>
        private bool isInDownloadMode;

        /// <summary>
        /// Backing field for the <see cref="DownloadButtonText"/>
        /// </summary>
        private string downloadButtonText = DownloadText;

        /// <summary>
        /// Gets or sets the text of the Download button
        /// </summary>
        public string DownloadButtonText
        {
            get => this.downloadButtonText;
            set => this.RaiseAndSetIfChanged(ref this.downloadButtonText, value);
        }
        
        /// <summary>
        /// Backing field for the <see cref="HasToRestartClientAfterDownload"/>
        /// </summary>
        private bool hasToRestartClientAfterDownload;

        /// <summary>
        /// Gets or sets an assert whether the IME should restart itself in order to run the installation right away
        /// </summary>
        public bool HasToRestartClientAfterDownload
        {
            get => this.hasToRestartClientAfterDownload;
            set => this.RaiseAndSetIfChanged(ref this.hasToRestartClientAfterDownload, value);
        }
        
        /// <summary>
        /// Gets or Sets the <see cref="IDialogResult"/>
        /// </summary>
        public IDialogResult DialogResult { get; set; }

        /// <summary>
        /// Gets or sets whether this dialog is busy
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Gets or sets the loading message
        /// </summary>
        public string LoadingMessage { get; set; }

        /// <summary>
        /// Gets or sets an assert whether there is any installation in progress
        /// </summary>
        public bool IsInstallationOrDownloadInProgress
        {
            get => this.isInstallationOrDownloadInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInstallationOrDownloadInProgress, value);
        }

        /// <summary>
        /// The attached Behavior
        /// </summary>
        public IUpdateDownloaderInstallerBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets an assert whether this is checking API 
        /// </summary>
        public bool IsCheckingApi
        {
            get => this.isCheckingApi;
            set => this.RaiseAndSetIfChanged(ref this.isCheckingApi, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="isInDownloadMode"/>
        /// </summary>
        public bool IsInDownloadMode
        {
            get => this.isInDownloadMode;
            set => this.RaiseAndSetIfChanged(ref this.isInDownloadMode, value);
        }

        /// <summary>
        /// Gets the cancellation token to use whenever the installations processes goes wrong or the process is canceled 
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the Command that will cancel the operations close the view
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Command that will install all plugin selected for installation
        /// </summary>
        public ReactiveCommand<object> InstallCommand { get; private set; }

        /// <summary>
        /// Gets the Command that will Download all plugin selected and ime version
        /// </summary>
        public ReactiveCommand<object> DownloadCommand { get; private set; }

        /// <summary>
        /// Gets the Command to select/unselect all available updates
        /// </summary>
        public ReactiveCommand<object> SelectAllUpdateCheckBoxCommand { get; private set; }
        
        /// <summary>
        /// Gets the Command to auto restart after selected things have done downloading
        /// </summary>
        public ReactiveCommand<object> RestartAfterDownloadCommand { get; private set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of type <see cref="PluginRowViewModel"/> that holds the properties for <see cref="PluginRow"/>
        /// </summary>
        public ReactiveList<PluginRowViewModel> AvailablePlugins { get; } = new ReactiveList<PluginRowViewModel>();

        /// <summary>
        /// Gets a <see cref="List{T}"/> of type <see cref="ImeRowViewModel"/> that holds the properties for <see cref="ImeRow"/>
        /// </summary>
        public ReactiveList<ImeRowViewModel> AvailableIme { get; } = new ReactiveList<ImeRowViewModel>();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <code>(FileInfo cdp4ckFile, Manifest manifest)</code>
        /// of the updatable plugins
        /// </summary>
        public IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> UpdatablePlugins { get; private set; }

        /// <summary>
        /// Gets <see cref="IEnumerable{T}"/> of type <code>(string ThingName, string Version)</code> containing new versions
        /// </summary>
        public IEnumerable<(string ThingName, string Version)> DownloadableThings { get; private set; } = new List<(string ThingName, string Version)>();
        
        /// <summary>
        /// Gets a instance of <see cref="IUpdateServerClient"/>
        /// </summary>
        private IUpdateServerClient UpdateServerClient
        {
            get
            {
                var client = ServiceLocator.Current.GetInstance<IUpdateServerClient>();
                client.BaseAddress = this.GetUpdateServerBaseAddress();
                return client;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstallerViewModel"/> class
        /// </summary>
        [ImportingConstructor]
        public UpdateDownloaderInstallerViewModel()
        {      
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstallerViewModel"/> class
        /// </summary>
        public UpdateDownloaderInstallerViewModel(bool shouldCheckApi)
        {
            this.appSettingService = ServiceLocator.Current.GetInstance<IAppSettingsService<ImeAppSettings>>();
            this.isInDownloadMode = true;
            this.InitializeCommand();
            this.InitializeDownloadRelativeCommand();

            if (shouldCheckApi)
            {
                this.CheckApiForUpdate();
            }
            
            this.WhenAny(x => x.IsCheckingApi, v => !v.Value)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateDownloaderProperties());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstallerViewModel"/> class
        /// </summary>
        /// <param name="updatablePlugins">the <see cref="IEnumerable{T}"/> of updatable plugins</param>
        public UpdateDownloaderInstallerViewModel(IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins)
        {
            this.UpdatablePlugins = updatablePlugins;
            this.UpdateInstallerProperties();
            this.InitializeCommand();
            this.InitializeInstallationRelativeCommand();
        }

        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/> relative to the download process
        /// </summary>
        private void InitializeDownloadRelativeCommand()
        {
            this.DownloadCommand = ReactiveCommand.Create(this.isInstallationOrDownloadInProgressObservable);
            this.DownloadCommand.Subscribe(async _ => await this.DownloadCommandExecute());
            this.RestartAfterDownloadCommand = ReactiveCommand.Create();
            this.RestartAfterDownloadCommand.Subscribe(_ => this.RestartAfterDownloadCommandExecute());
        }

        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/>
        /// </summary>
        private void InitializeCommand()
        {
            this.isInstallationOrDownloadInProgressObservable = this.WhenAny(x => x.isInstallationOrDownloadInProgress, b => !b.Value);
            this.SelectAllUpdateCheckBoxCommand = ReactiveCommand.Create(this.isInstallationOrDownloadInProgressObservable);
            this.SelectAllUpdateCheckBoxCommand.Subscribe(_ => this.SelectDeselectAllPluginCommandExecute());
            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.CancelCommandExecute());
        }

        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/> relative to the plugin installation process
        /// </summary>
        private void InitializeInstallationRelativeCommand()
        {
            this.InstallCommand = ReactiveCommand.Create(this.isInstallationOrDownloadInProgressObservable);

            var currentDispatcher = Dispatcher.CurrentDispatcher;
            
            this.InstallCommand.Subscribe(
                async _ => await this.InstallCommandExecute().ContinueWith(
                t =>
                { 
                    if (!t.IsCanceled && !t.IsFaulted && this.AvailablePlugins.Any(p => p.IsSelected))
                    {
                        currentDispatcher.InvokeAsync(this.Behavior.Close, DispatcherPriority.Background);
                    }
                }));
        }

        /// <summary>
        /// Execute the Install command that will run the installation of the plugins
        /// </summary>
        /// <returns>A new <see cref="Task"/></returns>
        private async Task InstallCommandExecute()
        {
            try
            {
                this.IsInstallationOrDownloadInProgress = true;

                if (!this.AvailablePlugins.Any(p => p.IsSelected))
                {
                    return;
                }

                this.CancellationTokenSource = new CancellationTokenSource();
                this.CancellationTokenSource.Token.Register(async () => await this.CancelInstallationsExecute());

                await Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelected).Select(plugin => Task.Run(plugin.Install, this.CancellationTokenSource.Token)).ToArray());
                await Task.Delay(1000);
            }
            catch (Exception exception)
            {
                if (!(exception is TaskCanceledException))
                {
                    this.CancellationTokenSource.Cancel();
                }

                LogManager.GetCurrentClassLogger().Error($"{exception} has occured while trying to install new plugin versions");
            }
            finally
            {
                this.IsInstallationOrDownloadInProgress = false;
                this.CancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Execute the Install command that will run the installation of the plugins
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CancelInstallationsExecute()
        {
            await Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelected).Select(plugin => Task.Run(plugin.HandlingCancelationOfInstallation)).ToArray());
        }
        
        /// <summary>
        /// Execute the <see cref="DownloadCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task DownloadCommandExecute()
        {
            try
            {
                this.IsInstallationOrDownloadInProgress = true;

                if (!this.AvailablePlugins.Any(p => p.IsSelected) && !this.AvailableIme.Any(i => i.IsSelected))
                {
                    return;
                }

                this.CancellationTokenSource = new CancellationTokenSource();
                this.CancellationTokenSource.Token.Register(async () => await this.CancelDownloadsExecute());

                await Task.WhenAll(
                    Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelected).Select(plugin => Task.Run(() => plugin.Download(this.UpdateServerClient), this.CancellationTokenSource.Token)).ToArray()),
                    Task.WhenAll(this.AvailableIme.Where(p => p.IsSelected).Select(ime => Task.Run(() => ime.Download(this.UpdateServerClient), this.CancellationTokenSource.Token)).ToArray()));

                await Task.Delay(1000);

                this.AvailableIme.RemoveAll(this.AvailableIme.Where(i => i.IsSelected).ToList());
                this.AvailablePlugins.RemoveAll(this.AvailablePlugins.Where(i => i.IsSelected).ToList());

                if (this.HasToRestartClientAfterDownload)
                {
                    ServiceLocator.Current.GetInstance<IProcessRunnerService>().Restart();
                }
            }
            catch (Exception exception)
            {
                if (!(exception is TaskCanceledException))
                {
                    this.CancellationTokenSource.Cancel();
                }

                this.logger.Error($"{exception} has occured while trying to download new plugin versions");
            }
            finally
            {
                this.IsInstallationOrDownloadInProgress = false;
                this.CancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Execute the Install command that will run the installation of the plugins
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CancelDownloadsExecute()
        {
            await Task.WhenAll(
                Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelected).Select(plugin => Task.Run(plugin.HandlingCancelationOfDownload)).ToArray()),
                Task.WhenAll(this.AvailableIme.Where(p => p.IsSelected).Select(ime => Task.Run(ime.HandlingCancelation)).ToArray()));
        }

        /// <summary>
        /// Checks the Update Server Api for updates
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task CheckApiForUpdate()
        {
            this.IsCheckingApi = true;
            var assemblyInfo = ServiceLocator.Current.GetInstance<IAssemblyInformationService>();

            try
            {
                var availableUpdates = await this.UpdateServerClient.CheckForUpdate(PluginUtilities.GetPluginManifests().ToList(), assemblyInfo.GetVersion(), assemblyInfo.GetProcessorArchitecture());
                this.DownloadableThings = this.SortDownloadedThingsWithAlreadyDownloadedOnes(availableUpdates.ToList());
            }
            catch (HttpRequestException httpException)
            {
                ServiceLocator.Current.GetInstance<IDialogNavigationService>().NavigateModal(new OkDialogViewModel("Error", httpException.GetBaseException().Message));
            }
            catch (Exception exception)
            {
                this.logger.Debug($"An exception has occured: {exception}");
            }
            finally
            {
                this.IsCheckingApi = false;
            }
        }

        /// <summary>
        /// Remove from the available updates the ones that have beeen downloaded already
        /// </summary>
        /// <param name="availableUpdates">The updates found on the Update Server</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of type <code>(string ThingName, string Version)</code> containing new versions</returns>
        private IEnumerable<(string ThingName, string Version)> SortDownloadedThingsWithAlreadyDownloadedOnes(List<(string ThingName, string Version)> availableUpdates)
        {
            foreach (var (_, manifest) in PluginUtilities.GetDownloadedInstallablePluginUpdate())
            {
                var (thingName, version) = availableUpdates.FirstOrDefault(u => u.ThingName == manifest.Name && u.Version == manifest.Version);

                if (!string.IsNullOrWhiteSpace(thingName) && !string.IsNullOrWhiteSpace(version))
                {
                    availableUpdates.Remove((thingName, version));
                }
            }

            var downloadableIme = availableUpdates.FirstOrDefault(v => v.ThingName == CDP4UpdateServerDal.UpdateServerClient.ImeKey);
            var downloadDirectory = ServiceLocator.Current.GetInstance<IUpdateFileSystemService>().ImeDownloadPath;
            var installers = downloadDirectory.Exists ? downloadDirectory.EnumerateFiles().ToArray() : null;

            if (downloadableIme == default || installers?.Any() != true)
            {
                return availableUpdates;
            }

            var lastVersion = installers.OrderBy(f => f.Name).LastOrDefault();

            if (lastVersion?.Name.Contains(downloadableIme.Version) == true)
            {
                availableUpdates.Remove(downloadableIme);
            }

            return availableUpdates;
        }

        /// <summary>
        /// Gets the Update Server base Address
        /// </summary>
        /// <returns>The address</returns>
        private Uri GetUpdateServerBaseAddress()
        {
            if (!this.appSettingService.AppSettings.UpdateServerAddresses.Any())
            {
                this.appSettingService.AppSettings.UpdateServerAddresses.Add(CDP4UpdateServerDal.UpdateServerClient.ImeCommunityEditionUpdateServerBaseAddress);
#if DEBUG
                this.appSettingService.AppSettings.UpdateServerAddresses.Add("https://localhost:5001");
#endif
                this.appSettingService.Save();
            }

            return new Uri(this.appSettingService.AppSettings.UpdateServerAddresses.Last(), UriKind.Absolute);
        }
        
        /// <summary>
        /// Execute the SelectDeselectAllPluginCommand selecting or deselecting all plugin row
        /// </summary>
        private void SelectDeselectAllPluginCommandExecute()
        {
            var shouldAllBeSelected = !this.AvailablePlugins.All(p => p.IsSelected);
            
            foreach (var pluginRow in this.AvailablePlugins)
            {
                pluginRow.IsSelected = shouldAllBeSelected;
            }

            foreach (var imeRow in this.AvailableIme)
            {
                imeRow.IsSelected = shouldAllBeSelected;
            }
        }

        /// <summary>
        /// Executes the <see cref="RestartAfterDownloadCommand"/>
        /// </summary>
        private void RestartAfterDownloadCommandExecute()
        {
            this.HasToRestartClientAfterDownload = !this.hasToRestartClientAfterDownload;
            this.DownloadButtonText = this.hasToRestartClientAfterDownload ? DownloadAndRestartText : DownloadText;
        }

        /// <summary>
        /// Execute the cancel command
        /// </summary>
        private void CancelCommandExecute()
        {
            this.CancellationTokenSource?.Cancel();
            this.Behavior.Close();
        }

        /// <summary>
        /// Update this view model properties when it is in install mode
        /// </summary>
        private void UpdateInstallerProperties()
        {
            foreach (var plugin in this.UpdatablePlugins)
            {
                var pluginRow = new PluginRowViewModel(plugin);
                this.AvailablePlugins.Add(pluginRow);
            }
        }

        /// <summary>
        /// Update this view model properties when it is in download mode
        /// </summary>
        private void UpdateDownloaderProperties()
        {
            if (this.DownloadableThings.Any())
            {
                foreach (var thing in this.DownloadableThings)
                {
                    if (thing.ThingName == CDP4UpdateServerDal.UpdateServerClient.ImeKey)
                    {
                        this.AvailableIme.Add(new ImeRowViewModel(thing.Version));
                    }
                    else
                    {
                        this.AvailablePlugins.Add(new PluginRowViewModel(thing));
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of the <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.DownloadCommand?.Dispose();
            this.InstallCommand?.Dispose();
            this.CancelCommand?.Dispose();
            this.RestartAfterDownloadCommand?.Dispose();
            this.SelectAllUpdateCheckBoxCommand?.Dispose();
        }
    }
}
