// -------------------------------------------------------------------------------------------------
// <copyright file="ModelViewRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.Views
{
    using System.ComponentModel.Composition;
    using CDP4PropertyGrid.ViewModels;
    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;
    
    /// <summary>
    /// Interaction logic for ModelViewRibbon.xaml
    /// </summary>
    [Export(typeof(ViewRibbonControl))]
    public partial class ViewRibbonControl : IView
    {
        [ImportingConstructor]
        public ViewRibbonControl()
        {
            this.InitializeComponent();
            this.DataContext = new ViewRibbonControlViewModel();
        }
    }
}