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

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    using CDP4Composition.Behaviors;
    using CDP4Composition.Modularity;
    using CDP4Composition.Views;

    using DevExpress.XtraPrinting.Native;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="PluginInstallerViewModel"/> is the view model of the <see cref="PluginInstallerViewModel"/> holding it properties its properties and interaction logic
    /// </summary>
    public class PluginInstallerViewModel : ReactiveObject, IPluginInstallerViewModel
    {
        /// <summary>
        ///  Assert whether any plugin are selected
        /// </summary>
        private IObservable<bool> areThereAnyPluginSelected;

        /// <summary>
        /// Observable of the field <see cref="thereIsNoInstallationInProgress"/>
        /// </summary>
        private IObservable<bool> thereIsNoInstallationInProgressObservable;

        /// <summary>
        /// Backing field for the <see cref="ThereIsNoInstallationInProgress"/> property
        /// </summary>
        private bool thereIsNoInstallationInProgress = true;

        /// <summary>
        /// Gets or sets an assert whether ther is any installation in progress
        /// </summary>
        public bool ThereIsNoInstallationInProgress
        {
            get => this.thereIsNoInstallationInProgress;
            set => this.RaiseAndSetIfChanged(ref this.thereIsNoInstallationInProgress, value);
        }
        
        /// <summary>
        /// The attached Behavior
        /// </summary>
        public IPluginUpdateInstallerBehavior Behavior { get; set; }
        
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
        /// Gets the Command to select/unselect all available updates
        /// </summary>
        public ReactiveCommand<object> SelectAllUpdateCheckBoxCommand { get; private set; }
        
        /// <summary>
        /// Gets a <see cref="List{T}"/> of type <see cref="PluginRowViewModel"/> that holds the properties for <see cref="PluginRow"/>
        /// </summary>
        public List<PluginRowViewModel> AvailablePlugins { get; } = new List<PluginRowViewModel>();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <code>(FileInfo cdp4ckFile, Manifest manifest)</code>
        /// of the updatable plugins
        /// </summary>
        public IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> UpdatablePlugins { get; }

        /// <summary>
        /// Instanciate a new <see cref="PluginInstallerViewModel"/>
        /// </summary>
        /// <param name="updatablePlugins">the <see cref="IEnumerable{T}"/> of updatable plugins</param>
        public PluginInstallerViewModel(IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins)
        {
            this.UpdatablePlugins = updatablePlugins;
            this.UpdateProperties();
            this.InitializeCommand();
        }
        
        /// <summary>
        /// Initialize the <see cref="IReactiveCommand"/>
        /// </summary>
        private void InitializeCommand()
        {
            this.thereIsNoInstallationInProgressObservable = this.WhenAnyValue(x => x.thereIsNoInstallationInProgress);
            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.CancelCommandExecute());

            this.areThereAnyPluginSelected = this.AvailablePlugins.WhenAnyValue(p => p).SelectMany(p => p.Select(x => x.IsSelectedForInstallation));
            this.areThereAnyPluginSelected.Subscribe(_ => System.Windows.MessageBox.Show("It's on fire"));

            this.InstallCommand = ReactiveCommand.Create(this.thereIsNoInstallationInProgressObservable);
            
            var currentDispatcher = Dispatcher.CurrentDispatcher;
            
            this.InstallCommand.Subscribe(
                async _ => await this.InstallCommandExecute().ContinueWith(
                t =>
                { 
                    if (!t.IsCanceled && !t.IsFaulted && this.AvailablePlugins.Any(p => p.IsSelectedForInstallation))
                    {
                        currentDispatcher.InvokeAsync(this.Behavior.Close, DispatcherPriority.Background);
                    }
                }));

            this.SelectAllUpdateCheckBoxCommand = ReactiveCommand.Create(this.thereIsNoInstallationInProgressObservable);
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
                this.ThereIsNoInstallationInProgress = false;

                if (!this.AvailablePlugins.Any(p => p.IsSelectedForInstallation))
                {
                    return;
                }

                this.CancellationTokenSource = new CancellationTokenSource();
                this.CancellationTokenSource.Token.Register(async () => await this.CancelInstallationsCommandExecute());

                await Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelectedForInstallation).Select(plugin => Task.Run(plugin.Install, this.CancellationTokenSource.Token)).ToArray());
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
                this.ThereIsNoInstallationInProgress = true;
                this.CancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// Execute the Install command that will run the installation of the plugins
        /// </summary>
        private async Task CancelInstallationsCommandExecute()
        {
            await Task.WhenAll(this.AvailablePlugins.Where(p => p.IsSelectedForInstallation).Select(plugin => Task.Run(plugin.HandlingCancelation, this.CancellationTokenSource.Token)).ToArray());
        }
        
        /// <summary>
        /// Execute the SelectDeselectAllPluginCommand selecting or deselecting all plugin row
        /// </summary>
        private void SelectDeselectAllPluginCommandExecute()
        {
            var shouldAllBeSelected = !this.AvailablePlugins.All(p => p.IsSelectedForInstallation);
            
            foreach (var plugin in this.AvailablePlugins)
            {
                plugin.IsSelectedForInstallation = shouldAllBeSelected;
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
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            foreach (var plugin in this.UpdatablePlugins)
            {
                var pluginRow = new PluginRowViewModel(plugin);
                this.AvailablePlugins.Add(pluginRow);
            }
        }
    }
}
