// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowserRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Composition.Ribbon;
    using CDP4EngineeringModel.ViewModels;
    using System.ComponentModel.Composition;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for <see cref="RelationshipBrowserRibbon"/> XAML
    /// </summary>
    [Export(typeof(RelationshipBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class RelationshipBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public RelationshipBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new RelationshipBrowserRibbonViewModel();
        }
    }
}
