// -------------------------------------------------------------------------------------------------
// <copyright file="IdCorrespondenceRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="IdCorrespondence"/>
    /// </summary>
    public partial class IdCorrespondenceRowViewModel : RowViewModelBase<IdCorrespondence>
    {

        /// <summary>
        /// Backing field for <see cref="InternalThing"/>
        /// </summary>
        private Guid internalThing;

        /// <summary>
        /// Backing field for <see cref="ExternalId"/>
        /// </summary>
        private string externalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdCorrespondenceRowViewModel"/> class
        /// </summary>
        /// <param name="idCorrespondence">The <see cref="IdCorrespondence"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public IdCorrespondenceRowViewModel(IdCorrespondence idCorrespondence, ISession session, IViewModelBase<Thing> containerViewModel) : base(idCorrespondence, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the InternalThing
        /// </summary>
        public Guid InternalThing
        {
            get { return this.internalThing; }
            set { this.RaiseAndSetIfChanged(ref this.internalThing, value); }
        }

        /// <summary>
        /// Gets or sets the ExternalId
        /// </summary>
        public string ExternalId
        {
            get { return this.externalId; }
            set { this.RaiseAndSetIfChanged(ref this.externalId, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.InternalThing = this.Thing.InternalThing;
            this.ExternalId = this.Thing.ExternalId;
        }
    }
}
