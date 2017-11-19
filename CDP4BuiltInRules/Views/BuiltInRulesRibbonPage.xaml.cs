// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPage.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Views
{
    using System.ComponentModel.Composition;
    using CDP4BuiltInRules.ViewModels;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4Composition.Services;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(BuiltInRulesRibbonPage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class BuiltInRulesRibbonPage : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesRibbonPage"/> class.
        /// </summary>
        /// <param name="panelNavigationService">
        /// The (MEF) injected <see cref="IPanelNavigationService"/> that is used to navigate to panels/browsers
        /// </param>
        /// <param name="dialogNavigationService">
        /// The (MEF) injected <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </param>
        /// <param name="ruleVerificationService">
        /// The (MEF) injected <see cref="IRuleVerificationService"/> that is used to verify an <see cref="Iteration"/> and provide access to the available <see cref="BuiltInRule"/>s
        /// </param>
        [ImportingConstructor]
        public BuiltInRulesRibbonPage(IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IRuleVerificationService ruleVerificationService)
        {
            this.InitializeComponent();
            this.DataContext = new BuiltInRulesRibbonPageViewModel(panelNavigationService, dialogNavigationService, ruleVerificationService);
        }
    }
}
