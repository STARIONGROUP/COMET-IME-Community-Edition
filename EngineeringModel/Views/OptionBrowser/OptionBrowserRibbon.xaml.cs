// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowserRibbon.xaml.cs" company="RHEA System S.A.">
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
    /// Interaction logic for OptionBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(OptionBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class OptionBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public OptionBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new OptionBrowserRibbonViewModel();
        }
    }
}