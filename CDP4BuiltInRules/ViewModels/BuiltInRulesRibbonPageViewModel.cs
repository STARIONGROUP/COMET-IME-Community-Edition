// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPageViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        public ReactiveCommand<object, object> OpenBrowser { get; protected set; }

        /// <summary>
        /// Executes the <see cref="OpenBrowser"/> <see cref="ICommand"/>
        /// </summary>
        private void ExecuteOpenBrowser()
        {
            if (this.builtInRulesBrowserViewModel == null)
            {
                this.builtInRulesBrowserViewModel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService, this.dialogNavigationService);
                this.panelNavigationService.OpenInDock(this.builtInRulesBrowserViewModel);
            }
            else
            {
                this.panelNavigationService.CloseInDock(this.builtInRulesBrowserViewModel);
                this.builtInRulesBrowserViewModel = null;
            }
        }
    }
}
