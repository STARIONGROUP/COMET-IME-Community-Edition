// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramBrowser.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using NLog;

    /// <summary>
    /// Interaction logic for DiagramBrowser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class DiagramBrowser : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramBrowser"/> class.
        /// </summary>
        public DiagramBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DiagramBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
