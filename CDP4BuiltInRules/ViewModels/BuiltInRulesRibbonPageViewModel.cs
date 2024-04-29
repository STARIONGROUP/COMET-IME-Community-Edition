// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPageViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4BuiltInRules.Views;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
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

            this.OpenBrowser = ReactiveCommandCreator.Create(this.ExecuteOpenBrowser);
        }

        /// <summary>
        /// Gets or sets the Create Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenBrowser { get; protected set; }

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
