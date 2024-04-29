// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowser.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for OptionBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class OptionBrowser : UserControl, IPanelView
    {
        public OptionBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public OptionBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}