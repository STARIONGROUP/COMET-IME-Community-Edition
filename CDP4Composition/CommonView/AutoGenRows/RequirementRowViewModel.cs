// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Requirement"/>
    /// </summary>
    public partial class RequirementRowViewModel : SimpleParameterizableThingRowViewModel<Requirement>
    {

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Group"/>
        /// </summary>
        private RequirementsGroup group;

        /// <summary>
        /// Backing field for <see cref="GroupShortName"/>
        /// </summary>
        private string groupShortName;

        /// <summary>
        /// Backing field for <see cref="GroupName"/>
        /// </summary>
        private string groupName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRowViewModel"/> class
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RequirementRowViewModel(Requirement requirement, ISession session, IViewModelBase<Thing> containerViewModel) : base(requirement, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the Group
        /// </summary>
        public RequirementsGroup Group
        {
            get { return this.group; }
            set { this.RaiseAndSetIfChanged(ref this.group, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Group"/>
        /// </summary>
        public string GroupShortName
        {
            get { return this.groupShortName; }
            set { this.RaiseAndSetIfChanged(ref this.groupShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Group"/>
        /// </summary>
        public string GroupName
        {
            get { return this.groupName; }
            set { this.RaiseAndSetIfChanged(ref this.groupName, value); }
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
            this.IsDeprecated = this.Thing.IsDeprecated;
			if (this.Thing.Group != null)
			{
				this.GroupShortName = this.Thing.Group.ShortName;
				this.GroupName = this.Thing.Group.Name;
			}			
            this.Group = this.Thing.Group;
        }
    }
}
