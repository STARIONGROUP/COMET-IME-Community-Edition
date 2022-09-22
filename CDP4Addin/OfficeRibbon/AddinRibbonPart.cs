// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinRibbonPart.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4AddinCE
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4AddinCE.Settings;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.AppSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ShellDialogs.ViewModels;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="AddinRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class AddinRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ISession"/> that is active in the addin
        /// </summary>
        private ISession session;

        /// <summary>
        /// The <see cref="IAppSettingsService{AddinAppSettings}"/> used to load application settings for the Addin 
        /// </summary>
        private readonly IAppSettingsService<AddinAppSettings> appSettingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddinRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// An instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="appSettingService">
        /// The <see cref="IAppSettingsService{AddinAppSettings}"/> used to load application settings for the Addin
        /// </param>
        public AddinRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IAppSettingsService<AddinAppSettings> appSettingService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
            this.appSettingService = appSettingService;
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
            switch (ribbonControlId)
            {
                case "CDP4_Open":
                    var dataSelection = new DataSourceSelectionViewModel(this.DialogNavigationService);
                    var dataSelectionResult = this.DialogNavigationService.NavigateModal(dataSelection) as DataSourceSelectionResult;

                    if (dataSelectionResult?.OpenModel ?? false)
                    {
                        this.OpenModelDialog();
                    }

                    break;
                case "CDP4_Close":
                    await this.session.Close();
                    break;
                case "CDP4_ProxySettings":
                    var proxyServerViewModel = new ProxyServerViewModel();
                    var proxyServerViewModelResult = this.DialogNavigationService.NavigateModal(proxyServerViewModel) as DataSourceSelectionResult;
                    break;
                case "CDP4_SelectModelToOpen":
                    this.OpenModelDialog();
                    break;
                case "CDP4_SelectModelToClose":
                    var sessionsClosing = new List<ISession> { this.session };
                    var modelClosingDialogViewModel = new ModelClosingDialogViewModel(sessionsClosing);
                    var modelClosingDialogViewModelResult = this.DialogNavigationService.NavigateModal(modelClosingDialogViewModel) as DataSourceSelectionResult;
                    break;
                case "CDP4_Plugins":
                    var modelPluginDialogViewModel = new PluginManagerViewModel<AddinAppSettings>(this.appSettingService);
                    var modelPluginDialogResult = this.DialogNavigationService.NavigateModal(modelPluginDialogViewModel) as DataSourceSelectionResult;
                    break;
                default:
                    logger.Debug("The ribbon control with Id {0} and Tag {1} is not handled by the current RibbonPart", ribbonControlId, ribbonControlTag);
                    break;
            }
        }

        /// <summary>
        /// Opens the Model Selection Dialog
        /// </summary>
        private void OpenModelDialog()
        {
            var sessionsOpening = new List<ISession> { this.session };
            var modelOpeningDialogViewModel = new ModelOpeningDialogViewModel(sessionsOpening, null);
            var modelOpeningDialogViewModelResult = this.DialogNavigationService.NavigateModal(modelOpeningDialogViewModel) as DataSourceSelectionResult;
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
                case "CDP4_Open":
                    return this.session == null;
                case "CDP4_Close":
                    return this.session != null;
                case "CDP4_ProxySettings":
                    return this.session == null;
                case "CDP4_SelectModelToOpen":
                    return this.session != null;
                case "CDP4_SelectModelToClose":
                    return (this.session != null) && (this.session.OpenIterations.Count > 0);
                case "CDP4_Plugins":
                    return true;
                default:
                    return false;
            }
        }

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
            return "addinribbon";
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
            if (!this.FluentRibbonManager.IsActive)
            {
                return;
            }

            if (sessionChange.Status == SessionStatus.Open)
            {
                this.session = sessionChange.Session;
            }

            if (sessionChange.Status == SessionStatus.Closed)
            {
                this.session = null;
            }
        }
    }
}
