// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationFloatingDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Annotations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The view-model that displays the <see cref="EngineeringModelDataAnnotation"/> of a modelling
    /// <see cref="Thing"/> together with its discussion board
    /// </summary>
    /// <remarks>
    /// Typed on <see cref="EngineeringModelDataAnnotation"/> rather than <see cref="ModellingAnnotationItem"/>: a
    /// model note is an annotation with a discussion but not a modelling annotation item, and the narrower type was
    /// why opening one threw an <see cref="InvalidCastException"/> instead of showing the board.
    /// </remarks>
    public class AnnotationFloatingDialogViewModel : FloatingDialogViewModelBase<EngineeringModelDataAnnotation>
    {
        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="PrimaryAnnotatedThingName"/>
        /// </summary>
        private string primaryAnnotatedThingName;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// The current <see cref="Participant"/>
        /// </summary>
        private readonly Participant currentParticipant;

        /// <summary>
        /// Backing field for <see cref="CanCreateDiscussionItem"/>
        /// </summary>
        private bool canCreateDiscussionItem;

        /// <summary>
        /// Backing field for <see cref="NewDiscussionItemText"/>
        /// </summary>
        private string newDiscussionItemText;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="Kind"/>
        /// </summary>
        private string kind;

        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private string status;

        /// <summary>
        /// Backing field for <see cref="IsProposalComposerVisible"/>
        /// </summary>
        private bool isProposalComposerVisible = true;

        /// <summary>
        /// Backing field for <see cref="NewProposalShortName"/>
        /// </summary>
        private string newProposalShortName;

        /// <summary>
        /// Backing field for <see cref="NewProposalTitle"/>
        /// </summary>
        private string newProposalTitle;

        /// <summary>
        /// Backing field for <see cref="NewProposalContent"/>
        /// </summary>
        private string newProposalContent;

        /// <summary>
        /// The service that writes the annotations.
        /// </summary>
        private readonly IAnnotationService annotationService = new AnnotationService();

        /// <summary>
        /// Backing field for <see cref="SelectedReplyTarget"/>
        /// </summary>
        private AnnotationReplyTarget selectedReplyTarget;

        /// <summary>
        /// The annotation whose board is shown. For a <see cref="ChangeProposal"/> this is the
        /// <see cref="ChangeRequest"/> it answers, so both windows show the same conversation.
        /// </summary>
        private readonly EngineeringModelDataAnnotation boardRoot;

        /// <summary>
        /// The <see cref="EngineeringModel"/> the annotation lives in.
        /// </summary>
        private readonly EngineeringModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationFloatingDialogViewModel"/> class
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <exception cref="InvalidOperationException">The annotation is not contained in an engineering model.</exception>
        public AnnotationFloatingDialogViewModel(EngineeringModelDataAnnotation annotation, ISession session)
            : base(annotation, session)
        {
            this.DiscussionRows = new DisposableReactiveList<DiscussionItemViewModel>();
            this.ChangeProposalRows = new DisposableReactiveList<ChangeProposalCardViewModel>();

            if (!(annotation.Container is EngineeringModel model))
            {
                throw new InvalidOperationException("This floating dialog shall only be used for modelling item annotations.");
            }

            // a proposal and the request it answers share one conversation, so both windows work off the request
            this.boardRoot = annotation is ChangeProposal proposal && proposal.ChangeRequest != null
                ? proposal.ChangeRequest
                : annotation;

            this.model = model;

            this.InitializeCommand();
            this.currentParticipant = model.GetActiveParticipant(this.Session.ActivePerson);
            this.UpdateProperties();

            // a proposal is contained by the model, not by this change request, so this window never saw one being
            // added and the board stayed empty until it was reopened
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ChangeProposal))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModelDataDiscussionItem)))
                    .Where(x => x.ChangedThing.GetContainerOfType<EngineeringModel>() == model)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties()));
        }

        /// <summary>
        /// Gets the list of <see cref="DiscussionItemViewModel"/>
        /// </summary>
        /// <remarks>
        /// For a <see cref="ChangeRequest"/> this holds the discussion of the request and of every
        /// <see cref="ChangeProposal"/> answering it, in one list ordered by creation date.
        /// </remarks>
        public DisposableReactiveList<DiscussionItemViewModel> DiscussionRows { get; private set; }

        /// <summary>
        /// Gets the <see cref="ChangeProposal"/>s answering this annotation; empty unless it is a
        /// <see cref="ChangeRequest"/>
        /// </summary>
        public DisposableReactiveList<ChangeProposalCardViewModel> ChangeProposalRows { get; private set; }

        /// <summary>
        /// Gets the annotations a reply can be aimed at: the change request and the proposals answering it
        /// </summary>
        public ReactiveList<AnnotationReplyTarget> PossibleReplyTargets { get; } = new ReactiveList<AnnotationReplyTarget>();

        /// <summary>
        /// Gets or sets the annotation a new reply is posted on
        /// </summary>
        /// <remarks>
        /// The board is shared, but a reply still belongs to one annotation: a remark about a specific proposal
        /// should hang off that proposal, while still showing in the request's thread.
        /// </remarks>
        public AnnotationReplyTarget SelectedReplyTarget
        {
            get => this.selectedReplyTarget;
            set => this.RaiseAndSetIfChanged(ref this.selectedReplyTarget, value);
        }

        /// <summary>
        /// Gets a value indicating whether there is more than one thing a reply could be aimed at
        /// </summary>
        public bool HasMultipleReplyTargets => this.PossibleReplyTargets.Count > 1;

        /// <summary>
        /// Gets a value indicating whether this annotation is a <see cref="ChangeRequest"/>, and so can be answered
        /// with a <see cref="ChangeProposal"/> raised from this board
        /// </summary>
        public bool IsChangeRequest => this.boardRoot is ChangeRequest;



        /// <summary>
        /// Gets or sets a value indicating whether the composer for a new <see cref="ChangeProposal"/> is shown
        /// </summary>
        /// <remarks>
        /// Collapsed once a proposal exists: the common case is then reading and working the proposals already
        /// there, not adding another, and the composer is large enough to push them off the window.
        /// </remarks>
        public bool IsProposalComposerVisible
        {
            get => this.isProposalComposerVisible;

            set
            {
                this.RaiseAndSetIfChanged(ref this.isProposalComposerVisible, value);
                this.RaisePropertyChanged(nameof(this.IsProposalComposerCollapsed));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the composer is collapsed, and so the button that reveals it is shown
        /// </summary>
        /// <remarks>
        /// A property rather than an inverting converter: the DevExpress converter in this version has no inverse
        /// and the repository has no inverting one.
        /// </remarks>
        public bool IsProposalComposerCollapsed => !this.IsProposalComposerVisible;

        /// <summary>
        /// Gets the <see cref="ICommand"/> that shows the composer for a new <see cref="ChangeProposal"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> ShowProposalComposerCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that abandons the composer and discards what was typed into it
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelProposalComposerCommand { get; private set; }

        /// <summary>
        /// Gets or sets the short-name of the change proposal to raise
        /// </summary>
        public string NewProposalShortName
        {
            get => this.newProposalShortName;
            set => this.RaiseAndSetIfChanged(ref this.newProposalShortName, value);
        }

        /// <summary>
        /// Gets or sets the title of the change proposal to raise
        /// </summary>
        public string NewProposalTitle
        {
            get => this.newProposalTitle;
            set => this.RaiseAndSetIfChanged(ref this.newProposalTitle, value);
        }

        /// <summary>
        /// Gets or sets the body text of the change proposal to raise
        /// </summary>
        public string NewProposalContent
        {
            get => this.newProposalContent;
            set => this.RaiseAndSetIfChanged(ref this.newProposalContent, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to raise a <see cref="ChangeProposal"/> answering this change request
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateChangeProposalCommand { get; private set; }

        /// <summary>
        /// Gets the title
        /// </summary>
        public string Title
        {
            get => this.title;
            private set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// Gets the content
        /// </summary>
        public string Content
        {
            get => this.content;
            private set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        /// <summary>
        /// Gets the human-readable kind of the annotation, so a reader can tell a waiver from a note at a glance
        /// </summary>
        public string Kind
        {
            get => this.kind;
            private set => this.RaiseAndSetIfChanged(ref this.kind, value);
        }

        /// <summary>
        /// Gets the status of the annotation, or an empty string for a kind that carries none
        /// </summary>
        public string Status
        {
            get => this.status;
            private set => this.RaiseAndSetIfChanged(ref this.status, value);
        }

        /// <summary>
        /// Gets or sets the discussion item text to post
        /// </summary>
        public string NewDiscussionItemText
        {
            get => this.newDiscussionItemText;
            set => this.RaiseAndSetIfChanged(ref this.newDiscussionItemText, value);
        }

        /// <summary>
        /// Gets the short name
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets a vlaue indicating whether the <see cref="PostDiscussionItemCommand"/> is enabled
        /// </summary>
        public bool CanCreateDiscussionItem
        {
            get => this.canCreateDiscussionItem;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateDiscussionItem, value);
        }

        /// <summary>
        /// Gets the user-friendly name for the primary annotated <see cref="Thing"/>
        /// </summary>
        public string PrimaryAnnotatedThingName
        {
            get => this.primaryAnnotatedThingName;
            private set => this.RaiseAndSetIfChanged(ref this.primaryAnnotatedThingName, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to post a <see cref="DiscussionItem"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> PostDiscussionItemCommand { get; private set; }

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
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        private void InitializeCommand()
        {
            this.CanCreateDiscussionItem = this.Session.PermissionService.CanWrite(ClassKind.EngineeringModelDataDiscussionItem, this.Thing);

            this.PostDiscussionItemCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecutePostDiscussionItemCommand,
                this.WhenAny(
                    x => x.CanCreateDiscussionItem,
                    x => x.NewDiscussionItemText,
                    (permission, text) => permission.Value && !string.IsNullOrWhiteSpace(text.Value)),
                RxApp.MainThreadScheduler);

            this.PostDiscussionItemCommand.ThrownExceptions.Select(ex => ex).Subscribe(
                x =>
                {
                    logger.Error("The inline update operation failed: {0}", x.Message);
                    this.ErrorMessage = x.Message;
                    this.IsBusy = false;
                });

            this.CreateChangeProposalCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteCreateChangeProposalCommand,
                this.WhenAnyValue(
                    x => x.NewProposalShortName,
                    x => x.NewProposalTitle,
                    x => x.NewProposalContent,
                    (shortName, title, content) =>
                        this.IsChangeRequest
                        && this.Session.PermissionService.CanWrite(ClassKind.ChangeProposal, this.Thing)
                        && !string.IsNullOrWhiteSpace(shortName)
                        && !string.IsNullOrWhiteSpace(title)
                        && !string.IsNullOrWhiteSpace(content)),
                RxApp.MainThreadScheduler);

            this.ShowProposalComposerCommand = ReactiveCommandCreator.Create(() => { this.IsProposalComposerVisible = true; });

            this.CancelProposalComposerCommand = ReactiveCommandCreator.Create(this.ExecuteCancelProposalComposerCommand);

            this.CreateChangeProposalCommand.ThrownExceptions.Select(ex => ex).Subscribe(
                x =>
                {
                    logger.Error("The change proposal could not be created: {0}", x.Message);
                    this.ErrorMessage = x.Message;
                    this.IsBusy = false;
                });
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            // the header describes the board root, so opening a proposal gives the same window as opening the
            // request it answers: request on top, proposals with their actions below, one shared discussion
            var modellingAnnotation = this.boardRoot as ModellingAnnotationItem;

            this.Kind = AnnotationKind.Describe(this.boardRoot);
            this.Title = string.IsNullOrWhiteSpace(modellingAnnotation?.Title) ? this.Kind : modellingAnnotation.Title;
            this.Content = this.boardRoot.Content;
            this.ShortName = AnnotationKind.QueryShortName(this.boardRoot);
            this.Status = AnnotationQuery.DescribeStatus(this.boardRoot);

            var annotatedThing = this.boardRoot.PrimaryAnnotatedThing?.ReferencedThing
                                 ?? this.boardRoot.RelatedThing.FirstOrDefault()?.ReferencedThing;

            this.PrimaryAnnotatedThingName = annotatedThing?.UserFriendlyName ?? "-";

            // re-evaluated on every change: gating the Send button on a permission read once at open left it dead
            // with no explanation after the participant's role changed
            this.CanCreateDiscussionItem = this.Session.PermissionService.CanWrite(ClassKind.EngineeringModelDataDiscussionItem, this.boardRoot);

            this.UpdateChangeProposals();

            var displayedDiscussionItems = this.DiscussionRows.Select(x => x.Thing).ToList();

            // a change request and the proposals answering it share one board: the proposals are the conversation
            // about the request, so splitting them across windows loses the thread
            var updatedDiscussionItems = this.boardRoot.Discussion
                .Concat(this.ChangeProposalRows.SelectMany(x => x.Thing.Discussion))
                .ToList();

            var removedDiscussionItems = displayedDiscussionItems.Except(updatedDiscussionItems).ToList();
            var addedDiscussionItems = updatedDiscussionItems.Except(displayedDiscussionItems).ToList();

            foreach (var removedDiscussionItem in removedDiscussionItems)
            {
                this.RemoveDiscussionItem(removedDiscussionItem);
            }

            foreach (var addedDiscussionItem in addedDiscussionItems)
            {
                this.AddDiscussionItem(addedDiscussionItem);
            }

            this.DiscussionRows.Sort((left, right) => left.Thing.CreatedOn.CompareTo(right.Thing.CreatedOn));
        }

        /// <summary>
        /// Re-reads the <see cref="ChangeProposal"/>s answering this annotation, when it is a
        /// <see cref="ChangeRequest"/>.
        /// </summary>
        private void UpdateChangeProposals()
        {
            if (!(this.boardRoot is ChangeRequest changeRequest))
            {
                return;
            }

            var proposals = AnnotationService.QueryChangeProposals(this.model, changeRequest);

            var displayed = this.ChangeProposalRows.Select(x => x.Thing).ToList();

            foreach (var removed in displayed.Except(proposals).ToList())
            {
                var row = this.ChangeProposalRows.SingleOrDefault(x => x.Thing == removed);

                if (row != null)
                {
                    this.ChangeProposalRows.RemoveAndDispose(row);
                }
            }

            foreach (var added in proposals.Except(displayed).ToList())
            {
                this.ChangeProposalRows.Add(new ChangeProposalCardViewModel(added, this.Session, this.annotationService));
            }

            // the composer takes the whole window, so it only stays open while there is nothing to read
            if (this.ChangeProposalRows.Count > 0)
            {
                this.IsProposalComposerVisible = false;
            }

            this.UpdateReplyTargets();

        }

        /// <summary>
        /// Rebuilds the list of annotations a reply can be aimed at, keeping the current selection where it still
        /// exists and otherwise defaulting to the annotation this window was opened on.
        /// </summary>
        private void UpdateReplyTargets()
        {
            var targets = new List<EngineeringModelDataAnnotation> { this.boardRoot };
            targets.AddRange(this.ChangeProposalRows.Select(x => x.Thing));

            var selected = this.SelectedReplyTarget?.Annotation;

            this.PossibleReplyTargets.Clear();
            this.PossibleReplyTargets.AddRange(targets.Select(x => new AnnotationReplyTarget(x)));

            this.SelectedReplyTarget =
                this.PossibleReplyTargets.FirstOrDefault(x => x.Annotation == selected)
                ?? this.PossibleReplyTargets.FirstOrDefault(x => x.Annotation == this.Thing)
                ?? this.PossibleReplyTargets.FirstOrDefault();

            this.RaisePropertyChanged(nameof(this.HasMultipleReplyTargets));
        }

        /// <summary>
        /// Executes the <see cref="CancelProposalComposerCommand"/>
        /// </summary>
        private void ExecuteCancelProposalComposerCommand()
        {
            this.NewProposalShortName = string.Empty;
            this.NewProposalTitle = string.Empty;
            this.NewProposalContent = string.Empty;

            this.IsProposalComposerVisible = false;
        }

        /// <summary>
        /// Executes the <see cref="CreateChangeProposalCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateChangeProposalCommand()
        {
            this.ErrorMessage = string.Empty;
            this.IsBusy = true;

            try
            {
                var changeRequest = (ChangeRequest)this.Thing;

                await this.annotationService.CreateChangeProposalAsync(
                    this.Session,
                    changeRequest,
                    this.currentParticipant,
                    changeRequest.Owner,
                    this.NewProposalTitle,
                    this.NewProposalShortName,
                    this.NewProposalContent,
                    changeRequest.Classification);

                this.NewProposalShortName = string.Empty;
                this.NewProposalTitle = string.Empty;
                this.NewProposalContent = string.Empty;
                this.IsProposalComposerVisible = false;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Add the view-model representing the <paramref name="discussionItem"/>
        /// </summary>
        /// <param name="discussionItem">The <see cref="DiscussionItem"/> to add</param>
        private void AddDiscussionItem(EngineeringModelDataDiscussionItem discussionItem)
        {
            var vm = new DiscussionItemViewModel(discussionItem, this.Session, this);
            this.DiscussionRows.Add(vm);
        }

        /// <summary>
        /// Remove the view-model representing the <paramref name="discussionItem"/>
        /// </summary>
        /// <param name="discussionItem">The <see cref="DiscussionItem"/> to remove</param>
        private void RemoveDiscussionItem(EngineeringModelDataDiscussionItem discussionItem)
        {
            var vm = this.DiscussionRows.SingleOrDefault(x => x.Thing == discussionItem);

            if (vm != null)
            {
                this.DiscussionRows.RemoveAndDispose(vm);
            }
        }

        /// <summary>
        /// Executes the <see cref="PostDiscussionItemCommand"/>
        /// </summary>
        private async Task ExecutePostDiscussionItemCommand()
        {
            this.ErrorMessage = string.Empty;
            this.IsBusy = true;

            // posted on the selected annotation, not necessarily the one this window was opened on, so a remark
            // about a proposal hangs off that proposal while still showing in the shared thread
            var target = this.SelectedReplyTarget?.Annotation ?? this.Thing;
            var clone = target.Clone(false);
            var discussionItem = new EngineeringModelDataDiscussionItem();
            discussionItem.Content = this.NewDiscussionItemText;
            discussionItem.Author = this.currentParticipant;
            discussionItem.LanguageCode = "en-GB";
            discussionItem.CreatedOn = DateTime.UtcNow;

            clone.Discussion.Add(discussionItem);

            var context = TransactionContextResolver.ResolveContext(clone);
            var transaction = new ThingTransaction(context, clone);
            transaction.Create(discussionItem);
            await this.DalWrite(transaction);
            this.IsBusy = false;
            this.NewDiscussionItemText = string.Empty;
        }
    }
}
