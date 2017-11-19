// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for Role Browser Ribbon view
    /// </summary>
    [Export(typeof(SiteRdlBrowserRibbon))]
    public partial class SiteRdlBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowserRibbon"/> class.
        /// </summary>
        public SiteRdlBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new SiteRdlBrowserRibbonViewModel();
        }
    }
}