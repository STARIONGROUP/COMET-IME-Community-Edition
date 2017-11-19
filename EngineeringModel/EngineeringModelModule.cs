// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Views;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="EngineeringModelModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(EngineeringModelModule), "Engineering Model Module - Community Edition")]
    public class EngineeringModelModule : IModule
    {
        /// <summary>
        /// The <see cref="IRegionManager"/> that is used by the <see cref="EngineeringModelModule"/> to register the regions
        /// </summary>
        private readonly IRegionManager regionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelModule"/> class.
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
        /// The (MEF injected) instance of <see cref="IDialogNavigationService"/>
        /// </param>
        /// <param name="permissionService">
        /// The (MEF injected) instance of <see cref="IPermissionService"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The (MEF injected) instance of <see cref="IThingDialogNavigationService"/>
        /// </param>
        [ImportingConstructor]
        public EngineeringModelModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService)
        {
            this.regionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="EngineeringModelModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="EngineeringModelModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="EngineeringModelModule"/> to support panel navigation
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Initialize the <see cref="EngineeringModelModule"/> by registering the <see cref="Region"/>s and the <see cref="RibbonPart"/>s
        /// </summary>
        public void Initialize()
        {
            this.RegisterRegions();
            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the view view-models with the <see cref="Region"/>s
        /// </summary>
        private void RegisterRegions()
        {
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(ModelHomeRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(ElementDefinitionRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(OptionBrowserRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(PublicationBrowserRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(FiniteStateBrowserRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RuleVerificationListRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RelationshipBrowserRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(CommonFileStoreBrowserRibbon));
            this.regionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(DomainFileStoreBrowserRibbon));
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var rdlRibbonPart = new EngineeringModelRibbonPart(10, this.PanelNavigationService, this.DialogNavigationService, this.ThingDialogNavigationService);
            this.RibbonManager.RegisterRibbonPart(rdlRibbonPart);
        }
    }
}