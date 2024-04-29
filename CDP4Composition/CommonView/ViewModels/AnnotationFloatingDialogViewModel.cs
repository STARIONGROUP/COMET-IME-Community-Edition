﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The view-model that displays the <see cref="ModellingAnnotationItem"/> of a modelling <see cref="Thing"/>
    /// </summary>
    public class AnnotationFloatingDialogViewModel : FloatingDialogViewModelBase<ModellingAnnotationItem>
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
        /// Initializes a new instance of the <see cref="AnnotationFloatingDialogViewModel"/> class
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        public AnnotationFloatingDialogViewModel(ModellingAnnotationItem annotation, ISession session)
            : base(annotation, session)
        {
            this.DiscussionRows = new DisposableReactiveList<DiscussionItemViewModel>();

            if (!(annotation.Container is EngineeringModel model))
            {
                throw new InvalidOperationException("This floating dialog shall only be used for modelling item annotations.");
            }

            this.InitializeCommand();
            this.currentParticipant = model.GetActiveParticipant(this.Session.ActivePerson);
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the list of <see cref="DiscussionItemViewModel"/>
        /// </summary>
        public DisposableReactiveList<DiscussionItemViewModel> DiscussionRows { get; private set; }

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
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Title = this.Thing.Title;
            this.Content = this.Thing.Content;
            this.ShortName = this.Thing.ShortName;
            this.PrimaryAnnotatedThingName = this.Thing.PrimaryAnnotatedThing.UserFriendlyName;

            var displayedDiscussionItems = this.DiscussionRows.Select(x => x.Thing).ToList();
            var updatedDiscussionItems = this.Thing.Discussion;

            var removedDiscussionItems = displayedDiscussionItems.Except(updatedDiscussionItems);
            var addedDiscussionItems = updatedDiscussionItems.Except(displayedDiscussionItems);

            foreach (var removedDiscussionItem in removedDiscussionItems)
            {
                this.RemoveDiscussionItem(removedDiscussionItem);
            }

            foreach (var addedDiscussionItem in addedDiscussionItems)
            {
                this.AddDiscussionItem(addedDiscussionItem);
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
            var clone = this.Thing.Clone(false);
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
