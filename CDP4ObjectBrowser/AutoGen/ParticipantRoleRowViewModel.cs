// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="ParticipantRole"/>
    /// </summary>
    public partial class ParticipantRoleRowViewModel : DefinedThingRowViewModel<ParticipantRole>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParticipantPermissionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel participantPermissionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleRowViewModel"/> class
        /// </summary>
        /// <param name="participantRole">The <see cref="ParticipantRole"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ParticipantRoleRowViewModel(ParticipantRole participantRole, ISession session, IViewModelBase<Thing> containerViewModel) : base(participantRole, session, containerViewModel)
        {
            this.participantPermissionFolder = new CDP4Composition.FolderRowViewModel("Participant Permission", "Participant Permission", this.Session, this);
            this.ContainedRows.Add(this.participantPermissionFolder);
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
            this.ComputeRows(this.Thing.ParticipantPermission, this.participantPermissionFolder, this.AddParticipantPermissionRowViewModel);
        }
        /// <summary>
        /// Add an Participant Permission row view model to the list of <see cref="ParticipantPermission"/>
        /// </summary>
        /// <param name="participantPermission">
        /// The <see cref="ParticipantPermission"/> that is to be added
        /// </param>
        private ParticipantPermissionRowViewModel AddParticipantPermissionRowViewModel(ParticipantPermission participantPermission)
        {
            return new ParticipantPermissionRowViewModel(participantPermission, this.Session, this);
        }
    }
}
