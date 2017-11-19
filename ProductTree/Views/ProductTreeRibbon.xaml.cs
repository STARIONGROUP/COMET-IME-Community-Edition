// ------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ProductTreeRibbon.xaml
    /// </summary>
    [Export(typeof(ProductTreeRibbon))]
    public partial class ProductTreeRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTreeRibbon"/> class
        /// </summary>
        [ImportingConstructor]
        public ProductTreeRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ProductTreeRibbonViewModel();
        }
    }
}