// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelHomeRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using CDP4EngineeringModel.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ModelHomeRibbon.xaml
    /// </summary>
    [Export(typeof(ModelHomeRibbon))]
    public partial class ModelHomeRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public ModelHomeRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ModelHomeRibbonViewModel();
        }
    }
}
