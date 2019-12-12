// -------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using BasicRdl.ViewModels;
    using DevExpress.Xpf.Bars;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for GlossaryBrowserRibbon.xaml
    /// </summary>
    public partial class GlossaryBrowserRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryBrowserRibbon"/> class.
        /// </summary>
        public GlossaryBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new GlossaryBrowserRibbonViewModel();
        }
    }
}
