// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationBrowserRibbon.xaml.cs" company="RHEA System S.A.">
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
    /// Interaction logic for PublicationBrowserRibbon.xaml
    /// </summary>
    [Export(typeof(PublicationBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class PublicationBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        [ImportingConstructor]
        public PublicationBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new PublicationBrowserRibbonViewModel();
        }
    }
}