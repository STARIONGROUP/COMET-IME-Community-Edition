// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRibbon.xaml.cs" company="RHEA System S.A.">
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
    /// Interaction logic for RuleVerificationListRibbonViewModel XAML
    /// </summary>
    [Export(typeof(RuleVerificationListRibbon))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class RuleVerificationListRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public RuleVerificationListRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new RuleVerificationListRibbonViewModel();
        }
    }
}
