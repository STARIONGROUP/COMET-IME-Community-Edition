// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinRibbonPart.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4ShellDialogs.ViewModels;
    using Microsoft.Practices.ServiceLocation;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ISession"/> that is active in the addin
        /// </summary>
        private ISession session;

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
        public AddinRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService)
        {
            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);            
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
            IDialogNavigationService dialogService;

            switch (ribbonControlId)
            {
                case "CDP4_Open":
                    dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
                    var dataSelection = new DataSourceSelectionViewModel();
                    var dataSelectionResult = dialogService.NavigateModal(dataSelection) as DataSourceSelectionResult;
                    break;
                case "CDP4_Close":
                    await this.session.Close();
                    break;
                case "CDP4_ProxySettings":
                    dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
                    var proxyServerViewModel = new ProxyServerViewModel();
                    var proxyServerViewModelResult = dialogService.NavigateModal(proxyServerViewModel) as DataSourceSelectionResult;
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
                case "CDP4_Open":
                    return this.session == null;
                case "CDP4_Close":
                    return this.session != null;
                case "CDP4_ProxySettings":
                    return this.session == null;
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