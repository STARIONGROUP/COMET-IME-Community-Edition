// ------------------------------------------------------------------------------------------------
// <copyright file="BudgetRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Budget.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;
    using ViewModels;

    /// <summary>
    /// Interaction logic for BudgetRibbon.xaml
    /// </summary>
    [Export(typeof(BudgetRibbon))]
    public partial class BudgetRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetRibbon"/> class
        /// </summary>
        [ImportingConstructor]
        public BudgetRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new BudgetRibbonViewModel();
        }
    }
}