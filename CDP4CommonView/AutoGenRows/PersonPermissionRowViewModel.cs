﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="PersonPermission"/>
    /// </summary>
    public partial class PersonPermissionRowViewModel : RowViewModelBase<PersonPermission>
    {

        /// <summary>
        /// Backing field for <see cref="AccessRight"/>
        /// </summary>
        private PersonAccessRightKind accessRight;

        /// <summary>
        /// Backing field for <see cref="ObjectClass"/>
        /// </summary>
        private ClassKind objectClass;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPermissionRowViewModel"/> class
        /// </summary>
        /// <param name="personPermission">The <see cref="PersonPermission"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public PersonPermissionRowViewModel(PersonPermission personPermission, ISession session, IViewModelBase<Thing> containerViewModel) : base(personPermission, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the AccessRight
        /// </summary>
        public PersonAccessRightKind AccessRight
        {
            get { return this.accessRight; }
            set { this.RaiseAndSetIfChanged(ref this.accessRight, value); }
        }

        /// <summary>
        /// Gets or sets the ObjectClass
        /// </summary>
        public ClassKind ObjectClass
        {
            get { return this.objectClass; }
            set { this.RaiseAndSetIfChanged(ref this.objectClass, value); }
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
            this.AccessRight = this.Thing.AccessRight;
            this.ObjectClass = this.Thing.ObjectClass;
            this.IsDeprecated = this.Thing.IsDeprecated;
        }
    }
}
