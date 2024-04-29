// -------------------------------------------------------------------------------------------------
// <copyright file="ActionItemRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
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
    /// Row class representing a <see cref="ActionItem"/>
    /// </summary>
    public partial class ActionItemRowViewModel : ModellingAnnotationItemRowViewModel<ActionItem>
    {

        /// <summary>
        /// Backing field for <see cref="DueDate"/>
        /// </summary>
        private DateTime dueDate;

        /// <summary>
        /// Backing field for <see cref="CloseOutDate"/>
        /// </summary>
        private DateTime closeOutDate;

        /// <summary>
        /// Backing field for <see cref="CloseOutStatement"/>
        /// </summary>
        private string closeOutStatement;

        /// <summary>
        /// Backing field for <see cref="Actionee"/>
        /// </summary>
        private Participant actionee;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemRowViewModel"/> class
        /// </summary>
        /// <param name="actionItem">The <see cref="ActionItem"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ActionItemRowViewModel(ActionItem actionItem, ISession session, IViewModelBase<Thing> containerViewModel) : base(actionItem, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the DueDate
        /// </summary>
        public DateTime DueDate
        {
            get { return this.dueDate; }
            set { this.RaiseAndSetIfChanged(ref this.dueDate, value); }
        }

        /// <summary>
        /// Gets or sets the CloseOutDate
        /// </summary>
        public DateTime CloseOutDate
        {
            get { return this.closeOutDate; }
            set { this.RaiseAndSetIfChanged(ref this.closeOutDate, value); }
        }

        /// <summary>
        /// Gets or sets the CloseOutStatement
        /// </summary>
        public string CloseOutStatement
        {
            get { return this.closeOutStatement; }
            set { this.RaiseAndSetIfChanged(ref this.closeOutStatement, value); }
        }

        /// <summary>
        /// Gets or sets the Actionee
        /// </summary>
        public Participant Actionee
        {
            get { return this.actionee; }
            set { this.RaiseAndSetIfChanged(ref this.actionee, value); }
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
            this.DueDate = this.Thing.DueDate;
            if(this.Thing.CloseOutDate.HasValue)
            {
                this.CloseOutDate = this.Thing.CloseOutDate.Value;
            }
            this.CloseOutStatement = this.Thing.CloseOutStatement;
            this.Actionee = this.Thing.Actionee;
        }
    }
}
