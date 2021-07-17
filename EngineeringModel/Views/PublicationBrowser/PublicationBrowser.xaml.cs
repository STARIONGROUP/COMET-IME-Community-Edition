// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for PublicationBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class PublicationBrowser : UserControl, IPanelView
    {
        public PublicationBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public PublicationBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
