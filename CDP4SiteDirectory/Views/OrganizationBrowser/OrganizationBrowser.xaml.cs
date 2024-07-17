// -------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowser.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    using NLog;

    /// <summary>
    /// Interaction logic for OrganizationBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class OrganizationBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationBrowser"/> class
        /// </summary>
        public OrganizationBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public OrganizationBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
