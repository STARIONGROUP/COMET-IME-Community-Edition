// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Reporting.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

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

    using CDP4Reporting.SubmittableParameterValues;
    using CDP4Reporting.Utilities;

    using Microsoft.Practices.ServiceLocation;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Printing;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.UI;

    using ICSharpCode.AvalonEdit.Document;

    using NLog;

    using ReactiveUI;

    using CDP4Reporting.DataCollection;
    using CDP4Reporting.DynamicTableChecker;
    using CDP4Reporting.Events;
    using CDP4Reporting.ReportScript;

    using DevExpress.Data.Helpers;

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
        public ReactiveCommand<object> ImportScriptCommand { get; set; }

        /// <summary>
        /// Saves code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<object> ExportScriptCommand { get; set; }

        /// <summary>
        /// Build code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<Unit> CompileScriptCommand { get; set; }

        /// <summary>
        /// Create a new Report 
        /// </summary>
        public ReactiveCommand<object> NewReportCommand { get; set; }

        /// <summary>
        /// Open rep4 zip archive which consists in datasource code file and report designer file
        /// </summary>
        public ReactiveCommand<object> OpenReportCommand { get; set; }

        /// <summary>
        /// Save editor code and report designer to rep4 zip archive
        /// </summary>
        public ReactiveCommand<object> SaveReportCommand { get; set; }

        /// <summary>
        /// Save editor code and report designer to rep4 zip archive and force the SaveFile dialog to be shown
        /// </summary>
        public ReactiveCommand<object> SaveReportAsCommand { get; set; }

        /// <summary>
        /// Fires when the DataSource text was changed
        /// </summary>
        public ReactiveCommand<object> DataSourceTextChangedCommand { get; set; }

        /// <summary>
        /// Rebuild the DataSource
        /// </summary>
        public ReactiveCommand<Unit> RebuildDatasourceCommand { get; set; }

        /// <summary>
        /// Rebuild the DataSource and refresh the preview panel
        /// </summary>
        public ReactiveCommand<Unit> RebuildDatasourceAndRefreshPreviewCommand { get; set; }

        /// <summary>
        /// Submit data from a previewed report
        /// </summary>
        public ReactiveCommand<Unit> SubmitParameterValuesCommand { get; set; }

        /// <summary>
        /// Fires when the DataSource text needs to be cleared
        /// </summary>
        public ReactiveCommand<object> ClearOutputCommand { get; set; }

        /// <summary>
        /// Fires when the Active Document changes in the Report Designer
        /// </summary>
        public ReactiveCommand<Unit> ActiveDocumentChangedCommand { get; set; }

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

            this.ExportScriptCommand = ReactiveCommand.Create();
            this.ExportScriptCommand.Subscribe(_ => this.ExportScript());

            this.ImportScriptCommand = ReactiveCommand.Create();
            this.ImportScriptCommand.Subscribe(_ => this.ImportScript());

            this.CompileScriptCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var source = this.Document.Text;
                await this.compilationConcurrentActionRunner.RunAction(() => this.ReportScriptHandler.CompileAssembly(source));
            });

            this.NewReportCommand = ReactiveCommand.Create();
            this.NewReportCommand.Subscribe(_ => this.CreateNewReport());

            this.OpenReportCommand = ReactiveCommand.Create();
            this.OpenReportCommand.Subscribe(_ => this.OpenReportProject());

            this.SaveReportCommand = ReactiveCommand.Create();
            this.SaveReportCommand.Subscribe(_ => this.SaveReportProject());

            this.SaveReportAsCommand = ReactiveCommand.Create();
            this.SaveReportAsCommand.Subscribe(_ => this.SaveReportProject(true));

            this.DataSourceTextChangedCommand = ReactiveCommand.Create();
            this.DataSourceTextChangedCommand.Subscribe(_ => this.CheckAutoCompileScript());

            this.RebuildDatasourceCommand = ReactiveCommand.CreateAsyncTask(async _ => await this.ExecuteRebuildDatasourceCommand());
            this.RebuildDatasourceAndRefreshPreviewCommand = ReactiveCommand.CreateAsyncTask(async _ => await this.ExecuteRebuildDatasourceAndRefreshPreviewCommand());

            this.SubmitParameterValuesCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CanSubmitParameterValues),
                x => this.SubmitParameterValues(),
                RxApp.MainThreadScheduler);

            this.ClearOutputCommand = ReactiveCommand.Create();
            this.ClearOutputCommand.Subscribe(_ => { this.Output = string.Empty; });

            this.ActiveDocumentChangedCommand = ReactiveCommand.CreateAsyncTask(x =>
                this.SetReportDesigner(((DependencyPropertyChangedEventArgs)x).NewValue), RxApp.MainThreadScheduler);

            this.WhenAnyValue(x => x.CurrentReport).Subscribe(x =>
            {
                x.AfterPrint += this.CheckSubmittableParameterValues;
                x.DataSourceDemanded += this.CheckDynamicTables;
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
                CDPMessageBus.Current.Listen<ReportOutputEvent>()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.AddOutput(x.Output))
            );

            this.InitializeDataSetExtensionsUsage();
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

            this.Document = new TextDocument(string.Empty);
            this.lastSavedDataSourceText = "";

            this.CurrentReport = GetNewXtraReport();
            this.ReportScriptHandler = new ReportScriptHandler<XtraReport, Parameter>(new XtraReportHandler(this.CurrentReport), new CodeDomCodeCompiler(this.AddOutput), x => this.Errors = x, this.AddOutput);

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

            this.ReportScriptHandler = new ReportScriptHandler<XtraReport, Parameter>(new XtraReportHandler(report), new CodeDomCodeCompiler(this.AddOutput), x => this.Errors = x, this.AddOutput);

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
                finally
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

                try
                {
                    changes.AddItem(refreshDataSource);
                    changes.RemoveItem(refreshDataSource);
                    changes.AddItem(refreshParameter);
                    changes.RemoveItem(refreshParameter);
                }
                finally
                {
                    //Ignore when this happend
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
            this.currentReportDesignerDocument?.SetValue(ReportDesignerDocument.HasChangesProperty, false);
        }
    }
}
