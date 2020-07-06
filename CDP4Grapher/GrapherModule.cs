// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher
{
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Grapher.Views;

    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="GrapherModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(GrapherModule), "Grapher Module")]
    public class GrapherModule : IModule
    {
        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="GrapherModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="GrapherModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="GrapherModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="GrapherModule"/> to support panel navigation
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        internal IPluginSettingsService PluginSettingsService { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="GrapherModule"/> class.
        /// </summary>
        /// <param name="regionManager">
        /// The (MEF injected) instance of <see cref="IRegionManager"/>
        /// </param>
        /// <param name="ribbonManager">
        /// The (MEF injected) instance of <see cref="IFluentRibbonManager"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// The (MEF injected) instance of <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The dialog Navigation Service.
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The (MEF injected) instance of <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="pluginSettingsService">
        /// The (MEF injected) instance of <see cref="IPluginSettingsService"/>
        /// </param>
        [ImportingConstructor]
        public GrapherModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
        }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegisterRegions();
        }

        /// <summary>
        /// Register the view view-models with the <see cref="Region"/>s
        /// </summary>
        private void RegisterRegions()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(GrapherRibbon));
        }
    }
}
