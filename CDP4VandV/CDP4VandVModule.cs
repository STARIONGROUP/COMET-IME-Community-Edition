// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4VandVModule.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4VandV
{
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the Verification &amp; Validation (V&amp;V) plugin.
    /// </summary>
    [Export(typeof(IModule))]
    public class CDP4VandVModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4VandVModule"/> class.
        /// </summary>
        /// <param name="ribbonManager">The (MEF injected) <see cref="IFluentRibbonManager"/>.</param>
        /// <param name="panelNavigationService">The (MEF injected) <see cref="IPanelNavigationService"/>.</param>
        /// <param name="thingDialogNavigationService">The (MEF injected) <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The (MEF injected) <see cref="IDialogNavigationService"/>.</param>
        [ImportingConstructor]
        public CDP4VandVModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> used to register Office Fluent Ribbon XML.
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to support panel navigation.
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used to support <see cref="CDP4Common.CommonData.Thing"/> dialog navigation.
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to support generic dialog navigation.
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        public void Initialize()
        {
        }
    }
}
