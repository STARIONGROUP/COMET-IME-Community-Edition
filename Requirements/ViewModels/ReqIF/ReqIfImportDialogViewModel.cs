// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    
    using CDP4Composition.Navigation;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Requirements.Settings.JsonConverters;

    using DevExpress.XtraRichEdit.Layout.Engine;

    using ReactiveUI;
    
    using ReqIFSharp;

    using NLog;

    /// <summary>
    /// The view-model to import reqif requirement data
    /// </summary>
    public class ReqIfImportDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Holds the string that defines that no saved mapping configuration should be use
        /// </summary>
        internal const string NoConfigurationText = "(None)";

        /// <summary>
        /// Holds the string that defines that the saved configuration will be automatically selected
        /// </summary>
        internal const string AutoConfigurationText = "(Auto)";

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
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        /// <param name="serializer"></param>
        public ReqIfImportDialogViewModel(IEnumerable<ISession> sessions, IEnumerable<Iteration> iterations, IOpenSaveFileDialogService fileDialogService, IPluginSettingsService pluginSettingsService, IReqIFDeSerializer serializer)
        {
            this.Sessions = sessions?.ToList() ?? throw new ArgumentNullException(nameof(sessions));
            this.fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
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
            
            this.OkCommand = ReactiveCommand.CreateAsyncTask(canOk, async _ => await this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
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
        /// Gets or sets the selected iteration to export
        /// </summary>
        public ReqIfExportIterationRowViewModel SelectedIteration
        {
            get => this.selectedIteration;
            set => this.RaiseAndSetIfChanged(ref this.selectedIteration, value);
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
        /// Gets or sets the selected mapping configuration
        /// </summary>
        public ImportMappingConfiguration SelectedMappingConfiguration
        {
            get => this.selectedMappingConfiguration;
            set => this.RaiseAndSetIfChanged(ref this.selectedMappingConfiguration, value);
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
        /// Gets the Browse Command
        /// </summary>
        public ReactiveCommand<object> BrowseCommand { get; private set; }

        /// <summary>
        /// Gets the available saved mapping configuration to choose from
        /// </summary>
        public ReactiveList<ImportMappingConfiguration> AvailableMappingConfiguration { get; private set; }

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
        private async Task ExecuteOk()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Importing...";

            try
            {
                var reqif = await Task.Run(() => this.serializer.Deserialize(this.Path));

                var configuration = this.GetMappingConfiguation(reqif);

                this.DialogResult = new ReqIfImportResult(reqif, this.SelectedIteration.Iteration, configuration, true);
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
        /// 
        /// </summary>
        /// <param name="reqif"></param>
        /// <returns></returns>
        internal ImportMappingConfiguration GetMappingConfiguation(ReqIF reqif)
        {
            if (this.SelectedMappingConfiguration.Name == NoConfigurationText)
            {
                return null;
            }

            var mappingConfigurations = this.pluginSettingsService.Read<RequirementsModuleSettings>(true, ConverterExtensions.BuildConverters(reqif, this.Sessions.Single(x => x.DataSourceUri == this.SelectedIteration.DataSourceUri), this.SelectedIteration.Iteration))
                .SavedConfigurations.Cast<ImportMappingConfiguration>();

            if (this.SelectedMappingConfiguration.Name != AutoConfigurationText)
            {
                return mappingConfigurations.FirstOrDefault(
                    x => x.Name == this.SelectedMappingConfiguration.Name);
            }

            return mappingConfigurations.FirstOrDefault(
                x => x.ReqIfId == reqif.TheHeader.First().Identifier);
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
        /// Reloads the saved configurations.
        /// </summary>
        private void ReloadSavedConfigurations()
        {
            var settings = this.pluginSettingsService.Read<RequirementsModuleSettings>();
            this.AvailableMappingConfiguration = new ReactiveList<ImportMappingConfiguration>(settings.SavedConfigurations.Cast<ImportMappingConfiguration>());

            this.AvailableMappingConfiguration.Insert(
                0, 
                new ImportMappingConfiguration()
                {
                    Name = NoConfigurationText,
                    Description = NoConfigurationText
                });

            this.AvailableMappingConfiguration.Insert(
                0, 
                new ImportMappingConfiguration()
                {
                    Name = AutoConfigurationText,
                    Description = AutoConfigurationText
                });

            this.SelectedMappingConfiguration = this.AvailableMappingConfiguration.FirstOrDefault();
        }
    }
}