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

    using CDP4Composition.Mvvm;

    /// <summary>
    /// Interaction logic for RequirementBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(ExtendedRibbonPageGroup))]
    public partial class DashboardBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardBrowserRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public DashboardBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new DashboardBrowserRibbonViewModel();
        }
    }
}