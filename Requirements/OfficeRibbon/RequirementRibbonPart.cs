﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRibbonPart.cs" company="Starion Group S.A.">
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
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4OfficeInfrastructure;

    using CDP4Requirements.Generator;
    using CDP4Requirements.ViewModels;

    using NLog;

    using ReactiveUI;

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
        /// The <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application
        /// </summary>
        private readonly IOfficeApplicationWrapper officeApplicationWrapper;

        /// <summary>
        /// Gets or sets the <see cref="IExcelQuery"/> that is used to query the excel application
        /// </summary>
        internal IExcelQuery ExcelQuery { get; set; }

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
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="officeApplicationWrapper">
        /// The instance of <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application.
        /// </param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public RequirementRibbonPart(int order, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper, ICDPMessageBus messageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, messageBus)
        {
            this.ExcelQuery = new ExcelQuery();
            this.officeApplicationWrapper = officeApplicationWrapper;

            this.openRequirementBrowser = new List<RequirementsBrowserViewModel>();
            this.Iterations = new List<Iteration>();

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
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

            if (ribbonControlId.Contains("GenerateRequirements"))
            {
                await this.RebuildRequirementsSheet(ribbonControlTag);
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

            if (ribbonControlId.Contains("GenerateRequirements"))
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

            if (ribbonControlId == "ShowRequirements" || ribbonControlId == "GenerateRequirements")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    Tuple<DomainOfExpertise, Participant> tuple;
                    this.Session.OpenIterations.TryGetValue(iteration, out tuple);

                    var label = $"{engineeringModel.EngineeringModelSetup.ShortName} - {iteration.IterationSetup.IterationNumber} : [{(tuple.Item1 == null ? string.Empty : tuple.Item1.ShortName)}]";
                    var menuContent = $"<button id=\"{ribbonControlId}_{iteration.Iid}\" label=\"{label}\" onAction=\"OnAction\" tag=\"{iteration.Iid}\" />";
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
        /// Update the <see cref="ControlIdentifiers"/> list to include the contents of the dynamic menu
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
                    this.PanelNavigationService.CloseInAddIn(browser);
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
                this.PanelNavigationService.CloseInAddIn(browser);
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
                this.PanelNavigationService.CloseInAddIn(browser);
                this.openRequirementBrowser.Remove(browser);
                return;
            }

            browser = new RequirementsBrowserViewModel(iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);

            this.openRequirementBrowser.Add(browser);
            this.PanelNavigationService.OpenInAddIn(browser);
        }

        /// <summary>
        /// Rebuilds the requirements sheet of <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/> for which the requirements sheet needs to be generated
        /// </param>
        private async Task RebuildRequirementsSheet(string iterationId)
        {
            var iterationUniqueId = Guid.Parse(iterationId);

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == iterationUniqueId);

            if (iteration == null)
            {
                Logger.Debug($"The Iteration with Iid {iterationUniqueId} could not be found in the open iteratinos, the Requiremens Sheet could not be generated");
                return;
            }

            var application = this.officeApplicationWrapper.Excel;

            if (application != null && application.ActiveWorkbook != null)
            {
                var generator = new RequirementSheetGenerator();
                generator.Generate(this.Session, application, application.ActiveWorkbook, iteration);
            }
        }
    }
}
