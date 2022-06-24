// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRowViewModel.cs" company="RHEA System S.A.">
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
    /// Represents a <see cref="Participant"/> object that references a <see cref="Person"/> and that is contained by the <see cref="PersonRowViewModel"/> 
    /// that represents the referenced <see cref="Person"/>
    /// </summary>
    public class ParticipantRowViewModel : CDP4CommonView.ParticipantRowViewModel
    {
        #region Fields

        /// <summary>
        /// Out property for the <see cref="DomainShortnames"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> domainShortnames;

        /// <summary>
        /// Out property for the <see cref="DomainShortnames"/> ModelName
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> modelName;

        /// <summary>
        /// Backing field for the <see cref="ParticipantRole"/> property.
        /// </summary>
        private ParticipantRole participantRole;

        /// <summary>
        /// Backing field for the <see cref="EngineeringModelSetup"/> property.
        /// </summary>
        private EngineeringModelSetup engineeringModelSetup;

        /// <summary>
        /// Backing field for <see cref="Domains"/>
        /// </summary>
        private ReactiveList<DomainOfExpertise> domains;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel"/> class.
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is being represented by the current row-view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParticipantRowViewModel(Participant participant, ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(participant, session, containerViewModel)
        {
            this.Domains = new ReactiveList<DomainOfExpertise>();

            this.WhenAnyValue(row => row.Domains)
                .Select(domains => domains.Aggregate(string.Empty,
                    (current, domainOfExpertise) => string.Format("{0} {1}", current, domainOfExpertise.ShortName)))
                .ToProperty(this, row => row.DomainShortnames, out this.domainShortnames);

            this.WhenAnyValue(row => row.EngineeringModelSetup)
                .Where(x => x != null)
                .Select(modelSetup => modelSetup.Name)
                .ToProperty(this, row => row.ModelName, out this.modelName);

            if (this.ContainerViewModel is PersonRowViewModel deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="DomainShortnames"/> of the row-view-model
        /// </summary>
        public string DomainShortnames
        {
            get { return this.domainShortnames.Value; }
        }

        /// <summary>
        /// Gets the name of the containing <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string ModelName
        {
            get { return this.modelName.Value; }
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/> list that is referenced by the <see cref="Participant"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> Domains
        {
            get { return this.domains; }
            private set { this.RaiseAndSetIfChanged(ref this.domains, value); }
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
        /// Gets or sets the <see cref="ParticipantRole"/>
        /// </summary>
        public ParticipantRole ParticipantRole
        {
            get { return this.participantRole; }
            set { this.RaiseAndSetIfChanged(ref this.participantRole, value); }
        }

        /// <summary>
        /// Gets or sets the container <see cref="EngineeringModelSetup"/> of the <see cref="Participant"/> that is represented by the row-view-model
        /// </summary>
        public EngineeringModelSetup EngineeringModelSetup
        {
            get { return this.engineeringModelSetup; }

            set { this.RaiseAndSetIfChanged(ref this.engineeringModelSetup, value); }
        }

        /// <summary>
        /// Update the properties of the current row-view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Domains = new ReactiveList<DomainOfExpertise>(this.Thing.Domain);
            this.ParticipantRole = this.Thing.Role;
            this.EngineeringModelSetup = (EngineeringModelSetup) this.Thing.Container;

            var person = this.Thing.Person;
            if (person == null)
            {
                return;
            }

            this.RowStatus = (this.Thing.IsActive && person.IsActive && !person.IsDeprecated)
                ? RowStatusKind.Active
                : RowStatusKind.Inactive;

            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }

            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
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

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="PersonRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is PersonRowViewModel deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
        }
    }
}