// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
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

namespace CDP4SiteDirectory
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal.Permission;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Views;

    /// <summary>
    /// Initializes the SiteDirectoryModule Plugin
    /// </summary>
    [ModuleExportName(typeof(SiteDirectoryModule), "Site Directory Browsers Module")]
    public class SiteDirectoryModule : IModule
    {
        /// <summary>
        /// the <see cref="IRegionManager"/> that is used by the <see cref="SiteDirectoryModule"/> to register the regions
        /// </summary>
        public readonly IRegionManager RegionManager;

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="SiteDirectoryModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="SiteDirectoryModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used in the application
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used in the application
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        internal IPluginSettingsService PluginSettingsService { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryModule"/> class.
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
        /// <param name="permissionService">The MEF injected instance of <see cref="IPermissionService"/></param>
        /// <param name="thingDialogNavigationService">The MEF injected instance of <see cref="IThingDialogNavigationService"/></param>
        /// <param name="dialogNavigationService">The MEF injected instance of <see cref="IDialogNavigationService"/></param>
        [ImportingConstructor]
        public SiteDirectoryModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
        }

        /// <summary>
        /// Initialize the module with the static views
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(ShowDeprecatedRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(HighlightingRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(ModelRibbon));            
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(OrganizationBrowserRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(PersonBrowserRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RoleBrowserRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(NaturalLanguageRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(DomainOfExpertiseRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(SiteRdlBrowserRibbon));
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(TeamCompositionBrowserRibbon));
            
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(SiteDirectoryRibbon));

            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var rdlRibbonPart = new SiteDirectoryRibbonPart(30, this.PanelNavigationService, this.ThingDialogNavigationService, this.DialogNavigationService, this.PluginSettingsService);
            this.RibbonManager.RegisterRibbonPart(rdlRibbonPart);
        }
    }
}