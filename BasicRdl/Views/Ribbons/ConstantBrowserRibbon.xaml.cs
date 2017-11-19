// -------------------------------------------------------------------------------------------------
// <copyright file="ConstantBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ConstantBrowserRibbon.xaml
    /// </summary>
    public partial class ConstantBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBrowserRibbon"/> class.
        /// </summary>
        public ConstantBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ConstantBrowserRibbonViewModel();
        }
    }
}
