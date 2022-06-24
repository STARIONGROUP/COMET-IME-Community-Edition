// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionCardViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{

    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view-model used in the <see cref="TeamCompositionBrowserViewModel"/> to represent <see cref="Participant"/>s
    /// </summary>
    public class TeamCompositionCardViewModel : RowViewModelBase<Participant>
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Out property for the <see cref="DomainShortnames"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> domainShortnames;

        /// <summary>
        /// Backing field for <see cref="Domains"/>
        /// </summary>
        private ReactiveList<DomainOfExpertise> domains;

        /// <summary>
        /// Backing field for the <see cref="Person"/> property.
        /// </summary>
        private string person;

        /// <summary>
        /// Backing field for the <see cref="EmailAddress"/> property;
        /// </summary>
        private string emailAddress;

        /// <summary>
        /// Backing field for the <see cref="TelephoneNumber"/> property;
        /// </summary>
        private string telephoneNumber;

        /// <summary>
        /// Backing field for the <see cref="ParticipantRole"/> property;
        /// </summary>
        private string participantRole;

        /// <summary>
        /// Backing field for the <see cref="PersonRole"/> property;
        /// </summary>
        private string personRole;

        /// <summary>
        /// Backing field for the <see cref="Organization"/> property;
        /// </summary>
        private string organization;

        /// <summary>
        /// Backing field for the <see cref="OrganizationalUnit"/> property;
        /// </summary>
        private string organizationalUnit;

        /// <summary>
        /// Backing field for the <see cref="IsActive"/> property;
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionCardViewModel"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container view model.
        /// </param>
        public TeamCompositionCardViewModel(Participant thing, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(thing, session, containerViewModel)
        {
            this.Domains = new ReactiveList<DomainOfExpertise>();

            this.WhenAnyValue(row => row.Domains)
                .Select(
                    domains => 
                    domains.Aggregate(
                        string.Empty,
                        (current, domainOfExpertise) => string.Format("{0} {1}", current, domainOfExpertise.ShortName)).Trim())
                .ToProperty(this, row => row.DomainShortnames, out this.domainShortnames);
            this.domainShortnames.ThrownExceptions.Subscribe(e => Logger.Error(e));

            var canEmail = this.WhenAnyValue(vm => vm.EmailAddress).Select(x => !string.IsNullOrEmpty(x));
            this.OpenEmail = ReactiveCommandCreator.Create(this.OpenEmailExecute, canEmail);

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to open an email browser
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenEmail { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise"/> list that is referenced by the <see cref="Participant"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> Domains
        {
            get
            {
                return this.domains;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.domains, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="Person"/> associated to the current <see cref="Participant"/>
        /// </summary>
        public string Person
        {
            get
            {
                return this.person;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.person, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="Organization"/> of the <see cref="Person"/> associated to the current <see cref="Participant"/>
        /// </summary>
        public string Organization
        {
            get
            {
                return this.organization;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.organization, value);
            }
        }

        /// <summary>
        /// Gets or sets the OrganizationalUnit of the <see cref="Person"/> associated to the current <see cref="Participant"/>
        /// </summary>
        public string OrganizationalUnit
        {
            get
            {
                return this.organizationalUnit;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.organizationalUnit, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="Participant"/> is active.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isActive, value);
            }
        }
    
        /// <summary>
        /// Gets or sets the <see cref="EmailAddress"/> of the <see cref="Person"/> associated to the current <see cref="Participant"/>
        /// </summary>
        /// <remarks>
        /// If the default <see cref="EmailAddress"/> is null, the first <see cref="EmailAddress"/> of the <see cref="Person"/> will be used, it it exists
        /// </remarks>
        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.emailAddress, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TelephoneNumber"/> of the <see cref="Person"/> associated to the current <see cref="Participant"/>
        /// </summary>
        /// <remarks>
        /// If the default <see cref="TelephoneNumber"/> is null, the first <see cref="TelephoneNumber"/> of the <see cref="Person"/> will be used, it it exists
        /// </remarks>
        public string TelephoneNumber
        {
            get
            {
                return this.telephoneNumber;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.telephoneNumber, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="DomainShortnames"/> of the row-view-model
        /// </summary>
        public string DomainShortnames
        {
            get { return this.domainShortnames.Value; }
        }

        /// <summary>
        /// Gets or sets the name of the associated <see cref="ParticipantRole"/>
        /// </summary>
        public string ParticipantRole
        {
            get
            {
                return this.participantRole;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.participantRole, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the associated name of the <see cref="PersonRole"/> of the associated <see cref="Person"/>
        /// </summary>
        public string PersonRole
        {
            get
            {
                return this.personRole;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.personRole, value);
            }
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Domains = new ReactiveList<DomainOfExpertise>(this.Thing.Domain);
            this.Person = this.Thing.Person.Name;

            this.Organization = this.Thing.Person.Organization?.Name ?? string.Empty;
            this.OrganizationalUnit = this.Thing.Person.OrganizationalUnit ?? string.Empty;
            this.ParticipantRole = this.Thing.Role.Name;
            this.PersonRole = this.Thing.Person.Role?.Name ?? string.Empty;
            this.IsActive = this.Thing.IsActive;

            this.UpdateEmailAddress();

            this.UpdateTelephoneNumber();
        }

        /// <summary>
        /// Update the <see cref="EmailAddress"/> property
        /// </summary>
        private void UpdateEmailAddress()
        {
            if (this.Thing.Person.DefaultEmailAddress != null)
            {
                this.EmailAddress = this.Thing.Person.DefaultEmailAddress.Value;
            }
            else
            {
                var email = this.Thing.Person.EmailAddress.FirstOrDefault();
                if (email != null)
                {
                    this.EmailAddress = email.Value;
                }
            }
        }

        /// <summary>
        /// Update the <see cref="TelephoneNumber"/> property
        /// </summary>
        private void UpdateTelephoneNumber()
        {
            if (this.Thing.Person.DefaultTelephoneNumber != null)
            {
                this.TelephoneNumber = this.Thing.Person.DefaultTelephoneNumber.Value;
            }
            else
            {
                var number = this.Thing.Person.TelephoneNumber.FirstOrDefault();
                if (number != null)
                {
                    this.TelephoneNumber = number.Value;
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenEmail"/> <see cref="ReactiveCommand"/>
        /// </summary>
        private void OpenEmailExecute()
        {
            try
            {
                var proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = string.Format("mailto:{0}?subject=[CDP4]", this.emailAddress);
                proc.Start();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }            
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
        /// Initializes the subscriptions
        /// </summary>
        /// TODO : subscription to be fied to listen on add/remove contained Thing for the current person. The subscription also need to be modified if the roles changes
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var personUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Person)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(personUpdateListener);

            var personRoleUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Person.Role)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(personRoleUpdateListener);

            var participantRoleUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Role)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(participantRoleUpdateListener);

            var telephoneNumnerListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(TelephoneNumber))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Container == this.Thing.Person)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(telephoneNumnerListener);

            var emailAddressListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EmailAddress))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Container == this.Thing.Person)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(emailAddressListener);
        }
    }
}
