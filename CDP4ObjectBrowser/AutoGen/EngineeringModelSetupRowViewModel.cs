// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public partial class EngineeringModelSetupRowViewModel : DefinedThingRowViewModel<EngineeringModelSetup>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParticipantRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel participantFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="RequiredRdlRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel requiredRdlFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="IterationSetupRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel iterationSetupFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">The <see cref="EngineeringModelSetup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public EngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, IViewModelBase<Thing> containerViewModel) : base(engineeringModelSetup, session, containerViewModel)
        {
            this.participantFolder = new CDP4Composition.FolderRowViewModel("Participant", "Participant", this.Session, this);
            this.ContainedRows.Add(this.participantFolder);
            this.requiredRdlFolder = new CDP4Composition.FolderRowViewModel("Required Rdl", "Required Rdl", this.Session, this);
            this.ContainedRows.Add(this.requiredRdlFolder);
            this.iterationSetupFolder = new CDP4Composition.FolderRowViewModel("Iteration Setup", "Iteration Setup", this.Session, this);
            this.ContainedRows.Add(this.iterationSetupFolder);
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
            this.ComputeRows(this.Thing.Participant, this.participantFolder, this.AddParticipantRowViewModel);
            this.ComputeRows(this.Thing.RequiredRdl, this.requiredRdlFolder, this.AddRequiredRdlRowViewModel);
            this.ComputeRows(this.Thing.IterationSetup, this.iterationSetupFolder, this.AddIterationSetupRowViewModel);
        }
        /// <summary>
        /// Add an Participant row view model to the list of <see cref="Participant"/>
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is to be added
        /// </param>
        private ParticipantRowViewModel AddParticipantRowViewModel(Participant participant)
        {
            return new ParticipantRowViewModel(participant, this.Session, this);
        }
        /// <summary>
        /// Add an Required Rdl row view model to the list of <see cref="RequiredRdl"/>
        /// </summary>
        /// <param name="requiredRdl">
        /// The <see cref="RequiredRdl"/> that is to be added
        /// </param>
        private ModelReferenceDataLibraryRowViewModel AddRequiredRdlRowViewModel(ModelReferenceDataLibrary requiredRdl)
        {
            return new ModelReferenceDataLibraryRowViewModel(requiredRdl, this.Session, this);
        }
        /// <summary>
        /// Add an Iteration Setup row view model to the list of <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="iterationSetup">
        /// The <see cref="IterationSetup"/> that is to be added
        /// </param>
        private IterationSetupRowViewModel AddIterationSetupRowViewModel(IterationSetup iterationSetup)
        {
            return new IterationSetupRowViewModel(iterationSetup, this.Session, this);
        }
    }
}
