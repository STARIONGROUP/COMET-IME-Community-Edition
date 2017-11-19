// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRibbonPart.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using NLog;
    using ReactiveUI;
    using ViewModels;

    /// <summary>
    /// The purpose of the <see cref="RequirementRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class RequirementRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of open <see cref="RequirementsBrowserViewModel"/>
        /// </summary>
        private readonly List<RequirementsBrowserViewModel> openRequirementBrowser;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRibbonPart"/> class.
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
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs
        /// </param>
        public RequirementRibbonPart(int order, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService)
        {
            this.openRequirementBrowser = new List<RequirementsBrowserViewModel>();
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

            if (ribbonControlId.Contains("ShowRequirement"))
            {
                this.ShowOrCloseRequirements(ribbonControlTag);
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

            if (ribbonControlId.Contains("ShowRequirements"))
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

            if (ribbonControlId == "ShowRequirements")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = string.Format("{0} - {1} : [{2}]", 
                                        engineeringModel.EngineeringModelSetup.ShortName, 
                                        iteration.IterationSetup.IterationNumber, 
                                        tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format("<button id=\"ShowRequirement_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />", iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            this.UpdateControlIdentifier(menuxml);      
            return menuxml;
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
            if (ribbonControlId.Contains("ShowRequirements"))
            {
                return converter.GetImage(ClassKind.RequirementsSpecification, false);
            }

            return null;
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
            return "RequirementRibbon";
        }

        /// <summary>
        /// Update the <see cref="ControlIdentiefers"/> list to include the contents of the dynamic menu
        /// </summary>
        /// <param name="dynamicMenuContent">
        /// The contents of the dynamic menu
        /// </param>
        private void UpdateControlIdentifier(string dynamicMenuContent)
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
                var browser = this.openRequirementBrowser.SingleOrDefault(x => x.Thing.Container == iteration);
                if (browser != null)
                {
                    this.PanelNavigationService.Close(browser, false);
                    this.openRequirementBrowser.Remove(browser);
                }

                this.Iterations.RemoveAll(x => x == iteration);
            }
        }

        /// <summary>
        /// Close all the panels and dispose of them
        /// </summary>
        private void CloseAll()
        {
            foreach (var browser in this.openRequirementBrowser)
            {
                this.PanelNavigationService.Close(browser, false);
            }

            this.openRequirementBrowser.Clear();
        }

        /// <summary>
        /// Show or close the <see cref="RequirementsBrowserViewModel"/>
        /// </summary>
        /// <param name="iterationId">
        /// the unique id of the <see cref="Iteration"/> that is being represented by the <see cref="RequirementsBrowserViewModel"/>
        /// </param>
        private void ShowOrCloseRequirements(string iterationId)
        {
            var iterationUniqueId = Guid.Parse(iterationId);

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == iterationUniqueId);
            if (iteration == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.openRequirementBrowser.SingleOrDefault(x => x.Thing == iteration);
            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.openRequirementBrowser.Remove(browser);
                return;
            }

            browser = new RequirementsBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService);

            this.openRequirementBrowser.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }
    }
}
