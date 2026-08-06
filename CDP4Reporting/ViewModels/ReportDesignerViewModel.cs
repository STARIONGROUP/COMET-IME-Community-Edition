// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Reactive.Threading.Tasks;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.Types;

    using CDP4CommonView.ViewModels;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4Reporting.DataCollection;
    using CDP4Reporting.DynamicTableChecker;
    using CDP4Reporting.Events;
    using CDP4Reporting.ReportScript;
    using CDP4Reporting.SubmittableParameterValues;
    using CDP4Reporting.Utilities;

    using CommonServiceLocator;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Printing;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.UI;

    using ICSharpCode.AvalonEdit.Document;

    using NLog;

    using ReactiveUI;

    using File = System.IO.File;
    using Parameter = DevExpress.XtraReports.Parameters.Parameter;

    /// <summary>
    /// The view-model for the Report Designer that lets users to create reports based on template source files.
    /// </summary>
    public partial class ReportDesignerViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IHaveAfterOnClosingLogic
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Reporting";

        /// <summary>
        /// The <see cref="ISubmittableParameterValuesCollector"/> used to collect submittable parameter values from the report previewer.
        /// </summary>
        private readonly ISubmittableParameterValuesCollector submittableParameterValuesCollector = ServiceLocator.Current.GetInstance<ISubmittableParameterValuesCollector>();

        /// <summary>
        /// The <see cref="IMessageBoxService"/> used to show messages.
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// The <see cref="IDynamicTableChecker{T}"/> used to check datatables in the report.
        /// </summary>
        private readonly IDynamicTableChecker<XtraReport> dynamicTableChecker = ServiceLocator.Current.GetInstance<IDynamicTableChecker<XtraReport>>();

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/> that is used to navigate to the File Open/Save dialog
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// The <see cref="SingleConcurrentActionRunner "/> that handles the compilation of a datasource
        /// </summary>
        private readonly SingleConcurrentActionRunner compilationConcurrentActionRunner = new SingleConcurrentActionRunner();

        /// <summary>
        /// The currently active <see cref="XtraReport"/> in the Report Designer
        /// </summary>
        private XtraReport currentReport = GetNewXtraReport();

        /// <summary>
        /// The currently active <see cref="TextDocument"/> in the Avalon Editor
        /// </summary>
        private TextDocument document;

        /// <summary>
        /// The currently active <see cref="ReportDesignerDocument"/> in the Report Designer
        /// </summary>
        private ReportDesignerDocument currentReportDesignerDocument;

        /// <summary>
        /// Backing field for <see cref="IsAutoCompileEnabled" />
        /// </summary>
        private bool isAutoCompileEnabled;

        /// <summary>
        /// The last saved datasource text
        /// </summary>
        private string lastSavedDataSourceText = string.Empty;

        /// <summary>
        /// Backing field for <see cref="CanSubmitParameterValues"/>
        /// </summary>
        private bool canSubmitParameterValues;

        /// <summary>
        /// A temporary <see cref="IEnumerable{SubmittableParameterValue}"/> that represent all submittable parameters that are present in the report preview
        /// </summary>
        private IEnumerable<SubmittableParameterValue> submittableParameterValues;

        /// <summary>
        /// Backing field for <see cref="Errors" />
        /// </summary>
        private string errors;

        /// <summary>
        /// Backing field for <see cref="Output" />
        /// </summary>
        private string output;

        /// <summary>
        /// Gets or sets the <see cref="ReportScriptHandler"/>
        /// </summary>
        public ReportScriptHandler<XtraReport, Parameter> ReportScriptHandler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the browser is dirty
        /// </summary>
        public override bool IsDirty
        {
            get => (bool)(this.currentReportDesignerDocument?.GetValue(ReportDesignerDocument.HasChangesProperty) ?? false) || !this.lastSavedDataSourceText.Equals(this.Document.Text);
            set { }
        }

        /// <summary>
        /// Gets or sets text editor document
        /// </summary>
        public TextDocument Document
        {
            get => this.document;
            set => this.RaiseAndSetIfChanged(ref this.document, value);
        }

        /// <summary>
        /// Gets or sets the Report Designer's current Report
        /// </summary>
        public XtraReport CurrentReport
        {
            get => this.currentReport;
            set => this.RaiseAndSetIfChanged(ref this.currentReport, value);
        }

        /// <summary>
        /// Gets or sets current edited file path
        /// </summary>
        public string CodeFilePath { get; set; }

        /// <summary>
        /// Gets or sets current archive zip file path that contains resx report designer file and datasource c# file
        /// </summary>
        public string CurrentReportProjectFilePath { get; set; }

        /// <summary>
        /// Gets or sets value for editor's errors
        /// </summary>
        public string Errors
        {
            get => this.errors;
            set => this.RaiseAndSetIfChanged(ref this.errors, value);
        }

        /// <summary>
        /// Gets or sets value for output's log messages
        /// </summary>
        public string Output
        {
            get => this.output;
            set => this.RaiseAndSetIfChanged(ref this.output, value);
        }

        /// <summary>
        /// Sets or gets a boolean indicating whether the Submit parameter button is enabled
        /// </summary>
        public bool CanSubmitParameterValues
        {
            get => this.canSubmitParameterValues;
            set => this.RaiseAndSetIfChanged(ref this.canSubmitParameterValues, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether automatically build is checked
        /// </summary>
        public bool IsAutoCompileEnabled
        {
            get => this.isAutoCompileEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isAutoCompileEnabled, value);
        }

        /// <summary>
        /// Open code file inside the editor
        /// </summary>
        public ReactiveCommand<Unit, Unit> ImportScriptCommand { get; set; }

        /// <summary>
        /// Saves code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExportScriptCommand { get; set; }

        /// <summary>
        /// Build code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<Unit, Unit> CompileScriptCommand { get; set; }

        /// <summary>
        /// Create a new Report 
        /// </summary>
        public ReactiveCommand<Unit, Unit> NewReportCommand { get; set; }

        /// <summary>
        /// Open rep4 zip archive which consists in datasource code file and report designer file
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenReportCommand { get; set; }

        /// <summary>
        /// Save editor code and report designer to rep4 zip archive
        /// </summary>
        public ReactiveCommand<bool, Unit> SaveReportCommand { get; set; }

        /// <summary>
        /// Save editor code and report designer to rep4 zip archive and force the SaveFile dialog to be shown
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveReportAsCommand { get; set; }

        /// <summary>
        /// Fires when the DataSource text was changed
        /// </summary>
        public ReactiveCommand<Unit, Unit> DataSourceTextChangedCommand { get; set; }

        /// <summary>
        /// Rebuild the DataSource
        /// </summary>
        public ReactiveCommand<Unit, Unit> RebuildDatasourceCommand { get; set; }

        /// <summary>
        /// Rebuild the DataSource and refresh the preview panel
        /// </summary>
        public ReactiveCommand<Unit, Unit> RebuildDatasourceAndRefreshPreviewCommand { get; set; }

        /// <summary>
        /// Submit data from a previewed report
        /// </summary>
        public ReactiveCommand<object, Unit> SubmitParameterValuesCommand { get; set; }

        /// <summary>
        /// Recalculates the preview as a non-saving "what-if": it applies the values currently edited in the
        /// preview to an in-memory clone of the <see cref="Iteration"/> and re-runs the report's data collector
        /// against that clone, so all roll-ups recompute with the real engine without touching the model.
        /// </summary>
        public ReactiveCommand<Unit, Unit> WhatIfRecalculateCommand { get; set; }

        /// <summary>
        /// Loads the what-if editor grid with the report's current editable leaf values.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadWhatIfEditorCommand { get; set; }

        /// <summary>
        /// Clears all what-if overrides and reloads the editor from the model.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetWhatIfCommand { get; set; }

        /// <summary>
        /// Gets the editable rows shown in the what-if editor grid.
        /// </summary>
        public ObservableCollection<WhatIfEditRowViewModel> WhatIfEditRows { get; } = new ObservableCollection<WhatIfEditRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="IsWhatIfEditorVisible"/>.
        /// </summary>
        private bool isWhatIfEditorVisible;

        /// <summary>
        /// Gets or sets a value indicating whether the what-if editor panel is shown (it appears after Load Values).
        /// </summary>
        public bool IsWhatIfEditorVisible
        {
            get => this.isWhatIfEditorVisible;
            set => this.RaiseAndSetIfChanged(ref this.isWhatIfEditorVisible, value);
        }

        /// <summary>
        /// The what-if value overrides, keyed by parameter write-back path. These persist across recomputes so
        /// the user's edits stick until reset.
        /// </summary>
        private readonly Dictionary<string, double> whatIfOverrides = new Dictionary<string, double>();

        /// <summary>Maps a value set to the grid rows that resolve to it (usages of a shared element-definition value).</summary>
        private readonly Dictionary<Guid, List<WhatIfEditRowViewModel>> whatIfValueSetToRows = new Dictionary<Guid, List<WhatIfEditRowViewModel>>();

        /// <summary>
        /// Snapshots of the model value sets that a what-if has edited (keyed by value-set Iid), so the in-memory
        /// edits can be reverted. A what-if edits the model's value sets directly - <see cref="Thing.Clone(bool)"/>
        /// does not remap the element-usage-to-definition references, so a clone would still resolve to (and mutate)
        /// the same value sets - and nothing is ever written to the server; the edits are undone on Load/Reset/close.
        /// </summary>
        private readonly Dictionary<Guid, WhatIfValueSetSnapshot> whatIfSnapshots = new Dictionary<Guid, WhatIfValueSetSnapshot>();

        /// <summary>
        /// The option the what-if editor works against, pinned when a report is opened or its datasource rebuilt (the
        /// report's selected option at that moment) and reused until the next rebuild/open, so the grid does not jump
        /// options between Load/Recalculate.
        /// </summary>
        private Option whatIfSelectedOption;

        /// <summary>
        /// Backing field for <see cref="IsEditModeEnabled"/>.
        /// </summary>
        private bool isEditModeEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the preview's submittable cells are editable, so the user can
        /// type new values before a what-if recalculation or a submit.
        /// </summary>
        public bool IsEditModeEnabled
        {
            get => this.isEditModeEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isEditModeEnabled, value);
        }

        /// <summary>
        /// Fires when the DataSource text needs to be cleared
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearOutputCommand { get; set; }

        /// <summary>
        /// Fires when the Active Document changes in the Report Designer
        /// </summary>
        public ReactiveCommand<DependencyPropertyChangedEventArgs, Unit> ActiveDocumentChangedCommand { get; set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerViewModel"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to display</param>
        /// <param name="session">The session.</param>
        /// <param name="thingDialogNavigationService">The thing navigation service.</param>
        /// <param name="panelNavigationService">The panel navigation service.</param>
        /// <param name="dialogNavigationService">The dialog navigation service.</param>
        /// <param name="pluginSettingsService">The plugin service.</param>
        public ReportDesignerViewModel(Iteration thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            var dataSetAssembly = typeof(DataSet).Assembly;
            DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(dataSetAssembly);

            ReportingSettings.OptionSelector = (options, option) =>
            {
                var thingSelectorDialogService = ServiceLocator.Current.GetInstance<IThingSelectorDialogService>();
                return thingSelectorDialogService.SelectThing(options, new string[] { "ShortName", "Name" });
            };

            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.Document = new TextDocument();
            this.Errors = string.Empty;
            this.Output = string.Empty;
            this.IsAutoCompileEnabled = false;

            this.openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.ExportScriptCommand = ReactiveCommandCreator.Create(this.ExportScript);

            this.ImportScriptCommand = ReactiveCommandCreator.Create(this.ImportScript);

            this.CompileScriptCommand = ReactiveCommandCreator.Create(async () =>
            {
                var source = this.Document.Text;
                await this.compilationConcurrentActionRunner.RunAction(() => this.ReportScriptHandler.CompileAssembly(source));
            });

            this.NewReportCommand = ReactiveCommandCreator.Create(this.CreateNewReport);

            this.OpenReportCommand = ReactiveCommandCreator.Create(this.OpenReportProject);

            this.SaveReportCommand = ReactiveCommandCreator.Create<bool>(this.SaveReportProject);

            this.SaveReportAsCommand = ReactiveCommandCreator.Create(() => this.SaveReportProject(true));

            this.DataSourceTextChangedCommand = ReactiveCommandCreator.Create(this.CheckAutoCompileScript);

            this.RebuildDatasourceCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteRebuildDatasourceCommand);

            this.RebuildDatasourceAndRefreshPreviewCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteRebuildDatasourceAndRefreshPreviewCommand);

            this.SubmitParameterValuesCommand = ReactiveCommandCreator.Create<object>(
                x => this.SubmitParameterValues().ToObservable(),
                this.WhenAnyValue(x => x.CanSubmitParameterValues));

            this.WhatIfRecalculateCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteWhatIfRecalculate);

            this.LoadWhatIfEditorCommand = ReactiveCommandCreator.Create(this.ToggleWhatIfEditor);

            this.ResetWhatIfCommand = ReactiveCommandCreator.Create(this.ResetWhatIf);

            this.ClearOutputCommand = ReactiveCommandCreator.Create(() => { this.Output = string.Empty; });

            this.ActiveDocumentChangedCommand = ReactiveCommandCreator.CreateAsyncTask<DependencyPropertyChangedEventArgs>(x =>
                this.SetReportDesigner(x.NewValue));

            this.WhenAnyValue(x => x.CurrentReport).Subscribe(x =>
            {
                x.AfterPrint += this.CheckSubmittableParameterValues;
                x.DataSourceDemanded += this.CheckDynamicTables;

                // A new/opened report must not carry over the previous report's what-if scenario: revert any edits
                // and hide the editor until the user loads it again for this report.
                this.ResetWhatIfState();
            });

            this.Changing
                .Where(x => x.PropertyName == nameof(this.CurrentReport))
                .Subscribe(x =>
                {
                    if (this.CurrentReport != null)
                    {
                        this.CurrentReport.AfterPrint -= this.CheckSubmittableParameterValues;
                        this.CurrentReport.DataSourceDemanded -= this.CheckDynamicTables;
                    }
                });

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ReportOutputEvent>()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.AddOutput(x.Output))
            );

            this.InitializeDataSetExtensionsUsage();
            this.InitializeNewReport();
        }

        /// <summary>
        /// Method that is here that does nothing, but makes sure that System.Data.DataSetExtensions.dll is
        /// available in the report script
        /// </summary>
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed",
            Justification = "Method that is here that does nothing, but makes sure that System.Data.DataSetExtensions.dll is available in the report script.")]
        private void InitializeDataSetExtensionsUsage()
        {
            var dataTable = new DataTable();
            var initializeExtension = dataTable.AsEnumerable();
        }

        /// <summary>
        /// Asynchronously runs the setting of the <see cref="currentReportDesignerDocument"/> field.
        /// </summary>
        /// <param name="newValue">The <see cref="ReportDesignerDocument"/> as an <see cref="object"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task SetReportDesigner(object newValue)
        {
            await Task.Run(() => this.currentReportDesignerDocument = (ReportDesignerDocument)newValue);
        }

        /// <summary>
        /// Trigger open script file operation
        /// </summary>
        private void ImportScript()
        {
            var filePath = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false, "CS(.cs)|*.cs", ".cs", string.Empty, 1);

            if (filePath == null || filePath.Length != 1)
            {
                return;
            }

            this.CodeFilePath = filePath.Single();

            this.Document.Text = File.ReadAllText(this.CodeFilePath);
        }

        /// <summary>
        /// Trigger save script file operation
        /// </summary>
        private void ExportScript()
        {
            var codeFilePath = this.CodeFilePath ?? "ReportDataSource.cs";

            var filePath = this.openSaveFileDialogService.GetSaveFileDialog(Path.GetFileName(codeFilePath), "cs", "CS(.cs) | *.cs", codeFilePath, 1);

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            this.CodeFilePath = filePath;

            if (!string.IsNullOrEmpty(this.CodeFilePath))
            {
                File.WriteAllText(this.CodeFilePath, this.Document.Text);
            }
        }

        /// <summary>
        /// Checks if AutoCompile script is active and start compilation accordingly.
        /// </summary>
        private void CheckAutoCompileScript()
        {
            if (!this.IsAutoCompileEnabled)
            {
                return;
            }

            var text = this.Document.Text;

            this.compilationConcurrentActionRunner.DelayRunAction(() => this.ReportScriptHandler.CompileAssembly(text), 2500);
        }

        /// <summary>
        /// Create a new report project
        /// </summary>
        private void CreateNewReport()
        {
            if (!this.IsSwitchReportProjectAllowed())
            {
                return;
            }

            this.InitializeNewReport();
        }

        /// <summary>
        /// Initializes a new Report
        /// </summary>
        private void InitializeNewReport()
        {
            this.Document = new TextDocument(string.Empty);
            this.lastSavedDataSourceText = "";

            this.CurrentReport = GetNewXtraReport();
            this.ReportScriptHandler = new ReportScriptHandler<XtraReport, Parameter>(new XtraReportHandler(this.CurrentReport, this.currentReportDesignerDocument), new CodeDomCodeCompiler(this.AddOutput), x => this.Errors = x, this.AddOutput);

            this.CurrentReportProjectFilePath = string.Empty;
        }

        /// <summary>
        /// Get a new <see cref="XtraReport"/> and initialize default settings
        /// </summary>
        /// <returns>The <see cref="XtraReport"/></returns>
        private static XtraReport GetNewXtraReport()
        {
            var newReport = new XtraReport
            {
                ReportUnit = ReportUnit.Pixels,
                SnapGridSize = 6F,
                SnapGridStepCount = 5,
                SnappingMode = SnappingMode.SnapToGridAndSnapLines
            };

            return newReport;
        }

        /// <summary>
        /// Open an existing report project
        /// </summary>
        private void OpenReportProject()
        {
            if (!this.IsSwitchReportProjectAllowed())
            {
                return;
            }

            var filePath = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false, "Report project files (*.rep4)|*.rep4|All files (*.*)|*.*", ".rep4", string.Empty, 1);

            var reportProjectFilePath = filePath?.SingleOrDefault();

            if (reportProjectFilePath == null)
            {
                return;
            }

            var report = new XtraReport();

            this.ReportScriptHandler = new ReportScriptHandler<XtraReport, Parameter>(new XtraReportHandler(report, this.currentReportDesignerDocument), new CodeDomCodeCompiler(this.AddOutput), x => this.Errors = x, this.AddOutput);

            using (var reportZipArchive = this.ReportScriptHandler.GetReportZipArchive(reportProjectFilePath))
            {
                report.LoadLayoutFromXml(reportZipArchive.ReportDefinition);

                this.Document = new TextDocument();

                if (reportZipArchive.DataSourceCode != null)
                {
                    using (var streamReader = new StreamReader(reportZipArchive.DataSourceCode))
                    {
                        var datasource = streamReader.ReadToEnd();
                        this.Document = new TextDocument(datasource);
                    }

                    this.ReportScriptHandler.CompileAssembly(this.Document.Text);
                }
            }

            this.CurrentReport = report;
            this.CurrentReportProjectFilePath = reportProjectFilePath;

            this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session);
            this.CaptureWhatIfOption();
            this.TriggerRefreshUI();

            this.lastSavedDataSourceText = this.Document.Text;
            this.currentReportDesignerDocument?.SetValue(ReportDesignerDocument.HasChangesProperty, false);
        }

        /// <summary>
        /// Checks if creating a new report, or opening an existing report is allowed.
        /// </summary>
        /// <returns>true if allowed, otherwise false. </returns>
        private bool IsSwitchReportProjectAllowed()
        {
            if (this.IsDirty)
            {
                var confirmation = new GenericConfirmationDialogViewModel("Warning",
                    "The currently active report has unsaved changes. \n Are you sure you want to continue and lose these changes?");

                var result = this.DialogNavigationService.NavigateModal(confirmation);

                return (result?.Result.HasValue ?? false) && result.Result.Value;
            }

            return true;
        }

        /// <summary>
        /// Save the report project
        /// </summary>
        /// <param name="forceDialog">Forces the file dialog to select where to save the report project.</param>
        private void SaveReportProject(bool forceDialog = false)
        {
            var archiveName = "ReportArchive";
            var initialPath = string.Empty;
            var fileShouldExist = !string.IsNullOrWhiteSpace(this.CurrentReportProjectFilePath);

            if (fileShouldExist)
            {
                archiveName = Path.GetFileNameWithoutExtension(this.CurrentReportProjectFilePath);
                initialPath = this.CurrentReportProjectFilePath;
            }

            var filePath = this.CurrentReportProjectFilePath;

            if (!fileShouldExist || forceDialog)
            {
                filePath = this.openSaveFileDialogService.GetSaveFileDialog(archiveName, "rep4", "Report project files (*.rep4)|*.rep4|All files (*.*)|*.*", initialPath, 1);
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            this.CurrentReportProjectFilePath = filePath;

            if (File.Exists(this.CurrentReportProjectFilePath))
            {
                File.Delete(this.CurrentReportProjectFilePath);
            }

            using (var reportStream = new MemoryStream())
            {
                this.CurrentReport.SaveLayoutToXml(reportStream);

                using (var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(this.Document.Text)))
                {
                    using (var zipFile = ZipFile.Open(this.CurrentReportProjectFilePath, ZipArchiveMode.Create))
                    {
                        using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                        {
                            reportStream.Position = 0;
                            reportStream.CopyTo(reportEntry);
                        }

                        using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                        {
                            dataSourceStream.Position = 0;
                            dataSourceStream.CopyTo(reportEntry);
                        }
                    }
                }

                this.lastSavedDataSourceText = this.Document.Text;
                this.currentReportDesignerDocument?.SetValue(ReportDesignerDocument.HasChangesProperty, false);
            }
        }

        /// <summary>
        /// Executes the <see cref="RebuildDatasourceCommand"/>
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task ExecuteRebuildDatasourceCommand()
        {
            this.IsBusy = true;

            try
            {
                var source = this.Document.Text;
                await this.compilationConcurrentActionRunner.RunAction(() => this.ReportScriptHandler.CompileAssembly(source));

                if (!this.ReportScriptHandler.CompileResults?.Errors.HasErrors ?? false)
                {
                    this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session);
                    this.CaptureWhatIfOption();
                    this.TriggerRefreshUI();
                }
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the <see cref="RebuildDatasourceAndRefreshPreviewCommand"/>
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task ExecuteRebuildDatasourceAndRefreshPreviewCommand()
        {
            this.IsBusy = true;

            try
            {
                var source = this.Document.Text;
                await this.compilationConcurrentActionRunner.RunAction(() => this.ReportScriptHandler.CompileAssembly(source));

                if (this.ReportScriptHandler.CompileResults?.Errors.HasErrors ?? false)
                {
                    this.messageBoxService.Show(this.ReportScriptHandler.CompileResults?.Errors.ToString(), "Compilation error", MessageBoxButton.OK, MessageBoxImage.Stop);
                }

                if (this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session, true))
                {
                    this.messageBoxService.Show(
                        "Report parameters were added to, or removed from the report definition. Reload the preview tab to reflect these changes in the parameters panel.",
                        "Reload preview tab",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                this.CaptureWhatIfOption();
                this.TriggerRefreshUI();

                var presenter = this.currentReportDesignerDocument?.Preview as DocumentPreviewControl;

                try
                {
                    foreach (var parameter in this.currentReport.Parameters)
                    {
                        if (parameter.Name.StartsWith("dyn_"))
                        {
                            var visibleParameter = presenter?.ParameterPanelViewModel.Parameters.SingleOrDefault(x => x.Name == parameter.Name);

                            if (visibleParameter != null)
                            {
                                visibleParameter.Value = parameter.Value;
                            }
                        }
                    }
                }
                catch
                {
                    //ignore this error...
                }

                presenter?.ParameterPanelViewModel.SubmitParameters();
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the <see cref="WhatIfRecalculateCommand"/>: recomputes the report against an in-memory clone
        /// of the iteration to which the values currently shown/edited in the preview have been applied, without
        /// saving anything to the model. "Submit Parameter Values" is used to commit the edits for real.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private async Task ExecuteWhatIfRecalculate()
        {
            if (this.CurrentReport == null || this.ReportScriptHandler.CurrentDataCollector == null)
            {
                return;
            }

            this.IsBusy = true;

            try
            {
                // Use the values edited in the what-if editor grid (persisted as overrides).
                var edits = this.BuildEditsFromOverrides();

                // Let the busy indicator paint before the (synchronous) recompute.
                await Task.Yield();

                // Revert any previous what-if edits so this recalculation is "model + current overrides", never
                // cumulative; then apply the current overrides to the model's value sets (in memory, never saved).
                this.RevertModel();
                var applied = this.ApplyOverridesToModel(edits);

                // Re-run the already-compiled C# collector against the (temporarily edited) model and refresh the
                // preview. No recompilation is needed, and this stays on the UI thread (it touches the AvalonEdit
                // TextDocument and the DevExpress preview).
                this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session, true);
                this.TriggerRefreshUI();
                this.RefreshPreviewDocument();

                this.AddOutput(edits.Count == 0
                    ? "Recalculated - but there were no What-if edits to apply. Change a value in the What-if Editor first, then Recalculate."
                    : $"Recalculated with {applied} of {edits.Count} edit(s) applied in memory (nothing saved - use Submit to commit, Load/Reset to revert). See details above if any were not applied.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "The what-if recalculation failed.");
                this.AddOutput($"The what-if recalculation failed: {ex.Message}");
                this.messageBoxService.Show(ex.Message, "Recalculate (what-if) failed", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Forces the preview document to regenerate so a what-if recompute is reflected immediately, without the
        /// user having to switch to the Designer tab and back.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void RefreshPreviewDocument()
        {
            try
            {
                var presenter = this.currentReportDesignerDocument?.Preview as DocumentPreviewControl;

                if (presenter?.ParameterPanelViewModel == null)
                {
                    return;
                }

                // Push the report parameter values that RebuildDataSource just recomputed (the "dyn_" dynamic
                // parameters - e.g. a headline figure a report derives from a specific element such as a bus
                // Power_budget) into the preview's parameter panel. Without this the preview keeps the pre-edit
                // values and any figure computed from them does not change. Mirrors the Rebuild Datasource (preview)
                // command's refresh.
                foreach (var parameter in this.currentReport.Parameters)
                {
                    if (parameter.Name.StartsWith("dyn_", StringComparison.Ordinal))
                    {
                        var previewParameter = presenter.ParameterPanelViewModel.Parameters.SingleOrDefault(x => x.Name == parameter.Name);

                        if (previewParameter != null)
                        {
                            previewParameter.Value = parameter.Value;
                        }
                    }
                }

                presenter.ParameterPanelViewModel.SubmitParameters();
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Could not refresh the report preview after the what-if recalculation.");
            }
        }

        /// <summary>
        /// Loads the what-if editor grid from the report's current data: one editable row per leaf value that
        /// carries a write-back path. Existing overrides are re-applied so previous edits are preserved.
        /// </summary>
        /// <summary>
        /// Starts or ends the what-if scenario: when the editor is closed it loads the editable values and opens the
        /// panel; when it is open it reverts every in-memory edit and closes the panel, returning to the original state.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ToggleWhatIfEditor()
        {
            if (this.IsWhatIfEditorVisible)
            {
                this.ResetWhatIfState();
                this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session, true);
                this.TriggerRefreshUI();
                this.RefreshPreviewDocument();
                this.AddOutput("What-if scenario ended - all edits reverted and the model restored to its original state.");
            }
            else
            {
                this.LoadWhatIfEditor();
            }
        }

        /// <summary>
        /// Loads the what-if editor grid from the report's model values.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void LoadWhatIfEditor()
        {
            // Undo any active what-if edits and rebuild against the model, so the loaded values are the true model
            // values and not a previous what-if result.
            try
            {
                this.RevertModel();
                this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session, true);
                this.TriggerRefreshUI();
                this.RefreshPreviewDocument();
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Could not revert and rebuild the report against the model before loading the what-if editor.");
            }

            this.WhatIfEditRows.Clear();
            this.whatIfValueSetToRows.Clear();

            // Load always starts from a clean slate: discard any previous what-if edits so both the "current" and
            // "what-if" columns show the real model values.
            this.whatIfOverrides.Clear();

            // Editable parameters are exactly those the report declares with [DefinedThingShortName] (so derived/
            // computed columns like a C#-computed TotalMass are never offered). The grid is built straight from the
            // model's nested parameters - no per-report "model path" column is needed - by matching each nested
            // parameter's short-name to a declared one. This works for every report shape uniformly.
            var shortNamesByColumn = this.ReflectEditableParameters();

            if (shortNamesByColumn.Count == 0)
            {
                this.AddOutput("Could not load the what-if editor: no editable parameters were found. Declare the report's leaf parameters with [DefinedThingShortName(\"shortName\", \"Column\")] in the Datasource.");
                return;
            }

            var columnByShortName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in shortNamesByColumn)
            {
                columnByShortName[pair.Value] = pair.Key;
            }

            // Scope the grid to the option pinned when the report was opened / its datasource last rebuilt (falling
            // back to the collector's current selection on first load). We keep using that option so the grid does not
            // jump options between Load and Recalculate.
            var selectedOption = this.whatIfSelectedOption
                                 ?? (this.ReportScriptHandler.CurrentDataCollector as IOptionDependentDataCollector)?.SelectedOption;
            var optionTrees = this.BuildOptionTrees();

            this.PopulateWhatIfRows(columnByShortName, optionTrees, selectedOption);

            // If the selected option could not be matched (e.g. an iteration-only collector), use every option.
            if (this.WhatIfEditRows.Count == 0 && selectedOption != null)
            {
                this.PopulateWhatIfRows(columnByShortName, optionTrees, null);
            }

            this.IsWhatIfEditorVisible = true;
            var optionText = selectedOption == null ? "all options" : $"option '{selectedOption.ShortName}'";
            this.AddOutput($"What-if editor loaded with {this.WhatIfEditRows.Count} editable value(s) across {columnByShortName.Count} parameter(s) for {optionText}: {string.Join(", ", columnByShortName.Keys)}. Edit the 'What-if' column, then click Recalculate.");
        }

        /// <summary>
        /// Populates the what-if grid from the model's nested parameters: one row per value set (so usages that share
        /// a value - and the same value across options - collapse to a single editable row) whose parameter matches a
        /// declared editable short-name. When <paramref name="onlyOption"/> is set, only that option's tree is used.
        /// A row is added for every value set the element actually owns, even when currently unvalued; state-dependent
        /// values are labelled with their state so the (otherwise identical) rows can be told apart.
        /// </summary>
        /// <param name="columnByShortName">Map of model short-name to the report's value column header.</param>
        /// <param name="optionTrees">The model's nested-parameter lists per option.</param>
        /// <param name="onlyOption">When set, restricts the grid to this option; null uses every option.</param>
        [ExcludeFromCodeCoverage]
        private void PopulateWhatIfRows(IReadOnlyDictionary<string, string> columnByShortName, IEnumerable<Tuple<Option, List<NestedParameter>>> optionTrees, Option onlyOption)
        {
            var seenValueSets = new HashSet<Guid>();

            foreach (var optionTree in optionTrees)
            {
                if (onlyOption != null && optionTree.Item1 != onlyOption)
                {
                    continue;
                }

                foreach (var nestedParameter in optionTree.Item2)
                {
                    var shortName = nestedParameter.AssociatedParameter?.ParameterType?.ShortName;

                    if (string.IsNullOrEmpty(shortName) || !columnByShortName.TryGetValue(shortName, out var column))
                    {
                        continue;
                    }

                    // Only rows for a scalar parameter the element actually owns (a real value set) are editable. An
                    // unset value is still shown - the element has the parameter - just with a blank current value.
                    if (nestedParameter.AssociatedParameter.ParameterType.NumberOfValues != 1
                        || !(nestedParameter.ValueSet is Thing valueSetThing)
                        || !seenValueSets.Add(valueSetThing.Iid))
                    {
                        continue;
                    }

                    double? original = null;

                    if (double.TryParse(nestedParameter.ActualValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                        || double.TryParse(nestedParameter.ActualValue, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                    {
                        original = parsed;
                    }

                    var elementDefinition = (nestedParameter.Container as NestedElement)?.GetElementDefinition();
                    var isAggregatingParent = elementDefinition != null && elementDefinition.ContainedElement.Count > 0;

                    if (isAggregatingParent && !original.HasValue)
                    {
                        continue;
                    }

                    var path = nestedParameter.Path;
                    var fullElementPath = string.IsNullOrEmpty(path) ? shortName : path.Split('\\')[0];

                    // Show the element's own short-name first (so it stays visible when the column truncates),
                    // followed by its containment path for context - e.g. "BUS  (…SpaceSeg.SV.DrySV)". This lets an
                    // element like the power BUS be told apart from its parent, which share a truncated prefix.
                    var lastSeparator = fullElementPath.LastIndexOf('.');
                    var element = lastSeparator > 0 && lastSeparator < fullElementPath.Length - 1
                        ? $"{fullElementPath.Substring(lastSeparator + 1)}  ({fullElementPath.Substring(0, lastSeparator)})"
                        : fullElementPath;

                    // Distinguish state-dependent rows (same element and parameter, one value set per state) by
                    // appending the state short-name to the parameter label.
                    var parameterLabel = nestedParameter.ActualState == null
                        ? column
                        : $"{column} [{nestedParameter.ActualState.ShortName}]";

                    var editRow = new WhatIfEditRowViewModel(element, parameterLabel, path, original, this.OnWhatIfValueChanged)
                    {
                        ValueSetIid = valueSetThing.Iid,
                        IsOverride = nestedParameter.AssociatedParameter is ParameterOverride
                    };

                    this.RegisterWhatIfRow(valueSetThing.Iid, editRow);
                    this.WhatIfEditRows.Add(editRow);
                }
            }
        }

        /// <summary>
        /// Reverts any active what-if edits, clears the editor grid and overrides, and hides the editor. Used when a
        /// report is opened/created so no scenario carries over, and on close/dispose to leave the model pristine.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ResetWhatIfState()
        {
            this.RevertModel();
            this.WhatIfEditRows.Clear();
            this.whatIfValueSetToRows.Clear();
            this.whatIfOverrides.Clear();
            this.whatIfSelectedOption = null;
            this.IsWhatIfEditorVisible = false;
        }

        /// <summary>
        /// Pins the option the what-if editor will use to the report's currently selected option. Called after a
        /// report is opened or its datasource is rebuilt, so the what-if grid keeps using that option until the next
        /// rebuild/open rather than re-resolving it on every Load.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void CaptureWhatIfOption()
        {
            var option = (this.ReportScriptHandler.CurrentDataCollector as IOptionDependentDataCollector)?.SelectedOption;

            if (option != null)
            {
                this.whatIfSelectedOption = option;
            }
        }

        /// <summary>
        /// Reflects the report's compiled assembly for parameters declared with <c>[DefinedThingShortName]</c>,
        /// returning a map of value column name to model short-name. These are the base (editable) parameters;
        /// derived columns (plain computed properties) are not included. Returns an empty map on any failure.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private Dictionary<string, string> ReflectEditableParameters()
        {
            var result = new Dictionary<string, string>();

            try
            {
                var collector = this.ReportScriptHandler.CurrentDataCollector;

                if (collector == null)
                {
                    return result;
                }

                foreach (var type in collector.GetType().Assembly.GetTypes())
                {
                    if (!typeof(DataCollectorRow).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    foreach (var property in type.GetProperties())
                    {
                        var attribute = property.GetCustomAttribute<DefinedThingShortNameAttribute>();

                        if (attribute != null && !string.IsNullOrEmpty(attribute.FieldName) && !string.IsNullOrEmpty(attribute.ShortName))
                        {
                            result[attribute.FieldName] = attribute.ShortName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, "Could not reflect the report's [DefinedThingShortName] parameters; the what-if editor will have nothing to edit.");
            }

            return result;
        }

        /// <summary>
        /// Builds the nested-parameter lists of the real model, per <see cref="Option"/>, for path resolution.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private List<Tuple<Option, List<NestedParameter>>> BuildOptionTrees()
        {
            var result = new List<Tuple<Option, List<NestedParameter>>>();
            var generator = new NestedElementTreeGenerator();

            foreach (Option option in this.Thing.Option)
            {
                try
                {
                    result.Add(Tuple.Create(option, generator.GetNestedParameters(option, false).ToList()));
                }
                catch (Exception ex)
                {
                    this.logger.Warn(ex, "Could not build the nested-parameter tree for option {0}.", option.ShortName);
                }
            }

            return result;
        }

        /// <summary>
        /// Registers a grid row under the value set it resolves to, for sibling synchronisation.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void RegisterWhatIfRow(Guid valueSetIid, WhatIfEditRowViewModel row)
        {
            if (!this.whatIfValueSetToRows.TryGetValue(valueSetIid, out var rows))
            {
                rows = new List<WhatIfEditRowViewModel>();
                this.whatIfValueSetToRows[valueSetIid] = rows;
            }

            rows.Add(row);
        }

        /// <summary>
        /// Clears all what-if overrides and reloads the editor from the current report data.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ResetWhatIf()
        {
            this.whatIfOverrides.Clear();
            this.LoadWhatIfEditor();
            this.AddOutput("What-if overrides cleared.");
        }

        /// <summary>
        /// Records or removes a what-if override when a grid cell's value changes.
        /// </summary>
        /// <param name="path">The parameter write-back path of the changed value.</param>
        /// <param name="value">The new value.</param>
        /// <param name="original">The originally computed value.</param>
        [ExcludeFromCodeCoverage]
        private void OnWhatIfValueChanged(WhatIfEditRowViewModel row)
        {
            if (row == null || string.IsNullOrEmpty(row.Path))
            {
                return;
            }

            // Usages that share the same value set (a shared element-definition value) must stay in sync: display
            // the new value on the siblings, and make sure only the edited row holds the override so the shared
            // value set is set exactly once on recalculation.
            if (this.whatIfValueSetToRows.TryGetValue(row.ValueSetIid, out var siblings))
            {
                foreach (var sibling in siblings)
                {
                    if (ReferenceEquals(sibling, row))
                    {
                        continue;
                    }

                    this.whatIfOverrides.Remove(sibling.Path);
                    sibling.SetValueSilently(row.Value);
                }
            }

            if (row.IsEdited && row.Value.HasValue)
            {
                this.whatIfOverrides[row.Path] = row.Value.Value;
            }
            else
            {
                this.whatIfOverrides.Remove(row.Path);
            }
        }

        /// <summary>
        /// Builds the list of edits to apply to the sandbox from the persisted what-if overrides.
        /// </summary>
        /// <returns>The submittable parameter values representing the overrides.</returns>
        [ExcludeFromCodeCoverage]
        private List<SubmittableParameterValue> BuildEditsFromOverrides()
        {
            var edits = new List<SubmittableParameterValue>();

            foreach (var pair in this.whatIfOverrides)
            {
                edits.Add(new SubmittableParameterValue(pair.Key, true)
                {
                    Text = pair.Value.ToString(CultureInfo.InvariantCulture)
                });
            }

            return edits;
        }

        /// <summary>
        /// Applies the what-if edits to the model's value sets in memory, snapshotting each value set first (via
        /// <see cref="SetModelManualValue"/>) so the edits can be reverted. Nothing is written to the server.
        /// Cloning is deliberately not used: <see cref="Thing.Clone(bool)"/> does not remap element-usage/definition
        /// references, so a clone would resolve to - and mutate - the same value sets anyway.
        /// </summary>
        /// <param name="edits">The what-if edits (parameter path + new value).</param>
        /// <returns>The number of edits applied.</returns>
        [ExcludeFromCodeCoverage]
        private int ApplyOverridesToModel(IReadOnlyList<SubmittableParameterValue> edits)
        {
            if (edits.Count == 0)
            {
                return 0;
            }

            var treeGenerator = new NestedElementTreeGenerator();
            var applied = new HashSet<string>();
            var diagnostics = new List<string>();

            foreach (Option option in this.Thing.Option)
            {
                var nestedParameters = treeGenerator.GetNestedParameters(option, false).ToList();

                foreach (var edit in edits)
                {
                    if (applied.Contains(edit.Path))
                    {
                        continue;
                    }

                    var path = ReportingUtilities.ConvertToOptionPath(edit.Path, option);

                    // An exact-option path only applies to the option it was created for.
                    if (edit.IsExactOptionPath && edit.Path != path)
                    {
                        continue;
                    }

                    var matches = option.GetNestedParameterValueSetsByPath(path, nestedParameters).ToList();

                    if (matches.Count == 0)
                    {
                        diagnostics.Add($"  - NOT FOUND in option '{option.ShortName}': {path}");
                        continue;
                    }

                    if (matches.Count > 1)
                    {
                        diagnostics.Add($"  - AMBIGUOUS ({matches.Count} matches) in option '{option.ShortName}': {path}");
                        continue;
                    }

                    var nestedParameter = matches.Single();

                    if (!(nestedParameter.AssociatedParameter is ParameterOrOverrideBase parameter)
                        || !(nestedParameter.ValueSet is ParameterValueSetBase valueSet))
                    {
                        diagnostics.Add($"  - unsupported value-set type for: {path}");
                        continue;
                    }

                    if (parameter.ParameterType == null || parameter.ParameterType.NumberOfValues != 1)
                    {
                        diagnostics.Add($"  - not a scalar parameter: {path}");
                        continue;
                    }

                    if (this.SetModelManualValue(valueSet, edit.Text))
                    {
                        applied.Add(edit.Path);
                        diagnostics.Add($"  - APPLIED {edit.Text} to {path}");
                    }
                    else
                    {
                        diagnostics.Add($"  - could not set value '{edit.Text}' for {path}");
                    }
                }
            }

            foreach (var edit in edits)
            {
                if (!applied.Contains(edit.Path) && diagnostics.All(d => d.IndexOf(edit.Path, StringComparison.Ordinal) < 0))
                {
                    diagnostics.Add($"  - no matching Option for path: {edit.Path}");
                }
            }

            if (diagnostics.Count > 0)
            {
                this.AddOutput("What-if apply details:\n" + string.Join("\n", diagnostics));
            }

            return applied.Count;
        }

        /// <summary>
        /// Snapshots (once) and then sets the value of a model <see cref="ParameterValueSetBase"/> from a value
        /// string. The snapshot lets the edit be reverted by <see cref="RevertModel"/>; nothing is written to the server.
        /// </summary>
        /// <param name="valueSet">The value set to update.</param>
        /// <param name="text">The value as entered in the what-if editor.</param>
        /// <returns>True if the value was set.</returns>
        [ExcludeFromCodeCoverage]
        private bool SetModelManualValue(ParameterValueSetBase valueSet, string text)
        {
            // Note: the value set's arrays may be empty for a never-valued (leaf) parameter; that is fine - only
            // scalar parameters reach here, and every array is replaced below with the single edited value.
            if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                && !double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                return false;
            }

            if (!this.whatIfSnapshots.ContainsKey(valueSet.Iid))
            {
                this.whatIfSnapshots[valueSet.Iid] = new WhatIfValueSetSnapshot(valueSet);
            }

            var newValue = value.ToString(CultureInfo.InvariantCulture);

            // Set every value array to the edited value and force the MANUAL switch, so the collector reads the
            // new value regardless of how it resolves the value set's ActualValue.
            valueSet.Manual = new ValueArray<string>(new[] { newValue });
            valueSet.Computed = new ValueArray<string>(new[] { newValue });
            valueSet.Reference = new ValueArray<string>(new[] { newValue });
            valueSet.Published = new ValueArray<string>(new[] { newValue });
            valueSet.ValueSwitch = ParameterSwitchKind.MANUAL;

            return true;
        }

        /// <summary>
        /// Reverts every value set edited by a what-if back to the snapshot taken before the edit, so the model is
        /// exactly as it was. Called on Load, Reset and when the panel closes.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void RevertModel()
        {
            foreach (var snapshot in this.whatIfSnapshots.Values)
            {
                snapshot.Restore();
            }

            this.whatIfSnapshots.Clear();
        }

        /// <summary>
        /// Captures the value arrays and switch of a <see cref="ParameterValueSetBase"/> so a what-if edit can be undone.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private sealed class WhatIfValueSetSnapshot
        {
            private readonly ParameterValueSetBase valueSet;
            private readonly ValueArray<string> manual;
            private readonly ValueArray<string> computed;
            private readonly ValueArray<string> reference;
            private readonly ValueArray<string> published;
            private readonly ParameterSwitchKind valueSwitch;

            public WhatIfValueSetSnapshot(ParameterValueSetBase valueSet)
            {
                this.valueSet = valueSet;
                this.manual = valueSet.Manual;
                this.computed = valueSet.Computed;
                this.reference = valueSet.Reference;
                this.published = valueSet.Published;
                this.valueSwitch = valueSet.ValueSwitch;
            }

            public void Restore()
            {
                this.valueSet.Manual = this.manual;
                this.valueSet.Computed = this.computed;
                this.valueSet.Reference = this.reference;
                this.valueSet.Published = this.published;
                this.valueSet.ValueSwitch = this.valueSwitch;
            }
        }

        /// <summary>
        /// Enables or disables DevExpress content editing on the report's submittable cells (those bound to a
        /// parameter path via a <c>Tag</c> expression binding), so the user can type new values in the preview.
        /// </summary>
        /// <param name="enabled">Whether editing should be enabled.</param>
        [ExcludeFromCodeCoverage]
        private void SetPreviewEditingEnabled(bool enabled)
        {
            var report = this.CurrentReport;

            if (report == null)
            {
                return;
            }

            foreach (var control in report.AllControls<XRLabel>())
            {
                var bindings = control.ExpressionBindings.Cast<ExpressionBinding>().ToList();

                // A cell is a submit target when it carries a parameter-path Tag binding.
                var isSubmittable = bindings.Any(binding =>
                    string.Equals(binding.PropertyName, "Tag", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(binding.Expression)
                    && binding.Expression.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0);

                // A cell is directly editable when its Text is bound to a single data field (e.g. [Mass]);
                // cells bound to computed expressions (e.g. sumSum(...)) cannot be content-edited by DevExpress.
                var textBinding = bindings.FirstOrDefault(binding => string.Equals(binding.PropertyName, "Text", StringComparison.OrdinalIgnoreCase));
                var isDataBound = IsBareFieldExpression(textBinding?.Expression);

                if (isSubmittable || isDataBound)
                {
                    control.EditOptions.Enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Determines whether an expression is a single bare data-field reference such as <c>[Mass]</c>.
        /// </summary>
        /// <param name="expression">The expression to test.</param>
        /// <returns>True when the expression is exactly one field reference.</returns>
        [ExcludeFromCodeCoverage]
        private static bool IsBareFieldExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            var trimmed = expression.Trim();

            return trimmed.Length > 2
                   && trimmed[0] == '['
                   && trimmed[trimmed.Length - 1] == ']'
                   && trimmed.IndexOf('[', 1) < 0;
        }

        /// <summary>
        /// Applies the requested edit-mode state: toggles content editing on the report's editable cells and
        /// re-renders the preview against the current model so the change takes effect (editing only becomes
        /// available in a freshly generated document).
        /// </summary>
        /// <param name="enabled">Whether editing should be enabled.</param>
        [ExcludeFromCodeCoverage]
        private void ApplyEditMode(bool enabled)
        {
            this.SetPreviewEditingEnabled(enabled);

            if (this.CurrentReport == null || this.ReportScriptHandler.CurrentDataCollector == null)
            {
                return;
            }

            this.IsBusy = true;

            try
            {
                this.AddOutput(enabled
                    ? "Reloading the preview for editing (you may be asked to select the Option). Please wait until this says 'ready'..."
                    : "Reloading the preview...");

                // Re-render the preview (against the real model) so the editable fields appear.
                this.ReportScriptHandler.RebuildDataSource(this.Thing, this.Session, true);
                this.TriggerRefreshUI();

                this.AddOutput(enabled
                    ? "Edit mode is ready. The report's editable cells can now be changed in the preview - if a cell is not directly editable, use the 'Editing Fields' button in the Document ribbon group. Change values, then click Recalculate."
                    : "Edit mode OFF - preview reloaded.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to toggle edit mode.");
                this.AddOutput($"Failed to toggle edit mode: {ex.Message}");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Triggers the UI of the Reporting Designer to refresh itself.
        /// Currently implemented using a call to MakeChanges that adds a temporary datasource and removes it immediately.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void TriggerRefreshUI()
        {
            this.currentReportDesignerDocument?.MakeChanges(changes =>
            {
                var refreshDataSource = new ObjectDataSource
                {
                    DataSource = new object(),
                    Name = "__temporaryDataSource__"
                };

                var refreshParameter = new Parameter
                {
                    Name = "__temporaryParameter__"
                };

                var triedAddDataSource = false;
                var triedAddParameter = false;

                try
                {
                    changes.AddItem(refreshDataSource);
                    triedAddDataSource = true;
                    changes.RemoveItem(refreshDataSource);
                    changes.AddItem(refreshParameter);
                    triedAddParameter = true;
                    changes.RemoveItem(refreshParameter);
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        if (triedAddDataSource)
                        {
                            changes.RemoveItem(refreshDataSource);
                        }

                        if (triedAddParameter)
                        {
                            changes.RemoveItem(refreshParameter);
                        }
                    }
                    catch
                    {
                    }
                }
            });
        }

        /// <summary>
        /// Add text to the output pane
        /// </summary>
        /// <param name="text">The text</param>
        private void AddOutput(string text)
        {
            this.Output += $"{DateTime.Now:HH:mm:ss} {text}{Environment.NewLine}";
        }

        /// <summary>
        /// Checks the report if dynamic tables need to be updated.
        /// </summary>
        /// <param name="sender">
        /// The sender
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/>
        /// </param>
        private void CheckDynamicTables(object sender, EventArgs e)
        {
            var report = sender as XtraReport;

            if (this.ReportScriptHandler != null)
            {
                this.dynamicTableChecker.Check(report, this.ReportScriptHandler.CurrentDataCollector);
            }
        }

        /// <summary>
        /// Check if the previewed/printed <see cref="XtraReport"/> contains any submittable parameter values.
        /// </summary>
        /// <param name="sender">
        /// The sender
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/>
        /// </param>
        private void CheckSubmittableParameterValues(object sender, EventArgs e)
        {
            var report = sender as XtraReport;

            this.submittableParameterValues = this.submittableParameterValuesCollector.Collect(report);

            this.CanSubmitParameterValues = this.submittableParameterValues.Any();
        }

        /// <summary>
        /// Executes the <see cref="SubmitParameterValuesCommand"/>
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task SubmitParameterValues()
        {
            if (!(this.ReportScriptHandler.CurrentDataCollector is IOptionDependentDataCollector optionDependentDataCollector))
            {
                return;
            }

            // A what-if scenario edits the model in memory; those values feed the submit. Guard against saving
            // exploratory what-if numbers by accident: warn while a scenario is active (edits applied to the model)
            // and let the user cancel. Use End What-if Scenario / Reset first to submit only real model values.
            if (this.whatIfSnapshots.Count > 0)
            {
                var confirmation = this.messageBoxService.Show(
                    "A what-if scenario is active, so its in-memory edits will be saved to the model by this submit. "
                    + "Continue and save the what-if values, or cancel and use 'End What-if Scenario' / 'Reset' first to submit only the original values?",
                    "Submit what-if values?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel);

                if (confirmation != MessageBoxResult.OK)
                {
                    return;
                }
            }

            var processedValueSets = this.GetProcessedValueSets(out var errorTexts);

            if (errorTexts.Any())
            {
                var okDialogViewModel = new OkDialogViewModel("Warning", $"The following errors were found during ValueSet lookup:\n\n {string.Join("\n", errorTexts)}");
                this.DialogNavigationService.NavigateModal(okDialogViewModel);
            }

            if (!processedValueSets.Any())
            {
                var okDialogViewModel = new OkDialogViewModel("Info", "No parameter changes found.");
                this.DialogNavigationService.NavigateModal(okDialogViewModel);

                return;
            }

            await this.ProcessProcessedValueSets(processedValueSets, optionDependentDataCollector.Iteration, optionDependentDataCollector.Session);
        }

        /// <summary>
        /// Gets a < see cref="Dictionary{Guid, ProcessedValueSet}"/> that contains the <see cref="ProcessedValueSet"/>s that were created using data
        /// from the <see cref="submittableParameterValues"/> field.
        /// </summary>
        /// <param name="errorTexts">
        /// A <see cref="List{String}"/> that contains text about all problems during the process
        /// </param>
        /// <returns>
        /// The < see cref="Dictionary{Guid, ProcessedValueSet}"/>.
        /// </returns>
        private Dictionary<Guid, ProcessedValueSet> GetProcessedValueSets(out List<string> errorTexts)
        {
            errorTexts = new List<string>();
            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();

            if (!(this.ReportScriptHandler.CurrentDataCollector is IOptionDependentDataCollector optionDependentDataCollector))
            {
                errorTexts.Add($"CurrentDataCollector should be of type {nameof(IOptionDependentDataCollector)}");
                return processedValueSets;
            }

            if (!this.submittableParameterValues.Any())
            {
                return processedValueSets;
            }

            foreach (Option option in optionDependentDataCollector.Iteration.Option)
            {
                var nestedElementTree = new NestedElementTreeGenerator();

                var allNestedParameters = nestedElementTree.GetNestedParameters(option).ToList();

                var ownedNestedParameters = nestedElementTree.GetNestedParameters(
                    option, optionDependentDataCollector.DomainOfExpertise).ToList();

                var processedValueSetGenerator = new ProcessedValueSetGenerator(optionDependentDataCollector);

                foreach (var submittableParameter in this.submittableParameterValues)
                {
                    if (!submittableParameter.IsExactOptionPath && option != optionDependentDataCollector.SelectedOption)
                    {
                        continue;
                    }

                    if (processedValueSetGenerator
                        .TryGetProcessedValueSet(
                            option,
                            allNestedParameters,
                            ownedNestedParameters,
                            submittableParameter,
                            ref processedValueSets,
                            out var errorText))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(errorText))
                    {
                        errorTexts.Add(errorText);
                    }
                }
            }

            return processedValueSets;
        }

        /// <summary>
        /// Processes <see cref="ProcessedValueSet"/>s and submits changed values back to the model.
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="Dictionary{Guid, ProcessedValueSet}"/> that contains the <see cref="ProcessedValueSet"/>s to process.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <returns>
        /// An awaitable <see cref="Task"/>
        /// </returns>
        private async Task ProcessProcessedValueSets(Dictionary<Guid, ProcessedValueSet> processedValueSets, Iteration iteration, ISession session)
        {
            try
            {
                var submitConfirmationViewModel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);
                var dialogResult = this.DialogNavigationService.NavigateModal(submitConfirmationViewModel);

                if ((dialogResult?.Result.HasValue ?? false) && dialogResult.Result.Value)
                {
                    var submitConfirmationDialogResult = (SubmitConfirmationDialogResult)dialogResult;

                    var context = TransactionContextResolver.ResolveContext(iteration);
                    var transaction = new ThingTransaction(context);

                    foreach (var clone in submitConfirmationDialogResult.Clones)
                    {
                        transaction.CreateOrUpdate(clone);
                    }

                    var operationContainer = transaction.FinalizeTransaction();

                    await session.Write(operationContainer);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error while trying to submit data to the model");

                var okDialogViewModel = new OkDialogViewModel("Error", $"Error while trying to submit data to the model:\n\n{ex.Message}");
                this.DialogNavigationService.NavigateModal(okDialogViewModel);
            }
        }

        /// <summary>
        /// The logic to be executed after handling an OnClosing event
        /// </summary>
        /// <remarks>
        /// The reason this method is executed here is because DevExpress implements its own OnClosing logic for a changed Report Designer canvas.
        /// Since we handle saving all report data ourselves, we don't want this logic to be executed.
        /// When this code gets hit we can assume that the user already answered a question about these changes and we can suppress DevExpress'
        /// message by setting the <see cref="ReportDesignerDocument.HasChangesProperty"/> to false.
        /// </remarks>
        public void AfterOnClosing()
        {
            // Undo any in-memory what-if edits so the panel never leaves the model dirty when it closes.
            this.RevertModel();

            this.currentReportDesignerDocument?.SetValue(ReportDesignerDocument.HasChangesProperty, false);
        }

        /// <summary>
        /// Disposes the view-model, reverting any in-memory what-if edits so the model is never left dirty.
        /// </summary>
        /// <param name="disposing">Whether managed resources are being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.RevertModel();
            }

            base.Dispose(disposing);
        }
    }
}
