// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeRibbonPart.cs" company="RHEA System S.A.">
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

namespace CDP4ProductTree
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

    using CDP4ProductTree.ViewModels;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ProductTreeRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class ProductTreeRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of open <see cref="ProductTreeViewModel"/>
        /// </summary>
        private readonly List<ProductTreeViewModel> openProductTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTreeRibbonPart"/> class.
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
        public ProductTreeRibbonPart(int order, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.openProductTree = new List<ProductTreeViewModel>();
            this.Iterations = new List<Iteration>();

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationChangeEventHandler);

            CDPMessageBus.Current.Listen<HidePanelEvent>()
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

            if (ribbonControlId.Contains("ShowProductTree_"))
            {
                this.ShowOrCloseProductTree(ribbonControlTag);
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

            if (ribbonControlId.Contains("ShowProductTree_"))
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

            if (ribbonControlId == "ShowProductTree_")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    foreach (Option option in iteration.Option)
                    {
                        var engineeringModel = (EngineeringModel)iteration.Container;

                        Tuple<DomainOfExpertise, Participant> tuple;
                        this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                        var label = string.Format(
                            "{0} - {1} - {2}: [{3}]",
                            engineeringModel.EngineeringModelSetup.ShortName,
                            iteration.IterationSetup.IterationNumber,
                            option.ShortName,
                            tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName);

                        // format of tag : iterationId_OptionId
                        var menuContent =
                            string.Format(
                                "<button id=\"ShowProductTree_{0}_{1}\" label=\"{2}\" onAction=\"OnAction\" tag=\"{0}_{1}\" />",
                                iteration.Iid,
                                option.Iid,
                                label);

                        sb.Append(menuContent);
                    }
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            this.UpdateControlIdentifier(menuxml);
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
            return "ProductTreeRibbon";
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
                var browser = this.openProductTree.SingleOrDefault(x => x.Thing.Container == iteration);

                if (browser != null)
                {
                    this.PanelNavigationService.Close(browser, false);
                    this.openProductTree.Remove(browser);
                }

                this.Iterations.RemoveAll(x => x == iteration);
            }
        }

        /// <summary>
        /// Close all the panels and dispose of them
        /// </summary>
        private void CloseAll()
        {
            foreach (var productTree in this.openProductTree)
            {
                this.PanelNavigationService.Close(productTree, false);
            }

            this.openProductTree.Clear();
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
                this.PanelNavigationService.Close(browser, false);

                if (this.openProductTree.Contains(browser))
                {
                    this.openProductTree.Remove(browser as ProductTreeViewModel);
                }
            }
        }

        /// <summary>
        /// Retrieve all <see cref="IPanelViewModel"/>s related to this <see cref="ProductTreeRibbonPart"/> that are currently open,
        /// grouped by the original private list they belong to.
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> of type <see cref="List{IPanelViewModel}"/> containing all <see cref="IPanelViewModel"/>s.
        /// </returns>
        internal List<List<IPanelViewModel>> GetAllOpenBrowsers()
        {
            return new List<List<IPanelViewModel>>
            {
                this.openProductTree.Cast<IPanelViewModel>().ToList()
            };
        }

        /// <summary>
        /// Show or close the <see cref="ProductTreeViewModel"/>
        /// </summary>
        /// <param name="optionId">
        /// the unique id of the <see cref="Option"/> that is being represented by the <see cref="ProductTreeViewModel"/>
        /// </param>
        private void ShowOrCloseProductTree(string optionId)
        {
            // format of tag iterationId_OptionId
            var iids = optionId.Split('_');

            var iterationUniqueId = Guid.Parse(iids[0]);
            var optionGuid = Guid.Parse(iids[1]);

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == iterationUniqueId);

            if (iteration == null)
            {
                return;
            }

            var option = iteration.Option.SingleOrDefault(x => x.Iid == optionGuid);

            if (option == null)
            {
                return;
            }

            // close the brower if it exists
            var browser = this.openProductTree.SingleOrDefault(x => x.Thing == option);

            if (browser != null)
            {
                this.PanelNavigationService.Close(browser, false);
                this.openProductTree.Remove(browser);
                return;
            }

            browser = new ProductTreeViewModel(option, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.openProductTree.Add(browser);
            this.PanelNavigationService.Open(browser, false);
        }
    }
}
