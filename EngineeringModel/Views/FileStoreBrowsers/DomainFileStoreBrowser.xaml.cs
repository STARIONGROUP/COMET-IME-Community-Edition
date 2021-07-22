// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowser.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for CommonFileStoreBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class DomainFileStoreBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowser"/> class.
        /// </summary>
        public DomainFileStoreBrowser()
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
        public DomainFileStoreBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
