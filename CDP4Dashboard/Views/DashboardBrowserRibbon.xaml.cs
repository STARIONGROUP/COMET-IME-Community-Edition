// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardBrowserRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;

    using CDP4Dashboard.ViewModels;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for RequirementBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(DashboardBrowserRibbon))]
    public partial class DashboardBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public DashboardBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new DashboardBrowserRibbonViewModel();
        }
    }
}