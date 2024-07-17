// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardBrowser.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for TeamCompositionBrowser XAML
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class DashboardBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardBrowser"/> class.
        /// </summary>
        public DashboardBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DashboardBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
