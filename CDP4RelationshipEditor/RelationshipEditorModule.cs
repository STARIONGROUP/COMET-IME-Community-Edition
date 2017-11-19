// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Views;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="RelationshipEditorModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(RelationshipEditorModule), "Relationship Editor Module - Community Edition")]
    public class RelationshipEditorModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipEditorModule"/> class.
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
        [ImportingConstructor]
        public RelationshipEditorModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="RelationshipEditorModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="RelationshipEditorModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="RelationshipEditorModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="RelationshipEditorModule"/> to support panel navigation
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

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
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(RelationshipEditorRibbon));
        }
    }
}