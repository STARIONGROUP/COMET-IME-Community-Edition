// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserRibbon.xaml.cs" company="RHEA System S.A.">
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
    /// Interaction logic for OptionBrowserRibbon
    /// </summary>
    [Export(typeof(CommonFileStoreBrowserRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CommonFileStoreBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowserRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public CommonFileStoreBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new CommonFileStoreBrowserRibbonViewModel();
        }
    }
}