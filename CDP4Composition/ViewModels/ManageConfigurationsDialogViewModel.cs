// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageConfigurationDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Composition.Navigation;
    using CDP4Composition.Mvvm;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.PluginSettingService;
    using CDP4Composition.ViewModels.DialogResult;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the dialog to manage matrix configurations
    /// </summary>
    /// <typeparam name="TPluginSettings">A type of <see cref="PluginSettings"/></typeparam>
    public class ManageConfigurationsDialogViewModel<TPluginSettings> : DialogViewModelBase where TPluginSettings : PluginSettings
    {
        /// <summary>
        /// The plugin settings service.
        /// </summary>
        private readonly IPluginSettingsService pluginSettingService;

        /// <summary>
        /// Backing field for <see cref="SelectedConfiguration"/>
        /// </summary>
        private IPluginSavedConfiguration selectedConfiguration;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConfigurationsDialogViewModel"/> class.
        /// </summary>
        /// <param name="pluginSettingService">An instance of <see cref="IPluginSettingsService"/>.</param>
        public ManageConfigurationsDialogViewModel(IPluginSettingsService pluginSettingService)
        {
            // reset the loading indicator
            this.IsBusy = false;

            this.pluginSettingService = pluginSettingService;

            var settings = pluginSettingService.Read<TPluginSettings>();

            this.SavedConfigurations = new ReactiveList<IPluginSavedConfiguration>(settings.SavedConfigurations);

            this.OkCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteOk, RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);

            var canDelete = this.WhenAny(vm => vm.SelectedConfiguration, sc => sc.Value != null);
            this.DeleteSelectedCommand = ReactiveCommandCreator.Create(this.ExecuteDeleteSelected, canDelete);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Delete selected command
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; private set; }

        /// <summary>
        /// Gets or sets the list of saved configurations.
        /// </summary>
        public ReactiveList<IPluginSavedConfiguration> SavedConfigurations { get; private set; }

        /// <summary>
        /// Gets or sets the selected configuration
        /// </summary>
        public IPluginSavedConfiguration SelectedConfiguration
        {
            get => this.selectedConfiguration;
            set => this.RaiseAndSetIfChanged(ref this.selectedConfiguration, value);
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            var settings = this.pluginSettingService.Read<TPluginSettings>();

            settings.SavedConfigurations = this.SavedConfigurations;

            this.IsBusy = true;

            try
            {
                this.LoadingMessage = "Saving Cofiguration...";
                await Task.Run(() => this.pluginSettingService.Write(settings));

                this.DialogResult = new ManageConfigurationsResult(true);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new ManageConfigurationsResult(false);
        }

        /// <summary>
        /// Executes the delete selected command
        /// </summary>
        private void ExecuteDeleteSelected()
        {
            if (this.SelectedConfiguration == null)
            {
                return;
            }

            this.SavedConfigurations.Remove(this.SelectedConfiguration);
            this.SelectedConfiguration = default;
        }
    }
}