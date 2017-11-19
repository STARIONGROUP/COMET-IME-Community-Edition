// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetGeneratorModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4OfficeInfrastructure;
    using Microsoft.Practices.Prism.Modularity;

    /// <summary>
    /// The purpose of the <see cref="ParameterSheetGeneratorModule"/> class is to enable this library
    /// to be loaded as a PRISM Module
    /// </summary>
    [ModuleExportName(typeof(ParameterSheetGeneratorModule), "Parameter Sheet Generator Module - Community Edition")]
    public class ParameterSheetGeneratorModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSheetGeneratorModule"/> class.
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
        /// <param name="officeApplicationWrapper">
        /// The MEF injected instance of <see cref="IOfficeApplicationWrapper"/>
        /// </param>
        [ImportingConstructor]
        public ParameterSheetGeneratorModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IOfficeApplicationWrapper officeApplicationWrapper)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.OfficeApplicationWrapper = officeApplicationWrapper;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="ParameterSheetGeneratorModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="ParameterSheetGeneratorModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used in the application
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used in the application
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IOfficeApplicationWrapper"/> used in the application
        /// </summary>
        internal IOfficeApplicationWrapper OfficeApplicationWrapper { get; private set; }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.RegisterRibbonPart();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonPart()
        {
            var ribbonPart = new ParameterSheetGeneratorRibbonPart(10, this.PanelNavigationService, this.ThingDialogNavigationService, this.DialogNavigationService, this.OfficeApplicationWrapper);
            this.RibbonManager.RegisterRibbonPart(ribbonPart);
        }
    }
}