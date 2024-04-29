// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObfuscationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Service used for resolving obfuscated rows. NOTE: The service is not sharable and bust be instantiated and initialized every time it needs to be used by the browser. Hence it is not an injectable service.
    /// </summary>
    public class ObfuscationService : IObfuscationService
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger logger;

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObfuscationService" /> class
        /// </summary>
        public ObfuscationService()
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.Disposables = new List<IDisposable>();
        }

        /// <summary>
        /// Gets the list of <see cref="IDisposable" /> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether Obfuscation is enabled for this iteration
        /// </summary>
        public bool ObfuscationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is part of the default organizational participation
        /// </summary>
        public bool IsDefaultOrganizationalParticipant { get; set; }

        /// <summary>
        /// Gets or sets the Organizational Participant of the Person
        /// </summary>
        public OrganizationalParticipant OrganizationalParticipant { get; set; }

        /// <summary>
        /// Gets or sets theIteration
        /// </summary>
        public Iteration Iteration { get; set; }

        /// <summary>
        /// Gets or sets the session
        /// </summary>
        public ISession Session { get; set; }

        /// <summary>
        /// Determines whether a row should be obfuscated based on the provided <see cref="Thing" /> of that row.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <returns>True if row is obfuscated.</returns>
        public bool IsRowObfuscated(Thing thing)
        {
            if (this.ObfuscationEnabled)
            {
                if (this.IsDefaultOrganizationalParticipant)
                {
                    return false;
                }

                if (this.OrganizationalParticipant == null)
                {
                    return true;
                }

                if (thing is ElementDefinition elementDefinition)
                {
                    return !elementDefinition.OrganizationalParticipant.Contains(this.OrganizationalParticipant);
                }

                if (thing is ElementUsage elementUsage)
                {
                    return !elementUsage.ElementDefinition.OrganizationalParticipant.Contains(this.OrganizationalParticipant);
                }

                // all else
                var closestUsage = thing.GetContainerOfType<ElementUsage>();

                // if usage exists in chain of containers
                if (closestUsage != null)
                {
                    return !closestUsage.ElementDefinition.OrganizationalParticipant.Contains(this.OrganizationalParticipant);
                }

                // if no usage, then definition
                var closestDefinition = thing.GetContainerOfType<ElementDefinition>();

                if (closestDefinition != null)
                {
                    return !closestDefinition.OrganizationalParticipant.Contains(this.OrganizationalParticipant);
                }

                // all else irrelevant set false
            }

            return false;
        }

        /// <summary>
        /// Initializes the obfuscation service for a particular iteration
        /// </summary>
        /// <param name="iteration">The Iteration this service is valid for.</param>
        /// <param name="session">The session.</param>
        public void Initialize(Iteration iteration, ISession session)
        {
            this.Iteration = iteration;
            this.Session = session;

            var engineeringModelSetupSubscription = session.CDPMessageBus.Listen<ObjectChangedEvent>((EngineeringModelSetup)this.Iteration.IterationSetup.Container)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.RecalculateOrganizationalPermissions());

            this.Disposables.Add(engineeringModelSetupSubscription);

            this.RecalculateOrganizationalPermissions();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Recomputes the permissions for this service
        /// </summary>
        private void RecalculateOrganizationalPermissions()
        {
            var modelOrganizationalParticipations = ((EngineeringModelSetup)this.Iteration.IterationSetup.Container).OrganizationalParticipant;
            var modelDefaultOrganizationalParticipant = ((EngineeringModelSetup)this.Iteration.IterationSetup.Container).DefaultOrganizationalParticipant;

            if (!modelOrganizationalParticipations.Any() || modelDefaultOrganizationalParticipant == null)
            {
                this.ObfuscationEnabled = false;
                this.IsDefaultOrganizationalParticipant = true;
                this.OrganizationalParticipant = null;
                return;
            }

            this.ObfuscationEnabled = true;

            this.OrganizationalParticipant = modelOrganizationalParticipations.FirstOrDefault(op => op.Organization.Iid.Equals(this.Session.ActivePerson.Organization?.Iid));

            if (this.OrganizationalParticipant != null)
            {
                this.IsDefaultOrganizationalParticipant = modelDefaultOrganizationalParticipant.Iid.Equals(this.OrganizationalParticipant.Iid);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing) //Free any other managed objects here
            {
                if (this.Disposables != null)
                {
                    foreach (var disposable in this.Disposables)
                    {
                        disposable.Dispose();
                    }
                }
                else
                {
                    logger.Trace("The Disposables collection of the {0} is null", this.GetType().Name);
                }
            }

            // Indicate that the instance has been disposed.
            this.isDisposed = true;
        }
    }
}
