// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseBrowser.xaml.cs" company="Starion Group S.A.">
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
    /// Interaction logic for DomainOfExpertiseBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class DomainOfExpertiseBrowser : UserControl, IPanelView
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
            }
        }
    }
}
