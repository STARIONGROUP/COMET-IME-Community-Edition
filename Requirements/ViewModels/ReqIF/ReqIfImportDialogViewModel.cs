// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Navigation;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;
    using CDP4Composition.ViewModels.DialogResult;

    using CDP4Dal;
    using CDP4Requirements.ReqIFDal;
    using ReactiveUI;
    using ReqIFSharp;
    using NLog;

    /// <summary>
    /// The view-model to import reqif requirement data
    /// </summary>
    public class ReqIfImportDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="SelectedIteration"/>
        /// </summary>
        private ReqIfExportIterationRowViewModel selectedIteration;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService fileDialogService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IPluginSettingsService"/> instance
        /// </summary>
        private readonly IPluginSettingsService pluginSettingsService;

        /// <summary>
        /// The <see cref="IReqIFSerializer"/>
        /// </summary>
        private readonly IReqIFDeSerializer serializer;

        /// <summary>
        /// Backing field for <see cref="CanExecuteImport"/>
        /// </summary>
        private bool canExecuteImport;

        /// <summary>
        /// Backing field for <see cref="SelectedMappingConfiguration"/>
        /// </summary>
        private ImportMappingConfiguration selectedMappingConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportDialogViewModel"/> class
        /// </summary>
        /// <param name="sessions">The list of <see cref="ISession"/> available</param>
        /// <param name="iterations">The list of <see cref="Iteration"/> available</param>
        /// <param name="fileDialogService">The <see cref="IOpenSaveFileDialogService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        /// <param name="serializer"></param>
        public ReqIfImportDialogViewModel(IEnumerable<ISession> sessions, IEnumerable<Iteration> iterations, IOpenSaveFileDialogService fileDialogService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IReqIFDeSerializer serializer)
        {
            this.Sessions = sessions?.ToList() ?? throw new ArgumentNullException(nameof(sessions));
            this.AvailableMappingConfiguration = new ReactiveList<ImportMappingConfiguration>(RequirementsModule.PluginSettings?.SavedConfigurations.Cast<ImportMappingConfiguration>());
            this.fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            this.dialogNavigationService = dialogNavigationService;
            this.pluginSettingsService = pluginSettingsService;
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            this.Iterations = new ReactiveList<ReqIfExportIterationRowViewModel>();

            foreach (var iteration in iterations ?? throw new ArgumentNullException(nameof(iterations)))
            {
                this.Iterations.Add(new ReqIfExportIterationRowViewModel(iteration));
            }

            this.AvailableMappingConfiguration = new ReactiveList<ImportMappingConfiguration>();

            this.ReloadSavedConfigurations();

            this.WhenAnyValue(vm => vm.Path).Subscribe(_ => this.UpdateCanExecuteImport());
            this.WhenAnyValue(vm => vm.SelectedIteration).Subscribe(_ => this.UpdateCanExecuteImport());
            var canOk = this.WhenAnyValue(vm => vm.CanExecuteImport);

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.ManageSavedConfiguration = ReactiveCommand.Create();
            this.ManageSavedConfiguration.Subscribe(_ => this.ExecuteManageSavedConfiguration());
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OkCommand"/> can be executed
        /// </summary>
        public bool CanExecuteImport
        {
            get => this.canExecuteImport;
            set => this.RaiseAndSetIfChanged(ref this.canExecuteImport, value);
        }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// Gets the <see cref="Iteration"/> row-representation
        /// </summary>
        public ReactiveList<ReqIfExportIterationRowViewModel> Iterations { get; private set; }

        /// <summary>
        /// Gets the available saved mapping configuration to choose from
        /// </summary>
        public ReactiveList<ImportMappingConfiguration> AvailableMappingConfiguration { get; private set; }

        /// <summary>
        /// Gets or sets the selected iteration to export
        /// </summary>
        public ReqIfExportIterationRowViewModel SelectedIteration
        {
            get => this.selectedIteration;
            set => this.RaiseAndSetIfChanged(ref this.selectedIteration, value);
        }

        /// <summary>
        /// Gets or sets the selected mapping configuration
        /// </summary>
        public ImportMappingConfiguration SelectedMappingConfiguration
        {
            get => this.selectedMappingConfiguration;
            set => this.RaiseAndSetIfChanged(ref this.selectedMappingConfiguration, value);
        }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path
        {
            get => this.path;
            set => this.RaiseAndSetIfChanged(ref this.path, value);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Browse Command
        /// </summary>
        public ReactiveCommand<object> BrowseCommand { get; private set; }

        /// <summary>
        /// Gets the Manage saved configuration Command
        /// </summary>
        public ReactiveCommand<object> ManageSavedConfiguration { get; private set; }
        
        /// <summary>
        /// Update the <see cref="CanExecuteImport"/> property
        /// </summary>
        private void UpdateCanExecuteImport()
        {
            if (this.SelectedIteration == null)
            {
                this.CanExecuteImport = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(this.Path))
            {
                this.canExecuteImport = false;
                return;
            }

            var session = this.Sessions.Single(x => x.DataSourceUri == this.SelectedIteration.DataSourceUri);
            var model = (EngineeringModel)this.SelectedIteration.Iteration.Container;
            var owner = model.GetActiveParticipant(session.ActivePerson);
            
            if (owner == null)
            {
                this.CanExecuteImport = false;
                return;
            }

            this.CanExecuteImport = true;
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        private async void ExecuteOk()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Importing...";

            try
            {
                var reqif = await Task.Run(() => this.serializer.Deserialize(this.Path));
                this.DialogResult = new ReqIfImportResult(reqif, this.SelectedIteration.Iteration, this.SelectedMappingConfiguration, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
            this.DialogResult = new ReqIfImportResult(null, null, null, false);
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteBrowse()
        {
            var result = this.fileDialogService.GetOpenFileDialog(true, true, false, "ReqIF files (*.reqif, *.xml)|*.reqif; *.xml|All Files (*.*)|*.*", ".reqif", this.Path, 1);
            
            if (result == null)
            {
                return;
            }

            this.Path = result.Single();
        }
        
        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteManageSavedConfiguration()
        {
            var manageConfigurationViewModel = new ManageConfigurationsDialogViewModel<RequirementsModuleSettings>(this.pluginSettingsService);
            var result = this.dialogNavigationService.NavigateModal(manageConfigurationViewModel) as ManageConfigurationsResult;

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            this.ReloadSavedConfigurations();
        }

        /// <summary>
        /// Reloads the saved configurations.
        /// </summary>
        private void ReloadSavedConfigurations()
        {
            var settings = this.pluginSettingsService.Read<RequirementsModuleSettings>();
            this.AvailableMappingConfiguration = new ReactiveList<ImportMappingConfiguration>(settings.SavedConfigurations.Cast<ImportMappingConfiguration>());

            this.AvailableMappingConfiguration.Insert(0, new ImportMappingConfiguration()
            {
                Name = "(None)",
                Description = "(None)"
            });
        }
    }
}