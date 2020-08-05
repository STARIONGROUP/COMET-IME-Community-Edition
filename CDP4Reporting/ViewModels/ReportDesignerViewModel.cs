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
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reactive;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using CDP4Reporting.DataSource;
    using CDP4Reporting.Parameters;
    using CDP4Reporting.Utilities;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.Parameters;
    using DevExpress.XtraReports.UI;

    using ICSharpCode.AvalonEdit.Document;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using ReactiveUI;

    using File = System.IO.File;
    using Parameter = DevExpress.XtraReports.Parameters.Parameter;

    /// <summary>
    /// The view-model for the Report Designer that lets users to create reports based on template source files.
    /// </summary>
    public partial class ReportDesignerViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
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
        public ReactiveCommand<object> OpenScriptCommand { get; set; }

        /// <summary>
        /// Saves code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<object> SaveScriptCommand { get; set; }

        /// <summary>
        /// Build code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<object> BuildScriptCommand { get; set; }

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
        public ReactiveCommand<object> RebuildDatasourceCommand { get; set; }

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
        /// Gets or sets current edited file path
        /// </summary>
        public string CodeFilePath { get; set; }

        /// <summary>
        /// Gets or sets current archive zip file path that contains resx report designer file and datasource c# file
        /// </summary>
        public string currentReportProjectFilePath { get; set; }

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
        public CompilerResults BuildResult { get; private set; }

        /// <summary>
        /// Backing field for <see cref="IsAutoBuildEnabled" />
        /// </summary>
        private bool isAutoBuildEnabled;

        /// <summary>
        /// The last saved datasource text
        /// </summary>
        private string lastSavedDataSourceText = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether automatically build is checked
        /// </summary>
        public bool IsAutoBuildEnabled
        {
            get => this.isAutoBuildEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isAutoBuildEnabled, value);
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
        public ReportDesignerViewModel(Iteration thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.Document = new TextDocument();
            this.Errors = string.Empty;
            this.Output = string.Empty;
            this.IsAutoBuildEnabled = false;

            this.openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.SaveScriptCommand = ReactiveCommand.Create();
            this.SaveScriptCommand.Subscribe(_ => this.SaveScript());

            this.OpenScriptCommand = ReactiveCommand.Create();
            this.OpenScriptCommand.Subscribe(_ => this.OpenScript());

            this.BuildScriptCommand = ReactiveCommand.Create();

            this.BuildScriptCommand.Subscribe(_ =>
            {
                var source = this.Document.Text;
                this.compilationConcurrentActionRunner.RunAction(() => this.CompileAssembly(source));
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
            this.DataSourceTextChangedCommand.Subscribe(_ => this.CheckAutoBuildScript());

            this.RebuildDatasourceCommand = ReactiveCommand.Create();
            this.RebuildDatasourceCommand.Subscribe(_ => this.RebuildDataSource());

            this.ClearOutputCommand = ReactiveCommand.Create();
            this.ClearOutputCommand.Subscribe(_ => { this.Output = string.Empty; });

            this.ActiveDocumentChangedCommand = ReactiveCommand.CreateAsyncTask(x =>
                this.SetReportDesigner(((DependencyPropertyChangedEventArgs) x).NewValue), RxApp.MainThreadScheduler);
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
        private void OpenScript()
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
        private void SaveScript()
        {
            if (string.IsNullOrEmpty(this.CodeFilePath))
            {
                var filePath = this.openSaveFileDialogService.GetSaveFileDialog("ReportDataSource", "cs", "CS(.cs) | *.cs", string.Empty, 1);

                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                this.CodeFilePath = filePath;
            }

            if (!string.IsNullOrEmpty(this.CodeFilePath))
            {
                File.WriteAllText(this.CodeFilePath, this.Document.Text);
            }
        }

        /// <summary>
        /// Checks if AutoBuild script is active and start compilation accordingly.
        /// </summary>
        private void CheckAutoBuildScript()
        {
            if (!this.IsAutoBuildEnabled)
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
            this.currentReportProjectFilePath = string.Empty;
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

            if (reportProjectFilePath != null)
            {
                var report = new XtraReport();
                var reportStream = this.GetReportStream(reportProjectFilePath);

                report.LoadLayoutFromXml(reportStream.Repx);

                this.Document = new TextDocument();

                if (reportStream.DataSource != null)
                {
                    using (var streamReader = new StreamReader(reportStream.DataSource))
                    {
                        var datasource = streamReader.ReadToEnd();
                        this.Document = new TextDocument(datasource);
                    }

                    this.CompileAssembly(this.Document.Text);
                }

                this.CurrentReport = report;
                this.currentReportProjectFilePath = reportProjectFilePath;
                this.lastSavedDataSourceText = this.Document.Text;

                this.RebuildDataSource();
            }
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
            var fileShouldExist = !string.IsNullOrWhiteSpace(this.currentReportProjectFilePath);

            if (fileShouldExist)
            {
                archiveName = Path.GetFileNameWithoutExtension(this.currentReportProjectFilePath);
                initialPath = this.currentReportProjectFilePath;
            }

            var filePath = this.currentReportProjectFilePath;

            if (!fileShouldExist || forceDialog)
            {
                filePath = this.openSaveFileDialogService.GetSaveFileDialog(archiveName, "rep4", "Report project files (*.rep4)|*.rep4|All files (*.*)|*.*", initialPath, 1);
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            this.currentReportProjectFilePath = filePath;

            if (File.Exists(this.currentReportProjectFilePath))
            {
                File.Delete(this.currentReportProjectFilePath);
            }

            using (var reportStream = new MemoryStream())
            {
                this.CurrentReport.SaveLayoutToXml(reportStream);

                using (var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(this.Document.Text)))
                {
                    using (var zipFile = ZipFile.Open(this.currentReportProjectFilePath, ZipArchiveMode.Create))
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
            var dataSourceName = "ReportDataSource";
            var dataSource = this.CurrentReport.ComponentStorage.OfType<ObjectDataSource>().ToList().FirstOrDefault(x => x.Name.Equals(dataSourceName));

            if (dataSource == null)
            {
                // Create new datasource
                dataSource = new ObjectDataSource
                {
                    DataSource = this.GetDataSource(),
                    Name = dataSourceName
                };

                this.CurrentReport.ComponentStorage.Add(dataSource);
                this.CurrentReport.DataSource = dataSource;
            }
            else
            {
                // Use existing datasource
                dataSource.DataSource = this.GetDataSource();
            }

            var parameters = this.GetParameters(dataSource.DataSource).ToList();
            var filterString = this.GetFilterString(parameters);

            this.currentReport.FilterString = filterString;

            this.CheckParameters(parameters);

            // Always rebuild datasource schema 
            dataSource.RebuildResultSchema();
        }

        /// <summary>
        /// Get the data representation for the report
        /// </summary>
        /// <returns>The datasource as an <see cref="object"/></returns>
        private object GetDataSource()
        {
            if (this.BuildResult == null)
            {
                this.AddOutput("Build data source code first.");
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.BuildResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.IsSubclassOf(typeof(ReportingDataSource))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    this.AddOutput($"No class that inherits from {typeof(ReportingDataSource)} was found.");
                    return null;
                }

                var instObj = this.BuildResult.CompiledAssembly.CreateInstance(editorFullClassName) as ReportingDataSource;

                if (instObj == null)
                {
                    this.AddOutput("Data source class not found.");
                    return null;
                }

                instObj.Initialize(this.Thing, this.Session);

                return instObj.CreateDataSource();
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
            var defaultValues = new Dictionary<string, object>();

            //Find existing dynamic parameters
            foreach (var reportParameter in this.CurrentReport.Parameters)
            {
                if (reportParameter.Name.StartsWith(ReportingParameter.NamePrefix))
                {
                    defaultValues.Add(reportParameter.Name, reportParameter.Value);
                    toBeRemoved.Add(reportParameter);
                }
            }

            // Remove old dynamic parameters
            if (toBeRemoved.Any())
            {
                foreach (var reportParameter in toBeRemoved)
                {
                    this.currentReportDesignerDocument.MakeChanges(
                        changes => { changes.RemoveItem(reportParameter); });
                }
            }

            if (!(reportingParameters?.Any() ?? false))
            {
                return;
            }

            // Create new dynamic parameters
            foreach (var reportingParameter in reportingParameters)
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
                if (defaultValues.ContainsKey(reportingParameter.ParameterName))
                {
                    newReportParameter.Value = defaultValues[reportingParameter.ParameterName];
                }

                // Add dynamic parameter to report definition
                this.currentReportDesignerDocument.MakeChanges(
                    changes => { changes.AddItem(newReportParameter); });
            }
        }

        /// <summary>
        /// Check if the compiled assembly built from the code editor contains a class that implements the <see cref="IReportingParameters"/> interface
        /// If true, then execute the class' <see cref="IReportingParameters.CreateParameters"/> method.
        /// </summary>
        /// <param name="dataSource">The datasource, which could be used to create parameters</param>
        /// <returns>The <see cref="IEnumerable{IReportingParameter}"/></returns>
        private IEnumerable<IReportingParameter> GetParameters(object dataSource)
        {
            if (this.BuildResult == null)
            {
                this.AddOutput("Build data source code first.");
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.BuildResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.GetInterfaces()
                            .Any(i => i == typeof(IReportingParameters))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    return null;
                }

                if (!(this.BuildResult.CompiledAssembly.CreateInstance(editorFullClassName) is IReportingParameters instObj))
                {
                    this.AddOutput("Report parameter class not found.");
                    return null;
                }

                return instObj.CreateParameters(dataSource);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.AssemblyResolver;
            }
        }

        /// <summary>
        /// Check if the compiled assembly built from the code editor contains a class that implements the <see cref="IReportingParameters"/> interface
        /// If true, then execute the class' <see cref="IReportingParameters.CreateFilterString"/> method.
        /// </summary>
        /// <param name="parameters">The <see cref="IEnumerable{IReportingParameter}"/>, which could be used to create a filter string</param>
        /// <returns>The create Filter string in <see cref="IReportingParameters.CreateFilterString"/></returns>.
        private string GetFilterString(IEnumerable<IReportingParameter> parameters)
        {
            if (this.BuildResult == null)
            {
                this.AddOutput("Build data source code first.");
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolver;

            try
            {
                var editorFullClassName =
                    this.BuildResult
                        .CompiledAssembly
                        .GetTypes()
                        .FirstOrDefault(t => t.GetInterfaces()
                            .Any(i => i == typeof(IReportingParameters))
                        )?.FullName;

                if (editorFullClassName == null)
                {
                    return null;
                }

                if (!(this.BuildResult.CompiledAssembly.CreateInstance(editorFullClassName) is IReportingParameters instObj))
                {
                    this.AddOutput("Report parameter class not found.");
                    return null;
                }

                return instObj.CreateFilterString(parameters);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.AssemblyResolver;
            }
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
        private ReportZipArchive GetReportStream(string rep4File)
        {
            var zipFile = ZipFile.OpenRead(rep4File);

            return new ReportZipArchive()
            {
                Repx = zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".repx"))?.Open(),
                DataSource = zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".cs"))?.Open()
            };
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

                this.BuildResult = compiler.CompileAssemblyFromSource(parameters, source);

                if (this.BuildResult.Errors.Count == 0)
                {
                    this.AddOutput("File succesfully compiled.");
                    this.Errors = string.Empty;
                    return;
                }

                var sbErrors = new StringBuilder($"{DateTime.Now:HH:mm:ss} Compilation Errors:");

                foreach (var error in this.BuildResult.Errors)
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
