// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelParticipantRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// Represents the row-view-model for <see cref="Participant"/> displayed in <see cref="ModelBrowserViewModel"/>
    /// </summary>
    public class ModelParticipantRowViewModel : CDP4CommonView.ParticipantRowViewModel
    {
        #region fields
        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel"/> class
        /// </summary>
        /// <param name="participant">The <see cref="Participant"/> this is associated to</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ModelParticipantRowViewModel(Participant participant, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(participant, session, containerViewModel)
        {
            this.UpdateProperties();
        }
        #endregion

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            private set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            private set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Updates the properties of the current view-model
        /// </summary>
        private void UpdateProperties()
        {
            var person = this.Thing.Person;
            if (person == null)
            {
                // its normally impossible
                return;
            }

            var givenname = person.GivenName ?? string.Empty;
            var surname = person.Surname ?? string.Empty;
            var organizationName = (person.Organization == null) ? "none" : person.Organization.Name;

            this.Name = givenname + " " + surname;
            this.Description = string.Format("DoE: {0}, Organization: {1}",
                string.Join(", ", this.Thing.Domain.Select(x => x.Name)), organizationName);

            this.RowStatus = (this.Thing.IsActive && person.IsActive && !person.IsDeprecated)
                ? RowStatusKind.Active
                : RowStatusKind.Inactive;

            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Person)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(thingSubscription);
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}