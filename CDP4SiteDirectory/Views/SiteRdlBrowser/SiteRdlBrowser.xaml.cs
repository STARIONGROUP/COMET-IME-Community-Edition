// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for Organization Browser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class SiteRdlBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowser"/> class
        /// </summary>
        public SiteRdlBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SiteRdlBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                var control = (GridControl)this.FindName("SiteRdlsGridControl");
                if (control != null)
                {
                    FilterStringService.FilterString.AddGridControl(control);
                    logger.Debug("{0} Added to the FilterStringService", control.Name);
                }
            }
        }
    }
}