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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using ICSharpCode.AvalonEdit.Document;
    using Microsoft.Practices.ServiceLocation;
    using NLog;
    using ReactiveUI;
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// The view-model for the Report Designer that lets users to create reports based on template source files.
    /// </summary>
    public class ReportDesignerViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Reporting";

        /// <summary>
        /// The Nlog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/> that is used to navigate to the File Open/Save dialog
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

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
        /// Automatically build code that has been typed in the editor
        /// </summary>
        public ReactiveCommand<Unit> AutomaticBuildCommand { get; set; }

        /// <summary>
        /// Open rep4 zip archive which consists in datasource code file and report designer file
        /// </summary>
        public ReactiveCommand<object> OpenReportCommand { get; set; }

        /// <summary>
        /// Save editor code and report designer to rep4 zip archive
        /// </summary>
        public ReactiveCommand<object> SaveReportCommand { get; set; }

        /// <summary>
        /// Gets or sets text editor document
        /// </summary>
        public TextDocument Document { get; set; }

        /// <summary>
        /// Gets or sets current edited file path
        /// </summary>
        public string CodeFilePath { get; set; }

        /// <summary>
        /// Gets or sets current archive zip file path that contains resx report designer file and datasource c# file
        /// </summary>
        public string ZipFilePath { get; set; }

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
        public string Output {
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
            this.BuildScriptCommand.Subscribe(_ => this.BuildScript());

            this.AutomaticBuildCommand = ReactiveCommand.CreateAsyncTask(_ => this.AutomaticBuildScript(), RxApp.MainThreadScheduler);

            this.OpenReportCommand = ReactiveCommand.Create();
            this.OpenReportCommand.Subscribe(_ => this.OpenReport());

            this.SaveReportCommand = ReactiveCommand.Create();
            this.SaveReportCommand.Subscribe(_ => this.SaveReport());

        }

        /// <summary>
        /// Trigger save file operation
        /// </summary>
        private void SaveScript()
        {
            if (string.IsNullOrEmpty(this.CodeFilePath))
            {
                var filePath = this.openSaveFileDialogService.GetSaveFileDialog("MassBudgetDataSource", "cs", "CS(.cs) | *.cs", string.Empty, 1);

                if (filePath == null)
                {
                    return;
                }

                this.CodeFilePath = filePath;
            }
            if (!string.IsNullOrEmpty(this.CodeFilePath))
            {
                System.IO.File.WriteAllText(this.CodeFilePath, this.Document.Text);
            }
        }

        /// <summary>
        /// Trigger open file operation
        /// </summary>
        private void OpenScript()
        {
            var filePath = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false, "CS(.cs)|*.cs", ".cs", string.Empty, 1);

            if (filePath == null || filePath.Length != 1)
            {
                return;
            }

            this.CodeFilePath = filePath.Single();

            this.Document.Text = System.IO.File.ReadAllText(this.CodeFilePath);
            this.IsDirty = true;
        }

        /// <summary>
        /// Trigger build operation
        /// </summary>
        private void BuildScript()
        {
            this.CompileAssembly();
        }

        /// <summary>
        /// Trigger automatic build operation
        /// </summary>
        public async Task AutomaticBuildScript()
        {
            await Task.Run(this.CompileAssembly);
        }

        /// <summary>
        /// Trigger open report command
        /// </summary>
        private void OpenReport()
        {
            var filePath = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false, "Report files (*.rep4)|*.rep4|All files (*.*)|*.*", ".rep4", string.Empty, 1);

            if (filePath == null || filePath.Length != 1)
            {
                return;
            }

            this.ZipFilePath = filePath.Single();

            CDPMessageBus.Current.SendMessage(new ReportDesignerEvent(this.ZipFilePath, ReportNotificationKind.REPORT_OPEN));
        }

        /// <summary>
        /// Trigger save report command
        /// </summary>
        private void SaveReport()
        {
            if (string.IsNullOrEmpty(this.ZipFilePath))
            {
                this.Output += $"{DateTime.Now:HH:mm:ss} Report not found.{Environment.NewLine}";
                return;
            }

            var filePath = this.openSaveFileDialogService.GetSaveFileDialog("ReportArchive", "rep4", "Report files (*.rep4)|*.rep4|All files (*.*)|*.*", string.Empty, 1);

            if (filePath == null)
            {
                return;
            }

            this.ZipFilePath = filePath;

            CDPMessageBus.Current.SendMessage(new ReportDesignerEvent(this.ZipFilePath, ReportNotificationKind.REPORT_SAVE));
        }

        /// <summary>
        /// Execute compile
        /// </summary>
        private void CompileAssembly()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Errors = string.Empty;

                if (string.IsNullOrEmpty(this.Document.Text))
                {
                    this.Output += $"{DateTime.Now:HH:mm:ss} Nothing to compile.{Environment.NewLine}";
                    return;
                }

                var compiler = new Microsoft.CSharp.CSharpCodeProvider();
                var parameters = new CompilerParameters();
                // TODO Figure out how to invoke from different paths(eg: from tests)
                var currentFolder = System.IO.File.Exists("CDP4Common.dll") ? "." : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.Collections.dll");
                parameters.ReferencedAssemblies.Add("System.Linq.dll");
                parameters.ReferencedAssemblies.Add("System.Windows.dll");

                parameters.ReferencedAssemblies.Add($"{currentFolder}\\CDP4Common.dll");
                parameters.ReferencedAssemblies.Add($"{currentFolder}\\CDP4Composition.dll");

                parameters.GenerateInMemory = true;
                parameters.GenerateExecutable = false;

                this.BuildResult = compiler.CompileAssemblyFromSource(parameters, this.Document.Text);

                if (this.BuildResult.Errors.Count == 0)
                {
                    this.Output += $"{DateTime.Now:HH:mm:ss} File succesfully compiled.{Environment.NewLine}";
                    this.Errors = string.Empty;
                    return;
                }

                var sbErrors = new StringBuilder($"{DateTime.Now:HH:mm:ss} Compilation Errors.");

                foreach (var error in this.BuildResult.Errors)
                {
                    sbErrors.AppendLine(error.ToString());
                }

                this.Errors = sbErrors.ToString();
                Logger.Trace(sbErrors.ToString());
            }));
        }
    }
}
