// ------------------------------------------------------------------------------------------------
// <copyright file="ManageConfigurationsDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Composition.Navigation;
    using CDP4Composition.PluginSettingService;
    using DialogResult;
    using ReactiveUI;
    using Settings;

    /// <summary>
    /// ViewModel for the dialog to manage matrix configurations
    /// </summary>
    public class ManageConfigurationsDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The dialog navigation service.
        /// </summary>
        private IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The plugin settings service.
        /// </summary>
        private IPluginSettingsService pluginSettingService;

        /// <summary>
        /// Backing field for <see cref="SelectedConfiguration"/>
        /// </summary>
        private SavedConfiguration selectedConfiguration;

        /// <summary>
        /// Backing field for <see cref="SavedConfigurations"/>
        /// </summary>
        private ReactiveList<SavedConfiguration> savedConfigurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConfigurationsDialogViewModel"/> class.
        /// </summary>
        /// <param name="dialogNavigationService">An instance of <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingService">An instance of <see cref="IPluginSettingsService"/>.</param>
        public ManageConfigurationsDialogViewModel(IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingService)
        {
            // reset the loading indicator
            this.IsBusy = false;

            this.dialogNavigationService = dialogNavigationService;
            this.pluginSettingService = pluginSettingService;

            var settings = pluginSettingService.Read<RelationshipMatrixPluginSettings>();

            this.SavedConfigurations = new ReactiveList<SavedConfiguration>(settings.SavedConfigurations);
            this.SavedConfigurations.ChangeTrackingEnabled = true;

            this.OkCommand = ReactiveCommand.CreateAsyncTask(x => this.ExecuteOk(), RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            var canDelete = this.WhenAny(vm => vm.SelectedConfiguration, sc => sc.Value != null);
            this.DeleteSelectedCommand = ReactiveCommand.Create(canDelete);
            this.DeleteSelectedCommand.Subscribe(_ => this.ExecuteDeleteSelected());
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Delete selected command
        /// </summary>
        public ReactiveCommand<object> DeleteSelectedCommand { get; private set; }

        /// <summary>
        /// Gets or sets the list of saved configurations.
        /// </summary>
        public ReactiveList<SavedConfiguration> SavedConfigurations
        {
            get { return this.savedConfigurations; }
            set { this.RaiseAndSetIfChanged(ref this.savedConfigurations, value); }
        }

        /// <summary>
        /// Gets or sets the selected configuration
        /// </summary>
        public SavedConfiguration SelectedConfiguration
        {
            get { return this.selectedConfiguration; }
            set { this.RaiseAndSetIfChanged(ref this.selectedConfiguration, value); }
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            var settings = this.pluginSettingService.Read<RelationshipMatrixPluginSettings>();

            settings.SavedConfigurations = this.SavedConfigurations.ToList();

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
            this.SelectedConfiguration = null;
        }
    }
}