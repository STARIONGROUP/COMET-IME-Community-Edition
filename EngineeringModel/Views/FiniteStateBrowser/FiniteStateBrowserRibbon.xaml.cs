// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Composition.Ribbon;

    using CDP4EngineeringModel.ViewModels;
    using System.ComponentModel.Composition;    
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for FiniteStateBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(FiniteStateBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class FiniteStateBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public FiniteStateBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new FiniteStateBrowserRibbonViewModel();
        }
    }
}