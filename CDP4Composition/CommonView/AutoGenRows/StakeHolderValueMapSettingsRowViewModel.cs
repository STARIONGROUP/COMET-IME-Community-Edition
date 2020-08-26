// -------------------------------------------------------------------------------------------------
// <copyright file="StakeHolderValueMapSettingsRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// Row class representing a <see cref="StakeHolderValueMapSettings"/>
    /// </summary>
    public partial class StakeHolderValueMapSettingsRowViewModel : RowViewModelBase<StakeHolderValueMapSettings>
    {

        /// <summary>
        /// Backing field for <see cref="GoalToValueGroupRelationship"/>
        /// </summary>
        private BinaryRelationshipRule goalToValueGroupRelationship;

        /// <summary>
        /// Backing field for <see cref="GoalToValueGroupRelationshipShortName"/>
        /// </summary>
        private string goalToValueGroupRelationshipShortName;

        /// <summary>
        /// Backing field for <see cref="GoalToValueGroupRelationshipName"/>
        /// </summary>
        private string goalToValueGroupRelationshipName;

        /// <summary>
        /// Backing field for <see cref="ValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        private BinaryRelationshipRule valueGroupToStakeholderValueRelationship;

        /// <summary>
        /// Backing field for <see cref="ValueGroupToStakeholderValueRelationshipShortName"/>
        /// </summary>
        private string valueGroupToStakeholderValueRelationshipShortName;

        /// <summary>
        /// Backing field for <see cref="ValueGroupToStakeholderValueRelationshipName"/>
        /// </summary>
        private string valueGroupToStakeholderValueRelationshipName;

        /// <summary>
        /// Backing field for <see cref="StakeholderValueToRequirementRelationship"/>
        /// </summary>
        private BinaryRelationshipRule stakeholderValueToRequirementRelationship;

        /// <summary>
        /// Backing field for <see cref="StakeholderValueToRequirementRelationshipShortName"/>
        /// </summary>
        private string stakeholderValueToRequirementRelationshipShortName;

        /// <summary>
        /// Backing field for <see cref="StakeholderValueToRequirementRelationshipName"/>
        /// </summary>
        private string stakeholderValueToRequirementRelationshipName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapSettingsRowViewModel"/> class
        /// </summary>
        /// <param name="stakeHolderValueMapSettings">The <see cref="StakeHolderValueMapSettings"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public StakeHolderValueMapSettingsRowViewModel(StakeHolderValueMapSettings stakeHolderValueMapSettings, ISession session, IViewModelBase<Thing> containerViewModel) : base(stakeHolderValueMapSettings, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the GoalToValueGroupRelationship
        /// </summary>
        public BinaryRelationshipRule GoalToValueGroupRelationship
        {
            get { return this.goalToValueGroupRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.goalToValueGroupRelationship, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="GoalToValueGroupRelationship"/>
        /// </summary>
        public string GoalToValueGroupRelationshipShortName
        {
            get { return this.goalToValueGroupRelationshipShortName; }
            set { this.RaiseAndSetIfChanged(ref this.goalToValueGroupRelationshipShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="GoalToValueGroupRelationship"/>
        /// </summary>
        public string GoalToValueGroupRelationshipName
        {
            get { return this.goalToValueGroupRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.goalToValueGroupRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the ValueGroupToStakeholderValueRelationship
        /// </summary>
        public BinaryRelationshipRule ValueGroupToStakeholderValueRelationship
        {
            get { return this.valueGroupToStakeholderValueRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.valueGroupToStakeholderValueRelationship, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        public string ValueGroupToStakeholderValueRelationshipShortName
        {
            get { return this.valueGroupToStakeholderValueRelationshipShortName; }
            set { this.RaiseAndSetIfChanged(ref this.valueGroupToStakeholderValueRelationshipShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        public string ValueGroupToStakeholderValueRelationshipName
        {
            get { return this.valueGroupToStakeholderValueRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.valueGroupToStakeholderValueRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the StakeholderValueToRequirementRelationship
        /// </summary>
        public BinaryRelationshipRule StakeholderValueToRequirementRelationship
        {
            get { return this.stakeholderValueToRequirementRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.stakeholderValueToRequirementRelationship, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="StakeholderValueToRequirementRelationship"/>
        /// </summary>
        public string StakeholderValueToRequirementRelationshipShortName
        {
            get { return this.stakeholderValueToRequirementRelationshipShortName; }
            set { this.RaiseAndSetIfChanged(ref this.stakeholderValueToRequirementRelationshipShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="StakeholderValueToRequirementRelationship"/>
        /// </summary>
        public string StakeholderValueToRequirementRelationshipName
        {
            get { return this.stakeholderValueToRequirementRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.stakeholderValueToRequirementRelationshipName, value); }
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
			if (this.Thing.GoalToValueGroupRelationship != null)
			{
				this.GoalToValueGroupRelationshipShortName = this.Thing.GoalToValueGroupRelationship.ShortName;
				this.GoalToValueGroupRelationshipName = this.Thing.GoalToValueGroupRelationship.Name;
			}			
            this.GoalToValueGroupRelationship = this.Thing.GoalToValueGroupRelationship;
			if (this.Thing.ValueGroupToStakeholderValueRelationship != null)
			{
				this.ValueGroupToStakeholderValueRelationshipShortName = this.Thing.ValueGroupToStakeholderValueRelationship.ShortName;
				this.ValueGroupToStakeholderValueRelationshipName = this.Thing.ValueGroupToStakeholderValueRelationship.Name;
			}			
            this.ValueGroupToStakeholderValueRelationship = this.Thing.ValueGroupToStakeholderValueRelationship;
			if (this.Thing.StakeholderValueToRequirementRelationship != null)
			{
				this.StakeholderValueToRequirementRelationshipShortName = this.Thing.StakeholderValueToRequirementRelationship.ShortName;
				this.StakeholderValueToRequirementRelationshipName = this.Thing.StakeholderValueToRequirementRelationship.Name;
			}			
            this.StakeholderValueToRequirementRelationship = this.Thing.StakeholderValueToRequirementRelationship;
        }
    }
}
