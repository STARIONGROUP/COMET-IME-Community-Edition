namespace CDP4Reporting.Views
{
    using System;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.XtraPrinting.Native;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public partial class ReportDesigner : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        public ReportDesigner()
        {
            this.InitializeComponent();

            this.reportDesigner.ActiveDocumentChanged += (sender, args) =>
            {
                if (args.NewValue != null)
                {
                    var localReport = ((DevExpress.Xpf.Reports.UserDesigner.ReportDesignerDocument)args.NewValue).Report;

                    foreach (var component in localReport.ComponentStorage.OfType<ObjectDataSource>().ToList())
                    {
                        localReport.ComponentStorage.Remove(component);
                        localReport.Container?.Remove(component);
                    }

                    var dataSource = new ObjectDataSource()
                    {
                        Name = "MassBudgetDataSource"
                    };

                    localReport.DataSource = dataSource;
                }
            };
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
            }
        }
    }
}
