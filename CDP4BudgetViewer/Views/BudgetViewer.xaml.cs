// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetViewer.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for BudgetViewer view
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class BudgetViewer : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetViewer"/> class
        /// </summary>
        public BudgetViewer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetViewer"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public BudgetViewer(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}