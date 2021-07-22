// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for CommonFileStoreBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class CommonFileStoreBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowser"/> class.
        /// </summary>
        public CommonFileStoreBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public CommonFileStoreBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
