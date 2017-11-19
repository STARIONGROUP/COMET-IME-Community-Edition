// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for GlossaryBrowser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class GlossaryBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryBrowser"/> class.
        /// </summary>
        public GlossaryBrowser()
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public GlossaryBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                var control = (TreeListControl)this.FindName("GlossaryTreeListControl");
                if (control != null)
                {
                    FilterStringService.FilterString.AddTreeListControl(control);
                    logger.Debug("{0} Added to the FilterStringService", control.Name);
                }
            }
        }
    }
}