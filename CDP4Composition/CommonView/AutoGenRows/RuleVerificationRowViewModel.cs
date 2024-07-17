// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="RuleVerification"/>
    /// </summary>
    public abstract partial class RuleVerificationRowViewModel<T> : RowViewModelBase<T> where T : RuleVerification
    {

        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="ExecutedOn"/>
        /// </summary>
        private DateTime executedOn;

        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private RuleVerificationStatusKind status;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationRowViewModel{T}"/> class
        /// </summary>
        /// <param name="ruleVerification">The <see cref="RuleVerification"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected RuleVerificationRowViewModel(T ruleVerification, ISession session, IViewModelBase<Thing> containerViewModel) : base(ruleVerification, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the ExecutedOn
        /// </summary>
        public DateTime ExecutedOn
        {
            get { return this.executedOn; }
            set { this.RaiseAndSetIfChanged(ref this.executedOn, value); }
        }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public RuleVerificationStatusKind Status
        {
            get { return this.status; }
            set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
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
            this.IsActive = this.Thing.IsActive;
            if(this.Thing.ExecutedOn.HasValue)
            {
                this.ExecutedOn = this.Thing.ExecutedOn.Value;
            }
            this.Status = this.Thing.Status;
            this.Name = this.Thing.Name;
        }
    }
}
