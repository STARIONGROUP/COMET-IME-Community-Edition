// -------------------------------------------------------------------------------------------------
// <copyright file="HighlightingRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for HighlightingRibbon
    /// </summary>
    [Export(typeof(HighlightingRibbon))]
    public partial class HighlightingRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingRibbon"/> class
        /// </summary>
        public HighlightingRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new HighlightingRibbonViewModel();
        }
    }
}