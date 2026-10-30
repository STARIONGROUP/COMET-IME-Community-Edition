// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewEditorRibbonPart.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
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

namespace CDP4CrossViewEditor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.Generator;
    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.OfficeDal;

    using CommonServiceLocator;

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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application
        /// </summary>
        private readonly IOfficeApplicationWrapper officeApplicationWrapper;

        /// <summary>
        /// The <see cref="IMessageBoxService"/> that tells the user what happened, resolved when it is first needed
        /// </summary>
        private IMessageBoxService messageBoxService;

        /// <summary>
        /// Gets or sets the <see cref="IExcelQuery"/> that is used to query the excel application
        /// </summary>
        private IExcelQuery ExcelQuery { get; set; }

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
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public CrossViewEditorRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper, ICDPMessageBus messageBus)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, messageBus)
        {
            this.officeApplicationWrapper = officeApplicationWrapper;
            this.ExcelQuery = new ExcelQuery();
            this.Iterations = new List<Iteration>();

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
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
                    var iteration = iterationEvent.ChangedThing as Iteration;
                    this.Iterations.RemoveAll(x => x == iteration);
                    break;
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
                Logger.Error("The current session is null");
                return;
            }

            try
            {
                if (ribbonControlId.StartsWith("Editor_", StringComparison.Ordinal))
                {
                    await this.LaunchCrossViewEditorAsync(ribbonControlTag);
                }
                else if (ribbonControlId.StartsWith("CrossViewRebuild_", StringComparison.Ordinal))
                {
                    await this.RebuildCrossViewAsync(ribbonControlTag);
                }
                else
                {
                    Logger.Debug($"The ribbon control with Id {ribbonControlId} and Tag {ribbonControlTag} is not handled by the current RibbonPart");
                }
            }
            catch (Exception ex)
            {
                // an exception that leaves a ribbon action reaches Excel, which closes itself over it, so it is reported here
                Logger.Error(ex, "The Cross View action {0} failed", ribbonControlId);
                this.Report($"The Cross View sheets could not be built:\n\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Rebuild the Cross View sheets of the specified <see cref="Iteration"/> with the selection and the settings of the last
        /// time, without opening the editor.
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/>
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>
        /// </returns>
        [ExcludeFromCodeCoverage]
        private async Task RebuildCrossViewAsync(string iterationId)
        {
            var context = this.QueryIterationContext(iterationId);

            if (context == null)
            {
                return;
            }

            var workbook = this.ExcelQuery.QueryActiveWorkbook(this.officeApplicationWrapper.Excel);

            if (workbook == null)
            {
                this.Report("There is no open workbook to rebuild.", MessageBoxImage.Exclamation);
                return;
            }

            var preservedData = new CrossviewWorkbookDataDal(workbook).Read();

            if (preservedData == null)
            {
                this.Report(
                    "This workbook holds no Cross View selection yet. Use the Cross View Editor once to choose the elements and the parameter types, after which this button rebuilds them.",
                    MessageBoxImage.Exclamation);

                return;
            }

            if (!this.IsIterationOfWorkbook(workbook, context.Value.Iteration))
            {
                return;
            }

            var settings = CrossviewSheetUtilities.ReadSettings(workbook);
            var followProductTree = settings?.FollowProductTree ?? false;

            // an option that was deleted from the model since would leave no option dependent column and no sheet of its own
            var options = context.Value.Iteration.Option.Select(option => option.Iid).Where(iid => settings?.Options.Contains(iid) == true).ToList();

            var workbookMetadata = new WorkbookMetadata
            {
                ElementDefinitions = CrossviewSheetRowAssembler.QuerySelection(context.Value.Iteration, preservedData.SavedElementDefinitions, followProductTree),
                FollowProductTree = followProductTree,
                ParameterTypes = preservedData.SavedParameterTypes.ToList(),
                Options = options,
                ParameterValues = preservedData.ManuallySavedValues,
                PersistValues = settings?.PersistValues ?? true,
                GenerateCalculationSheet = settings?.GenerateCalculationSheet ?? false,
                SummedParameterTypes = settings?.SummedParameterTypes ?? new List<Guid>()
            };

            var workbookOperator = new WorkbookOperator(this.officeApplicationWrapper.Excel, workbook, workbookMetadata);

            await workbookOperator.Rebuild(this.Session, context.Value.Iteration, context.Value.Participant);
        }

        /// <summary>
        /// Queries whether a workbook is to be rebuilt from an <see cref="Iteration"/>, asking the user when the workbook was last
        /// built from another one.
        /// </summary>
        /// <param name="workbook">
        /// The workbook
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that was chosen in the menu
        /// </param>
        /// <returns>
        /// True when the workbook was built from this iteration, or the user confirms the change
        /// </returns>
        /// <remarks>
        /// Rebuilding from the wrong model would put another product tree on the calculation sheets, and with it drop the work of
        /// the user on them. Moving a workbook on to the next iteration of the same model is a deliberate change, hence the question.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private bool IsIterationOfWorkbook(NetOffice.ExcelApi.Workbook workbook, Iteration iteration)
        {
            var builtFrom = new WorkbookSessionDal(workbook).Read()?.IterationSetup?.Iid;

            if (builtFrom == null || builtFrom == iteration.IterationSetup?.Iid)
            {
                return true;
            }

            if (this.messageBoxService == null)
            {
                this.messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();
            }

            var answer = this.messageBoxService.Show(
                "This workbook was built from another iteration than the one chosen. Rebuilding it from this one puts this product tree on its sheets, and what you added on rows that are not in it is set aside.\n\nRebuild from this iteration?",
                "Cross View",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return answer == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Queries the <see cref="Iteration"/> a ribbon control stands for, together with the active <see cref="Participant"/>.
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/>
        /// </param>
        /// <returns>
        /// The iteration with its participant, or null when the action cannot be carried out
        /// </returns>
        [ExcludeFromCodeCoverage]
        private (Iteration Iteration, Participant Participant)? QueryIterationContext(string iterationId)
        {
            if (iterationId == string.Empty)
            {
                Logger.Debug("The cross editor workbook cannot be build: the iteration id is empty");
                return null;
            }

            var uniqueId = Guid.Parse(iterationId);
            var iteration = this.Iterations.SingleOrDefault(x => x.Iid == uniqueId);

            if (iteration == null)
            {
                Logger.Debug($"The cross editor workbook cannot be build: iteration {uniqueId} cannot be found");
                return null;
            }

            if (!(iteration.Container is EngineeringModel engineeringModel))
            {
                Logger.Error("The cross editor workbook cannot be build: Iteration container object is null");
                return null;
            }

            if (this.officeApplicationWrapper.Excel == null)
            {
                Logger.Error("The cross editor workbook cannot be build: The Excel Application object is null");
                return null;
            }

            return (iteration, engineeringModel.EngineeringModelSetup.Participant.FirstOrDefault(x => x.Person == this.Session.ActivePerson));
        }

        /// <summary>
        /// Tells the user what happened, on screen and on the status bar.
        /// </summary>
        /// <param name="message">
        /// The message
        /// </param>
        /// <param name="icon">
        /// The <see cref="MessageBoxImage"/> of the message
        /// </param>
        /// <remarks>
        /// The Excel integration does not reliably write a log file, so the message on screen is the only diagnostic the user has.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private void Report(string message, MessageBoxImage icon)
        {
            Logger.Info(message);

            try
            {
                if (this.officeApplicationWrapper.Excel != null)
                {
                    this.officeApplicationWrapper.Excel.StatusBar = $"CDP4: {message.Split('\n')[0]}";
                }

                if (this.messageBoxService == null)
                {
                    this.messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();
                }

                this.messageBoxService.Show(message, "Cross View", MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                // this is the last resort of the add-in, so a failure to report may not become a failure of its own
                Logger.Error(ex, "The message could not be shown to the user");
            }
        }

        /// <summary>
        /// Launch parameter editor for the specified iteration
        /// </summary>
        /// <param name="iterationId">
        /// The unique id of the <see cref="Iteration"/>
        /// </param>
        [ExcludeFromCodeCoverage]
        private async Task LaunchCrossViewEditorAsync(string iterationId)
        {
            var context = this.QueryIterationContext(iterationId);

            if (context == null)
            {
                return;
            }

            var iteration = context.Value.Iteration;
            var activeParticipant = context.Value.Participant;

            var activeWorkbook = this.ExcelQuery.QueryActiveWorkbook(this.officeApplicationWrapper.Excel);

            var crossViewDialogViewModel = new CrossViewDialogViewModel(this.officeApplicationWrapper.Excel, iteration, this.Session, activeWorkbook);
            this.DialogNavigationService.NavigateModal(crossViewDialogViewModel);

            var dialogResult = crossViewDialogViewModel.DialogResult as WorkbookSelectionDialogResult;

            if (dialogResult?.Result != null && dialogResult.Result.Value)
            {
                var workbook = dialogResult.Workbook;

                var workbookMetadata = new WorkbookMetadata
                {
                    ElementDefinitions = dialogResult.WorkbookElements.Select(x => x.Iid),
                    ParameterTypes = dialogResult.WorkbookParameterType.Select(x => x.Iid),
                    Options = dialogResult.Options.Select(x => x.Iid).ToList(),
                    FollowProductTree = dialogResult.FollowProductTree,
                    ParameterValues = dialogResult.WorkbookChangedValues,
                    PersistValues = dialogResult.PersistValues,
                    GenerateCalculationSheet = dialogResult.GenerateCalculationSheet,
                    SummedParameterTypes = dialogResult.SummedParameterTypes.Select(x => x.Iid).ToList()
                };

                var workbookOperator = new WorkbookOperator(this.officeApplicationWrapper.Excel, workbook, workbookMetadata);

                await workbookOperator.Rebuild(this.Session, iteration, activeParticipant);
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

            if (ribbonControlId != "Editor" && ribbonControlId != "CrossViewRebuild")
            {
                return menuxml;
            }

            var sb = new StringBuilder(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

            foreach (var iteration in this.Iterations)
            {
                var engineeringModel = (EngineeringModel)iteration.Container;

                var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

                var domainShortName = selectedDomainOfExpertise == null ? string.Empty : selectedDomainOfExpertise.ShortName;
                var label = $"{engineeringModel.EngineeringModelSetup.ShortName} - {iteration.IterationSetup.IterationNumber} : [{domainShortName}]";

                var menuContent = $"<button id=\"{ribbonControlId}_{iteration.Iid}\" label=\"{label}\" onAction=\"OnAction\" tag=\"{iteration.Iid}\" />";
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
            return (ribbonControlId == "Editor" || ribbonControlId == "CrossViewRebuild") && this.IsEditorEnabled();
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
            switch (ribbonControlId)
            {
                case "Editor":
                    return "Editor";

                case "CrossViewRebuild":
                    return "Rebuild";

                default:
                    return string.Empty;
            }
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
