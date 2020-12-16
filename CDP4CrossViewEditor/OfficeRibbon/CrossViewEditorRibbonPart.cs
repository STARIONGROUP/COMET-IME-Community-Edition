// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewEditorRibbonPart.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.OfficeDal;
    using NetOffice.ExcelApi;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="CrossViewEditorRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class CrossViewEditorRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application
        /// </summary>
        private readonly IOfficeApplicationWrapper officeApplicationWrapper;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets a List of <see cref="Iteration"/> that are opened
        /// </summary>
        public List<Iteration> Iterations { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewEditorRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// The instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The instance of <see cref="IThingDialogNavigationService"/> that orchestrates navigation of <see cref="IThingDialogView"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="officeApplicationWrapper">
        /// The instance of <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application.
        /// </param>
        public CrossViewEditorRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.officeApplicationWrapper = officeApplicationWrapper;
            this.Iterations = new List<Iteration>();

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationChangeEventHandler);
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
            return "CrossViewEditorRibbon";
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

            switch (sessionChange.Status)
            {
                case SessionStatus.Open:
                    this.Session = sessionChange.Session;
                    break;

                case SessionStatus.Closed:
                    this.Iterations.Clear();
                    this.Session = null;
                    break;
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

            switch (iterationEvent.EventKind)
            {
                case EventKind.Added:
                    this.Iterations.Add(iterationEvent.ChangedThing as Iteration);
                    break;

                case EventKind.Removed:
                {
                    var iteration = iterationEvent.ChangedThing as Iteration;
                    this.Iterations.RemoveAll(x => x == iteration);
                    break;
                }
            }
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
                logger.Error("The current session is null");
                return;
            }

            if (ribbonControlId.StartsWith("Editor_"))
            {
                this.LaunchCrossViewEditorAsync(ribbonControlTag);
            }
            else
            {
                logger.Debug($"The ribbon control with Id {ribbonControlId} and Tag {ribbonControlTag} is not handled by the current RibbonPart");
            }
        }

        /// <summary>
        /// Launch parameter editor for the specified iteration
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/>
        /// </param>
        private void LaunchCrossViewEditorAsync(string iterationId)
        {
            if (iterationId == string.Empty)
            {
                logger.Debug("The cross editor workbook cannot be build: the iteration id is empty");
                return;
            }

            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);

            if (iteration == null)
            {
                logger.Debug($"The cross editor workbook cannot be build: iteration {uniqueId} cannot be found");
                return;
            }

            if (this.officeApplicationWrapper.Excel == null)
            {
                logger.Error("The cross editor workbook cannot be build: The Excel Application object is null");
                return;
            }

            var workbook = this.QueryIterationWorkbook(this.officeApplicationWrapper.Excel, iteration);

            if (workbook != null || !(iteration.Container is EngineeringModel))
            {
                return;
            }

            var crossViewDialogViewModel = new CrossViewDialogViewModel(iteration);
            this.DialogNavigationService.NavigateModal(crossViewDialogViewModel);
        }

        /// <summary>
        /// Gets the workbook that corresponds to the specified <see cref="Iteration"/>
        /// </summary>
        /// <param name="application">
        /// The Excel application
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the workbook is queried
        /// </param>
        /// <returns>
        /// The <see cref="Workbook"/> that corresponds to the queried Iteration, null if the workbook cannot be found.
        /// </returns>
        private Workbook QueryIterationWorkbook(Application application, Iteration iteration)
        {
            foreach (var workbook in application.Workbooks)
            {
                var workbookSessionDal = new WorkbookSessionDal(workbook);
                var workbookSession = workbookSessionDal.Read();

                if (workbookSession == null)
                {
                    continue;
                }

                if (workbookSession.IterationSetup.IterationIid == iteration.Iid)
                {
                    return workbook;
                }
            }

            return null;
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
            var menuxml = string.Empty;

            if (ribbonControlId != "Editor")
            {
                return menuxml;
            }

            var sb = new StringBuilder(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

            foreach (var iteration in this.Iterations)
            {
                var engineeringModel = (EngineeringModel) iteration.Container;

                var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

                var domainShortName = selectedDomainOfExpertise == null ? string.Empty : selectedDomainOfExpertise.ShortName;
                var label = $"{engineeringModel.EngineeringModelSetup.ShortName} - {iteration.IterationSetup.IterationNumber} : [{domainShortName}]";

                var menuContent = $"<button id=\"Editor_{iteration.Iid}\" label=\"{label}\" onAction=\"OnAction\" tag=\"{iteration.Iid}\" />";
                sb.Append(menuContent);
            }

            sb.Append(@"</menu>");
            menuxml = sb.ToString();

            this.UpdateControlIdentifiers(menuxml);

            return menuxml;
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
            return ribbonControlId == "Editor" && this.IsEditorEnabled();
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
        /// A string that represents the content of the label(min length of 1 character, max length of 1024 chars)
        /// </returns>
        public override string GetLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            return ribbonControlId == "Editor" ? "Editor" : string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the Editor button on the Ribbon is active or not
        /// </summary>
        /// <returns>
        /// returns true if the Rebuild is enabled, false otherwise
        /// </returns>
        private bool IsEditorEnabled()
        {
            return this.Iterations.Count != 0 && this.Session != null;
        }
    }
}
