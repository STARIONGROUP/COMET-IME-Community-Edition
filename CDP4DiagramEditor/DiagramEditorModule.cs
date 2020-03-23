// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4BuiltInRulesModule.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2020 RHEA S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor
{
    using System.ComponentModel.Composition;

    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    using Views;

    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4DiagramEditor.Views.ContextualRibbonPages;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="CDP4DiagramEditorModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(CDP4DiagramEditorModule), "Diagram editor Module")]
    public class CDP4DiagramEditorModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4DiagramEditorModule"/> class.
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
        public CDP4DiagramEditorModule(IRegionManager regionManager, IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
        {
            this.RegionManager = regionManager;
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="CDP4DiagramEditorModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="CDP4DiagramEditorModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="CDP4DiagramEditorModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that is used by the <see cref="CDP4DiagramEditorModule"/> to support <see cref="Thing"/> dialog navigation
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used by the <see cref="CDP4DiagramEditorModule"/> to support generic dialog navigation.
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(DiagramRibbonPageCategory));
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(CDP4DiagramEditorRibbon));
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(DiagramToolsRibbonPage));
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(DiagramDesignRibbonPage));
        }
    }
}
