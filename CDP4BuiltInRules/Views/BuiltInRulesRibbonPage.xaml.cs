// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPage.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Views
{
    using System.ComponentModel.Composition;

    using CDP4BuiltInRules.ViewModels;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4Composition.Services;

    /// <summary>
    /// Interaction logic for ObjectBrowserRibbon
    /// </summary>
    [Export(typeof(ExtendedRibbonPageGroup))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class BuiltInRulesRibbonPage : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRulesRibbonPage" /> class.
        /// </summary>
        /// <param name="panelNavigationService">
        /// The (MEF) injected <see cref="IPanelNavigationService" /> that is used to navigate to panels/browsers
        /// </param>
        /// <param name="dialogNavigationService">
        /// The (MEF) injected <see cref="IDialogNavigationService" /> that is used to navigate to generic dialogs
        /// </param>
        /// <param name="ruleVerificationService">
        /// The (MEF) injected <see cref="IRuleVerificationService" /> that is used to verify an <see cref="Iteration" /> and
        /// provide access to the available <see cref="BuiltInRule" />s
        /// </param>
        [ImportingConstructor]
        public BuiltInRulesRibbonPage(IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IRuleVerificationService ruleVerificationService)
        {
            this.InitializeComponent();
            this.DataContext = new BuiltInRulesRibbonPageViewModel(panelNavigationService, dialogNavigationService, ruleVerificationService);
        }
    }
}
