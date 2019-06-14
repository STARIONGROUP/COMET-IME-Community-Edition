// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Exceptions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Requirements.Views;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using NLog;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="RequirementsModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(RequirementsModule), "Requirements Module - Community Edition")]
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
        /// <param name="regionManager">
        /// The (MEF injected) instance of <see cref="IRegionManager"/>
        /// </param>
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
        [ImportingConstructor]
        public RequirementsModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="RequirementsModule"/> to register the regions
        /// </summary>
        public IRegionManager RegionManager { get; private set; }

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
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RequirementsRibbon));
            this.RegisterRibbonParts();

            try
            {
                var settings = this.PluginSettingsService.Read<RequirementsModuleSettings>();
                PluginSettings = settings;
            }
            catch (PluginSettingsException pluginSettingsException)
            {
                var relationshipMatrixPluginSettings = new RequirementsModuleSettings();
                this.PluginSettingsService.Write(relationshipMatrixPluginSettings);
                logger.Error(pluginSettingsException);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var requirementRibbonPart = new RequirementRibbonPart(2000, this.PanelNavigationService, this.DialogNavigationService, this.ThingDialogNavigationService, this.PluginSettingsService);
            this.RibbonManager.RegisterRibbonPart(requirementRibbonPart);
        }
    }
}