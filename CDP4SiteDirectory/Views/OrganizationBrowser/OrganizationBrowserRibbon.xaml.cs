// -------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;
    
    /// <summary>
    /// Interaction logic for LogInfoControls.xaml
    /// </summary>
    [Export(typeof(OrganizationBrowserRibbon))]
    public partial class OrganizationBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationBrowserRibbon"/> class
        /// </summary>
        public OrganizationBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new OrganizationBrowserRibbonViewModel();
        }
    }
}