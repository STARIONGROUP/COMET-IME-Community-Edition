// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRibbonPart.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4SiteDirectory.ViewModels;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="SiteDirectoryRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class SiteDirectoryRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ViewModels.DomainOfExpertiseBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private DomainOfExpertiseBrowserViewModel domainOfExpertiseBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.ModelBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private ModelBrowserViewModel modelBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.NaturalLanguageBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private NaturalLanguageBrowserViewModel naturalLanguageBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.OrganizationBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private OrganizationBrowserViewModel organizationBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.PersonBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private PersonBrowserViewModel personBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.RoleBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private RoleBrowserViewModel roleBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.PersonBrowserViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private SiteRdlBrowserViewModel siteRdlBrowserViewModel;

        /// <summary>
        /// The <see cref="ViewModels.ShowDeprecatedBrowserRibbonViewModel"/> of one <see cref="ISession"/> that can be opened using the current <see cref="RibbonPart"/>
        /// </summary>
        private readonly ShowDeprecatedBrowserRibbonViewModel showDeprecatedBrowserRibbonViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// The instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">The instance of <see cref="IThingDialogNavigationService"/> that orchestrates navigation of <see cref="IThingDialogView"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public SiteDirectoryRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, ICDPMessageBus messageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.showDeprecatedBrowserRibbonViewModel = new ShowDeprecatedBrowserRibbonViewModel(messageBus);
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public ISession Session { get; private set; }

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
            return "SiteDirectoryRibbon";
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

                this.domainOfExpertiseBrowserViewModel = new DomainOfExpertiseBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.modelBrowserViewModel = new ModelBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.naturalLanguageBrowserViewModel = new NaturalLanguageBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.organizationBrowserViewModel = new OrganizationBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.personBrowserViewModel = new PersonBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.roleBrowserViewModel = new RoleBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
                this.siteRdlBrowserViewModel = new SiteRdlBrowserViewModel(session, siteDirectory, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

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
            this.PanelNavigationService.CloseInAddIn(this.domainOfExpertiseBrowserViewModel);
            this.domainOfExpertiseBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.modelBrowserViewModel);
            this.modelBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.naturalLanguageBrowserViewModel);
            this.naturalLanguageBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.organizationBrowserViewModel);
            this.organizationBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.personBrowserViewModel);
            this.personBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.roleBrowserViewModel);
            this.roleBrowserViewModel = null;

            this.PanelNavigationService.CloseInAddIn(this.siteRdlBrowserViewModel);
            this.siteRdlBrowserViewModel = null;
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
                case "ShowDomainsOfExpertise":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.domainOfExpertiseBrowserViewModel);
                    break;
                case "ShowModels":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.modelBrowserViewModel);
                    break;
                case "ShowLanguages":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.naturalLanguageBrowserViewModel);
                    break;
                case "ShowOrganizations":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.organizationBrowserViewModel);
                    break;
                case "ShowPersons":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.personBrowserViewModel);
                    break;
                case "ShowRoles":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.roleBrowserViewModel);
                    break;
                case "ShowSiteRDLs":
                    this.PanelNavigationService.OpenExistingOrOpenInAddIn(this.siteRdlBrowserViewModel);
                    break;
                case "ShowHideDeprecatedThings":
                    this.showDeprecatedBrowserRibbonViewModel.ShowDeprecatedThings = !this.showDeprecatedBrowserRibbonViewModel.ShowDeprecatedThings;
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
                case "ShowDomainsOfExpertise":
                    return this.domainOfExpertiseBrowserViewModel != null;
                case "ShowModels":
                    return this.modelBrowserViewModel != null;
                case "ShowLanguages":
                    return this.naturalLanguageBrowserViewModel != null;
                case "ShowOrganizations":
                    return this.organizationBrowserViewModel != null;
                case "ShowPersons":
                    return this.personBrowserViewModel != null;
                case "ShowRoles":
                    return this.roleBrowserViewModel != null;
                case "ShowSiteRDLs":
                    return this.siteRdlBrowserViewModel != null;
                case "ShowHideDeprecatedThings":
                    return this.showDeprecatedBrowserRibbonViewModel.HasSession;
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
                case "ShowDomainsOfExpertise":
                    return converter.GetImage(ClassKind.DomainOfExpertise, false);
                case "ShowModels":
                    return converter.GetImage(ClassKind.EngineeringModel, false);
                case "ShowLanguages":
                    return converter.GetImage(ClassKind.NaturalLanguage, false);
                case "ShowOrganizations":
                    return converter.GetImage(ClassKind.Organization, false);
                case "ShowPersons":
                    return converter.GetImage(ClassKind.Person, false);
                case "ShowRoles":
                    return converter.GetImage(ClassKind.PersonRole, false);
                case "ShowSiteRDLs":
                    return converter.GetImage(ClassKind.SiteReferenceDataLibrary, false);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the label as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the label
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 1024 characters
        /// </remarks>
        public override string GetLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            switch (ribbonControlId)
            {
                case "ShowHideDeprecatedThings":
                    return this.showDeprecatedBrowserRibbonViewModel.ShowDeprecatedThings ? "Hide Deprecated Things" : "Show Deprecated Things";
                default:
                    return string.Empty;
            }
        }
    }
}
