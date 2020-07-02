// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixModule.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using NLog;
    using Views;
    using CDP4Composition.Services;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="RelationshipMatrixModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(RelationshipMatrixModule), "Module that provides a relationship-matrix view")]
    public class RelationshipMatrixModule : IModule
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixModule"/> class.
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
        /// <param name="pluginSettingService">
        /// The (MEF injected) instance of <see cref="IPluginSettingsService"/>
        /// pluginSettingService
        /// </param>
        [ImportingConstructor]
        public RelationshipMatrixModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.PluginSettingService = pluginSettingService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="RelationshipMatrixModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="RelationshipMatrixModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="RelationshipMatrixModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="RelationshipMatrixModule"/> to support panel navigation
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPluginSettingsService"/>
        /// </summary>
        internal IPluginSettingsService PluginSettingService { get; private set; }
        
        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegisterRegions();
            this.RegisterRibbonParts();
            this.ReadPluginSettings();            
        }

        /// <summary>
        /// Register the view view-models with the <see cref="Region"/>s
        /// </summary>
        private void RegisterRegions()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RelationshipMatrixRibbon));
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
        }

        /// <summary>
        /// Reads the plugin settings from disk
        /// </summary>
        internal void ReadPluginSettings()
        {
            try
            {
                var settings = this.PluginSettingService.Read<RelationshipMatrixPluginSettings>();

                if (!settings.PossibleDisplayKinds.Any())
                {
                    // if setting is empty, repopulate with default set and save it
                    settings.PossibleDisplayKinds = RelationshipMatrixPluginSettings.DefaultDisplayKinds.ToList();
                    this.PluginSettingService.Write(settings);
                }
            }
            catch (PluginSettingsException pluginSettingsException)
            {
                var relationshipMatrixPluginSettings = new RelationshipMatrixPluginSettings(true);
                this.PluginSettingService.Write(relationshipMatrixPluginSettings);
                
                logger.Error(pluginSettingsException);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw;
            }
        }
    }
}
