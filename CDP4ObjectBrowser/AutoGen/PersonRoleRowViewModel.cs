// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="PersonRole"/>
    /// </summary>
    public partial class PersonRoleRowViewModel : DefinedThingRowViewModel<PersonRole>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="PersonPermissionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel personPermissionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleRowViewModel"/> class
        /// </summary>
        /// <param name="personRole">The <see cref="PersonRole"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public PersonRoleRowViewModel(PersonRole personRole, ISession session, IViewModelBase<Thing> containerViewModel) : base(personRole, session, containerViewModel)
        {
            this.personPermissionFolder = new CDP4Composition.FolderRowViewModel("Person Permission", "Person Permission", this.Session, this);
            this.ContainedRows.Add(this.personPermissionFolder);
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
            this.ComputeRows(this.Thing.PersonPermission, this.personPermissionFolder, this.AddPersonPermissionRowViewModel);
        }
        /// <summary>
        /// Add an Person Permission row view model to the list of <see cref="PersonPermission"/>
        /// </summary>
        /// <param name="personPermission">
        /// The <see cref="PersonPermission"/> that is to be added
        /// </param>
        private PersonPermissionRowViewModel AddPersonPermissionRowViewModel(PersonPermission personPermission)
        {
            return new PersonPermissionRowViewModel(personPermission, this.Session, this);
        }
    }
}
