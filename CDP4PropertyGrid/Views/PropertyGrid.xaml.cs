// -------------------------------------------------------------------------------------------------
// <copyright file="PropertyGrid.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.Views
{
    using System.Windows.Controls;

    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    [PanelViewExport(RegionNames.RightPanel)]
    public partial class PropertyGrid : UserControl, IPanelView
    {
        public PropertyGrid()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGrid"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public PropertyGrid(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}