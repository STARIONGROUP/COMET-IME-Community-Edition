// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowserRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Ribbon;

    using CDP4EngineeringModel.ViewModels;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for OptionBrowserRibbon
    /// </summary>
    [Export(typeof(DomainFileStoreBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class DomainFileStoreBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreBrowserRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public DomainFileStoreBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new DomainFileStoreBrowserRibbonViewModel();
        }
    }
}
