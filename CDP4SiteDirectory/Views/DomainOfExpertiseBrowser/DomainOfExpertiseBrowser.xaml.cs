// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for DomainOfExpertiseBrowser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class DomainOfExpertiseBrowser : IPanelView, IPanelFilterableDataGridView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseBrowser"/> class.
        /// </summary>
        public DomainOfExpertiseBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DomainOfExpertiseBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                this.Control = this.DomainsGridControl;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataControlBase"/> that is to be set up for filtering service.
        /// </summary>
        public DataControlBase Control { get; private set; }
    }
}
