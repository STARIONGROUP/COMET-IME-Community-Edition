// ------------------------------------------------------------------------------------------------
// <copyright file="DiscussionItemViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Reactive;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using EventAggregator;
    using ReactiveUI;

    /// <summary>
    /// The view-model representing a <see cref="EngineeringModelDataDiscussionItem"/> displayed in a floating window
    /// </summary>
    public class DiscussionItemViewModel : EngineeringModelDataDiscussionItemRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ParticipantFullName"/>
        /// </summary>
        private string participantFullName;

        /// <summary>
        /// Backing field for <see cref="IsModifiedAtVisible"/>
        /// </summary>
        private bool isModifiedAtVisible;

        /// <summary>
        /// Backing field for <see cref="CanEditDiscussionItem"/>
        /// </summary>
        private bool canEditDiscussionItem;

        /// <summary>
        /// Backing field for <see cref="EventPublisher"/>
        /// </summary>
        private EventPublisher eventPublisher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscussionItemViewModel"/> class
        /// </summary>
        /// <param name="discussionItem">The <see cref="DiscussionItem"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerviewModel">The container <see cref="IViewModelBase{T}"/></param>
        public DiscussionItemViewModel(EngineeringModelDataDiscussionItem discussionItem, ISession session, IViewModelBase<Thing> containerviewModel)
            : base(discussionItem, session, containerviewModel)
        {
            this.CanEditDiscussionItem = this.Session.PermissionService.CanWrite(this.Thing);
            this.EventPublisher = new EventPublisher();
            this.InitializeCommands();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="EventPublisher"/>
        /// </summary>
        public EventPublisher EventPublisher
        {
            get { return this.eventPublisher; }
            private set { this.RaiseAndSetIfChanged(ref this.eventPublisher, value); }
        }

        /// <summary>
        /// Gets the save <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> SaveCommand { get; private set; }

        /// <summary>
        /// Gets the cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="DiscussionItem"/> is editable
        /// </summary>
        public bool CanEditDiscussionItem
        {
            get { return this.canEditDiscussionItem; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditDiscussionItem, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the IsModified is visible
        /// </summary>
        public bool IsModifiedAtVisible
        {
            get { return this.isModifiedAtVisible; }
            private set { this.RaiseAndSetIfChanged(ref this.isModifiedAtVisible, value); }
        }

        /// <summary>
        /// Gets the participant full name along the domain
        /// </summary>
        public string ParticipantFullName
        {
            get { return this.participantFullName; }
            private set { this.RaiseAndSetIfChanged(ref this.participantFullName, value); }
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
        /// Initiliazes the <see cref="ICommand"/>
        /// </summary>
        private void InitializeCommands()
        {
            this.SaveCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.CanEditDiscussionItem), x => this.ExecuteSaveCommand(), RxApp.MainThreadScheduler);

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(
                _ =>
                {
                    this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing));
                    this.EventPublisher.Publish(new ConfirmationEvent(true));
                });
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.ParticipantFullName = this.Thing == null || this.Thing.Author.Person == null
                ? "Error: Not Set"
                : this.Thing.Author.Person.Name;

            this.IsModifiedAtVisible = this.ModifiedOn > this.CreatedOn;
        }

        /// <summary>
        /// Execute the <see cref="SaveCommand"/>
        /// </summary>
        private async Task ExecuteSaveCommand()
        {
            var clone = this.Thing.Clone(false);
            clone.Content = this.Content;

            await this.DalWrite(clone);
            this.EventPublisher.Publish(new ConfirmationEvent(true));
        }
    }
}