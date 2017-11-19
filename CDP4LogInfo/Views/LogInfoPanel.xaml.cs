// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoPanel.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Views
{
    using System.Windows.Controls;

    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for LogInfoPanel view
    /// </summary>
    [PanelViewExport(RegionNames.RightPanel)]
    public partial class LogInfoPanel : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoPanel"/> class
        /// </summary>
        public LogInfoPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoPanel"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public LogInfoPanel(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}