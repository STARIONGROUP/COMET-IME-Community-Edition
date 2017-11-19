// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for CategoryRibbon.xaml
    /// </summary>
    [Export(typeof(CategoryRibbon))]
    public partial class CategoryRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRibbon"/> class.
        /// </summary>
        public CategoryRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new CategoryRibbonViewModel();
        }
    }
}
