// -------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowser.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.Windows;
    using CDP4Composition;
    using System.Windows.Controls;
    using CDP4Composition.Attributes;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// Interaction logic for FiniteStateBrowser.xaml
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class FiniteStateBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteStateBrowser"/> class
        /// </summary>
        public FiniteStateBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteStateBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public FiniteStateBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}