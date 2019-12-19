// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using DevExpress.Xpf.Bars;
    using ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ReferenceSourceRibbon view
    /// </summary>
    public partial class ReferenceSourceRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceRibbon"/> class.
        /// </summary>
        public ReferenceSourceRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ReferenceSourceRibbonViewModel();
        }
    }
}
