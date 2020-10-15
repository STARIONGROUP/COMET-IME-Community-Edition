// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Reporting.ViewModels
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reactive;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using CDP4Reporting.DataCollection;
    using CDP4Reporting.Parameters;
    using CDP4Reporting.Utilities;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.Parameters;
    using DevExpress.XtraReports.UI;

    using ICSharpCode.AvalonEdit.Document;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    using File = System.IO.File;
    using Parameter = DevExpress.XtraReports.Parameters.Parameter;

    /// <summary>
    /// The view-model for the Report Designer that lets users to create reports based on template source files.
    /// </summary>
    public partial class ReportPreviewViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Reporting";

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
        private XtraReport currentReport = new XtraReport();

        /// <summary>
        /// The currently active <see cref="TextDocument"/> in the Avalon Editor
        /// </summary>
        private TextDocument document;

        /// <summary>
        /// The currently active <see cref="ReportDesignerDocument"/> in the Report Designer
        /// </summary>
        private ReportDesignerDocument currentReportDesignerDocument;

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
        /// Fires when the DataSource text was changed
        /// </summary>
        public ReactiveCommand<Unit> RebuildDatasourceCommand { get; set; }

        /// <summary>
        /// Fires when the DataSource text needs to be cleared
        /// </summary>
        public ReactiveCommand<object> ClearOutputCommand { get; set; }

        /// <summary>
        /// Fires when the Active Document changes in the Report Designer
        /// </summary>
        public ReactiveCommand<Unit> ActiveDocumentChangedCommand { get; set; }

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
        /// Gets or sets the Report Designer's current Report
        /// </summary>
        public ObservableCollection<ReportPreviewItemViewModel> Reports
        {
            get => this.reports;
            set => this.RaiseAndSetIfChanged(ref this.reports, value);
        }

        public class ReportPreviewItemViewModel : ReactiveObject
        {
            private XtraReport report;

            /// <summary>
            /// Gets or sets the Report Designer's current Report
            /// </summary>
            public XtraReport Report
            {
                get => this.report;
                set => this.RaiseAndSetIfChanged(ref this.report, value);
            }

            public ReportPreviewItemViewModel(ReportPreviewViewModel reportPreviewViewModel)
            {
                this.OpenReportCommand = ReactiveCommand.Create();
                this.OpenReportCommand.Subscribe(_ => reportPreviewViewModel.OpenReportProject());

                reportPreviewViewModel.OpenReportProject();

                this.Report = reportPreviewViewModel.CurrentReport;

                Dispatcher.CurrentDispatcher.InvokeAsync(() => this.Report.CreateDocument(), DispatcherPriority.Background);
            }

            public ReactiveCommand<object> OpenReportCommand { get; set; }
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
        /// Backing field for <see cref="Errors" />
        /// </summary>
        private string errors;

        /// <summary>
        /// Gets or sets value for editor's errors
        /// </summary>
        public string Errors
        {
            get => this.errors;
            set => this.RaiseAndSetIfChanged(ref this.errors, value);
        }

        /// <summary>
        /// Backing field for <see cref="Output" />
        /// </summary>
        private string output;

        /// <summary>
        /// Gets or sets value for output's log messages
        /// </summary>
        public string Output
        {
            get => this.output;
            set => this.RaiseAndSetIfChanged(ref this.output, value);
        }

        /// <summary>
        /// Gets or sets build output result
        /// </summary>
        public CompilerResults CompileResult { get; private set; }

        /// <summary>
        /// Backing field for <see cref="IsAutoCompileEnabled" />
        /// </summary>
        private bool isAutoCompileEnabled;

        /// <summary>
        /// The last saved datasource text
        /// </summary>
        private string lastSavedDataSourceText = string.Empty;

        private ObservableCollection<ReportPreviewItemViewModel> reports = new ObservableCollection<ReportPreviewItemViewModel>();

        /// <summary>
        /// Gets or sets a value indicating whether automatically build is checked
        /// </summary>
        public bool IsAutoCompileEnabled
        {
            get => this.isAutoCompileEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isAutoCompileEnabled, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerViewModel"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to display</param>
        /// <param name="session">The session.</param>
        /// <param name="thingDialogNavigationService">The thing navigation service.</param>
        /// <param name="panelNavigationService">The panel navigation service.</param>
        /// <param name="dialogNavigationService">The dialog navigation service.</param>
        /// <param name="pluginSettingsService">The plugin service.</param>
        public ReportPreviewViewModel(Iteration thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

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
                await this.compilationConcurrentActionRunner.RunAction(() => this.CompileAssembly(source));
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

            this.RebuildDatasourceCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                this.IsBusy = true;

                try
                {
                    var source = this.Document.Text;
                    await this.compilationConcurrentActionRunner.RunAction(() => this.CompileAssembly(source));

                    if (!this.CompileResult?.Errors.HasErrors ?? false)
                    {
                            this.RebuildDataSource();
                    }
                }
                finally
                {
                    this.IsBusy = false;
                }
            });

            this.ClearOutputCommand = ReactiveCommand.Create();
            this.ClearOutputCommand.Subscribe(_ => { this.Output = string.Empty; });

            this.ActiveDocumentChangedCommand = ReactiveCommand.CreateAsyncTask(x =>
                this.SetReportDesigner(((DependencyPropertyChangedEventArgs) x).NewValue), RxApp.MainThreadScheduler);

            this.InitializeDataSetExtensionsUsage();

            Dispatcher.CurrentDispatcher.InvokeAsync(() => this.Reports.Add(new ReportPreviewItemViewModel(this)));
            Dispatcher.CurrentDispatcher.InvokeAsync(() => this.Reports.Add(new ReportPreviewItemViewModel(this)));
        }

        /// <summary>
        /// Method that is here that does nothing, but makes sure that System.Data.DataSetExtensions.dll is
        /// available in the report script
        /// </summary>
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
            await Task.Run(() => this.currentReportDesignerDocument = (ReportDesignerDocument) newValue);
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
            this.IsDirty = true;
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

            this.compilationConcurrentActionRunner.DelayRunAction(() => this.CompileAssembly(this.Document.Text), 2500);
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
            this.CurrentReport = new XtraReport();
            this.CurrentReportProjectFilePath = string.Empty;
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

            using (var reportZipArchive = this.GetReportZipArchive(reportProjectFilePath))
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

                    this.CompileAssembly(this.Document.Text);
                }
            }

            this.CurrentReport = report;
            this.CurrentReportProjectFilePath = reportProjectFilePath;

            this.RebuildDataSource();

            this.lastSavedDataSourceText = this.Document.Text;
            this.currentReportDesignerDocument?.SetValue(ReportDesignerDocument.HasChangesProperty, false);
        }

        /// <summary>
        /// Checks if creating a new report, or opening an existing report is allowed.
        /// </summary>
        /// <returns>true if allowed, otherwise false. </returns>
        private bool IsSwitchReportProjectAllowed()
        {
            if ((bool)(this.currentReportDesignerDocument?.GetValue(ReportDesignerDocument.HasChangesProperty) ?? false) || !this.lastSavedDataSourceText.Equals(this.Document.Text))
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

                using (var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(this.Document.Text)))
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
        /// Rebuild the report's datasource
        /// </summary>
        private void RebuildDataSource()
        {
            const string dataSourceName = "ReportDataSource";

            var reportDataSource = 
                this.CurrentReport.ComponentStorage.OfType<ObjectDataSource>()
                    .FirstOrDefault(x => x.Name.Equals(dataSourceName));
            
            object dataSource;

            try
            {
                dataSource = this.GetDataSource();
            }
            catch (Exception ex)
            {
                this.Errors = $"{ex.Message}\n{ex.StackTrace}";
                return;
            }

            var parameters = this.GetParameters(dataSource);

            if (parameters?.Any() ?? false)
            {
                var filterString = this.GetFilterString(parameters);

                this.currentReport.FilterString = filterString;

                this.CheckParameters(parameters);
            }

            if (reportDataSource == null)
            {
                // Create new datasource
                reportDataSource = new ObjectDataSource
                {
                    DataSource = dataSource,
                    Name = dataSourceName
                };

                this.CurrentReport.ComponentStorage.Add(reportDataSource);
                this.CurrentReport.DataSource = reportDataSource;

                // Add dynamic parameter to report definition
                this.currentReportDesignerDocument?.MakeChanges(
                    changes => { changes.AddItem(reportDataSource); });
            }
            else
            {
                // Use existing datasource
                reportDataSource.DataSource = dataSource;
                reportDataSource.RebuildResultSchema();

                this.TriggerRefreshReportingDesigner();
            }
        }

        /// <summary>
        /// Triggers the UI of the Reporting Designer to refresh itself.
        /// Currently implemented using a call to MakeChanges that adds a temporary datasource and removes it immediately.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void TriggerRefreshReportingDesigner()
        {
            this.currentReportDesignerDocument?.MakeChanges(changes =>
            {
                var refreshDataSource = new ObjectDataSource
                {
                    DataSource = new object(),
                    Name = "__temporaryDataSource__"
                };

                changes.AddItem(refreshDataSource);
                changes.RemoveItem(refreshDataSource);
            });
        }

        /// <summary>
        /// Get the data representation for the report
        /// </summary>
        /// <returns>The datasource as an <see cref="object"/></returns>
        private object GetDataSource()
        {
            if (this.CompileResult == null)
            {
                this.AddOutput("Build data source code first.");
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.CompileResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.GetInterfaces()
                            .Any(i => i == typeof(IDataCollector))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    this.AddOutput($"No class that implements from {nameof(IDataCollector)} was found.");
                    return null;
                }

                var instObj = this.CompileResult.CompiledAssembly.CreateInstance(editorFullClassName) as IDataCollector;

                if (instObj == null)
                {
                    this.AddOutput("DataCollector class not found.");
                    return null;
                }

                if (instObj is IReportScriptDataProvider reportScriptDataProvider)
                {
                    reportScriptDataProvider.Initialize(this.Thing, this.Session);
                }

                return instObj.CreateDataObject();
            }
            catch (Exception ex)
            {
                this.Errors = $"{ex.Message}\n{ex.StackTrace}";
                throw;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.AssemblyResolver;
            }
        }

        /// <summary>
        /// Check if the compiled assembly built from the code editor contains a class that implements the <see cref="IReportingParameters"/> interface
        /// If true, then create report parameters using that class.
        /// </summary>
        /// <param name="parameters">
        /// The <see cref="IEnumerable{IReportingParameter}"/> that contains the report's parameters
        /// </param>
        private void CheckParameters(IEnumerable<IReportingParameter> parameters)
        {
            var reportingParameters = parameters.ToList();

            var toBeRemoved = new List<Parameter>();
            var previouslySetValues = new Dictionary<string, object>();

            //Find existing dynamic parameters
            foreach (var reportParameter in this.CurrentReport.Parameters)
            {
                if (reportParameter.Name.StartsWith(ReportingParameter.NamePrefix))
                {
                    previouslySetValues.Add(reportParameter.Name, reportParameter.Value);
                    toBeRemoved.Add(reportParameter);
                }
            }

            // Remove old dynamic parameters
            if (toBeRemoved.Any())
            {
                foreach (var reportParameter in toBeRemoved)
                {
                    this.currentReportDesignerDocument?.MakeChanges(
                        changes => { changes.RemoveItem(reportParameter); });
                }
            }

            if (!(reportingParameters.Any()))
            {
                return;
            }

            // Create new dynamic parameters
            foreach (var reportingParameter in reportingParameters)
            {
                var previouslySetValue =
                    reportingParameter.Visible && previouslySetValues.ContainsKey(reportingParameter.ParameterName)
                        ? previouslySetValues[reportingParameter.ParameterName]
                        : null;

                this.CreateDynamicParameter(reportingParameter, previouslySetValue);
            }
        }

        /// <summary>
        /// Create a dynamic parameter, set its default value based on previously set values and add it to the current Report
        /// </summary>
        /// <param name="reportingParameter">The <see cref="IReportingParameter"/></param>
        /// <param name="previouslySetValue">The previously set value in the report designer.</param>
        private void CreateDynamicParameter(IReportingParameter reportingParameter, object previouslySetValue)
        {
            var newReportParameter = new Parameter
            {
                Name = reportingParameter.ParameterName,
                Description = reportingParameter.Name,
                Type = reportingParameter.Type,
                Visible = true
            };

            if (reportingParameter.LookUpValues.Any())
            {
                var staticListLookupSettings = new StaticListLookUpSettings();
                newReportParameter.LookUpSettings = staticListLookupSettings;

                foreach (var keyValuePair in reportingParameter.LookUpValues)
                {
                    staticListLookupSettings.LookUpValues.Add(new LookUpValue(keyValuePair.Key, keyValuePair.Value));
                }
            }

            // Restore default values
            if (previouslySetValue!= null)
            {
                newReportParameter.Value = previouslySetValue;
            }
            else if (reportingParameter.DefaultValue != null)
            {
                newReportParameter.Value = reportingParameter.DefaultValue;
            }

            newReportParameter.Visible = reportingParameter.Visible;

            // Add dynamic parameter to report definition
            this.currentReportDesignerDocument?.MakeChanges(
                changes => { changes.AddItem(newReportParameter); });
        }

        /// <summary>
        /// Check if the compiled assembly built from the code editor contains a class that implements the <see cref="IReportingParameters"/> interface
        /// If true, then execute the class' <see cref="IReportingParameters.CreateParameters"/> method.
        /// </summary>
        /// <param name="dataSource">The datasource, which could be used to create parameters</param>
        /// <returns>The <see cref="IEnumerable{IReportingParameter}"/></returns>
        private IReadOnlyList<IReportingParameter> GetParameters(object dataSource)
        {
            var result = new List<IReportingParameter>();

            if (this.CompileResult == null)
            {
                this.AddOutput("Compile data source code first.");
                return result;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.CompileResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.GetInterfaces()
                            .Any(i => i == typeof(IReportingParameters))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    return result;
                }

                if (!(this.CompileResult.CompiledAssembly.CreateInstance(editorFullClassName) is IReportingParameters instObj))
                {
                    this.AddOutput("Report parameter class not found.");
                    return result;
                }

                if (instObj is IReportScriptDataProvider reportScriptDataProvider)
                {
                    reportScriptDataProvider.Initialize(this.Thing, this.Session);
                }

                result = instObj.CreateParameters(dataSource)?.ToList();
            }
            catch (Exception ex)
            {
                this.AddOutput(ex.ToString());
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.AssemblyResolver;
            }

            return result;
        }

        /// <summary>
        /// Check if the compiled assembly built from the code editor contains a class that implements the <see cref="IReportingParameters"/> interface
        /// If true, then execute the class' <see cref="IReportingParameters.CreateFilterString"/> method.
        /// </summary>
        /// <param name="parameters">The <see cref="IEnumerable{IReportingParameter}"/>, which could be used to create a filter string</param>
        /// <returns>The create Filter string in <see cref="IReportingParameters.CreateFilterString"/></returns>.
        private string GetFilterString(IEnumerable<IReportingParameter> parameters)
        {
            if (this.CompileResult == null)
            {
                this.AddOutput("Compile data source code first.");
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.CompileResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.GetInterfaces()
                            .Any(i => i == typeof(IReportingParameters))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    return null;
                }

                if (!(this.CompileResult.CompiledAssembly.CreateInstance(editorFullClassName) is IReportingParameters instObj))
                {
                    this.AddOutput("Report parameter class not found.");
                    return null;
                }

                return instObj.CreateFilterString(parameters);
            }
            catch (Exception ex)
            {
                this.AddOutput(ex.ToString());
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.AssemblyResolver;
            }

            return null;
        }

        /// <summary>
        /// Needed for using the CDP4Reporting assembly
        /// </summary>
        /// <param name="sender">The sender <see cref="object"/></param>
        /// <param name="args">The <see cref="ResolveEventArgs"/></param>
        /// <returns></returns>
        private Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            if (args.Name == this.GetType().Assembly.FullName)
            {
                return this.GetType().Assembly;
            }

            return null;
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
        /// Get report zip archive components
        /// </summary>
        /// <param name="rep4File">archive zip file</param>
        /// <returns>The <see cref="ReportZipArchive"/></returns>
        private ReportZipArchive GetReportZipArchive(string rep4File)
        {
            using (var zipFile = ZipFile.OpenRead(rep4File))
            {
                var repxStream = new MemoryStream();
                var dataSourceStream = new MemoryStream();

                zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".repx"))?.Open().CopyTo(repxStream);
                repxStream.Position = 0;
                zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".cs"))?.Open().CopyTo(dataSourceStream);
                dataSourceStream.Position = 0;

                return new ReportZipArchive
                {
                    ReportDefinition = repxStream,
                    DataSourceCode = dataSourceStream
                };
            }
        }

        /// <summary>
        /// Execute compilation of the code in the Code Editor
        /// </summary>
        private void CompileAssembly(string source)
        {
            try
            {
                this.Errors = string.Empty;

                if (string.IsNullOrEmpty(source))
                {
                    this.AddOutput("Nothing to compile.");
                    return;
                }

                var compiler = new Microsoft.CSharp.CSharpCodeProvider();

                var parameters = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };

                var currentAssemblies =
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => !x.IsDynamic)
                        .Select(x => x.Location)
                        .ToArray();

                parameters.ReferencedAssemblies.AddRange(currentAssemblies);

                this.CompileResult = compiler.CompileAssemblyFromSource(parameters, source);

                if (this.CompileResult.Errors.Count == 0)
                {
                    this.AddOutput("File succesfully compiled.");
                    this.Errors = string.Empty;
                    return;
                }

                var sbErrors = new StringBuilder($"{DateTime.Now:HH:mm:ss} Compilation Errors:");

                foreach (var error in this.CompileResult.Errors)
                {
                    sbErrors.AppendLine(error.ToString());
                }

                this.Errors = sbErrors.ToString();
            }
            catch (Exception ex)
            {
                var exception = ex;

                while (exception != null)
                {
                    this.AddOutput($"{ex.Message}\\n{ex.StackTrace}\\n");
                    exception = exception.InnerException;
                }
            }
        }
    }
}
