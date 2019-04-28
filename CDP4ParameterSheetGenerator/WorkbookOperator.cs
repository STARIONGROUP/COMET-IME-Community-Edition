// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookOperator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4OfficeInfrastructure.OfficeDal;
    using CDP4ParameterSheetGenerator.Generator;
    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.OptionSheet;
    using CDP4ParameterSheetGenerator.ParameterSheet;
    using CDP4ParameterSheetGenerator.ViewModels;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;
    using NLog;
    using RebuildKind = CDP4ParameterSheetGenerator.ViewModels.RebuildKind;

    /// <summary>
    /// The purpose of the <see cref="WorkbookOperator"/> is to delegate the operations on a <see cref="Workbook"/>
    /// to the proper classes
    /// </summary>
    public class WorkbookOperator
    {
        /// <summary>
        /// A selection of class kinds that are contained by the engineering model.
        /// </summary>
        private static readonly ClassKind[] EngineeringModelKinds = new[]
                                    {
                                        ClassKind.EngineeringModel, 
                                        ClassKind.Iteration, 
                                        ClassKind.CommonFileStore,
                                        ClassKind.ModelLogEntry
                                    };

        /// <summary>
        /// The NLog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Workbook"/> that is being managed by the current <see cref="WorkbookOperator"/>  
        /// </summary>
        private readonly Workbook workbook;

        /// <summary>
        /// The excel <see cref="Application"/> that is being managed by the current <see cref="WorkbookOperator"/>  
        /// </summary>
        private readonly Application application;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookOperator"/> class.
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is being operated on.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs
        /// </param>
        public WorkbookOperator(Application application, Workbook workbook, IDialogNavigationService dialogNavigationService)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application), "the Excel application may not be null");
            }

            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook), "the workbook may not be null");
            }

            workbook.Activate();

            this.workbook = workbook;
            this.application = application;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to dialogs
        /// </summary>
        public IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Rebuild the Parameter Sheet
        /// </summary>
        /// <param name="session">
        /// The current <see cref="ISession"/> that is rebuilding the parameter sheet
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="ElementDefinition"/>s, <see cref="ElementUsage"/>s and <see cref="Parameter"/>s that
        /// are being written to the workbook
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/> for which the workbook is being rebuilt.
        /// </param>
        public async Task Rebuild(ISession session, Iteration iteration, Participant participant)
        {
            this.application.StatusBar = string.Empty;

            var workbookSession = await this.CreateWorkbookSession(session.Dal, session.Credentials);

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets;
                
                var parameterSheetProcessor = new ParameterSheetProcessor(workbookSession, iteration);
                parameterSheetProcessor.ValidateValuesAndCheckForChanges(this.application, this.workbook, out processedValueSets);
                
                if (processedValueSets.Any())
                {    
                    var workbookRebuildViewModel = new WorkbookRebuildViewModel(processedValueSets, ValueSetKind.All);
                    var dialogResult = this.DialogNavigationService.NavigateModal(workbookRebuildViewModel);

                    if (dialogResult.Result.HasValue && dialogResult.Result.Value)
                    {
                        var rebuildKind = ((WorkbookRebuildDialogResult)dialogResult).RebuildKind;
                        switch (rebuildKind)
                        {
                            case RebuildKind.Overwrite:
                                processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
                                break;
                            case RebuildKind.RestoreChanges:
                                // keep clones
                                break;
                        }
                    }
                    else
                    {
                        var parameterSheetRowHighligter = new ParameterSheetRowHighligter();
                        parameterSheetRowHighligter.HighlightRows(this.application, this.workbook, processedValueSets);

                        this.application.StatusBar = "Rebuild Parameter sheet has been cancelled";
                        return;
                    }
                }

                await this.RefreshSessionData(session);

                this.WriteParameterSheet(session, iteration, participant, processedValueSets);
                this.WriteOptionSheets(session, iteration, participant);

                this.WriteSessionInfoToWorkbook(session, iteration, participant);
                this.WriteWorkbookDataToWorkbook(iteration);

                this.ActivateParametersSheet();

                this.application.StatusBar = string.Format("Rebuild Parameter completed in [{0}] ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "Rebuild Parameter sheet failed";
                Logger.Error(ex);
            }
            finally
            {
                this.application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Refresh the data in cache with the latest data from the data-source
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the data-source
        /// </param>
        private async Task RefreshSessionData(ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.application.Cursor = XlMousePointer.xlWait;
            this.application.StatusBar = $"CDP4: refreshing data from the data source {session.DataSourceUri}";
            
            await session.Refresh();
            
            this.application.StatusBar = $"CD4: data refreshed in {sw.ElapsedMilliseconds} [ms]";
            this.application.Cursor = XlMousePointer.xlDefault;
        }

        /// <summary>
        /// Submits the changes the user has made to outputs on the Parameter sheet to the data-source.
        /// </summary>
        /// <param name="session">
        /// The current <see cref="ISession"/> that is submitting the outputs.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that that contains the value sets that value-sets that if changed need to be submitted.
        /// </param>
        public async Task SubmitOutput(ISession session, Iteration iteration)
        {
            this.application.StatusBar = string.Empty;

            var workbookSession = await this.CreateWorkbookSession(session.Dal, session.Credentials);

            try
            {
                var parameterSheetRowHighligter = new ParameterSheetRowHighligter();
                parameterSheetRowHighligter.ResetRows(this.application, this.workbook);

                IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets;

                var parameterSheetProcessor = new ParameterSheetProcessor(workbookSession, iteration);
                parameterSheetProcessor.ValidateValuesAndCheckForChanges(this.application, this.workbook, out processedValueSets);

                if (!processedValueSets.Any())
                {
                    this.application.StatusBar = "CDP4: Submit cancelled, No values changed";
                    return;
                }
                
                var parameterValueSetBaseProcessedValueSets = new Dictionary<Guid, ProcessedValueSet>();
                foreach (var processedValueSet in processedValueSets)
                {
                    if (processedValueSet.Value.ClonedThing is ParameterValueSetBase)
                    {
                        parameterValueSetBaseProcessedValueSets.Add(processedValueSet.Value.ClonedThing.Iid, processedValueSet.Value);
                    }
                }
                
                if (parameterValueSetBaseProcessedValueSets.Any())
                {
                    var submitConfirmationViewModel = new SubmitConfirmationViewModel(parameterValueSetBaseProcessedValueSets, ValueSetKind.ParameterAndOrverride);
                    var dialogResult = this.DialogNavigationService.NavigateModal(submitConfirmationViewModel);

                    if (dialogResult.Result.HasValue && dialogResult.Result.Value)
                    {
                        var submitConfirmationDialogResult = (SubmitConfirmationDialogResult)dialogResult;

                        var context = TransactionContextResolver.ResolveContext(iteration);
                        var transaction = new ThingTransaction(context);

                        foreach (var clone in submitConfirmationDialogResult.Clones)
                        {
                            transaction.CreateOrUpdate(clone);
                        }

                        // TODO: enable when OperationContainer.ResolveRoute supports this
                        //var clonedEngineeringModel = (EngineeringModel)iteration.Container.Clone();
                        //var logEntry = new ModelLogEntry(iid: Guid.NewGuid())
                        //                   {
                        //                       Content = submitConfirmationDialogResult.SubmitMessage,
                        //                       Author = session.ActivePerson,
                        //                       LanguageCode = "en-GB",
                        //                       Level = LogLevelKind.USER,
                        //                       AffectedItemIid = submitConfirmationDialogResult.Clones.Select(clone => clone.Iid).ToList()
                        //                   };
                        //clonedEngineeringModel.LogEntry.Add(logEntry);
                        //transaction.Create(logEntry);

                        var operationContainer = transaction.FinalizeTransaction();

                        this.application.StatusBar = string.Format("CDP4: Submitting data to {0}", session.DataSourceUri);
                        this.application.Cursor = XlMousePointer.xlWait;

                        var sw = new Stopwatch();
                        sw.Start();
                        await session.Write(operationContainer);
                        sw.Stop();
                        this.application.StatusBar = string.Format("CDP4: SitedirectoryData submitted in {0} [ms]", sw.ElapsedMilliseconds);

                        this.application.StatusBar = "CDP4: Writing session data to workbook";
                        sw.Start();
                        this.WriteWorkbookDataToWorkbook(iteration);
                        sw.Stop();
                        this.application.StatusBar = string.Format("CDP4: Session data written in {0} [ms]", sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        this.application.StatusBar = "CDP4: Submit output parameters has been cancelled by the user.";

                        parameterSheetRowHighligter.HighlightRows(this.application, this.workbook, parameterValueSetBaseProcessedValueSets);
                    }
                }
                else
                {
                    this.application.StatusBar = "CDP4: Submit output parameters has been cancelled, no values were changed.";
                }
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "CDP4: Submit output parameters failed.";
                Logger.Error(ex);
            }
            finally
            {
                this.application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Submits the changes the user has made to inputs on the Parameter sheet to the data-source.
        /// </summary>
        /// <param name="session">
        /// The current <see cref="ISession"/> that is submitting the outputs.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that that contains the value sets that value-sets that if changed need to be submitted.
        /// </param>
        public async Task SubmitInput(ISession session, Iteration iteration)
        {
            this.application.StatusBar = string.Empty;

            var workbookSession = await this.CreateWorkbookSession(session.Dal, session.Credentials);

            try
            {
                IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets;

                var parameterSheetProcessor = new ParameterSheetProcessor(workbookSession, iteration);
                parameterSheetProcessor.ValidateValuesAndCheckForChanges(this.application, this.workbook, out processedValueSets);
                
                if (!processedValueSets.Any())
                {
                    this.application.StatusBar = "Submit cancelled: no values changed";
                    return;
                }

                var parameterSubscriptionValueSetProcessedValueSets = new Dictionary<Guid, ProcessedValueSet>();
                foreach (var processedValueSet in processedValueSets)
                {
                    if (processedValueSet.Value.ClonedThing is ParameterSubscriptionValueSet)
                    {
                        parameterSubscriptionValueSetProcessedValueSets.Add(processedValueSet.Value.ClonedThing.Iid, processedValueSet.Value);
                    }
                }
                
                if (parameterSubscriptionValueSetProcessedValueSets.Any())
                {
                    var submitConfirmationViewModel = new SubmitConfirmationViewModel(parameterSubscriptionValueSetProcessedValueSets, ValueSetKind.ParameterSubscription);
                    var dialogResult = this.DialogNavigationService.NavigateModal(submitConfirmationViewModel);

                    if (dialogResult.Result.HasValue && dialogResult.Result.Value)
                    {
                        var submitConfirmationDialogResult = (SubmitConfirmationDialogResult)dialogResult;

                        var context = TransactionContextResolver.ResolveContext(iteration);
                        var transaction = new ThingTransaction(context);

                        foreach (var clone in submitConfirmationDialogResult.Clones)
                        {
                            transaction.CreateOrUpdate(clone);
                        }

                        // TODO: enable when OperationContainer.ResolveRoute supports this
                        //var clonedEngineeringModel = (EngineeringModel)iteration.Container.Clone();
                        //var logEntry = new ModelLogEntry(iid: Guid.NewGuid())
                        //                   {
                        //                       Content = submitConfirmationDialogResult.SubmitMessage,
                        //                       Author = session.ActivePerson,
                        //                       LanguageCode = "en-GB",
                        //                       Level = LogLevelKind.USER,
                        //                       AffectedItemIid = submitConfirmationDialogResult.Clones.Select(clone => clone.Iid).ToList()
                        //                   };
                        //clonedEngineeringModel.LogEntry.Add(logEntry);
                        //transaction.Create(logEntry);

                        var operationContainer = transaction.FinalizeTransaction();

                        this.application.StatusBar = string.Format("CDP4: Submitting data to {0}", session.DataSourceUri);
                        this.application.Cursor = XlMousePointer.xlWait;

                        var sw = new Stopwatch();
                        sw.Start();
                        await session.Write(operationContainer);
                        sw.Stop();
                        this.application.StatusBar = string.Format("CDP4: SitedirectoryData submitted in {0} [ms]", sw.ElapsedMilliseconds);

                        this.application.StatusBar = "CDP4: Writing session data to workbook";
                        sw.Start();
                        this.WriteWorkbookDataToWorkbook(iteration);
                        sw.Stop();
                        this.application.StatusBar = string.Format("CDP4: Session data written in {0} [ms]", sw.ElapsedMilliseconds);
                    }
                    else
                    {
                        this.application.StatusBar = "CDP4: Submit input parameters has been cancelled by the user.";

                        var parameterSheetRowHighligter = new ParameterSheetRowHighligter();
                        parameterSheetRowHighligter.HighlightRows(this.application, this.workbook, parameterSubscriptionValueSetProcessedValueSets);
                    }
                }
                else
                {
                    this.application.StatusBar = "CDP4: Submit input parameters has been cancelled, no values were changed.";
                }
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "CDP4: Submit input parameters failed.";
                Logger.Error(ex);
            }
            finally
            {
                this.application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Submits the changes the user has made to inputs and outputs on the Parameter sheet to the data-source.
        /// </summary>
        /// <param name="session">
        /// The current <see cref="ISession"/> that is submitting the outputs.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that that contains the value sets that value-sets that if changed need to be submitted.
        /// </param>
        public async Task SubmitAll(ISession session, Iteration iteration)
        {
            this.application.StatusBar = string.Empty;

            var workbookSession = await this.CreateWorkbookSession(session.Dal, session.Credentials);

            try
            {
                IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets;
                var parameterSheetProcessor = new ParameterSheetProcessor(workbookSession, iteration);
                parameterSheetProcessor.ValidateValuesAndCheckForChanges(this.application, this.workbook, out processedValueSets);
                
                if (!processedValueSets.Any())
                {
                    this.application.StatusBar = "Submit cancelled: no values changed";
                    return;
                }
                
                var submitConfirmationViewModel = new SubmitConfirmationViewModel(processedValueSets, ValueSetKind.All);
                var dialogResult = this.DialogNavigationService.NavigateModal(submitConfirmationViewModel);

                if (dialogResult.Result.HasValue && dialogResult.Result.Value)
                {
                    var submitConfirmationDialogResult = (SubmitConfirmationDialogResult)dialogResult;

                    var context = TransactionContextResolver.ResolveContext(iteration);
                    var transaction = new ThingTransaction(context);

                    foreach (var clone in submitConfirmationDialogResult.Clones)
                    {
                        transaction.CreateOrUpdate(clone);
                    }

                    // TODO: enable when OperationContainer.ResolveRoute supports this
                    //var clonedEngineeringModel = (EngineeringModel)iteration.Container.Clone();
                    //var logEntry = new ModelLogEntry(iid: Guid.NewGuid())
                    //                   {
                    //                       Content = submitConfirmationDialogResult.SubmitMessage,
                    //                       Author = session.ActivePerson,
                    //                       LanguageCode = "en-GB",
                    //                       Level = LogLevelKind.USER,
                    //                       AffectedItemIid = submitConfirmationDialogResult.Clones.Select(clone => clone.Iid).ToList()
                    //                   };
                    //clonedEngineeringModel.LogEntry.Add(logEntry);
                    //transaction.Create(logEntry);

                    var operationContainer = transaction.FinalizeTransaction();

                    this.application.StatusBar = string.Format("CDP4: Submitting data to {0}", session.DataSourceUri);
                    this.application.Cursor = XlMousePointer.xlWait;

                    var sw = new Stopwatch();
                    sw.Start();
                    await session.Write(operationContainer);
                    sw.Stop();
                    this.application.StatusBar = string.Format("CDP4: SitedirectoryData submitted in {0} [ms]", sw.ElapsedMilliseconds);

                    this.application.StatusBar = "CDP4: Writing session data to workbook";
                    sw.Start();
                    this.WriteWorkbookDataToWorkbook(iteration);
                    sw.Stop();
                    this.application.StatusBar = string.Format("CDP4: Session data written in {0} [ms]", sw.ElapsedMilliseconds);
                }
                else
                {
                    this.application.StatusBar = "CDP4: Submit parameters and subscription has been cancelled by the user.";

                    var parameterSheetRowHighligter = new ParameterSheetRowHighligter();
                    parameterSheetRowHighligter.HighlightRows(this.application, this.workbook, processedValueSets);
                }
                
            }
            catch (Exception ex)
            {
                this.application.StatusBar = "CDP4: submission failed.";
                Logger.Error(ex);
            }
            finally
            {
                this.application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Creates a workbook <see cref="ISession"/> where the cache of the associated <see cref="Assembler"/>
        /// is populated with data sourced from the workbook
        /// </summary>
        /// <param name="dal">
        /// The <see cref="IDal"/> instance used to communicate with the data-source.
        /// </param>
        /// <param name="credentials">
        /// The <see cref="Credentials"/> used to communicate with the data-source.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ISession"/> that is specific to the <see cref="Workbook"/>
        /// </returns>
        private async Task<ISession> CreateWorkbookSession(IDal dal, Credentials credentials)
        {
            var workbookDataDal = new WorkbookDataDal(this.workbook);
            var workbookData = workbookDataDal.Read();

            var workbookSession = new Session(dal, credentials);

            if (workbookData != null)
            {
                if (workbookData.SiteDirectoryThings != null && workbookData.SiteDirectoryThings.Any())
                {
                    await workbookSession.Assembler.Synchronize(workbookData.SiteDirectoryThings, false);
                }

                if (workbookData.IterationThings != null && workbookData.IterationThings.Any())
                {
                    var iterationThings = workbookData.IterationThings.ToList();

                    this.SetIterationContainer(ref iterationThings);

                    await workbookSession.Assembler.Synchronize(iterationThings, false);
                }
            }

            return workbookSession;
        }

        /// <summary>
        /// Set the <see cref="Iteration"/> container id for all applicable <see cref="Thing"/>
        /// </summary>
        /// <param name="dtos">
        /// The <see cref="Thing"/> to set
        /// </param>
        private void SetIterationContainer(ref List<CDP4Common.DTO.Thing> dtos)
        {
            var iteration = dtos.OfType<CDP4Common.DTO.Iteration>().Single();

            foreach (var thing in dtos.Where(x => !EngineeringModelKinds.Contains(x.ClassKind)))
            {
                // all the returned thing are iteration contained
                thing.IterationContainerId = iteration.Iid;
            }   
        }

        /// <summary>
        /// Write session information to the workbook as a custom XML part
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> related with the the workbook.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is written to the workbook
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is written to the workbook
        /// </param>
        private void WriteSessionInfoToWorkbook(ISession session, Iteration iteration, Participant participant)
        {
            var engineeringModel = (EngineeringModel)iteration.Container;

            var selectedDomainOfExpertise = session.QuerySelectedDomainOfExpertise(iteration);

            var workbookSession = new WorkbookSession(participant.Person, engineeringModel.EngineeringModelSetup, iteration.IterationSetup, selectedDomainOfExpertise);
            var workbookSessionDal = new WorkbookSessionDal(this.workbook);
            workbookSessionDal.Write(workbookSession);
        }

        /// <summary>
        /// Write the <see cref="Iteration"/> data to the workbook as a custom XML part
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is being stored in the workbook
        /// </param>
        private void WriteWorkbookDataToWorkbook(Iteration iteration)
        {
            var workbookData = new WorkbookData(iteration);
            var workbookDataDal = new WorkbookDataDal(this.workbook);
            workbookDataDal.Write(workbookData);
        }

        /// <summary>
        /// Write the parameter sheet to the workbook
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> related with the the workbook.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="ElementDefinition"/>s, <see cref="ElementUsage"/>s and <see cref="Parameter"/>s that
        /// are being written to the workbook
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/> for which the workbook is being rebuilt.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s that have been changed on the Parameter sheet that need to be kept
        /// </param>
        private void WriteParameterSheet(ISession session, Iteration iteration, Participant participant, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            var parameterSheetGenerator = new ParameterSheetGenerator(session, iteration, participant);
            parameterSheetGenerator.Rebuild(this.application, this.workbook, processedValueSets);
        }

        /// <summary>
        /// Write the option sheets to the workbook
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> related with the the workbook.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="Option"/>s that are being written to the workbook
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/> for which the workbook is being rebuilt.
        /// </param>
        private void WriteOptionSheets(ISession session, Iteration iteration, Participant participant)
        {
            var optionSheetGenerator = new OptionSheetGenerator(session, iteration, participant);
            optionSheetGenerator.Rebuild(this.application, this.workbook);
        }

        /// <summary>
        /// Activates the Parameter sheet
        /// </summary>
        private void ActivateParametersSheet()
        {
            try
            {
                var worksheet = (Worksheet)this.workbook.Worksheets[ParameterSheetConstants.ParameterSheetName];
                worksheet.Activate();
                worksheet.Cells[1, 1].Select();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
