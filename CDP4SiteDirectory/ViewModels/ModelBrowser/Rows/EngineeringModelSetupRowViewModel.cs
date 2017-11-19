// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Converters;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using FolderRowViewModel = CDP4Composition.FolderRowViewModel;

    /// <summary>
    /// The Row-view-model representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class EngineeringModelSetupRowViewModel : CDP4CommonView.EngineeringModelSetupRowViewModel
    {
        #region Fields

        /// <summary>
        /// The row containing all the <see cref="Participant"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel participantFolderRow;

        /// <summary>
        /// The row containing all the <see cref="IterationSetup"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel iterationSetupFolderRow;

        /// <summary>
        /// The row containing all the <see cref="IterationSetup"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel activeDomainFolderRow;

        /// <summary>
        /// Backing field for the <see cref="Description"/> property
        /// </summary>
        private string description;

        /// <summary>
        /// The underscore capitals to spaced TitleCase converter.
        /// </summary>
        private readonly UnderscoreCapitalsToSpacedTitleCaseConverter titleConverter = new UnderscoreCapitalsToSpacedTitleCaseConverter();
        
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The engineering Model Setup.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container <see cref="IViewModelBase{T}"/>
        /// </param>
        public EngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(engineeringModelSetup, session, containerViewModel)
        {
            this.participantFolderRow = new FolderRowViewModel("Participants", "Participants", this.Session, this);
            this.iterationSetupFolderRow = new FolderRowViewModel("Iterations", "Iterations", this.Session, this);
            this.activeDomainFolderRow = new FolderRowViewModel("Active Domains", "Active Domains", this.Session, this);

            this.ContainedRows.Add(this.participantFolderRow);
            this.ContainedRows.Add(this.iterationSetupFolderRow);
            this.ContainedRows.Add(this.activeDomainFolderRow);

            this.UpdateProperties();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the description of the <see cref="EngineeringModelSetup"/> that is represented by the current row-view-model
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }
        #endregion

        #region override row-base
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
        /// Update the properties of the current row-view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Description = string.Format(
                "Phase: {0}, Kind: {1}",
                this.titleConverter.Convert(this.Thing.StudyPhase, null, null, null),
                this.titleConverter.Convert(this.Thing.Kind, null, null, null));
            this.UpdateContainers();
        }

        /// <summary>
        /// Initializes containers
        /// </summary>
        private void UpdateContainers()
        {
            var currentIterationSetup = this.Thing.IterationSetup;
            var newparticipant = this.Thing.Participant.Except(this.participantFolderRow.ContainedRows.Select(x => (Participant)x.Thing)).ToList();
            var newiteration = currentIterationSetup.Except(this.iterationSetupFolderRow.ContainedRows.Select(x => (IterationSetup)x.Thing)).ToList();
            var newDomain = this.Thing.ActiveDomain.Except(this.activeDomainFolderRow.ContainedRows.Select(x => (DomainOfExpertise)x.Thing)).ToList();

            var oldparticipant = this.participantFolderRow.ContainedRows.Select(x => (Participant)x.Thing).Except(this.Thing.Participant).ToList();
            var olditeration = this.iterationSetupFolderRow.ContainedRows.Select(x => (IterationSetup)x.Thing).Except(currentIterationSetup).ToList();
            var oldDomain = this.activeDomainFolderRow.ContainedRows.Select(x => (DomainOfExpertise)x.Thing).Except(this.Thing.ActiveDomain).ToList();

            foreach (var participant in oldparticipant)
            {
                this.RemoveParticipant(participant);
            }

            foreach (var iterationSetup in olditeration)
            {
                this.RemoveIteration(iterationSetup);
            }

            foreach (var domain in oldDomain)
            {
                this.RemoveDomain(domain);
            }

            foreach (var participant in newparticipant)
            {
                this.AddParticipant(participant);
            }

            foreach (var iteration in newiteration)
            {
                this.AddIteration(iteration);
            }

            foreach (var domain in newDomain)
            {
                this.AddDomain(domain);
            }

            var orderedCollection = this.activeDomainFolderRow.ContainedRows.OfType<DomainOfExpertiseRowViewModel>().OrderBy(x => x.Name).ToArray();
            this.activeDomainFolderRow.ContainedRows.Clear();
            this.activeDomainFolderRow.ContainedRows.AddRange(orderedCollection);
        }

        /// <summary>
        /// Remove the <see cref="Participant"/>
        /// </summary>
        /// <param name="participant">
        /// the <see cref="Participant"/> object to remove
        /// </param>
        private void RemoveParticipant(Participant participant)
        {
            var row = this.participantFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == participant);
            if (row != null)
            {
                this.participantFolderRow.ContainedRows.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Add the <see cref="Participant"/>
        /// </summary>
        /// <param name="participant">
        /// the <see cref="Participant"/> object to add
        /// </param>
        private void AddParticipant(Participant participant)
        {
            var row = new ModelParticipantRowViewModel(participant, this.Session, this);
            this.participantFolderRow.ContainedRows.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="iteration">
        /// the <see cref="IterationSetup"/> object to remove
        /// </param>
        private void RemoveIteration(IterationSetup iteration)
        {
            var row = this.iterationSetupFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == iteration);
            if (row != null)
            {
                this.iterationSetupFolderRow.ContainedRows.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Add the <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="iteration">
        /// the <see cref="IterationSetup"/> object to add
        /// </param>
        private void AddIteration(IterationSetup iteration)
        {
            var row = new IterationSetupRowViewModel(iteration, this.Session, this);
            this.iterationSetupFolderRow.ContainedRows.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="domain">
        /// the <see cref="DomainOfExpertise"/> object to remove
        /// </param>
        private void RemoveDomain(DomainOfExpertise domain)
        {
            var row = this.activeDomainFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == domain);
            if (row != null)
            {
                this.activeDomainFolderRow.ContainedRows.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Add the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="domain">
        /// the <see cref="DomainOfExpertise"/> object to add
        /// </param>
        private void AddDomain(DomainOfExpertise domain)
        {
            var row = new DomainOfExpertiseRowViewModel(domain, this.Session, this);
            this.activeDomainFolderRow.ContainedRows.Add(row);
        }
    }
}