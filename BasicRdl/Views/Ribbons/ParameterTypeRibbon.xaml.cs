// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ParameterTypeRibbon.xaml
    /// </summary>
    [Export(typeof(ParameterTypeRibbon))]
    public partial class ParameterTypeRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRibbon"/> class.
        /// </summary>
        public ParameterTypeRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ParameterTypeRibbonViewModel();
        }
    }
}
