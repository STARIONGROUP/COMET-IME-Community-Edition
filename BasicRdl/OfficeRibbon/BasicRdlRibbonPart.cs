// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicRdlRibbonPart.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Threading.Tasks;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.FavoritesService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="BasicRdlRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class BasicRdlRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ViewModels.MeasurementUnitsBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private MeasurementUnitsBrowserViewModel measurementUnitsBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.MeasurementScalesBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private MeasurementScalesBrowserViewModel measurementScalesBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.RulesBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private RulesBrowserViewModel rulesBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.CategoryBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private CategoryBrowserViewModel categoryBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.ParameterTypesBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private ParameterTypesBrowserViewModel parameterTypesBrowserViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicRdlRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// The instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">The instance of <see cref="IThingDialogNavigationService"/> that orchestrates navigation of <see cref="IThingDialogView"/></param>
        /// <param name="dialogNavigationService">The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs</param>
        /// <param name="pluginSettingsService">The instance of <see cref="IPluginSettingsService"/> that reads and writes </param>
        /// <param name="favoritesService">The instance of <see cref="IDialogNavigationService"/> that</param>
        public BasicRdlRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IFavoritesService favoritesService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
            this.FavoritesService = favoritesService;
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFavoritesService"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public IFavoritesService FavoritesService { get; private set; }

        /// <summary>
        /// Gets the the current executing assembly
        /// </summary>
        /// <returns>
        /// an instance of <see cref="Assembly"/>
        /// </returns>
        protected override Assembly GetCurrentAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Gets the name of the resource that contains the Ribbon XML
        /// </summary>
        /// <returns>
        /// The name of the ribbon XML resource
        /// </returns>
        protected override string GetRibbonXmlResourceName()
        {
            return "rdlribbon";
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (this.FluentRibbonManager == null)
            {
                return;
            }

            if (!this.FluentRibbonManager.IsActive)
            {
                return;
            }

            if (sessionChange.Status == SessionStatus.Open)
            {
                var session = sessionChange.Session;
                var siteDirectory = session.RetrieveSiteDirectory();

                this.measurementUnitsBrowserViewModel = new MeasurementUnitsBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.measurementScalesBrowserViewModel = new MeasurementScalesBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.rulesBrowserViewModel = new RulesBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.categoryBrowserViewModel = new CategoryBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.parameterTypesBrowserViewModel = new ParameterTypesBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService, this.FavoritesService);

                this.Session = session;
            }

            if (sessionChange.Status == SessionStatus.Closed)
            {
                this.CloseAll();
                this.Session = null;
            }
        }

        /// <summary>
        /// Close all the panels and dispose of them
        /// </summary>
        private void CloseAll()
        {
            this.PanelNavigationService.CloseInAddIn(this.measurementUnitsBrowserViewModel);
            this.measurementUnitsBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.measurementScalesBrowserViewModel);
            this.measurementScalesBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.rulesBrowserViewModel);
            this.rulesBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.categoryBrowserViewModel);
            this.categoryBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.parameterTypesBrowserViewModel);
            this.parameterTypesBrowserViewModel = null;
        }

        /// <summary>
        /// Invokes the action as a result of a ribbon control being clicked, selected, etc.
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>        
        public override async Task OnAction(string ribbonControlId, string ribbonControlTag = "")
        {
            if (this.Session == null)
            {
                return;
            }

            switch (ribbonControlId)
            {
                case "ShowMeasurementUnits":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.measurementUnitsBrowserViewModel);
                    break;
                case "ShowMeasurementScales":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.measurementScalesBrowserViewModel);
                    break;
                case "ShowParameterTypes":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.parameterTypesBrowserViewModel);
                    break;
                case "ShowRules":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.rulesBrowserViewModel);
                    break;
                case "ShowCategories":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.categoryBrowserViewModel);
                    break;
                default:
                    logger.Debug("The ribbon control with Id {0} and Tag {1} is not handled by the current RibbonPart", ribbonControlId, ribbonControlTag);
                    break;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a control is enabled or disabled
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>        
        /// <returns>
        /// true if enabled, false if not enabled
        /// </returns>
        public override bool GetEnabled(string ribbonControlId, string ribbonControlTag = "")
        {
            switch (ribbonControlId)
            {
                case "ShowMeasurementUnits":
                    return this.measurementUnitsBrowserViewModel != null;
                case "ShowMeasurementScales":
                    return this.measurementScalesBrowserViewModel != null;
                case "ShowParameterTypes":
                    return this.parameterTypesBrowserViewModel != null;
                case "ShowRules":
                    return this.rulesBrowserViewModel != null;
                case "ShowCategories":
                    return this.categoryBrowserViewModel != null;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="Image"/> to decorate the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        public override Image GetImage(string ribbonControlId, string ribbonControlTag = "")
        {
            var converter = new ThingToIconUriConverter();

            switch (ribbonControlId)
            {
                case "ShowMeasurementUnits":
                    return converter.GetImage(ClassKind.MeasurementUnit, false);
                case "ShowMeasurementScales":
                    return converter.GetImage(ClassKind.MeasurementScale, false);
                case "ShowParameterTypes":
                    return converter.GetImage(ClassKind.ParameterType, false);
                case "ShowRules":
                    return converter.GetImage(ClassKind.Rule, false);
                case "ShowCategories":
                    return converter.GetImage(ClassKind.Category, false);
                default:
                    return null;
            }
        }
    }
}
