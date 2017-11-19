// -------------------------------------------------------------------------------------------------
// <copyright file="RulesRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for RulesRibbon.xaml
    /// </summary>
    [Export(typeof(RulesRibbon))]
    public partial class RulesRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RulesRibbon"/> class.
        /// </summary>
        public RulesRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new RulesRibbonViewModel();
        }
    }
}
