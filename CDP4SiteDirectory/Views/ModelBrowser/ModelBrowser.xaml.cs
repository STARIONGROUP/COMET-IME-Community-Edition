// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelBrowser.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for ModelBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class ModelBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBrowser"/> class
        /// </summary>
        public ModelBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ModelBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}