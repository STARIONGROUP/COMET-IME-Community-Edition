// -------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4BuiltInRules.Views
{
    using System.Windows.Controls;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for <see cref="BuiltInRulesBrowser"/> XAML
    /// </summary>
    [PanelViewExport(RegionNames.RightPanel)]
    public partial class BuiltInRulesBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesBrowser"/> class.
        /// </summary>
        public BuiltInRulesBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public BuiltInRulesBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
