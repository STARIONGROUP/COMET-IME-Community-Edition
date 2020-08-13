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
    using System.IO;
    using System.Linq;
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
    using CDP4IME.Settings;
    using CDP4IME.Views;

    using CDP4UpdateServerDal;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="UpdateDownloaderInstallerViewModel"/> is the view model of the <see cref="UpdateDownloaderInstallerViewModel"/> holding it properties its properties and interaction logic
    /// </summary>
    //[DialogViewModelExport(nameof(UpdateDownloaderInstaller), "Plugin Installer")]
    public class UpdateDownloaderInstallerViewModel : ReactiveObject, IPluginInstallerViewModel, IDialogViewModel
    {
        /// <summary>
        /// Holds the base Update Server Address
        /// </summary>
        private const string ImeCommunityEditionUpdateServerBaseAddress = "https://store.cdp4.org";
        
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
        /// Backing field for the <see cref="WindowTitle"/> property
        /// </summary>
        private string windowTitle;

        /// <summary>
        /// Backing field for <see cref="IsInDownloadMode"/> property
        /// </summary>
        private bool isInDownloadMode;

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
        public IPluginInstallerBehavior Behavior { get; set; }

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
        /// Gets or sets the title
        /// </summary>
        public string WindowTitle
        {
            get => this.windowTitle;
            set => this.RaiseAndSetIfChanged(ref this.windowTitle, value);
        }

        /// <summary>
        /// Gets the cancellation token to use whenever the installations processes goes wrong or the process is canceled 
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the Command that will cancel the update operation if any and close the view
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
        /// Gets the Command that will check the API for available update
        /// </summary>
        public ReactiveCommand<object> CheckApiCommand { get; private set; }

        /// <summary>
        /// Gets the Command to select/unselect all available updates
        /// </summary>
        public ReactiveCommand<object> SelectAllUpdateCheckBoxCommand { get; private set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of type <see cref="PluginRowViewModel"/> that holds the properties for <see cref="PluginRow"/>
        /// </summary>
        public List<PluginRowViewModel> AvailablePlugins { get; } = new List<PluginRowViewModel>();

        /// <summary>
        /// Gets a <see cref="List{T}"/> of type <see cref="ImeRowViewModel"/> that holds the properties for <see cref="ImeRow"/>
        /// </summary>
        public List<ImeRowViewModel> AvailableIme { get; } = new List<ImeRowViewModel>();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <code>(FileInfo cdp4ckFile, Manifest manifest)</code>
        /// of the updatable plugins
        /// </summary>
        public IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> UpdatablePlugins { get; private set; }

        /// <summary>
        /// Gets an <see cref="Dictionary{T,T}"/> that holds the things that the user can choose to download.
        /// </summary>
        public Dictionary<string, string> DownloadableThings { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstallerViewModel"/> class
        /// </summary>
        //[ImportingConstructor]
        public UpdateDownloaderInstallerViewModel()
        {
            this.appSettingService = ServiceLocator.Current.GetInstance<IAppSettingsService<ImeAppSettings>>();
            this.isInDownloadMode = true;
            this.WindowTitle = "Update Downloader";
            this.InitializeCommand();
            this.InitializeDownloadRelativeCommand();
            Task.Run(async () => await this.CheckApiForUpdate()).ContinueWith(_ => this.UpdateDownloaderProperties());
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

            this.CheckApiCommand = ReactiveCommand.Create();
            this.CheckApiCommand.Subscribe(async _ => await this.CheckApiForUpdate());
        }

        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/>
        /// </summary>
        private void InitializeCommand()
        {
            this.isInstallationOrDownloadInProgressObservable = this.WhenAny(x => x.isInstallationOrDownloadInProgress, b => !b.Value);
        }

        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/> relative to the plugin installation process
        /// </summary>
        private void InitializeInstallationRelativeCommand()
        {
            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.CancelCommandExecute());
            
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

            this.SelectAllUpdateCheckBoxCommand = ReactiveCommand.Create(isInstallationOrDownloadInProgressObservable );
            this.SelectAllUpdateCheckBoxCommand.Subscribe(_ => this.SelectDeselectAllPluginCommandExecute());
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
                this.CancellationTokenSource.Token.Register(async () => await this.CancelInstallationsCommandExecute());

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
        private async Task CancelInstallationsCommandExecute()
        {
            await Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelected).Select(plugin => Task.Run(plugin.HandlingCancelation)).ToArray());
        }

        /// <summary>
        /// Execute the <see cref="CheckApiCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task CheckApiForUpdate()
        {
            this.IsCheckingApi = true;
            var serverAddress = this.GetUpdateServerBaseAddress();
            
            if (Uri.TryCreate(serverAddress, UriKind.Absolute, out var url))
            {
                var assemblyInfo = ServiceLocator.Current.GetInstance<IAssemblyInformationService>();

                try
                {
                    var client = new UpdateServerClient(url);
                    var availableUpdates = await client.CheckForUpdate(PluginUtilities.GetPluginManifests().ToList(), assemblyInfo.GetVersion(), assemblyInfo.GetProcessorArchitecture());
                    await Task.Delay(10000);
                    this.DownloadableThings = this.SortDownloadedThingsWithAlreadyDownloadedOnes(availableUpdates);
                }
                catch (Exception exception)
                {
                    this.logger.Debug($"An exception has occured: {exception}");
                }
            }
            else
            {
                //logger.Info($"The value {this.selectedUpdateServer} is not a valid URI");
            }

            this.IsCheckingApi = false;
        }

        /// <summary>
        /// Remove from the available updates the ones that have beeen downloaded already
        /// </summary>
        /// <param name="availableUpdates">The updates found on the Update Server</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/></returns>
        private Dictionary<string, string> SortDownloadedThingsWithAlreadyDownloadedOnes(Dictionary<string, string> availableUpdates)
        {
            foreach (var (_, manifest) in PluginUtilities.GetDownloadedInstallablePluginUpdate())
            {
                var update = availableUpdates.FirstOrDefault(u => u.Key == manifest.Name && u.Value == manifest.Version);

                if (!string.IsNullOrWhiteSpace(update.Key) && availableUpdates.TryGetValue(update.Key, out _))
                {
                    availableUpdates.Remove(update.Key);
                }
            }

            if (availableUpdates.TryGetValue(UpdateServerClient.ImeKey, out var newImeVersion))
            {
                if (true)
                {
                    
                }
            }

            return availableUpdates;

        }

        private string GetUpdateServerBaseAddress()
        {
            if (!this.appSettingService.AppSettings.UpdateServerAddresses.Any())
            {
                this.appSettingService.AppSettings.UpdateServerAddresses.Add(ImeCommunityEditionUpdateServerBaseAddress);
#if DEBUG
                this.appSettingService.AppSettings.UpdateServerAddresses.Add("https://localhost:5001");
#endif
                this.appSettingService.Save();
            }

            return this.appSettingService.AppSettings.UpdateServerAddresses.Last();
        }

        /// <summary>
        /// Execute the <see cref="DownloadCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task DownloadCommandExecute()
        {
            
        }

        /// <summary>
        /// Execute the SelectDeselectAllPluginCommand selecting or deselecting all plugin row
        /// </summary>
        private void SelectDeselectAllPluginCommandExecute()
        {
            var shouldAllBeSelected = !this.AvailablePlugins.All(p => p.IsSelected);
            
            foreach (var plugin in this.AvailablePlugins)
            {
                plugin.IsSelected = shouldAllBeSelected;
            }
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
            this.WindowTitle = "Update Installer";

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
                foreach (var pluginRow in this.DownloadableThings.Select(thing => new PluginRowViewModel(thing)))
                {
                    this.AvailablePlugins.Add(pluginRow);
                }
            }
            else
            {
                ServiceLocator.Current.GetInstance<IDialogNavigationService>().NavigateModal(new OkDialogViewModel("Congratulations", "You are up to date"));
                this.Behavior?.Close();
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
        }
    }
}
