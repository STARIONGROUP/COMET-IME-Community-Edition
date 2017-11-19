// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRibbon.xaml.cs" company="RHEA System S.A.">
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
    /// Interaction logic for ElementDefinitionRibbon.xaml
    /// </summary>
    [Export(typeof(ElementDefinitionRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ElementDefinitionRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public ElementDefinitionRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ElementDefinitionRibbonViewModel();
        }
    }
}