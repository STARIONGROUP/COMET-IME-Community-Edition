// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Attributes;
    using CDP4LogInfo.Views;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// Initializes the LogInfo Plugin
    /// </summary>
    [ModuleExportName(typeof(LogInfoModule), "Log Information Module - Community Edition")]
    public class LogInfoModule : IModule
    {
        /// <summary>
        /// the <see cref="IRegionManager"/> that is used by the <see cref="LogInfoModule"/> to register the regions
        /// </summary>
        public readonly IRegionManager RegionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoModule"/> class
        /// </summary>
        /// <param name="regionManager">The application <see cref="IRegionManager"/></param>
        [ImportingConstructor]
        public LogInfoModule(IRegionManager regionManager)
        {
            this.RegionManager = regionManager;
        }

        /// <summary>
        /// Initialize the module with the static views
        /// </summary>
        public void Initialize()
        {
            this.RegionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(LogInfoControls));
        }
    }
}