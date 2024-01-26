// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetGeneratorRibbonPart.cs" company="RHEA System S.A.">
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

namespace CDP4ParameterSheetGenerator
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

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.OfficeDal;

    using CDP4ParameterSheetGenerator.ViewModels;

    using NetOffice.ExcelApi;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ParameterSheetGeneratorRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class ParameterSheetGeneratorRibbonPart : RibbonPart
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
        /// Gets or sets the <see cref="IExcelQuery"/> that is used to query the excel application
        /// </summary>
        internal IExcelQuery ExcelQuery { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSheetGeneratorRibbonPart"/> class.
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
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public ParameterSheetGeneratorRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper, ICDPMessageBus messageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, messageBus)
        {
            this.ExcelQuery = new ExcelQuery();
            this.officeApplicationWrapper = officeApplicationWrapper;
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
            return "parametersheetgeneratorribbon";
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
                return;
            }

            if (sessionChange.Status == SessionStatus.Closed)
            {
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
                this.Iterations.RemoveAll(x => x == iteration);
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
                return;
            }

            if (ribbonControlId.StartsWith("Rebuild_"))
            {
                await this.RebuildWorkbook(ribbonControlTag);
                return;
            }

            switch (ribbonControlId)
            {
                case "SynchronizeAll":
                    break;
                case "SynchronizeParameters":
                    break;
                case "SynchronizeSubscriptions":
                    break;
                case "SubmitAll":
                    await this.SubmitAll();
                    break;
                case "SubmitParameters":
                    await this.SubmitParameters();
                    break;
                case "SubmitSubscriptions":
                    await this.SubmitParameterSubscriptions();
                    break;
                default:
                    logger.Debug("The ribbon control with Id {0} and Tag {1} is not handled by the current RibbonPart", ribbonControlId, ribbonControlTag);
                    break;
            }
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

            if (ribbonControlId == "Rebuild")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel)iteration.Container;

                    var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

                    var domainShortName = selectedDomainOfExpertise == null ? string.Empty : selectedDomainOfExpertise.ShortName;
                    var label = $"{engineeringModel.EngineeringModelSetup.ShortName} - {iteration.IterationSetup.IterationNumber} : [{domainShortName}]";

                    var menuContent = string.Format("<button id=\"Rebuild_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />", iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

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
            switch (ribbonControlId)
            {
                case "Rebuild":
                    return this.IsRebuildEnabled();
                case "SynchronizeAll":
                    return false;

                // TODO: replace with when synchronize has been implemented - return this.IsSubmitOrSynchornizeEnabled();
                case "SynchronizeParameters":
                    return false;

                // TODO: replace with when synchronize has been implemented - return this.IsSubmitOrSynchornizeEnabled();
                case "SynchronizeSubscriptions":
                    return false;

                // TODO: replace with when synchronize has been implemented - return this.IsSubmitOrSynchornizeEnabled();
                case "SubmitAll":
                    return this.IsSubmitOrSynchronizeEnabled();
                case "SubmitParameters":
                    return this.IsSubmitOrSynchronizeEnabled();
                case "SubmitSubscriptions":
                    return this.IsSubmitOrSynchronizeEnabled();
                default:
                    return false;
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
                case "Rebuild":
                    var label = this.QueryRebuildLabel();
                    return label;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Queries the content of the Rebuild control label
        /// </summary>
        /// <returns>
        /// the label content
        /// </returns>
        private string QueryRebuildLabel()
        {
            if (this.Session == null)
            {
                return "Rebuild";
            }

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(this.officeApplicationWrapper.Excel);

            if (activeWorkbook == null)
            {
                return "Rebuild";
            }

            var workbookSessionDal = new WorkbookSessionDal(activeWorkbook);
            var workbookSession = workbookSessionDal.Read();

            if (workbookSession != null)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0} : {1}", workbookSession.EngineeringModelSetup.ShortName, workbookSession.IterationSetup.IterationNumber);
                sb.AppendLine();
                sb.Append(workbookSession.DomainOfExpertise.ShortName);

                return sb.ToString();
            }

            return "Rebuild";
        }

        /// <summary>
        /// Gets a value indicating whether the Rebuild button on the Ribbon is active or not
        /// </summary>
        /// <returns>
        /// returns true if the Rebuild is enabled, false otherwise
        /// </returns>
        /// <remarks>
        /// A rebuild is possible when an active workbook exists.
        /// </remarks>
        private bool IsRebuildEnabled()
        {
            if (this.Session == null)
            {
                return false;
            }

            if (this.Iterations.Count == 0)
            {
                return false;
            }

            return this.ExcelQuery.IsActiveWorkbookAvailable(this.officeApplicationWrapper.Excel);
        }

        /// <summary>
        /// Gets a value indicating whether the submit buttons are enabled or not
        /// </summary>
        /// <returns>
        /// true if enabled, false if not
        /// </returns>
        private bool IsSubmitOrSynchronizeEnabled()
        {
            if (this.Session == null)
            {
                return false;
            }

            if (this.Iterations.Count == 0)
            {
                return false;
            }

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(this.officeApplicationWrapper.Excel);

            if (activeWorkbook == null)
            {
                return false;
            }

            var workbookSessionDal = new WorkbookSessionDal(activeWorkbook);
            var workbookSession = workbookSessionDal.Read();

            if (workbookSession == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rebuild the parameter sheet of the specified iteration
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/> for which the workbook needs to be rebuild
        /// </param>
        private async Task RebuildWorkbook(string iterationId)
        {
            var application = this.officeApplicationWrapper.Excel;

            if (iterationId == string.Empty)
            {
                logger.Debug("The workbook cannot be rebuilt: the iteration id is empty");
                return;
            }

            if (application != null)
            {
                var uniqueId = Guid.Parse(iterationId);
                var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);

                if (iteration == null)
                {
                    logger.Debug("The workbook cannot be rebuilt: iteration {0} cannot be found", uniqueId);
                    return;
                }

                var engineeringModel = iteration.Container as EngineeringModel;
                var activeParticipant = engineeringModel.EngineeringModelSetup.Participant.Single(x => x.Person == this.Session.ActivePerson);

                var workbook = this.QueryIterationWorkbook(this.officeApplicationWrapper.Excel, iteration);

                if (workbook == null)
                {
                    var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

                    var workbookSelectionViewModel = new WorkbookSelectionViewModel(application, engineeringModel.EngineeringModelSetup, iteration.IterationSetup, selectedDomainOfExpertise);
                    this.DialogNavigationService.NavigateModal(workbookSelectionViewModel);

                    var dialogResult = workbookSelectionViewModel.DialogResult;

                    if (dialogResult.Result.HasValue && dialogResult.Result.Value)
                    {
                        workbook = ((WorkbookSelectionDialogResult)dialogResult).Workbook;
                    }
                    else
                    {
                        var message = "No workbook selected, the rebuild has been cancelled";
                        application.StatusBar = message;
                        logger.Debug(message);
                        return;
                    }
                }

                try
                {
                    var workbookOperator = new WorkbookOperator(application, workbook, this.DialogNavigationService, this.CDPMessageBus);
                    await workbookOperator.Rebuild(this.Session, iteration, activeParticipant);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Submit the parameter value sets that have changed on the Parameter sheet of the active workbook.
        /// </summary>
        private async Task SubmitParameters()
        {
            var application = this.officeApplicationWrapper.Excel;

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(application);

            if (activeWorkbook == null)
            {
                return;
            }

            var workbookSessionDal = new WorkbookSessionDal(activeWorkbook);
            var workbookSession = workbookSessionDal.Read();

            if (workbookSession == null)
            {
                return;
            }

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == workbookSession.IterationSetup.IterationIid);

            if (iteration == null)
            {
                logger.Debug("The output parameters cannot be submitted: iteration {0} cannot be found", workbookSession.IterationSetup.IterationIid);
                return;
            }

            try
            {
                var workbookOperator = new WorkbookOperator(application, activeWorkbook, this.DialogNavigationService, this.CDPMessageBus);
                await workbookOperator.SubmitOutput(this.Session, iteration);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Submit the subscription value sets that have changed on the Parameter sheet of the active workbook.
        /// </summary>
        private async Task SubmitParameterSubscriptions()
        {
            var application = this.officeApplicationWrapper.Excel;

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(application);

            if (activeWorkbook == null)
            {
                return;
            }

            var workbookSessionDal = new WorkbookSessionDal(activeWorkbook);
            var workbookSession = workbookSessionDal.Read();

            if (workbookSession == null)
            {
                return;
            }

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == workbookSession.IterationSetup.IterationIid);

            if (iteration == null)
            {
                logger.Debug("The input parameters cannot be submitted: iteration {0} cannot be found", workbookSession.IterationSetup.IterationIid);
                return;
            }

            try
            {
                var workbookOperator = new WorkbookOperator(application, activeWorkbook, this.DialogNavigationService, this.CDPMessageBus);
                await workbookOperator.SubmitInput(this.Session, iteration);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Submit all values sets that have changed on the parameter sheet of the active workbook
        /// </summary>
        private async Task SubmitAll()
        {
            var application = this.officeApplicationWrapper.Excel;

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(application);

            if (activeWorkbook == null)
            {
                return;
            }

            var workbookSessionDal = new WorkbookSessionDal(activeWorkbook);
            var workbookSession = workbookSessionDal.Read();

            if (workbookSession == null)
            {
                return;
            }

            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == workbookSession.IterationSetup.IterationIid);

            if (iteration == null)
            {
                logger.Debug("The values cannot be submitted: iteration {0} cannot be found", workbookSession.IterationSetup.IterationIid);
                return;
            }

            try
            {
                var workbookOperator = new WorkbookOperator(application, activeWorkbook, this.DialogNavigationService, this.CDPMessageBus);
                await workbookOperator.SubmitAll(this.Session, iteration);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
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
    }
}
