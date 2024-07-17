// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="EngineeringModel"/>
    /// </summary>
    public partial class EngineeringModelRowViewModel : TopContainerRowViewModel<EngineeringModel>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="CommonFileStoreRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel commonFileStoreFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="LogEntryRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel logEntryFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="IterationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel iterationFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="BookRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel bookFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="GenericNoteRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel genericNoteFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ModellingAnnotationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel modellingAnnotationFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModel">The <see cref="EngineeringModel"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public EngineeringModelRowViewModel(EngineeringModel engineeringModel, ISession session, IViewModelBase<Thing> containerViewModel) : base(engineeringModel, session, containerViewModel)
        {
            this.commonFileStoreFolder = new CDP4Composition.FolderRowViewModel("Common File Store", "Common File Store", this.Session, this);
            this.ContainedRows.Add(this.commonFileStoreFolder);
            this.logEntryFolder = new CDP4Composition.FolderRowViewModel("Log Entry", "Log Entry", this.Session, this);
            this.ContainedRows.Add(this.logEntryFolder);
            this.iterationFolder = new CDP4Composition.FolderRowViewModel("Iteration", "Iteration", this.Session, this);
            this.ContainedRows.Add(this.iterationFolder);
            this.bookFolder = new CDP4Composition.FolderRowViewModel("Book", "Book", this.Session, this);
            this.ContainedRows.Add(this.bookFolder);
            this.genericNoteFolder = new CDP4Composition.FolderRowViewModel("Generic Note", "Generic Note", this.Session, this);
            this.ContainedRows.Add(this.genericNoteFolder);
            this.modellingAnnotationFolder = new CDP4Composition.FolderRowViewModel("Modelling Annotation", "Modelling Annotation", this.Session, this);
            this.ContainedRows.Add(this.modellingAnnotationFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.CommonFileStore, this.commonFileStoreFolder, this.AddCommonFileStoreRowViewModel);
            this.ComputeRows(this.Thing.LogEntry, this.logEntryFolder, this.AddLogEntryRowViewModel);
            this.ComputeRows(this.Thing.Iteration, this.iterationFolder, this.AddIterationRowViewModel);
            this.ComputeRows(this.Thing.Book, this.bookFolder, this.AddBookRowViewModel);
            this.ComputeRows(this.Thing.GenericNote, this.genericNoteFolder, this.AddGenericNoteRowViewModel);
            this.ComputeRows(this.Thing.ModellingAnnotation, this.modellingAnnotationFolder, this.AddModellingAnnotationRowViewModel);
        }
        /// <summary>
        /// Add an Common File Store row view model to the list of <see cref="CommonFileStore"/>
        /// </summary>
        /// <param name="commonFileStore">
        /// The <see cref="CommonFileStore"/> that is to be added
        /// </param>
        private CommonFileStoreRowViewModel AddCommonFileStoreRowViewModel(CommonFileStore commonFileStore)
        {
            return new CommonFileStoreRowViewModel(commonFileStore, this.Session, this);
        }
        /// <summary>
        /// Add an Log Entry row view model to the list of <see cref="LogEntry"/>
        /// </summary>
        /// <param name="logEntry">
        /// The <see cref="LogEntry"/> that is to be added
        /// </param>
        private ModelLogEntryRowViewModel AddLogEntryRowViewModel(ModelLogEntry logEntry)
        {
            return new ModelLogEntryRowViewModel(logEntry, this.Session, this);
        }
        /// <summary>
        /// Add an Iteration row view model to the list of <see cref="Iteration"/>
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be added
        /// </param>
        private IterationRowViewModel AddIterationRowViewModel(Iteration iteration)
        {
            return new IterationRowViewModel(iteration, this.Session, this);
        }
        /// <summary>
        /// Add an Book row view model to the list of <see cref="Book"/>
        /// </summary>
        /// <param name="book">
        /// The <see cref="Book"/> that is to be added
        /// </param>
        private BookRowViewModel AddBookRowViewModel(Book book)
        {
            return new BookRowViewModel(book, this.Session, this);
        }
        /// <summary>
        /// Add an Generic Note row view model to the list of <see cref="GenericNote"/>
        /// </summary>
        /// <param name="genericNote">
        /// The <see cref="GenericNote"/> that is to be added
        /// </param>
        private EngineeringModelDataNoteRowViewModel AddGenericNoteRowViewModel(EngineeringModelDataNote genericNote)
        {
            return new EngineeringModelDataNoteRowViewModel(genericNote, this.Session, this);
        }
        /// <summary>
        /// Add an Modelling Annotation row view model to the list of <see cref="ModellingAnnotationItem"/>
        /// </summary>
        /// <param name="modellingAnnotation">
        /// The <see cref="ModellingAnnotation"/> that is to be added
        /// </param>
        private IModellingAnnotationItemRowViewModel<ModellingAnnotationItem> AddModellingAnnotationRowViewModel(ModellingAnnotationItem modellingAnnotation)
        {
        var requestForWaiver = modellingAnnotation as RequestForWaiver;
        if (requestForWaiver != null)
        {
            return new RequestForWaiverRowViewModel(requestForWaiver, this.Session, this);
        }
        var requestForDeviation = modellingAnnotation as RequestForDeviation;
        if (requestForDeviation != null)
        {
            return new RequestForDeviationRowViewModel(requestForDeviation, this.Session, this);
        }
        var changeRequest = modellingAnnotation as ChangeRequest;
        if (changeRequest != null)
        {
            return new ChangeRequestRowViewModel(changeRequest, this.Session, this);
        }
        var reviewItemDiscrepancy = modellingAnnotation as ReviewItemDiscrepancy;
        if (reviewItemDiscrepancy != null)
        {
            return new ReviewItemDiscrepancyRowViewModel(reviewItemDiscrepancy, this.Session, this);
        }
        var actionItem = modellingAnnotation as ActionItem;
        if (actionItem != null)
        {
            return new ActionItemRowViewModel(actionItem, this.Session, this);
        }
        var changeProposal = modellingAnnotation as ChangeProposal;
        if (changeProposal != null)
        {
            return new ChangeProposalRowViewModel(changeProposal, this.Session, this);
        }
        var contractChangeNotice = modellingAnnotation as ContractChangeNotice;
        if (contractChangeNotice != null)
        {
            return new ContractChangeNoticeRowViewModel(contractChangeNotice, this.Session, this);
        }
        throw new Exception("No ModellingAnnotationItem to return");
        }
    }
}
