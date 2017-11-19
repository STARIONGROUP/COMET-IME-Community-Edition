// -------------------------------------------------------------------------------------------------
// <copyright file="ErrorBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for <see cref="ErrorBrowser"/> XAML
    /// </summary>
    [PanelViewExport(RegionNames.RightPanel)]
    public partial class ErrorBrowser : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBrowser"/> class.
        /// </summary>
        public ErrorBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ErrorBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
