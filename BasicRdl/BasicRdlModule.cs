// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlModule.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace BasicRdl
{
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.FavoritesService;

    using CDP4Dal;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="BasicRdlModule"/> Component
    /// </summary>
    [Export(typeof(IModule))]
    public class BasicRdlModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRdlModule"/> class.
        /// </summary>
        /// <param name="ribbonManager">
        /// The (MEF injected) instance of <see cref="IFluentRibbonManager"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// The (MEF injected) instance of <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The MEF injected instance of <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The MEF injected instance of <see cref="IDialogNavigationService"/>
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="favoritesService">The <see cref="IFavoritesService"/> to be used to store and retrieve favorite things.</param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        [ImportingConstructor]
        public BasicRdlModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IFavoritesService favoritesService, ICDPMessageBus messageBus)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
            this.FavoritesService = favoritesService;
            this.MessageBus = messageBus;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="BasicRdlModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="BasicRdlModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used in the application
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/>
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        internal IPluginSettingsService PluginSettingsService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFavoritesService"/> used to read and write lists of favorite things to user preferences.
        /// </summary>
        internal IFavoritesService FavoritesService { get; private set; }

        /// <summary name="MessageBus">
        /// Gets the <see cref="ICDPMessageBus"/>
        /// </summary>
        internal ICDPMessageBus MessageBus { get; }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var rdlRibbonPart = new BasicRdlRibbonPart(20, this.PanelNavigationService, this.ThingDialogNavigationService, this.DialogNavigationService, this.PluginSettingsService, this.FavoritesService, this.MessageBus);
            this.RibbonManager.RegisterRibbonPart(rdlRibbonPart);
        }
    }
}
