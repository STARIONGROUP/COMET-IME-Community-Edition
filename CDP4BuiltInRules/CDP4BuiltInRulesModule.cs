// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4BuiltInRulesModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules
{
    using System.ComponentModel.Composition;
    using CDP4BuiltInRules.Views;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="CDP4BuiltInRulesModule"/> Component
    /// </summary>
    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="CDP4BuiltInRulesModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(CDP4BuiltInRulesModule), "Built-In Rules Module - Community Edition")]
    public class CDP4BuiltInRulesModule : IModule 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4BuiltInRulesModule"/> class.
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
        /// <param name="thingDialogNavigationService">The MEF injected instance of <see cref="IThingDialogNavigationService"/></param>
        /// <param name="dialogNavigationService">The MEF injected instance of <see cref="IDialogNavigationService"/></param>
        [ImportingConstructor]
        public CDP4BuiltInRulesModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="CDP4BuiltInRulesModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="CDP4BuiltInRulesModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="CDP4BuiltInRulesModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that is used by the <see cref="CDP4BuiltInRulesModule"/> to support <see cref="Thing"/> dialog navigation
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="CDP4BuiltInRulesModule"/> to support generic dialog navigation.
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(BuiltInRulesRibbonPage));
            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            // TODO: create a BuiltInRulesRibbonPart
        }
    }
}
