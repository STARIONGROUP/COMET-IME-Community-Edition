// -------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPageViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4BuiltInRules.Views;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="BuiltInRulesRibbonPage"/> view
    /// </summary>
    public class BuiltInRulesRibbonPageViewModel
    {
        /// <summary>
        /// The <see cref="IPanelNavigationService"/> used for panel/browser navigation
        /// </summary>
        private readonly IPanelNavigationService panelNavigationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IRuleVerificationService"/> used <see cref="Rule"/> and <see cref="BuiltInRule"/> verification.
        /// </summary>
        private readonly IRuleVerificationService ruleVerificationService;

        /// <summary>
        /// The single <see cref="BuiltInRulesBrowserViewModel"/> that can be opened or closed from the browser
        /// </summary>
        private BuiltInRulesBrowserViewModel builtInRulesBrowserViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesRibbonPageViewModel"/> class.
        /// </summary>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/> used to navigate to panels/browsers
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/> that is used to navigate to generic dialogs
        /// </param>
        /// <param name="ruleVerificationService">
        /// The <see cref="IRuleVerificationService"/> that is used to verify an <see cref="Iteration"/> and provide access to the available <see cref="BuiltInRule"/>s
        /// </param>
        public BuiltInRulesRibbonPageViewModel(IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IRuleVerificationService ruleVerificationService)
        {
            this.panelNavigationService = panelNavigationService;
            this.dialogNavigationService = dialogNavigationService;
            this.ruleVerificationService = ruleVerificationService;

            this.OpenBrowser = ReactiveCommand.Create();
            this.OpenBrowser.Subscribe(_ => this.ExecuteOpenBrowser());
        }

        /// <summary>
        /// Gets or sets the Create Command
        /// </summary>
        public ReactiveCommand<object> OpenBrowser { get; protected set; }

        /// <summary>
        /// Executes the <see cref="OpenBrowser"/> <see cref="ICommand"/>
        /// </summary>
        private void ExecuteOpenBrowser()
        {
            if (this.builtInRulesBrowserViewModel == null)
            {
                this.builtInRulesBrowserViewModel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService, this.dialogNavigationService);
                this.panelNavigationService.Open(this.builtInRulesBrowserViewModel, true);
            }
            else
            {
                this.panelNavigationService.Close(this.builtInRulesBrowserViewModel, true);
                this.builtInRulesBrowserViewModel = null;
            }
        }
    }
}
