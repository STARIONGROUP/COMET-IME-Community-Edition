// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementsSpecificationEditor
{
    using System;
    using System.Reactive.Linq;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.EventAggregator;
    using CDP4CommonView.ViewModels;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.Utils;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="RequirementRowViewModel" /> is the view-model that represents a <see cref="Requirement" /> in the
    /// <see cref="RequirementsSpecificationEditorViewModel" />
    /// </summary>
    public class RequirementRowViewModel : CDP4CommonView.RequirementRowViewModel, IBreadCrumb
    {
        /// <summary>
        /// Backing field for the <see cref="BreadCrumb" /> property
        /// </summary>
        private string breadCrumb;

        /// <summary>
        /// Backing field for the <see cref="Categories" /> property
        /// </summary>
        private string categories;

        /// <summary>
        /// The <see cref="Definition" /> to display
        /// </summary>
        private Definition definition;

        /// <summary>
        /// Backing field for the <see cref="DefinitionContent" /> property
        /// </summary>
        private string definitionContent;

        /// <summary>
        /// The subscription on <see cref="definition" />
        /// </summary>
        private IDisposable definitionSubscription;

        /// <summary>
        /// Backing field for <see cref="EventPublisher" />
        /// </summary>
        private EventPublisher eventPublisher;

        /// <summary>
        /// Backing field for the <see cref="IsDirty" /> property
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// the content of the <see cref="Definition" />
        /// </summary>
        private string originalDefinitionContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRowViewModel" /> class
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase{Thing}" /> that is the container of this
        /// <see cref="IRowViewModelBase{Thing}" />
        /// </param>
        public RequirementRowViewModel(
            Requirement requirement,
            ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(requirement, session, containerViewModel)
        {
            this.InitializeCommands();
            this.EventPublisher = new EventPublisher();

            this.CanEdit = this.Session.PermissionService.CanWrite(this.Thing);

            this.WhenAnyValue(vm => vm.DefinitionContent).Subscribe(
                _ =>
                {
                    var originalDefinition = this.Thing.Definition.FirstOrDefault();
                    var originalDefinitionText = originalDefinition == null ? string.Empty : originalDefinition.Content;
                    this.IsDirty = this.DefinitionContent != originalDefinitionText;
                });

            this.UpdateProperties();
        }

        /// <summary>
        /// Check the edit permission using <see cref="PermissionService" />
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets the <see cref="EventPublisher" />
        /// </summary>
        public EventPublisher EventPublisher
        {
            get => this.eventPublisher;
            private set => this.RaiseAndSetIfChanged(ref this.eventPublisher, value);
        }

        /// <summary>
        /// Gets or sets the Save <see cref="ReactiveCommand" /> to save the content of the note.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Cancel <see cref="ReactiveCommand" /> to save the content of the note.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the content of the <see cref="Definition" /> of the <see cref="Requirement" />
        /// </summary>
        public string DefinitionContent
        {
            get => this.definitionContent;
            set => this.RaiseAndSetIfChanged(ref this.definitionContent, value);
        }

        /// <summary>
        /// Gets or sets the categories of the <see cref="Requirement" /> as comma separated short names
        /// </summary>
        public string Categories
        {
            get => this.categories;
            set => this.RaiseAndSetIfChanged(ref this.categories, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the current row is dirty or not
        /// </summary>
        public bool IsDirty
        {
            get => this.isDirty;
            set => this.RaiseAndSetIfChanged(ref this.isDirty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="BreadCrumb" /> property
        /// </summary>
        /// <remarks>
        /// The <see cref="BreadCrumb" /> property is used for sorting
        /// </remarks>
        public string BreadCrumb
        {
            get => this.breadCrumb;
            set => this.RaiseAndSetIfChanged(ref this.breadCrumb, value);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing" /> that is being represented by the view-model
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
        /// Dispose of this row
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.definitionSubscription != null)
            {
                this.definitionSubscription.Dispose();
            }

            this.SaveCommand.Dispose();
            this.CancelCommand.Dispose();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.OwnerShortName = this.Thing.Owner != null ? this.Thing.Owner.ShortName : string.Empty;

            this.SetDefinitionContent();

            this.SetCategories();

            this.BreadCrumb = this.Thing.BreadCrumb();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> handler for this <see cref="definition" />
        /// </summary>
        /// <param name="defEvent">The <see cref="ObjectChangedEvent" /></param>
        private void OnDefinitionUpdate(ObjectChangedEvent defEvent)
        {
            if (defEvent.EventKind == EventKind.Removed)
            {
                this.definition = null;
                this.definitionSubscription.Dispose();
            }

            this.SetDefinitionContent();
        }

        /// <summary>
        /// Sets the value of the <see cref="DefinitionContent" /> property.
        /// </summary>
        private void SetDefinitionContent()
        {
            if (this.definition == null)
            {
                this.definition = this.Thing.Definition.FirstOrDefault();

                if (this.definition == null)
                {
                    return;
                }

                this.definitionSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.definition)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.OnDefinitionUpdate);
            }

            this.originalDefinitionContent = this.definition.Content;
            this.DefinitionContent = this.originalDefinitionContent;
        }

        /// <summary>
        /// Set the <see cref="Categories" /> property based on the <see cref="Requirement.Category" /> property
        /// </summary>
        private void SetCategories()
        {
            if (!this.Thing.Category.Any())
            {
                this.Categories = "-";
            }
            else
            {
                this.Categories = string.Join(", ", this.Thing.Category.Select(x => x.ShortName));
            }
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand" />s.
        /// </summary>
        private void InitializeCommands()
        {
            var canSave = this.WhenAnyValue(vm => vm.IsDirty).Select(_ => this.IsDirty);

            this.SaveCommand =
                ReactiveCommandCreator.CreateAsyncTask(this.WriteDefinitionUpdate, canSave);

            this.CancelCommand = 
                ReactiveCommandCreator.Create(() => this.ResetContent(true), canSave);
        }

        /// <summary>
        /// Resets the content to the content of the first <see cref="Definition" /> of the <see cref="Requirement" />
        /// </summary>
        /// <param name="withWarning">
        /// If true, a warning message will be displayed.
        /// </param>
        private void ResetContent(bool withWarning)
        {
            // give a warning. If user cancels the changes are not reset.
            if (withWarning)
            {
                var yesNoDialog = new YesNoDialogViewModel(
                    "Cancel Edit",
                    "Press Yes if you want to cancel the edit. All changes will be lost");

                var dialogResult = this.dialogNavigationService.NavigateModal(yesNoDialog);

                if ((dialogResult != null) && (dialogResult.Result != null) && dialogResult.Result.Value)
                {
                    this.DefinitionContent = this.originalDefinitionContent;
                    this.EventPublisher.Publish(new ConfirmationEvent(true));
                }
            }
        }

        /// <summary>
        /// Writes updates to the <see cref="Definition" /> to the data-soource
        /// </summary>
        private async Task WriteDefinitionUpdate()
        {
            var definition = this.Thing.Definition.FirstOrDefault();

            if (definition != null)
            {
                var definitionClone = definition.Clone(false);
                definitionClone.Content = this.DefinitionContent;

                await this.DalWrite(definitionClone);

                if (!this.HasError)
                {
                    this.EventPublisher.Publish(new ConfirmationEvent(true));
                }
            }
        }
    }
}
