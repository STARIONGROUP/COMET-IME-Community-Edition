// -------------------------------------------------------------------------------------------------
// <copyright file="PropertyGridModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using Views;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="PropertyGrid"/> Component
    /// </summary>
    [ModuleExportName(typeof(PropertyGridModule), "Property Grid Module - Community Edition")]
    public class PropertyGridModule : IModule
    {
        /// <summary>
        /// The region manager.
        /// </summary>
        public readonly IRegionManager RegionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridModule"/> class.
        /// </summary>
        /// <param name="regionManager">
        /// The region manager.
        /// </param>
        [ImportingConstructor]
        public PropertyGridModule(IRegionManager regionManager)
        {
            this.RegionManager = regionManager;
        }

        /// <summary>
        /// Initializes the module
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(RegionNames.RibbonRegion, typeof(ViewRibbonControl));
        }
    }
}