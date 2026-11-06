// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeProposalCardViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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
    using CDP4Common.ReportingData;

    using CDP4Composition.Annotations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The view-model of a single <see cref="ChangeProposal"/> shown on the discussion board of the
    /// <see cref="ChangeRequest"/> it answers, with the actions that can be taken on it.
    /// </summary>
    /// <remarks>
    /// Wraps rather than extends the generated <c>ChangeProposalRowViewModel</c>: the card needs a status-change
    /// command and a delete command, and the generated row is read-only and must not be hand-edited.
    /// </remarks>
    public class ChangeProposalCardViewModel : ViewModelBase<ChangeProposal>
    {
        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="StatusDisplay"/>
        /// </summary>
        private string statusDisplay;

        /// <summary>
        /// Backing field for <see cref="IsEditing"/>
        /// </summary>
        private bool isEditing;

        /// <summary>
        /// Backing field for <see cref="EditShortName"/>
        /// </summary>
        private string editShortName;

        /// <summary>
        /// Backing field for <see cref="EditTitle"/>
        /// </summary>
        private string editTitle;

        /// <summary>
        /// Backing field for <see cref="EditContent"/>
        /// </summary>
        private string editContent;

        /// <summary>
        /// The service that writes the annotations.
        /// </summary>
        private readonly IAnnotationService annotationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeProposalCardViewModel"/> class
        /// </summary>
        /// <param name="changeProposal">The <see cref="ChangeProposal"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="annotationService">The <see cref="IAnnotationService"/> used to write the changes</param>
        public ChangeProposalCardViewModel(ChangeProposal changeProposal, ISession session, IAnnotationService annotationService)
            : base(changeProposal, session)
        {
            this.annotationService = annotationService;

            var canWrite = this.Session.PermissionService.CanWrite(changeProposal);

            this.DeleteCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteDeleteCommand,
                Observable.Return(canWrite),
                RxApp.MainThreadScheduler);

            this.EditCommand = ReactiveCommandCreator.Create(this.ExecuteEditCommand, Observable.Return(canWrite));

            this.CancelEditCommand = ReactiveCommandCreator.Create(() => { this.IsEditing = false; });

            this.SaveEditCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteSaveEditCommand,
                this.WhenAnyValue(
                    x => x.EditShortName,
                    x => x.EditTitle,
                    x => x.EditContent,
                    (shortName, title, body) =>
                        canWrite
                        && !string.IsNullOrWhiteSpace(shortName)
                        && !string.IsNullOrWhiteSpace(title)
                        && !string.IsNullOrWhiteSpace(body)),
                RxApp.MainThreadScheduler);

            this.SetStatusCommand = ReactiveCommandCreator.CreateAsyncTask<AnnotationStatusKind>(
                status => this.annotationService.SetStatusAsync(this.Session, this.Thing, status));

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the short-name of the proposal
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets the title of the proposal
        /// </summary>
        public string Title
        {
            get => this.title;
            private set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// Gets the body text of the proposal
        /// </summary>
        public string Content
        {
            get => this.content;
            private set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that writes the <see cref="AnnotationStatusKind"/> passed as its
        /// parameter onto the proposal
        /// </summary>
        public ReactiveCommand<AnnotationStatusKind, Unit> SetStatusCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that deletes the proposal
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

        /// <summary>
        /// Gets the status of the proposal as shown on the card, so applying a new one visibly changes something
        /// </summary>
        public string StatusDisplay
        {
            get => this.statusDisplay;
            private set => this.RaiseAndSetIfChanged(ref this.statusDisplay, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the card is in edit mode
        /// </summary>
        public bool IsEditing
        {
            get => this.isEditing;

            set
            {
                this.RaiseAndSetIfChanged(ref this.isEditing, value);
                this.RaisePropertyChanged(nameof(this.IsNotEditing));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the card is showing its read-only face
        /// </summary>
        public bool IsNotEditing => !this.IsEditing;

        /// <summary>
        /// Gets or sets the short-name being edited
        /// </summary>
        public string EditShortName
        {
            get => this.editShortName;
            set => this.RaiseAndSetIfChanged(ref this.editShortName, value);
        }

        /// <summary>
        /// Gets or sets the title being edited
        /// </summary>
        public string EditTitle
        {
            get => this.editTitle;
            set => this.RaiseAndSetIfChanged(ref this.editTitle, value);
        }

        /// <summary>
        /// Gets or sets the body text being edited
        /// </summary>
        public string EditContent
        {
            get => this.editContent;
            set => this.RaiseAndSetIfChanged(ref this.editContent, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that puts the card into edit mode
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that writes the edited text
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveEditCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> that abandons the edit
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates on the
        /// <see cref="ChangeProposal"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">The payload of the event that is being handled</param>
        protected override void ObjectChangeEventHandler(CDP4Dal.Events.ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.ShortName = this.Thing.ShortName;
            this.Title = this.Thing.Title;
            this.Content = this.Thing.Content;
            this.StatusDisplay = this.Thing.Status.ToString();
        }

        /// <summary>
        /// Executes the <see cref="EditCommand"/>
        /// </summary>
        private void ExecuteEditCommand()
        {
            this.EditShortName = this.Thing.ShortName;
            this.EditTitle = this.Thing.Title;
            this.EditContent = this.Thing.Content;
            this.IsEditing = true;
        }

        /// <summary>
        /// Executes the <see cref="SaveEditCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task ExecuteSaveEditCommand()
        {
            await this.annotationService.UpdateAsync(this.Session, this.Thing, this.EditTitle, this.EditShortName, this.EditContent);

            this.IsEditing = false;
        }

        /// <summary>
        /// Executes the <see cref="DeleteCommand"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task ExecuteDeleteCommand()
        {
            // the stock delete confirmation, so this looks like every other delete in the IME
            var dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var dialogResult = dialogNavigationService.NavigateModal(new ConfirmationDialogViewModel(this.Thing));

            if (dialogResult?.Result != true)
            {
                return;
            }

            await this.annotationService.DeleteAsync(this.Session, this.Thing);
        }
    }
}
