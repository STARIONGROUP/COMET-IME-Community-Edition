// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewEditorModule.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor
{
    using System.ComponentModel.Composition;

    using CDP4Composition;
    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;

    using CDP4OfficeInfrastructure;

    /// <summary>
    /// The purpose of the <see cref="CrossViewEditorModule"/> class is to enable this library
    /// to be loaded as an <see cref="IModule"/>
    /// </summary>
    [Export(typeof(IModule))]
    public class CrossViewEditorModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewEditorModule"/> class.
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
        /// <param name="messageBus">
        /// The MEF injected instance of <see cref="ICDPMessageBus"/>
        /// </param>
        [ImportingConstructor]
        public CrossViewEditorModule(IFluentRibbonManager ribbonManager, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IOfficeApplicationWrapper officeApplicationWrapper, ICDPMessageBus messageBus)
        {
            this.RibbonManager = ribbonManager;
            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.OfficeApplicationWrapper = officeApplicationWrapper;
            this.MessageBus = messageBus;
        }

        /// <summary>
        /// Gets the <see cref="IFluentRibbonManager"/> that is used by the <see cref="CrossViewEditorModule"/> to register Office Fluent Ribbon XML
        /// </summary>
        internal IFluentRibbonManager RibbonManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="CrossViewEditorModule"/> to support panel navigation
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
        /// Gets the <see cref="ICDPMessageBus"/> used in the application
        /// </summary>
        public ICDPMessageBus MessageBus { get; }

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
            var ribbonPart = new CrossViewEditorRibbonPart(3000, this.PanelNavigationService, this.ThingDialogNavigationService, this.DialogNavigationService, null, this.OfficeApplicationWrapper, this.MessageBus);
            this.RibbonManager.RegisterRibbonPart(ribbonPart);
        }
    }
}
