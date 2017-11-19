// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="IterationSetup"/>
    /// </summary>
    public partial class IterationSetupRowViewModel : RowViewModelBase<IterationSetup>
    {

        /// <summary>
        /// Backing field for <see cref="IterationIid"/>
        /// </summary>
        private Guid iterationIid;

        /// <summary>
        /// Backing field for <see cref="IterationNumber"/>
        /// </summary>
        private int iterationNumber;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="FrozenOn"/>
        /// </summary>
        private DateTime frozenOn;

        /// <summary>
        /// Backing field for <see cref="IsDeleted"/>
        /// </summary>
        private bool isDeleted;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="SourceIterationSetup"/>
        /// </summary>
        private IterationSetup sourceIterationSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupRowViewModel"/> class
        /// </summary>
        /// <param name="iterationSetup">The <see cref="IterationSetup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public IterationSetupRowViewModel(IterationSetup iterationSetup, ISession session, IViewModelBase<Thing> containerViewModel) : base(iterationSetup, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IterationIid
        /// </summary>
        public Guid IterationIid
        {
            get { return this.iterationIid; }
            set { this.RaiseAndSetIfChanged(ref this.iterationIid, value); }
        }

        /// <summary>
        /// Gets or sets the IterationNumber
        /// </summary>
        public int IterationNumber
        {
            get { return this.iterationNumber; }
            set { this.RaiseAndSetIfChanged(ref this.iterationNumber, value); }
        }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets or sets the FrozenOn
        /// </summary>
        public DateTime FrozenOn
        {
            get { return this.frozenOn; }
            set { this.RaiseAndSetIfChanged(ref this.frozenOn, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeleted
        /// </summary>
        public bool IsDeleted
        {
            get { return this.isDeleted; }
            set { this.RaiseAndSetIfChanged(ref this.isDeleted, value); }
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the SourceIterationSetup
        /// </summary>
        public IterationSetup SourceIterationSetup
        {
            get { return this.sourceIterationSetup; }
            set { this.RaiseAndSetIfChanged(ref this.sourceIterationSetup, value); }
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
            this.IterationIid = this.Thing.IterationIid;
            this.IterationNumber = this.Thing.IterationNumber;
            this.Description = this.Thing.Description;
            if(this.Thing.FrozenOn.HasValue)
            {
                this.FrozenOn = this.Thing.FrozenOn.Value;
            }
            this.IsDeleted = this.Thing.IsDeleted;
            this.CreatedOn = this.Thing.CreatedOn;
            this.SourceIterationSetup = this.Thing.SourceIterationSetup;
        }
    }
}
