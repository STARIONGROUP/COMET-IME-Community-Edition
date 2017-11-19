// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="Person"/>
    /// </summary>
    public partial class PersonRowViewModel : ObjectBrowserRowViewModel<Person>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="EmailAddressRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel emailAddressFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="TelephoneNumberRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel telephoneNumberFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="UserPreferenceRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel userPreferenceFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel"/> class
        /// </summary>
        /// <param name="person">The <see cref="Person"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public PersonRowViewModel(Person person, ISession session, IViewModelBase<Thing> containerViewModel) : base(person, session, containerViewModel)
        {
            this.emailAddressFolder = new CDP4Composition.FolderRowViewModel("Email Address", "Email Address", this.Session, this);
            this.ContainedRows.Add(this.emailAddressFolder);
            this.telephoneNumberFolder = new CDP4Composition.FolderRowViewModel("Telephone Number", "Telephone Number", this.Session, this);
            this.ContainedRows.Add(this.telephoneNumberFolder);
            this.userPreferenceFolder = new CDP4Composition.FolderRowViewModel("User Preference", "User Preference", this.Session, this);
            this.ContainedRows.Add(this.userPreferenceFolder);
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
            this.ComputeRows(this.Thing.EmailAddress, this.emailAddressFolder, this.AddEmailAddressRowViewModel);
            this.ComputeRows(this.Thing.TelephoneNumber, this.telephoneNumberFolder, this.AddTelephoneNumberRowViewModel);
            this.ComputeRows(this.Thing.UserPreference, this.userPreferenceFolder, this.AddUserPreferenceRowViewModel);
        }
        /// <summary>
        /// Add an Email Address row view model to the list of <see cref="EmailAddress"/>
        /// </summary>
        /// <param name="emailAddress">
        /// The <see cref="EmailAddress"/> that is to be added
        /// </param>
        private EmailAddressRowViewModel AddEmailAddressRowViewModel(EmailAddress emailAddress)
        {
            return new EmailAddressRowViewModel(emailAddress, this.Session, this);
        }
        /// <summary>
        /// Add an Telephone Number row view model to the list of <see cref="TelephoneNumber"/>
        /// </summary>
        /// <param name="telephoneNumber">
        /// The <see cref="TelephoneNumber"/> that is to be added
        /// </param>
        private TelephoneNumberRowViewModel AddTelephoneNumberRowViewModel(TelephoneNumber telephoneNumber)
        {
            return new TelephoneNumberRowViewModel(telephoneNumber, this.Session, this);
        }
        /// <summary>
        /// Add an User Preference row view model to the list of <see cref="UserPreference"/>
        /// </summary>
        /// <param name="userPreference">
        /// The <see cref="UserPreference"/> that is to be added
        /// </param>
        private UserPreferenceRowViewModel AddUserPreferenceRowViewModel(UserPreference userPreference)
        {
            return new UserPreferenceRowViewModel(userPreference, this.Session, this);
        }
    }
}
