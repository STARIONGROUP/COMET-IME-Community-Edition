﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModel.cs" company="RHEA System S.A.">
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
    /// Represents a <see cref="Person"/> object that is contained in a <see cref="SiteDirectory"/>
    /// </summary>
    public class PersonRowViewModel : CDP4CommonView.PersonRowViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel"/> class.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> that is being represented by the current row-view-model</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PersonRowViewModel(Person person, ISession session, IViewModelBase<Thing> containerViewModel) 
            : base(person, session, containerViewModel)
        {
            var sitedir = (SiteDirectory)this.Thing.TopContainer;
            this.UpdateParticipants(sitedir);
            this.UpdateProperties();
        }
        #endregion

        #region row-base
        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var sitedir = (SiteDirectory)this.Thing.TopContainer;

            var siteDirSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(sitedir)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => (SiteDirectory)x.ChangedThing)
                .Subscribe(this.UpdateParticipants);
            this.Disposables.Add(siteDirSubscription);
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
        #endregion

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.RowStatus = (!this.Thing.IsDeprecated && this.Thing.IsActive)
                ? RowStatusKind.Active
                : RowStatusKind.Inactive;

            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }
        }

        /// <summary>
        /// Update the <see cref="Participant"/> row contained in a <see cref="SiteDirectory"/> for the current <see cref="Person"/> 
        /// </summary>
        /// <param name="siteDir">The <see cref="SiteDirectory"/></param>
        private void UpdateParticipants(SiteDirectory siteDir)
        {
            var currentParticipants = this.ContainedRows.Select(x => (Participant) x.Thing).ToList();
            var updatedParticipants = siteDir.Model.SelectMany(x => x.Participant).Where(x => x.Person == this.Thing).ToList();

            var addedParticipants = updatedParticipants.Except(currentParticipants).ToList();
            var removedParticipants = currentParticipants.Except(updatedParticipants).ToList();

            foreach (var addedParticipant in addedParticipants)
            {
                var participantRowViewModel = new ParticipantRowViewModel(addedParticipant, this.Session, this);
                this.ContainedRows.Add(participantRowViewModel);
            }

            foreach (var removedParticipant in removedParticipants)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == removedParticipant);
                if (row != null)
                {
                    this.ContainedRows.Remove(row);
                }
            }
        }
    }
}
