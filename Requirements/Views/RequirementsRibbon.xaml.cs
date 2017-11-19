// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Ribbon;
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(RequirementsRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class RequirementsRibbon : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public RequirementsRibbon()
        {
            this.InitializeComponent();
        }
    }
}
