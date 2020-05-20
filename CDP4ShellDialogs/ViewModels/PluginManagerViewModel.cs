// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services.AppSettingService;

    using DevExpress.Mvvm.POCO;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="PluginManagerViewModel{T}"/> is to allow a user to inspect
    /// and interact with MEF loaded modules
    /// </summary>
    public class PluginManagerViewModel<T> : DialogViewModelBase where T : AppSettings
    { 
        /// <summary>
        /// The <see cref="ReactiveList{T}"/> of <see cref="PluginRowViewModel"/>s contained withing this view.
        /// </summary>
        private ReactiveList<PluginRowViewModel> plugins;

        /// <summary>
        /// The selected <see cref="PluginRowViewModel"/>
        /// </summary>
        private PluginRowViewModel selectedPlugin;

        /// <summary>
        /// The backing field for the <see cref="IsDirty"/> property
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerViewModel{T}"/> class.
        /// </summary>
        public PluginManagerViewModel(IAppSettingsService<T> appSettingsService)
        {
            this.AppSettingsService = appSettingsService;

            this.Plugins = new ReactiveList<PluginRowViewModel>
            {
                ChangeTrackingEnabled = true
            };
            this.Plugins.ItemChanged.Subscribe(
                x =>
            {
                x.Sender.IsRowDirty = true;
                this.IsDirty = true;
            });

            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(_ => this.ExecuteClose());

            this.SaveCommand = ReactiveCommand.Create();
            this.SaveCommand.Subscribe(_ => this.ExecuteSave());

            this.PopulateModuleList();
        }

        /// <summary>
        /// Gets the Close Command
        /// </summary>
        public ReactiveCommand<object> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the Save Command
        /// </summary>
        public ReactiveCommand<object> SaveCommand { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of <see cref="PluginRowViewModel"/>s contained withing this view.
        /// </summary>
        public ReactiveList<PluginRowViewModel> Plugins
        {
            get
            {
                return this.plugins;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.plugins, value);
            }
        }

        /// <summary>
        /// Gets or sets the IsDirty
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isDirty, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="PluginRowViewModel"/>
        /// </summary>
        public PluginRowViewModel SelectedPlugin
        {
            get
            {
                return this.selectedPlugin;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedPlugin, value);
            }
        }

        /// <summary>
        /// Gets or sets the application settings service <see cref="IAppSettingsService{T}"/>
        /// </summary>
        private IAppSettingsService<T> AppSettingsService { get; set; }

        /// <summary>
        /// Executes the Close Command
        /// </summary>
        private void ExecuteClose()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the save Command
        /// </summary>
        private void ExecuteSave()
        {
            var pluginsToProcess = this.Plugins.Where(x => x.IsRowDirty || !x.IsPluginEnabled).ToList();
            this.AppSettingsService.AppSettings.DisabledPlugins.Clear();

            foreach (var item in pluginsToProcess)
            {
                var wasAlreadyDisabled = this.AppSettingsService.AppSettings.DisabledPlugins.Any(p => p == item.ProjectGuid);
                
                if (item.IsPluginEnabled && wasAlreadyDisabled)
                {
                    this.AppSettingsService.AppSettings.DisabledPlugins.Remove(item.ProjectGuid);
                }
                else if (!item.IsPluginEnabled && !wasAlreadyDisabled)
                {
                    this.AppSettingsService.AppSettings.DisabledPlugins.Add(item.ProjectGuid);
                }
            }

            this.AppSettingsService.Save();
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Populates the list of modules
        /// </summary>
        private void PopulateModuleList()
        {
            if (this.AppSettingsService != null)
            {
                var disabledPlugins = this.AppSettingsService.AppSettings.DisabledPlugins;
                var presentPlugins = PluginUtilities.GetPluginManifests();

                foreach (var pluginSetting in presentPlugins)
                {
                    this.Plugins.Add(new PluginRowViewModel(pluginSetting, disabledPlugins.All(p => p != pluginSetting.ProjectGuid)));
                }
            }
        }
    }
}
