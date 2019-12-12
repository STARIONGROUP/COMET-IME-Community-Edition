// -------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using BasicRdl.ViewModels;
    using DevExpress.Xpf.Bars;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for UnitPrefixRibbon view
    /// </summary>
    public partial class UnitPrefixRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitPrefixRibbon"/> class.
        /// </summary>
        public UnitPrefixRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new UnitPrefixRibbonViewModel();
        }
    }
}
