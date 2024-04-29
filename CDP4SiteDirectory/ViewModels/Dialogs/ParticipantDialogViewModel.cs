// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The Dialog-view-model to create, edit or inspect a <see cref="Participant"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Participant)]
    public class ParticipantDialogViewModel : CDP4CommonView.ParticipantDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParticipantDialogViewModel()
        {
        }

        /// <summary>
        /// Holds a reference to the <see cref="IDisposable"/> subscription that belongs to the current <see cref="ReactiveList{DomainOfExpertise}"/> instance in <see cref="ParticipantDialogViewModel.Domain"/>
        /// </summary>
        private IDisposable domainSubScription = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialogViewModel"/> class
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParticipantDialogViewModel(Participant participant, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(participant, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedPerson).Where(x => x != null).Subscribe(_ =>
            {
                if (this.dialogKind == ThingDialogKind.Create)
                {
                    if (this.SelectedPerson.DefaultDomain != null &&
                        this.PossibleDomain.Contains(this.SelectedPerson.DefaultDomain))
                    {
                        this.Domain = new ReactiveList<DomainOfExpertise>
                        {
                            this.PossibleDomain.Single(x => x == this.SelectedPerson.DefaultDomain)
                        };
                    }
                    else
                    {
                        this.Domain = new ReactiveList<DomainOfExpertise>();
                    }
                }

                this.UpdateOkCanExecute();
            });

            this.WhenAnyValue(vm => vm.SelectedRole).Subscribe(
                _ =>
                {
                    this.UpdateOkCanExecute();
                });

            this.WhenAnyValue(vm => vm.SelectedSelectedDomain).Subscribe(_ => this.UpdateOkCanExecute());

            this.WhenAnyValue(vm => vm.Domain).Subscribe(_ =>
            {
                this.domainSubScription?.Dispose();
                this.domainSubScription = this.Domain?.Changed.Subscribe(x => this.SetSelectedDomain());
                this.SetSelectedDomain();
            });

            this.isSelectedRoleDeprecated =
                this.WhenAny(x => x.SelectedRole, selectedRole => selectedRole.Value?.IsDeprecated == true)
                    .ToProperty(this, x => x.IsSelectedRoleDeprecated, out this.isSelectedRoleDeprecated, scheduler: RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Returns true if <see cref="ParticipantRole"/> is selected and is deprecated
        /// </summary>
        public bool IsSelectedRoleDeprecated
        {
            get { return this.isSelectedRoleDeprecated.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> isSelectedRoleDeprecated;

        /// <summary>
        /// Initializes the lists and subscription
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleDomain = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Populates the possible <see cref="Person"/>
        /// </summary>
        protected override void PopulatePossiblePerson()
        {
            base.PopulatePossiblePerson();

            var sitedir = this.Session.RetrieveSiteDirectory();
            var model = this.Container as EngineeringModelSetup;
            if (model == null)
            {
                throw new InvalidOperationException("the container of this participant is not an EngineeringModelSetup.");
            }

            if (this.dialogKind == ThingDialogKind.Create)
            {
                foreach (var person in sitedir.Person.OrderBy(x => x.Name).Except(model.Participant.Select(p => p.Person)))
                {
                    this.PossiblePerson.Add(person);
                }
            }
            else
            {
                this.PossiblePerson.Add(this.SelectedPerson);
            }
        }

        /// <summary>
        /// Populates the possible <see cref="ParticipantRole"/>
        /// </summary>
        protected override void PopulatePossibleRole()
        {
            base.PopulatePossibleRole();

            var sitedir = this.Session.RetrieveSiteDirectory();
            foreach (var role in sitedir.ParticipantRole.OrderBy(x => x.Name))
            {
                this.PossibleRole.Add(role);
            }
        }

        /// <summary>
        /// Populates the possible <see cref="DomainOfExpertise"/>
        /// </summary>
        protected override void PopulateDomain()
        {
            base.PopulateDomain();
            var model = this.Container as EngineeringModelSetup;
            if (model == null)
            {
                throw new InvalidOperationException("the container of this participant is not an EngineeringModelSetup.");
            }

            foreach (var domain in model.ActiveDomain.OrderBy(x => x.Name))
            {
                this.PossibleDomain.Add(domain);
            }

            this.SelectedSelectedDomain = this.Domain.FirstOrDefault();
        }

        /// <summary>
        /// Updates the <see cref="ParticipantDialogViewModel.OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();

            this.OkCanExecute = 
                this.OkCanExecute && 
                this.SelectedPerson != null && 
                this.SelectedRole != null && 
                !this.Domain.IsEmpty && 
                this.SelectedSelectedDomain != null && 
                this.Domain.Any(x => x.Iid == this.SelectedSelectedDomain?.Iid);
        }

        /// <summary>
        /// Populate the possible <see cref="ParticipantDialogViewModel.SelectedSelectedDomain"/> choices
        /// </summary>
        /// <remarks>
        /// The <see cref="Participant.SelectedDomain"/> needs to be set, else wsp returns an error
        /// </remarks>
        private void SetSelectedDomain()
        {
            if (this.Domain.All(x => x.Iid != this.SelectedSelectedDomain?.Iid))
            {
                this.SelectedSelectedDomain = this.Domain.FirstOrDefault();
            }

            this.UpdateOkCanExecute();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.domainSubScription?.Dispose();
        }
    }
}
