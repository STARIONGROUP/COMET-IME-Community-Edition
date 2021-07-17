// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for RelationshipBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class RelationshipBrowser : UserControl, IPanelView
    {
        public RelationshipBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RelationshipBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
