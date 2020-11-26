// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelRibbonPart.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ViewModels;
    using ReactiveUI;
    using CDP4Common.SiteDirectoryData;

    using CDP4EngineeringModel.Services;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="EngineeringModelRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class EngineeringModelRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of open <see cref="ElementDefinitionsBrowserViewModel"/>
        /// </summary>
        private readonly List<ElementDefinitionsBrowserViewModel> openElementDefinitionBrowser;

        /// <summary>
        /// The list of open <see cref="OptionBrowserViewModel"/>
        /// </summary>
        private readonly List<OptionBrowserViewModel> openOptionBrowser;

        /// <summary>
        /// The list of open <see cref="FiniteStateBrowserViewModel"/>
        /// </summary>
        private readonly List<FiniteStateBrowserViewModel> finiteStateBrowserViewModel;

        /// <summary>
        /// The list of open <see cref="PublicationBrowserViewModel"/>
        /// </summary>
        private readonly List<PublicationBrowserViewModel> publicationBrowserViewModel;

        /// <summary>
        /// The <see cref="IParameterSubscriptionBatchService"/> used to create multiple <see cref="ParameterSubscription"/>s in a batch operation
        /// </summary>
        private readonly IParameterSubscriptionBatchService parameterSubscriptionBatchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// An instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// An instance of <see cref="IDialogNavigationService"/> that orchestrates dialog navigation.
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public EngineeringModelRibbonPart(int order, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService, IParameterSubscriptionBatchService parameterSubscriptionBatchService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.parameterSubscriptionBatchService = parameterSubscriptionBatchService;

            this.openElementDefinitionBrowser = new List<ElementDefinitionsBrowserViewModel>();
            this.openOptionBrowser = new List<OptionBrowserViewModel>();
            this.finiteStateBrowserViewModel = new List<FiniteStateBrowserViewModel>();
            this.publicationBrowserViewModel = new List<PublicationBrowserViewModel>();
            this.Iterations = new List<Iteration>();

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationChangeEventHandler);
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets a List of <see cref="Iteration"/> that are opened
        /// </summary>
        public List<Iteration> Iterations { get; private set; }

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
            Logger.Trace("OnAction: Ribbon Control Id: {0}; Ribbon Control Tag: {1}", ribbonControlId, ribbonControlTag);

            if (this.Session == null)
            {
                return;
            }

            if (ribbonControlId.Contains("ShowElementDefinitionsBrowser_"))
            {
                this.ShowOrCloseElementDefinitionsBrowser(ribbonControlTag);
            }

            if (ribbonControlId.Contains("ShowOptionBrowser_"))
            {
                this.ShowOrCloseOptionBrowser(ribbonControlTag);
            }

            if (ribbonControlId.Contains("ShowFiniteStateBrowser_"))
            {
                this.ShowOrCloseFiniteStateBrowser(ribbonControlTag);
            }

            if (ribbonControlId.Contains("ShowPublicationBrowser_"))
            {
                this.ShowOrClosePublicationBrowser(ribbonControlTag);
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
            Logger.Trace("GetEnabled: Ribbon Control Id: {0}; Ribbon Control Tag: {1}", ribbonControlId, ribbonControlTag);

            if (this.Session == null)
            {
                return false;
            }

            if (ribbonControlId == "CDP4_SelectModelToOpen")
            {
                return true;
            }

            if (ribbonControlId == "CDP4_SelectModelToClose")
            {
                return this.Iterations.Count > 0;
            }

            if (ribbonControlId.Contains("ShowElementDefinitionsBrowser_"))
            {
                return this.Iterations.Any();
            }

            if (ribbonControlId.Contains("ShowOptionBrowser_"))
            {
                return this.Iterations.Any();
            }

            if (ribbonControlId.Contains("ShowFiniteStateBrowser_"))
            {
                return this.Iterations.Any();
            }

            if (ribbonControlId.Contains("ShowPublicationBrowser_"))
            {
                return this.Iterations.Any();
            }

            return false;
        }

        /// <summary>
        /// Gets the the content of a Dynamic Menu
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// Ribbon XML that is the content of the Dynamic Menu
        /// </returns>
        public override string GetContent(string ribbonControlId, string ribbonControlTag = "")
        {
            Logger.Trace("GetContent: Ribbon Control Id: {0}; Ribbon Control Tag: {1}", ribbonControlId, ribbonControlTag);

            var menuxml = string.Empty;

            if (ribbonControlId == "ShowElementDefinitionsBrowser_")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = string.Format("{0} - {1} : [{2}]", engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber, tuple.Item1 == null ? String.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowElementDefinitionsBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            if (ribbonControlId == "ShowOptionBrowser_")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = string.Format("{0} - {1} : [{2}]", engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber, tuple.Item1 == null ? String.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowOptionBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            if (ribbonControlId == "ShowFiniteStateBrowser_")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = string.Format("{0} - {1} : [{2}]", engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber, tuple.Item1 == null ? String.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowFiniteStateBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            if (ribbonControlId == "ShowPublicationBrowser_")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = string.Format("{0} - {1} : [{2}]", engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber, tuple.Item1 == null ? String.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowPublicationBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            this.UpdateControlIdentiefers(menuxml);
            return menuxml;
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
            return "engineeringmodelribbon";
        }

        /// <summary>
        /// Update the <see cref="ControlIdentiefers"/> list to include the contents of the dynamic menu
        /// </summary>
        /// <param name="dynamicMenuContent">
        /// The contents of the dynamic menu
        /// </param>
        private void UpdateControlIdentiefers(string dynamicMenuContent)
        {
            XDocument doc;
            this.ControlIdentifiers.Clear();

            // Add the original RibbonXml identifiers
            doc = XDocument.Parse(this.RibbonXml);
            var docids = doc.Descendants().Attributes("id").Select(x => x.Value).ToList();
            this.ControlIdentifiers.AddRange(docids);

            // Append the dynamic menu identifiers
            doc = XDocument.Parse(dynamicMenuContent);
            var dynamicids = doc.Descendants().Attributes("id").Select(x => x.Value).ToList();
            this.ControlIdentifiers.AddRange(dynamicids);
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
                this.Session = sessionChange.Session;
            }

            if (sessionChange.Status == SessionStatus.Closed)
            {
                this.CloseAll();
                this.Iterations.Clear();
                this.Session = null;
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler for <see cref="Iteration"/>
        /// </summary>
        /// <param name="iterationEvent">The <see cref="ObjectChangedEvent"/></param>
        private void IterationChangeEventHandler(ObjectChangedEvent iterationEvent)
        {
            if (this.Session == null)
            {
                return;
            }

            if (iterationEvent.EventKind == EventKind.Added)
            {
                this.Iterations.Add(iterationEvent.ChangedThing as Iteration);
            }
            else if (iterationEvent.EventKind == EventKind.Removed)
            {
                var iteration = iterationEvent.ChangedThing as Iteration;
                var browser = this.openElementDefinitionBrowser.SingleOrDefault(x => x.Thing == iteration);
                if (browser != null)
                {
                    this.PanelNavigationService.Close(browser, false);
                    this.openElementDefinitionBrowser.Remove(browser);
                }

                this.Iterations.RemoveAll(x => x == iteration);
            }
        }

        /// <summary>
        /// Close all the panels and dispose of them
        /// </summary>
        private void CloseAll()
        {
            foreach (var browserViewModel in this.openElementDefinitionBrowser)
            {
                this.PanelNavigationService.Close(browserViewModel, false);
            }

            this.openElementDefinitionBrowser.Clear();
        }

        /// <summary>
        /// Show or close the <see cref="ElementDefinitionsBrowserViewModel"/>
        /// </summary>
        /// <param name="iterationId">
        /// the unique id of the <see cref="Iteration"/> that is being represented by the <see cref="ElementDefinitionsBrowserViewModel"/>
        /// </param>
        private void ShowOrCloseElementDefinitionsBrowser(string iterationId)
        {
            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);
            if (iteration == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.openElementDefinitionBrowser.SingleOrDefault(x => x.Thing == iteration);
            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.openElementDefinitionBrowser.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;
            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new ElementDefinitionsBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService, this.parameterSubscriptionBatchService);

            this.openElementDefinitionBrowser.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }

        /// <summary>
        /// Show or close the <see cref="OptionBrowserViewModel"/>
        /// </summary>
        /// <param name="iterationId">
        /// the unique id of the <see cref="Iteration"/> that is being represented by the <see cref="OptionBrowserViewModel"/>
        /// </param>
        private void ShowOrCloseOptionBrowser(string iterationId)
        {
            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);
            if (iteration == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.openOptionBrowser.SingleOrDefault(x => x.Thing == iteration);
            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.openOptionBrowser.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;
            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new OptionBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.openOptionBrowser.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }

        /// <summary>
        /// Show or close the <see cref="FiniteStateBrowserViewModel"/>
        /// </summary>
        /// <param name="iterationId">
        /// the unique id of the <see cref="Iteration"/> that is being represented by the <see cref="FiniteStateBrowserViewModel"/>
        /// </param>
        private void ShowOrCloseFiniteStateBrowser(string iterationId)
        {
            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);
            if (iteration == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.finiteStateBrowserViewModel.SingleOrDefault(x => x.Thing == iteration);
            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.finiteStateBrowserViewModel.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;
            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new FiniteStateBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.finiteStateBrowserViewModel.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }

        /// <summary>
        /// Show or close the <see cref="PublicationBrowserViewModel"/>
        /// </summary>
        /// <param name="iterationId">
        /// the unique id of the <see cref="Iteration"/> that is being represented by the <see cref="PublicationBrowserViewModel"/>
        /// </param>
        private void ShowOrClosePublicationBrowser(string iterationId)
        {
            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);
            if (iteration == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.publicationBrowserViewModel.SingleOrDefault(x => x.Thing == iteration);
            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.publicationBrowserViewModel.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;
            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new PublicationBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.publicationBrowserViewModel.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }
    }
}