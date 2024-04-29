﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelRibbonPart.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels;

    using NLog;

    using ReactiveUI;

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
        private readonly List<ElementDefinitionsBrowserViewModel> openElementDefinitionBrowsers;

        /// <summary>
        /// The list of open <see cref="OptionBrowserViewModel"/>
        /// </summary>
        private readonly List<OptionBrowserViewModel> openOptionBrowsers;

        /// <summary>
        /// The list of open <see cref="FiniteStateBrowserViewModel"/>
        /// </summary>
        private readonly List<FiniteStateBrowserViewModel> openFiniteStateBrowsers;

        /// <summary>
        /// The list of open <see cref="PublicationBrowserViewModel"/>
        /// </summary>
        private readonly List<PublicationBrowserViewModel> openPublicationBrowsers;

        /// <summary>
        /// The <see cref="IParameterSubscriptionBatchService"/> used to create multiple <see cref="ParameterSubscription"/>s in a batch operation
        /// </summary>
        private readonly IParameterSubscriptionBatchService parameterSubscriptionBatchService;

        /// <summary>
        /// The <see cref="IParameterActualFiniteStateListApplicationBatchService"/> used to update multiple <see cref="Parameter"/>s
        /// to set the <see cref="ActualFiniteStateList"/> in a batch operation
        /// </summary>
        private readonly IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService;

        /// <summary>
        /// The <see cref="IChangeOwnershipBatchService"/> used to change the ownership of multiple <see cref="IOwnedThing"/>s in a batch operation
        /// </summary>
        private readonly IChangeOwnershipBatchService changeOwnershipBatchService;

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
        /// <param name="parameterActualFiniteStateListApplicationBatchService">
        /// The <see cref="IParameterActualFiniteStateListApplicationBatchService"/> used to update multiple <see cref="Parameter"/>s
        /// to set the <see cref="ActualFiniteStateList"/> in a batch operation
        /// </param>
        /// <param name="changeOwnershipBatchService">
        /// The <see cref="IChangeOwnershipBatchService"/> used to change the ownership of multiple <see cref="IOwnedThing"/>s in a batch operation
        /// </param>
        public EngineeringModelRibbonPart(int order, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService, IParameterSubscriptionBatchService parameterSubscriptionBatchService, IParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService, IChangeOwnershipBatchService changeOwnershipBatchService, ICDPMessageBus messageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, messageBus)
        {
            this.parameterSubscriptionBatchService = parameterSubscriptionBatchService;
            this.parameterActualFiniteStateListApplicationBatchService = parameterActualFiniteStateListApplicationBatchService;
            this.changeOwnershipBatchService = changeOwnershipBatchService;

            this.openElementDefinitionBrowsers = new List<ElementDefinitionsBrowserViewModel>();
            this.openOptionBrowsers = new List<OptionBrowserViewModel>();
            this.openFiniteStateBrowsers = new List<FiniteStateBrowserViewModel>();
            this.openPublicationBrowsers = new List<PublicationBrowserViewModel>();
            this.Iterations = new List<Iteration>();

            this.CDPMessageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationChangeEventHandler);

            this.CDPMessageBus.Listen<HidePanelEvent>()
                .Subscribe(this.CloseHiddenPanel);
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

                    var label = string.Format(
                        "{0} - {1} : [{2}]",
                        engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber,
                        tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowElementDefinitionsBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid,
                            label);

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

                    var label = string.Format(
                        "{0} - {1} : [{2}]",
                        engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber,
                        tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowOptionBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid,
                            label);

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

                    var label = string.Format(
                        "{0} - {1} : [{2}]",
                        engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber,
                        tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowFiniteStateBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid,
                            label);

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

                    var label = string.Format(
                        "{0} - {1} : [{2}]",
                        engineeringModel.EngineeringModelSetup.ShortName,
                        iteration.IterationSetup.IterationNumber,
                        tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                    var menuContent =
                        string.Format(
                            "<button id=\"ShowPublicationBrowser_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />",
                            iteration.Iid,
                            label);

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
                var browser = this.openElementDefinitionBrowsers.SingleOrDefault(x => x.Thing == iteration);

                if (browser != null)
                {
                    this.PanelNavigationService.CloseInAddIn(browser);
                    this.openElementDefinitionBrowsers.Remove(browser);
                }

                this.Iterations.RemoveAll(x => x == iteration);
            }
        }

        /// <summary>
        /// Close a panel when it is being hidden.
        /// </summary>
        /// <param name="hidePanelEvent">
        /// The <see cref="HidePanelEvent"/>
        /// </param>
        private void CloseHiddenPanel(HidePanelEvent hidePanelEvent)
        {
            var allBrowsers = this.GetAllOpenBrowsers().SelectMany(x => x);
            var browser = allBrowsers.SingleOrDefault(x => x.Identifier == hidePanelEvent.Identifier);

            if (browser != null)
            {
                this.PanelNavigationService.CloseInAddIn(browser);

                if (this.openElementDefinitionBrowsers.Contains(browser))
                {
                    this.openElementDefinitionBrowsers.Remove(browser as ElementDefinitionsBrowserViewModel);
                }
                else if (this.openOptionBrowsers.Contains(browser))
                {
                    this.openOptionBrowsers.Remove(browser as OptionBrowserViewModel);
                }
                else if (this.openFiniteStateBrowsers.Contains(browser))
                {
                    this.openFiniteStateBrowsers.Remove(browser as FiniteStateBrowserViewModel);
                }
                else if (this.openPublicationBrowsers.Contains(browser))
                {
                    this.openPublicationBrowsers.Remove(browser as PublicationBrowserViewModel);
                }
            }
        }

        /// <summary>
        /// Retrieve all <see cref="IPanelViewModel"/>s related to this <see cref="EngineeringModelRibbonPart"/> that are currently open,
        /// grouped by the original private list they belong to.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of type <see cref="List{IPanelViewModel}"/> containing all <see cref="IPanelViewModel"/>s.
        /// </returns>
        internal List<List<IPanelViewModel>> GetAllOpenBrowsers()
        {
            return new List<List<IPanelViewModel>>
            {
                this.openFiniteStateBrowsers.Cast<IPanelViewModel>().ToList(),
                this.openElementDefinitionBrowsers.Cast<IPanelViewModel>().ToList(),
                this.openOptionBrowsers.Cast<IPanelViewModel>().ToList(),
                this.openPublicationBrowsers.Cast<IPanelViewModel>().ToList(),
            };
        }

        /// <summary>
        /// Close all the panels and dispose of them
        /// </summary>
        private void CloseAll()
        {
            foreach (var browserViewModel in this.openElementDefinitionBrowsers)
            {
                this.PanelNavigationService.CloseInAddIn(browserViewModel);
            }

            this.openElementDefinitionBrowsers.Clear();
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
            var browser = this.openElementDefinitionBrowsers.SingleOrDefault(x => x.Thing == iteration);

            if (browser != null)
            {
                this.PanelNavigationService.CloseInAddIn(browser);
                this.openElementDefinitionBrowsers.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;

            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new ElementDefinitionsBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService, this.parameterSubscriptionBatchService, this.changeOwnershipBatchService);

            this.openElementDefinitionBrowsers.Add(browser);
            this.PanelNavigationService.OpenInAddIn(browser);
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
            var browser = this.openOptionBrowsers.SingleOrDefault(x => x.Thing == iteration);

            if (browser != null)
            {
                this.PanelNavigationService.CloseInAddIn(browser);
                this.openOptionBrowsers.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;

            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new OptionBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.openOptionBrowsers.Add(browser);
            this.PanelNavigationService.OpenInAddIn(browser);
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
            var browser = this.openFiniteStateBrowsers.SingleOrDefault(x => x.Thing == iteration);

            if (browser != null)
            {
                this.PanelNavigationService.CloseInAddIn(browser);
                this.openFiniteStateBrowsers.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;

            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new FiniteStateBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService, this.parameterActualFiniteStateListApplicationBatchService);

            this.openFiniteStateBrowsers.Add(browser);
            this.PanelNavigationService.OpenInAddIn(browser);
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
            var browser = this.openPublicationBrowsers.SingleOrDefault(x => x.Thing == iteration);

            if (browser != null)
            {
                this.PanelNavigationService.CloseInAddIn(browser);
                this.openPublicationBrowsers.Remove(browser);
                return;
            }

            var model = (EngineeringModel)iteration.Container;

            if (model == null)
            {
                throw new InvalidOperationException("The Container of an Iteration is not a EngineeringModel.");
            }

            browser = new PublicationBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.openPublicationBrowsers.Add(browser);
            this.PanelNavigationService.OpenInAddIn(browser);
        }
    }
}
