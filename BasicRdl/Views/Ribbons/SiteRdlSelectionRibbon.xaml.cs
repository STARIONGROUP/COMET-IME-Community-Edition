// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for RDL selection Ribbon
    /// </summary>
    [Export(typeof(SiteRdlSelectionRibbon))]
    public partial class SiteRdlSelectionRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlSelectionRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public SiteRdlSelectionRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new SiteRdlSelectionRibbonViewModel();
        }
    }
}
