// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(DashboardRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class DashboardRibbon : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public DashboardRibbon()
        {
            this.InitializeComponent();
        }
    }
}
