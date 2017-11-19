// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for SiteDirectoryRibbon.xaml
    /// </summary>
    [Export(typeof(SiteDirectoryRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class SiteDirectoryRibbon : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public SiteDirectoryRibbon()
        {
            this.InitializeComponent();
        }
    }
}
