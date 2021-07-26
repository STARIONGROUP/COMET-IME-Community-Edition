// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using System;
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4OfficeInfrastructure;

    using CDP4Requirements.Settings.JsonConverters;
    
    using NLog;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="RequirementsModule"/> Component
    /// </summary>
    [Export(typeof(IModule))]
    public class RequirementsModule : IModule
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the module settings
        /// </summary>
        public static RequirementsModuleSettings PluginSettings { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsModule"/> class.
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
        /// The MEF injected instance of <see cref="IPluginSettingsService"/>
        /// </param>
        /// <param name="officeApplicationWrapper">
        /// The MEF injected instance of <see cref="IOfficeApplicationWrapper"/>
        /// </param>
        [ImportingConstructor]
        public RequirementsModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
            this.OfficeApplicationWrapper = officeApplicationWrapper;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="RequirementsModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="RequirementsModule"/> to support panel navigation
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
        /// Gets the <see cref="IOfficeApplicationWrapper"/> used in the application
        /// </summary>
        internal IOfficeApplicationWrapper OfficeApplicationWrapper { get; private set; }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            try
            {
                var settings = this.PluginSettingsService.Read<RequirementsModuleSettings>(true, ReqIfJsonConverterUtility.BuildConverters());

                PluginSettings = settings ?? new RequirementsModuleSettings();
            }
            catch (PluginSettingsException pluginSettingsException)
            {
                var moduleSettings = new RequirementsModuleSettings();
                this.PluginSettingsService.Write(moduleSettings);
                logger.Error(pluginSettingsException);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw ex;
            }

            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var requirementRibbonPart = new RequirementRibbonPart(2000, this.PanelNavigationService, this.DialogNavigationService, this.ThingDialogNavigationService, this.PluginSettingsService, this.OfficeApplicationWrapper);
            this.RibbonManager.RegisterRibbonPart(requirementRibbonPart);
        }
    }
}
