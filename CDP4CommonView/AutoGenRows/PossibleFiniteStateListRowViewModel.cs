// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="PossibleFiniteStateList"/>
    /// </summary>
    public partial class PossibleFiniteStateListRowViewModel : DefinedThingRowViewModel<PossibleFiniteStateList>
    {

        /// <summary>
        /// Backing field for <see cref="DefaultState"/>
        /// </summary>
        private PossibleFiniteState defaultState;

        /// <summary>
        /// Backing field for <see cref="DefaultStateShortName"/>
        /// </summary>
        private string defaultStateShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultStateName"/>
        /// </summary>
        private string defaultStateName;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/>
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListRowViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">The <see cref="PossibleFiniteStateList"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public PossibleFiniteStateListRowViewModel(PossibleFiniteStateList possibleFiniteStateList, ISession session, IViewModelBase<Thing> containerViewModel) : base(possibleFiniteStateList, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the DefaultState
        /// </summary>
        public PossibleFiniteState DefaultState
        {
            get { return this.defaultState; }
            set { this.RaiseAndSetIfChanged(ref this.defaultState, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultState"/>
        /// </summary>
        public string DefaultStateShortName
        {
            get { return this.defaultStateShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultStateShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultState"/>
        /// </summary>
        public string DefaultStateName
        {
            get { return this.defaultStateName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultStateName, value); }
        }

        /// <summary>
        /// Gets or sets the Owner
        /// </summary>
        public DomainOfExpertise Owner
        {
            get { return this.owner; }
            set { this.RaiseAndSetIfChanged(ref this.owner, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Owner"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Owner"/>
        /// </summary>
        public string OwnerName
        {
            get { return this.ownerName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerName, value); }
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
            this.DefaultState = this.Thing.DefaultState;
            this.Owner = this.Thing.Owner;
        }
    }
}
