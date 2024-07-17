// -------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowser.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for FiniteStateBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
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