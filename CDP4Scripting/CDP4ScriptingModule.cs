// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4ScriptingModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Scripting
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Views;

    /// <summary>
    ///  The <see cref="IModule"/> implementation for the <see cref="CDP4ScriptingModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(CDP4ScriptingModule), "Scripting Module - Community Edition")]
    public class CDP4ScriptingModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4ScriptingModule"/> class.
        /// </summary>
        /// <param name="regionManager">
        /// The (MEF) injected instance of <see cref="IRegionManager"/>.
        /// </param>
        /// <param name="panelNavigationService">
        /// The (MEF injected) instance of <see cref="IPanelNavigationService"/>
        /// </param>
        [ImportingConstructor]
        public CDP4ScriptingModule(IRegionManager regionManager, IPanelNavigationService panelNavigationService)
        {
            this.RegionManager = regionManager;
            this.PanelNavigationService = panelNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IRegionManager"/> that is used by the <see cref="CDP4ScriptingModule"/> to register the regions
        /// </summary>
        internal IRegionManager RegionManager { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="CDP4ScriptingModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }
        
        /// <summary>
        /// Initialize the module with the static views
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(ScriptingEngineRibbonPageGroup));
        }
    }
}
