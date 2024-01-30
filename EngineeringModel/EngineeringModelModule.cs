// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel
{
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4EngineeringModel.Services;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="EngineeringModelModule"/> Component
    /// </summary>
    [Export(typeof(IModule))]
    public class EngineeringModelModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelModule"/> class.
        /// </summary>
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
        /// <param name="parameterSubscriptionBatchService">
        /// The (MEF injected) instance of <see cref="IParameterSubscriptionBatchService"/>
        /// </param>
        /// <param name="parameterActualFiniteStateListApplicationBatchService">
        /// The (MEF injected) instance of <see cref="IParameterActualFiniteStateListApplicationBatchService"/>
        /// </param>
        /// <param name="changeOwnershipBatchService">
        /// The (MEF injected) instance of <see cref="IChangeOwnershipBatchService"/>
        /// </param>
        /// <param name="messageBus">
        /// The (MEF injected) instance of <see cref="ICDPMessageBus"/>
        /// </param>
        [ImportingConstructor]
        public EngineeringModelModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService, IParameterSubscriptionBatchService parameterSubscriptionBatchService, IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService, IChangeOwnershipBatchService changeOwnershipBatchService, ICDPMessageBus messageBus)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.PluginSettingsService = pluginSettingsService;
            this.ParameterSubscriptionBatchService = parameterSubscriptionBatchService;
            this.ParameterActualFiniteStateListApplicationBatchService = parameterActualFiniteStateListApplicationBatchService;
            this.ChangeOwnershipBatchService = changeOwnershipBatchService;
            this.CDPMessageBus = messageBus;
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
        /// Gets the <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </summary>
        internal IPluginSettingsService PluginSettingsService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterSubscriptionBatchService"/> used to create multiple <see cref="ParameterSubscription"/>s
        /// in a batch operation
        /// </summary>
        internal IParameterSubscriptionBatchService ParameterSubscriptionBatchService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IParameterActualFiniteStateListApplicationBatchService"/> used to update multiple <see cref="Parameter"/>s state dependency
        /// in a batch operation
        /// </summary>
        internal IParameterActualFiniteStateListApplicationBatchService ParameterActualFiniteStateListApplicationBatchService { get; private set; }

        /// <summary>
        /// The <see cref="IChangeOwnershipBatchService"/> used to change the ownership of multiple <see cref="IOwnedThing"/>s in a batch operation
        /// </summary>
        internal IChangeOwnershipBatchService ChangeOwnershipBatchService { get; private set; }

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        public ICDPMessageBus CDPMessageBus { get; }

        /// <summary>
        /// Initialize the module
        /// </summary>
        public void Initialize()
        {
            this.RegisterRibbonParts();
        }

        /// <summary>
        /// Register the <see cref="RibbonPart"/> implementations of the current Module
        /// </summary>
        private void RegisterRibbonParts()
        {
            var rdlRibbonPart = new EngineeringModelRibbonPart(10, this.PanelNavigationService, this.DialogNavigationService, this.ThingDialogNavigationService, this.PluginSettingsService, this.ParameterSubscriptionBatchService, this.ParameterActualFiniteStateListApplicationBatchService, this.ChangeOwnershipBatchService, this.CDPMessageBus);
            this.RibbonManager.RegisterRibbonPart(rdlRibbonPart);
        }
    }
}
