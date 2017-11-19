// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Views;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="CDP4ObjectBrowser"/> Component
    /// </summary>
    [ModuleExportName(typeof(ObjectBrowserModule), "Object Browser Module - Community Edition")]
    public class ObjectBrowserModule : IModule
    {
        /// <summary>
        /// the <see cref="IRegionManager"/> that is used by the <see cref="ObjectBrowserModule"/> to register the regions
        /// </summary>
        private readonly IRegionManager regionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserModule"/> class.
        /// </summary>
        /// <param name="regionManager">
        /// The (MEF injected) region manager.
        /// </param>
        [ImportingConstructor]
        public ObjectBrowserModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(ObjectBrowserRibbonPage));
        }
    }
}
