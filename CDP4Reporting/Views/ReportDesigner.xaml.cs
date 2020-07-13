namespace CDP4Reporting.Views
{
    using System.Windows;
    using System.CodeDom.Compiler;
    using System.ComponentModel.Composition;
    using System.Linq;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.XtraReports.Security;

    using CDP4Common.EngineeringModelData;
    using CDP4Reporting.ViewModels;
    using CDP4Composition.Reporting;
    using DevExpress.Xpf.Bars;
    using System.Windows.Forms;
    using DevExpress.Xpf.Reports.UserDesigner;
    using System.Text;
    using DevExpress.XtraReports.UI;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using DevExpress.CodeParser;
    using DevExpress.Xpo.Helpers;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ReportDesigner : System.Windows.Controls.UserControl, IPanelView
    {
        private string codeFile;
        private CompilerResults result;
        private Task compileTask;

        public bool IsBuildEnabled { get; set; }

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        public ReportDesigner()
        {
            this.InitializeComponent();

            ScriptPermissionManager.GlobalInstance = new ScriptPermissionManager(ExecutionMode.Deny);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReportDesigner(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();

                IsBuildEnabled = false;

                this.reportDesigner.ActiveDocumentChanged += ReportDesigner_ActiveDocumentChanged;
                this.textEditor.TextChanged += TextEditor_TextChanged;
            }
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (!IsBuildEnabled)
            {
                return;
            }

            var tokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = tokenSource.Token;

            if (compileTask != null && compileTask.Status.Equals(TaskStatus.Running))
            {
                tokenSource.Cancel();
            }

            compileTask = Task.Run(delegate
            {
                Thread.Sleep(5 * 1000);
                CompileAssembly();
            }, cancelToken);
        }

        #endregion

        #region Events

        /// <summary>
        /// Trigger active document changed event, when a new report was loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportDesigner_ActiveDocumentChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var localReport = ((ReportDesignerDocument)e.NewValue).Report;
                foreach (var component in localReport.ComponentStorage.OfType<ObjectDataSource>().ToList())
                {
                    localReport.ComponentStorage.Remove(component);
                    localReport.Container?.Remove(component);
                }
                SetReportDataSource((this.DataContext as ReportDesignerViewModel).Thing as Iteration);
            }
        }

        /// <summary>
        /// Trigger open file operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "CS files (*.cs)|*.cs";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                codeFile = dlg.FileName;
                textEditor.Load(codeFile);
                textEditor.Visibility = Visibility.Visible;

                CompileAssembly();
            }
        }

        /// <summary>
        /// Trigger save file operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (string.IsNullOrEmpty(codeFile))
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    DefaultExt = ".cs"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    codeFile = dlg.FileName;
                }
            }

            if (!string.IsNullOrEmpty(codeFile))
            {
                textEditor.Save(codeFile);
            }
        }

        /// <summary>
        /// Trigger manual code building
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuildCodeButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            CompileAssembly();
        }

        /// <summary>
        /// Trigger automatic code building
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutomaticBuildButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            IsBuildEnabled = !IsBuildEnabled;
        }

        /// <summary>
        /// Trigger context menu action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch (e.Item.Content.ToString())
            {
                case "Copy":
                    OutputTextBox.Copy();
                    break;
                case "Clear":
                    OutputTextBox.Text = string.Empty;
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Compile code from the text editor
        /// </summary>
        private void CompileAssembly()
        {
            Dispatcher.BeginInvoke((Action)(() => {
                ErrorsTextBox.Text = string.Empty;

                if (string.IsNullOrEmpty(textEditor.Text))
                {
                    OutputTextBox.Text += $"{DateTime.Now:HH:mm:ss} Nothing to compile.{Environment.NewLine}";
                    layoutOutput.Visibility = Visibility.Visible;
                    return;
                }

                var compiler = new Microsoft.CSharp.CSharpCodeProvider();

                var parms = new CompilerParameters();

                parms.ReferencedAssemblies.Add("System.dll");
                parms.ReferencedAssemblies.Add("System.Core.dll");
                parms.ReferencedAssemblies.Add("System.Collections.dll");
                parms.ReferencedAssemblies.Add("System.Linq.dll");
                parms.ReferencedAssemblies.Add("System.Windows.dll");

                parms.ReferencedAssemblies.Add("CDP4Common.dll");
                parms.ReferencedAssemblies.Add("CDP4Composition.dll");

                parms.GenerateInMemory = true;
                parms.GenerateExecutable = false;

                result = compiler.CompileAssemblyFromSource(parms, textEditor.Text);

                if (result.Errors.Count == 0)
                {
                    OutputTextBox.Text += $"{DateTime.Now:HH:mm:ss} File succesfully compiled.{Environment.NewLine}";
                    layoutOutput.Visibility = Visibility.Visible;
                    ErrorsTextBox.Text = string.Empty;
                    return;
                }

                StringBuilder sbErrors = new StringBuilder($"{DateTime.Now:HH:mm:ss} Compilation Errors");
                foreach (var error in result.Errors)
                {
                    sbErrors.AppendLine(error.ToString());
                }
                ErrorsTextBox.Text = sbErrors.ToString();
                layoutErrors.Visibility = Visibility.Visible;
            }));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="currentIteration"></param>
        private void SetReportDataSource(Iteration currentIteration)
        {
            if (currentIteration == null)
            {
                return;

            }
            var inst = result.CompiledAssembly.CreateInstance("CDP4Reporting.MassBudgetDataSource") as ICDP4ObjectDataSource;

            var dataSourceName = "MassBudgetDataSource";
            var dataSource = new ObjectDataSource()
            {
                DataSource = inst?.CreateDataSource(currentIteration),
                Name = dataSourceName
            };
            this.reportDesigner.ActiveDocument.Report.DataSource = dataSource;
        }

        #endregion
    }
}
