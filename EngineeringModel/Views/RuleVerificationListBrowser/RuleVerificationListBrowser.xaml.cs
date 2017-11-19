// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.Windows.Controls;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for RuleVerificationListView XAML
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class RuleVerificationListBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListBrowser"/> class.
        /// </summary>
        public RuleVerificationListBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RuleVerificationListBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
